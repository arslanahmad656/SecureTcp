using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SecureTcp.Shared.Events;
public class ServerStoppedListeningEventArgs (EndPoint endPoint) : EventArgs
{
    public EndPoint EndPoint => endPoint;
}
