using System.Diagnostics;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.InteropServices.JavaScript;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.httpserver.utils;

public class RequestHandler
{
    private TcpClient _clientSocket;
    private Router? _router;

    public RequestHandler(TcpClient clientSocket, Router? router)
    {
        _clientSocket = clientSocket;
        _router = router;
    }

    public void Run()
    {
        try
        {
            
            var writer = new StreamWriter(_clientSocket.GetStream()) { AutoFlush = true };
            var reader = new StreamReader(_clientSocket.GetStream());
            Debug.Assert(writer != null, "Router cannot be null");
            Debug.Assert(reader != null, "Router cannot be null");
            
            Response response;
            Request request= new RequestBuilder().BuildRequest(reader);
            if (String.IsNullOrEmpty(request.pathname) ) {
                response = new Response
                {
                    HttpStatus=HttpStatus.BAD_REQUEST,
                    ContentTypeOf=ContentTypeOf.JSON,
                    Content="[]"
                };
                    
            }
            else
            {
                try
                {
                    response = _router.resolve(request.getServiceRoute()).HandleRequest(request);
                }
                catch (Exception e)
                {
                    response = new Response {
                        HttpStatus=HttpStatus.BAD_REQUEST,
                        ContentTypeOf=ContentTypeOf.JSON,
                        Content=$"[{e}]"
                    };
                }
            }
                
            writer.Write(response.Get());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}