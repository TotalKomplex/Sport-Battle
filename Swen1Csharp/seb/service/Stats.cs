
using Newtonsoft.Json;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.seb.service;

public class Stats : Service
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

        cmd.Parameters.AddWithValue("username", username);
        cmd.CommandText = $"SELECT total_pushups, elo FROM users WHERE username = @username";
        var a = Convert.ToString(cmd.ExecuteScalar());

        using var reader = cmd.ExecuteReader();

        reader.Read();
        _response.ContentTypeOf = ContentTypeOf.JSON;
        Dictionary<string, int> stats = new Dictionary<string, int>
        {
                    { "Total Pushups", reader.GetInt32(0) },
                    { "Elo", reader.GetInt32(1) }
        };
        return _response.Return(JsonConvert.SerializeObject(stats));
    }
}