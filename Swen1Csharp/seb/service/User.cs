
using Newtonsoft.Json;
using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Swen1Csharp.seb.service;

public class User : Service
{
    private Response _response = new Response();
    public RouterOverhead routerOverhead { get; set; } = new RouterOverhead();
    public Response HandleRequest(Request request)
    {
        using var cmd = routerOverhead.GetCommand();
        using var con = cmd.Connection;
        if (request._method.ToString() == "POST")
            return CreateUser(request, cmd);

        string username = request.pathParts[1];
        cmd.Parameters.AddWithValue("username", username);
        cmd.CommandText = $"SELECT token FROM users WHERE username = @username";
        string? token = Convert.ToString(cmd.ExecuteScalar());

        if (request.token == token && token != "")
        {
            if (request._method.ToString() == "GET")
                return GetUser(cmd); //request muss nucht mitgesednet werden da der username eh als Parameter im Command
            return UpdateUser(request, cmd);
        }

        return _response.Return("Wrong token or username");


    }
    private Response CreateUser(Request request, NpgsqlCommand cmd)
    {
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.body);
        string username = dict["Username"];
        string password = dict["Password"];

        cmd.Parameters.AddWithValue("username", username);
        cmd.Parameters.AddWithValue("password", password);
        cmd.CommandText = $"SELECT COUNT(*) FROM users WHERE username = @username";

        if (!Convert.ToBoolean(cmd.ExecuteScalar()))
        {
            cmd.CommandText = "INSERT INTO users (username, password, name) VALUES (@username, @password, @username)";
            cmd.ExecuteNonQuery();

            return _response.Return($"Succesfully Created User {username}");
        }
        else
        {
            return _response.Return($"Username {username} already taken!");
        }
    }
    private Response GetUser(NpgsqlCommand cmd)
    {
        cmd.CommandText = $"SELECT name, bio, image FROM users WHERE username = @username";
        using var reader = cmd.ExecuteReader();
        reader.Read();
        _response.ContentTypeOf = ContentTypeOf.JSON;
        Dictionary<string, string> userData = new Dictionary<string, string>
        {
                    { "Name", reader.GetString(0) },
                    { "Bio", reader.GetString(1) },
                    { "Image", reader.GetString(2) }
        };
        return _response.Return(JsonConvert.SerializeObject(userData));
    }
    private Response UpdateUser(Request request, NpgsqlCommand cmd)
    {
        Dictionary<string, string>? dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.body);

        cmd.Parameters.AddWithValue("name", dict["Name"]);
        cmd.Parameters.AddWithValue("bio", dict["Bio"]);
        cmd.Parameters.AddWithValue("image", dict["Image"]);
        cmd.CommandText = "UPDATE users SET name = @name, bio = @bio, image = @image WHERE username = @username";
        cmd.ExecuteNonQuery();
        return _response.Return($"Updated user: {request.pathParts[1]}");
    }
}