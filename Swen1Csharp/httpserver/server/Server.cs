using System.Net;
using System.Net.Sockets;
using Swen1Csharp.httpserver.utils;
using System.Collections.Generic;

namespace Swen1Csharp.httpserver.server;

public class Server
{
    private readonly int _port;
    private readonly Router? _router;

    public Server(int port, Router router)
    {
        this._port = port;
        this._router = router;
    }

    public void Start()
    {
        

        var httpServer = new TcpListener(IPAddress.Loopback, _port);
        httpServer.Start();
        Console.WriteLine("Start http-server...");
        while (true)
        {
            var clientSocket = httpServer.AcceptTcpClient();
            if (clientSocket != null)
            {
                RequestHandler socketHandler = new RequestHandler(clientSocket, _router);
                ThreadPool.QueueUserWorkItem(state => socketHandler.Run());
            }
            else
            {
                throw new Exception("socket NUll");
            }
        }
    }

}