using System.Runtime.InteropServices.JavaScript;

namespace Swen1Csharp.httpserver.server;

public class HeaderMap
{
    public static String CONTENT_LENGTH_HEADER = "Content-Length";
    public static String HEADER_NAME_VALUE_SEPARATOR = ":";
    private Dictionary<String, String> _headers = new Dictionary<string, string>();

    public int getContentLenght()
    {
        String header = _headers[CONTENT_LENGTH_HEADER];
        if (header == "")
        {
            return 0;
        }

        return int.Parse(header);
    }
    public Tuple<bool,string> getToken()
    {
        if(_headers.ContainsKey("Authorization"))
            return Tuple.Create(true,_headers["Authorization"]);
        return Tuple.Create(false, "");
    }
    public void ingest(String headerLine)
    {
        if (headerLine.Contains(HEADER_NAME_VALUE_SEPARATOR))
        {
            var split = headerLine.Split(HEADER_NAME_VALUE_SEPARATOR , 2);
            _headers[split[0]] = split[1].Trim();
        }
        else
        {
            Console.WriteLine("start"+headerLine+"end");
            throw new Exception("False Line");
        }
    }

    public String getHeader(String headerName)
    {
        return _headers[headerName];
    }
}