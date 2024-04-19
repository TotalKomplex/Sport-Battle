namespace Swen1Csharp.httpserver.http;

public enum ContentTypeOf
{
    [StringValue("text/plain")]
    PLAIN_TEXT,

    [StringValue("text/html")]
    HTML,

    [StringValue("application/json")]
    JSON
}