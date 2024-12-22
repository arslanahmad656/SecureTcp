using System.Net;

namespace SecureTcp.Shared.Events;
public class ServerStartedListeningEventArgs (EndPoint endPoint): EventArgs
{
    public EndPoint EndPoint => endPoint;
}