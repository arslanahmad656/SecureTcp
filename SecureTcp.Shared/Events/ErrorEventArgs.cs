using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureTcp.Shared.Events;
public class ErrorEventArgs(string message, Exception ex) : EventArgs
{
    public string Message { get; } = message;
    public Exception Exception { get; } = ex;
}
