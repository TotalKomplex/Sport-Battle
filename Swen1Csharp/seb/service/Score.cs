
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.seb.service;

public class Score : Service
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
        Tournament.CalculateTournament(cmd);

        cmd.CommandText = $"SELECT name, total_pushups, elo FROM users ORDER BY elo DESC";

        using var reader = cmd.ExecuteReader();
        var highscores = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            var a = new Dictionary<string, object>
            {
                { "Name", reader.GetString(0)},
                { "Total Pushups", reader.GetInt32(1) },
                { "Elo", reader.GetInt32(2) }
            };
            highscores.Add(a);
        }

        _response.ContentTypeOf = ContentTypeOf.JSON;
        return _response.Return(JsonConvert.SerializeObject(highscores));
    }
}