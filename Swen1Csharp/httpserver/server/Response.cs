using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Mime;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using Swen1Csharp.httpserver.http;

namespace Swen1Csharp.httpserver.server;

public class Response
{
    private int _httpStatus;
    private String _contentTypeOf;
    private String _message;
    private String _content;
    public String Content {
        set{_content = value; }
        get {return _content; }
    }
    public ContentTypeOf ContentTypeOf{
        set { _contentTypeOf = value.GetStringValue();}
    }
    public HttpStatus HttpStatus { set
        {
            _httpStatus = (int)value;
            _message = value.ToString();
        } 
    }
    public Response Return(string content)
    {
        this.Content = content;
        return this;
    }
    public Response()
    {
        _httpStatus = (int)HttpStatus.OK;
        _message = HttpStatus.OK.ToString();
        _contentTypeOf = ContentTypeOf.PLAIN_TEXT.GetStringValue();
        _content = "";
    }
    public string Get()
    {
        string localDatetime = DateTime.UtcNow.ToString("R");
        StringBuilder response = new StringBuilder();

        response.Append("HTTP/1.1 ");
        response.Append(this._httpStatus);
        response.Append(" ");
        response.Append(this._message);
        response.Append("\r\n");

        response.Append("Cache-Control: max-age=0\r\n");
        response.Append("Connection: close\r\n");
        response.Append("Date: ");
        response.Append(localDatetime);
        response.Append("\r\n");

        response.Append("Expires: ");
        response.Append(localDatetime);
        response.Append("\r\n");

        response.Append("Content-Type: ");
        response.Append(_contentTypeOf);
        response.Append("\r\n");

        response.Append("Content-Length: ");
        response.Append(_content.Length);
        response.Append("\r\n");

        response.Append("\r\n");
        response.Append(_content);

        return response.ToString();
    }
}