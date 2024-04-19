using Swen1Csharp.seb;

namespace Swen1Csharp.httpserver.server;

public interface Service {
    Response HandleRequest(Request request);
    public RouterOverhead routerOverhead { get; set; }
}
