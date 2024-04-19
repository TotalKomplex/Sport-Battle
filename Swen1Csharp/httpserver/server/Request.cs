using System.Xml;
using Swen1Csharp.httpserver.http;

namespace Swen1Csharp.httpserver.server;

public class Request
{
    public Method _method { get; set; }
    public String urlContent { set;get;}
    public String? pathname { set; get;}
    public String[]? pathParts{ get; set;}
    public HeaderMap headerMap { get; set; } = new HeaderMap();
    public String body{ get; set;}
    public String token { get; set; } = "";
    public String getServiceRoute()
    {
        if (pathParts == null || !this.pathParts.Any())
        {
            return "";
        }

        return "/" + this.pathParts[0];
    }
    public void SetPathname(string pathname)
    {
        this.pathname = pathname;
        pathParts= pathname.Split('/')[1..]; //ab element 1 weil sonst ["","users","etc"] statt ["users","etc"]
    }
}