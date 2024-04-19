namespace Swen1Csharp.httpserver.http;

public class StringValueAttribute : Attribute
{
    public string Value { get; private set; }

    public StringValueAttribute(string value)
    {
        this.Value = value;
    }
}