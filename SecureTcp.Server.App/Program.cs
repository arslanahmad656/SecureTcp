using SecureTcp.Server;
using SecureTcp.Shared.Models;
using System.Net;
using System.Security.Authentication;

using var server = new Listener(new (
    new (IPAddress.Loopback, 9000),
    new (@"c:\uccert.pfx", "secret"),
    SslProtocols.Tls12 | SslProtocols.Tls13,
    false,
    true));

server.ServerStartedListeningEvent += (_, eargs) => Console.WriteLine($"Server started listening at {eargs.EndPoint}.");
server.ServerStoppedListeningEvent += (_, eargs) => Console.WriteLine($"Server stopped listening from {eargs.EndPoint}.");
server.DisposeStartedEvent += (_, eargs) => Console.WriteLine("Server started to dispose off.");
server.DisposeCompletedEvent += (_, eargs) => Console.WriteLine("Server disposed off completely.");
server.ClientConnectedEvent += (_, eargs) => Console.WriteLine($"Client connected. Client Id: {eargs.ClientId}.");
server.ClientDisconnectedEvent += (_, eargs) => Console.WriteLine($"Client disconnected. Client Id: {eargs.ClientId}.");
server.ErrorEvent += (_, eargs) => Console.WriteLine($"Error occurred. {eargs.Message}. {Environment.NewLine} {eargs.Exception.Message}. {Environment.NewLine} {eargs.Exception.StackTrace}");
server.MessageReceivedEvent += (_, eargs) => Console.WriteLine($"Message received from client {eargs.ClientId}: {eargs.Message}");
server.TlsHandShakeCompletedEvent += (_, eargs) => Console.WriteLine($"TLS handshake completed with {eargs.ClientId}.");

var token = new CancellationTokenSource();
_ = server.Listen(token.Token);

while (true)
{
    Console.WriteLine("Enter exit to exit: ");
    var line = Console.ReadLine();
    if (string.Equals("exit", line, StringComparison.InvariantCultureIgnoreCase))
    {
        token.Cancel();
        break;
    }
}

Console.WriteLine("Program end...");
Console.ReadLine();