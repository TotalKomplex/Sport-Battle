using Swen1Csharp.httpserver.server;
using Swen1Csharp.httpserver.utils;
using Swen1Csharp.seb.service;


Server server = new Server(10001, configureRouter());
server.Start();

static Router configureRouter()
{
    Router router = new Router();
    router.addService("/echo", new EchoService());
    router.addService("/users", new User());
    router.addService("/sessions", new Login());
    router.addService("/stats", new Stats());
    router.addService("/score", new Score());
    router.addService("/history", new History());
    router.addService("/tournament", new Tournament());

    return router;
}