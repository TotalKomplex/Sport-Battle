using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Swen1Csharp.httpserver.http;
using Swen1Csharp.httpserver.server;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace Swen1Csharp.Tests.httpserver.server;


[TestFixture]
public class ResponseTest
{

    [Test]
    public void Correct_Response_Fromat()
    {
        Response response = new Response();
        response.Content = "[1,2,3]";
        response.ContentTypeOf = ContentTypeOf.JSON;
        
        var expected=@"HTTP/1.1 200 OK
Cache-Control: max-age=0
Connection: close
Date: Thu, 18 Apr 2024 23:02:58 GMT
Expires: Thu, 18 Apr 2024 23:02:58 GMT
Content-Type: application/json
Content-Length: 7

[1,2,3]".Split("\n");
        var got = response.Get().Split("\n");
        for(var i=0;i< got.Length;i++)
        {
            TestContext.WriteLine(i);
            if (!got[i].Contains("Expires") && !got[i].Contains("Date"))
            {
                Assert.AreEqual(got[i], expected[i]);
            }
            
        }
        
    }
}