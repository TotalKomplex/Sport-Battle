using Swen1Csharp.httpserver.server;

namespace Swen1Csharp.httpserver.utils;

public class Router
{
    public Dictionary<String, Service> _serviceRegistry;

    public Router()
    {
        _serviceRegistry = new Dictionary<String, Service>();
    }

    public Service resolve(String route)
    {
        return _serviceRegistry[route];
    }

    public void addService(String route, Service service)
    {
        _serviceRegistry.Add(route, service);
    }
}