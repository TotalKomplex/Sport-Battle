
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Npgsql.Internal;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace Swen1Csharp.seb.service;

public class Tournament : Service
{
    private Response _response = new Response();
    public RouterOverhead routerOverhead { get; set; } = new RouterOverhead();
    public Response HandleRequest(Request request)
    {
        using var cmd = routerOverhead.GetCommand();
        using var con = cmd.Connection;

        (bool success, _) = routerOverhead.CheckToken(cmd, request.token);
        if (!success)
            return _response.Return("Unknown token!");

        cmd.CommandText = "SELECT id FROM tournaments LIMIT 1";
        if (cmd.ExecuteScalar() == null)
            return _response.Return("No Tournament found!");

        if (request._method == Method.POST)
            return TournamentHistory(cmd, request);

        //tournament muss calculaten damit auch users updated werden
        var tournament = CalculateTournament(cmd);
     
        if (CheckTournamentStatus(cmd))
            return _response.Return("No Tournament running!");
        var info = $"Current leaders: [{String.Join(", ", tournament.leaders)}], with a lead of {tournament.record}";
        Console.WriteLine(info);
        return _response.Return(info);
        
    }

    private Response TournamentHistory(NpgsqlCommand cmd,Request request)
    {

        var dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(request.body);
        cmd.CommandText = $"SELECT id, start_time FROM tournaments";
        if (dict["ID"]>0)
        {
            cmd.Parameters.AddWithValue("id", dict["ID"]);
            cmd.CommandText = "SELECT id FROM tournaments WHERE id = @id";
            if (cmd.ExecuteScalar() == null)
                return _response.Return("Tournament not found!");
            var tournament = CalculateTournament(cmd, dict["ID"]);
            return _response.Return($"The Tournament winner(s) is/are: [{String.Join(", ", tournament.leaders)}], with a lead of {tournament.record}");
        }

        cmd.CommandText = $"SELECT id, start_time FROM tournaments";

        using var reader = cmd.ExecuteReader();

        var tournaments = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            var a = new Dictionary<string, object>
            {
                { "id", reader.GetInt32(0)},
                { "start_time", reader.GetDateTime(1).ToString("MM/dd/yyyy H:mm:ss") },
            };
            tournaments.Add(a);
        }

        _response.ContentTypeOf = ContentTypeOf.JSON;
        return _response.Return(JsonConvert.SerializeObject(tournaments));
    }

    private static void UpdateUsersFromTournament(NpgsqlCommand cmd, TournamentModel tournament)
    {
        // Retrieve the winners_id from the tournaments table for the given tournamentID
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("tournamentId", tournament.tournamentID);
        cmd.CommandText = "SELECT winners_id FROM tournaments WHERE id = @tournamentId";
        var result = cmd.ExecuteScalar();

        // Check if winners_id is not empty, if so, return because the tournament has already been evaluated
        if (result != null && result != DBNull.Value)
            return;
            
        foreach (string loser in tournament.losers)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("losername", loser);
            cmd.CommandText = "UPDATE users SET elo = elo - 1 WHERE username = @losername";
            cmd.ExecuteNonQuery();
        }
        if(tournament.leaders.Length == 1) { //only one winner 2 elo is added
            cmd.Parameters.AddWithValue("winner", tournament.leaders[0]);
            cmd.CommandText = "UPDATE users SET elo = elo + 2 WHERE username = @winner";
            cmd.ExecuteNonQuery();
        }
        else
        {
            foreach (string winner in tournament.leaders)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("winnername", winner);
                cmd.CommandText = "UPDATE users SET elo = elo - 1 WHERE username = @winnername";
                cmd.ExecuteNonQuery();
            }
        }
        // Set tournament table winner_id to tournament.winnerIDs
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("tournamentId", tournament.tournamentID);
        cmd.Parameters.AddWithValue("winnersId", tournament.winnerIDs); // Assuming tournament.winnerIDs is an array or list of IDs
        cmd.CommandText = "UPDATE tournaments SET winners_id = @winnersId WHERE id = @tournamentId";
        cmd.ExecuteNonQuery();
    }
    public static TournamentModel CalculateTournament(NpgsqlCommand cmd, int t_id=-1)
    {
        if (t_id == -1)
        {
            cmd.CommandText = "SELECT id FROM tournaments ORDER BY start_time DESC LIMIT 1";
            var _t_id = cmd.ExecuteScalar();
            if (_t_id is int a)
                t_id = a;
            else return new TournamentModel();
        }

        var tournament= new TournamentModel { tournamentID = t_id };
        cmd.Parameters.AddWithValue("t_id", t_id);
        cmd.CommandText = "SELECT user_id, number FROM history WHERE tournament_id = @t_id";
        var reader = cmd.ExecuteReader();
        Dictionary<int, int> tournamentEntries = new(); //<int, int> userid and pushup count
        while (reader.Read())
        {
            int user_id = reader.GetInt32(0);
            int number = reader.GetInt32(1);
            if (tournamentEntries.ContainsKey(user_id))
                tournamentEntries[user_id] += number;
            else
                tournamentEntries.Add(user_id, number);
        }
        reader.Close();

        tournament.record = tournamentEntries.Values.Max();
        tournament.participants = tournamentEntries.Keys.ToArray();
        var winnerIDList = new List<int>();
        var loserIDList = new List<int>();
        
        foreach (var pair in tournamentEntries)
        {
            if (pair.Value == tournament.record)
                winnerIDList.Add(pair.Key);
            else
                loserIDList.Add(pair.Key);
        }
        tournament.winnerIDs = winnerIDList.ToArray();
        tournament.leaders = UserIDToUsername(cmd, tournament.winnerIDs);
        tournament.losers = UserIDToUsername(cmd, loserIDList.ToArray());

        cmd.Parameters.AddWithValue("t_id", t_id);
        cmd.CommandText = "SELECT start_time FROM tournaments where id = @t_id";
        tournament.startTime = (DateTime)cmd.ExecuteScalar();
        tournament.closed = ((DateTime.Now - tournament.startTime).TotalMinutes) >= 2;
        //CheckTournamentStatus checks if newest tournament is running, we need if specific tournament is running
        if (tournament.closed)
            UpdateUsersFromTournament(cmd,tournament);
        return tournament;
    }
    private static string[] UserIDToUsername(NpgsqlCommand cmd, int[] userID)
    {
        if(userID.Length == 0)
            return [];
        
        var idList = new List<string>();
        var index = 0;
        cmd.Parameters.Clear();
        foreach (var id in userID)
        {
            var paramName = "@id" + index;
            cmd.Parameters.AddWithValue(paramName, id);
            idList.Add(paramName);
            index++;
        }
        cmd.CommandText = String.Format("SELECT username FROM users WHERE id IN({0})", string.Join(",", idList));

        using var reader = cmd.ExecuteReader();
        var usernames = new List<string>();
        while (reader.Read())
        {
            usernames.Add(reader.GetString(0));
        }
        return usernames.ToArray();
    }
    public static bool CheckTournamentStatus(NpgsqlCommand cmd)
    {
        cmd.CommandText = "SELECT start_time FROM tournaments ORDER BY start_time DESC LIMIT 1";
        var tournamentStartTime = Convert.ToDateTime(cmd.ExecuteScalar());
        return ((DateTime.Now - tournamentStartTime).TotalMinutes)>=2;
    }
}