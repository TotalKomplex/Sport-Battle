
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Swen1Csharp.seb.service;

public class History : Service
{
    private Response _response = new Response();
    public RouterOverhead routerOverhead { get; set; } = new RouterOverhead();
    public Response HandleRequest(Request request)
    {
        using var cmd = routerOverhead.GetCommand();
        using var con = cmd.Connection;

        (bool success, string username) = routerOverhead.CheckToken(cmd, request.token);
        if (!success)
            return _response.Return("Unknown token!");

        cmd.Parameters.AddWithValue("username", username);
        if (request._method.ToString() == "GET")
            return GetHistory(cmd);
        return AddHistory(request, cmd, username);
        

    }
    private Response AddHistory(Request request, NpgsqlCommand cmd, string username)
    {
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.body);
        string count = dict["Count"];
        string duration = dict["DurationInSeconds"];
        _response.Content = "";
        if (Tournament.CheckTournamentStatus(cmd))
        {
            Tournament.CalculateTournament(cmd);
            cmd.Parameters.AddWithValue("start_time", DateTime.Now);
            cmd.CommandText = "INSERT INTO tournaments (start_time) VALUES (@start_time)";
            cmd.ExecuteNonQuery();
            _response.Content += "Started new tournament. 2 minutes left!\n";
            Console.WriteLine($"User {username} started a new Tournament!");
        }

        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("count", Int32.Parse(count));
        cmd.Parameters.AddWithValue("duration", Int32.Parse(duration));

        cmd.CommandText = "SELECT id FROM users WHERE username = @username";
        cmd.Parameters.AddWithValue("user_id", cmd.ExecuteScalar());

        cmd.CommandText = "SELECT id FROM tournaments ORDER BY start_time DESC LIMIT 1";
        cmd.Parameters.AddWithValue("tournament_id", cmd.ExecuteScalar());

        cmd.CommandText = @"INSERT INTO history (number, duration, user_id, tournament_id)
                            VALUES (@count, @duration, @user_id, @tournament_id)";
        cmd.ExecuteNonQuery();

        cmd.CommandText = "UPDATE users SET total_pushups = total_pushups + @count WHERE username = @username";
        cmd.ExecuteNonQuery();
        _response.Content += $"Succesfully created history entry with count: {count}";
        Console.WriteLine($"User {username} added {count} pushups.");
        return _response;

        
    }
    private Response GetHistory(NpgsqlCommand cmd)
    {
        cmd.CommandText = @"
        SELECT h.*
        FROM history h
        INNER JOIN users u ON h.user_id = u.id
        WHERE u.username = @username;
    ";
        using var reader = cmd.ExecuteReader();
        List<Dictionary<string, int>> history = new List<Dictionary<string, int>>();

        while (reader.Read())
        {
            Dictionary<string, int> a = new Dictionary<string, int>
                {
                    { "Pushups", reader.GetInt32(1) },
                    { "Duration", reader.GetInt32(2) },
                };
            history.Add(a);
        }

        _response.ContentTypeOf = ContentTypeOf.JSON;
        return _response.Return(JsonConvert.SerializeObject(history));
    }
}