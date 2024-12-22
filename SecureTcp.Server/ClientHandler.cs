using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ErrorEventArgs = SecureTcp.Shared.Events.ErrorEventArgs;

namespace SecureTcp.Server;
internal class ClientHandler(Guid clientId, SslStream stream)
{
    private readonly StreamReader reader = new(stream);
    private readonly StreamWriter writer = new(stream) { AutoFlush = true };

    private event EventHandler<ErrorEventArgs>? ErrorEvent; 

    public async Task<string?> ReceiveMessage()
    {
        try
        {
            var message = await reader.ReadLineAsync().ConfigureAwait(false);
            return message;
        }
        catch (Exception ex)
        {
            ex.Data.Add("clientid", clientId);
            ErrorEvent?.Invoke(this, new ErrorEventArgs("Error occurred while receiving the message.", ex));
            return null;
        }
    }

    public async Task SendMessage(string message)
    {
        try
        {
            await writer.WriteLineAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ex.Data.Add("clientid", clientId);
            ErrorEvent?.Invoke(this, new ErrorEventArgs("Error occurred while sending the message.", ex));
        }
    }
}
