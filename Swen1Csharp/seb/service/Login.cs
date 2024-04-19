
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System.Collections.Generic;

namespace Swen1Csharp.seb.service;

public class Login : Service
{
    private Response _response = new Response();
    public RouterOverhead routerOverhead { get; set; } = new RouterOverhead();
    public Response HandleRequest(Request request)
    {
        Dictionary<string, string>? dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.body);
        string username = dict["Username"];
        string password = dict["Password"];

        using var cmd = routerOverhead.GetCommand();
        using var con = cmd.Connection;

        cmd.Parameters.AddWithValue("username", username);
        cmd.CommandText = $"SELECT password FROM users WHERE username = @username";
        var a = Convert.ToString(cmd.ExecuteScalar());
        if (a is null || a != password)
        {
            return _response.Return("Wrong username or password!");
        }
        //string token = Guid.NewGuid().ToString();
        string token = $"Basic {username}-sebToken";
        cmd.Parameters.AddWithValue("token", token);
        cmd.Parameters.AddWithValue("timestamp", DateTime.Now);
        cmd.CommandText = "UPDATE users SET token = @token, token_time = @timestamp WHERE username = @username";
        cmd.ExecuteNonQuery();

        return _response.Return(token);
    }
}