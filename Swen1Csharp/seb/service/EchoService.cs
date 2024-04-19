using Npgsql;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.seb.service;

public class EchoService : Service
{
    public RouterOverhead routerOverhead { get; set; } = new RouterOverhead();
    private Response _response = new Response();
    public Response HandleRequest(Request request)
    {
        using var cmd = routerOverhead.GetCommand();
        using var con = cmd.Connection;
        

        (bool success, string username)= routerOverhead.CheckToken(cmd, request.token);
        if (!success)
            return _response.Return("Unknown token!");

        Console.WriteLine(@$"Username {username}");
        return _response.Return("Echo");
    }
}