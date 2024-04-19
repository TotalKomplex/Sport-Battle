using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography.X509Certificates;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.httpserver.utils;

public class RequestBuilder
{
    public Request BuildRequest(StreamReader reader)
    {
        Request request = new Request();
        string? line = reader.ReadLine();

        if (line != null)
        {
            string[] splitFirstLine = line.Split(' ');
            if (Enum.TryParse<Method>(splitFirstLine[0], out Method method))
            {
                request._method = method;
            }
            else
            {
                throw new ArgumentException($"Invalid HTTP method: {splitFirstLine[0]}");
            }


            request.SetPathname(splitFirstLine[1]);
            line = reader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                request.headerMap.ingest(line);
                line = reader.ReadLine();
            }
            
            if (request.headerMap.getToken().Item1)
            {
                request.token = request.headerMap.getToken().Item2;
                if (request._method.ToString() == "GET")
                {
                    return request;
                }
            }

            int length = request.headerMap.getContentLenght();
            if (length > 0)
            {
                char[] charBuffer = new char[length];
                reader.Read(charBuffer, 0, length);

                request.body = new string(charBuffer);
            }
        }
        return request;
    }
}