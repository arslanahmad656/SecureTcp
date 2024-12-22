using SecureTcp.Shared.Events;
using SecureTcp.Shared.Models;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using ErrorEventArgs = SecureTcp.Shared.Events.ErrorEventArgs;

namespace SecureTcp.Server;
public sealed class Listener(ListenerParams parameters) : IDisposable
{
    private TcpListener? listener;
    private bool disposed;
    private readonly Dictionary<Guid, TcpClient> connectedClients = [];
    public bool IsListening => listener != null;

    private Lazy<X509Certificate2> certificate = new(() => new(parameters.CertificateInfo.FilePath, parameters.CertificateInfo.Password));

    public event EventHandler<ServerStartedListeningEventArgs>? ServerStartedListeningEvent;
    public event EventHandler<ClientConnectedEventArgs>? ClientConnectedEvent;
    public event EventHandler<ClientConnectedEventArgs>? ClientDisconnectedEvent;
    public event EventHandler<ServerStoppedListeningEventArgs>? ServerStoppedListeningEvent;
    public event EventHandler<ErrorEventArgs>? ErrorEvent;
    public event EventHandler<TlsHandShakeCompletedEventArgs>? TlsHandShakeCompletedEvent;
    public event EventHandler<MessageReceivedEventArgs>? MessageReceivedEvent;
    public event EventHandler? DisposeStartedEvent;
    public event EventHandler? DisposeCompletedEvent;

    public async Task Listen(CancellationToken cancellationToken)
    {
        try
        {
            listener = new(parameters.Address.Ip, (int)parameters.Address.Port);
            listener.Start();
            ServerStartedListeningEvent?.Invoke(this, new(listener.LocalEndpoint));
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (disposed)
                    {
                        throw new InvalidOperationException("The listener has already been disposed off.");
                    }

                    var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                    var clientId = Guid.NewGuid();
                    ClientConnectedEvent?.Invoke(this, new () { ClientId = clientId });
                    _ = HandleClient(client, clientId, cancellationToken); // Start a background task for each connected client
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        catch (Exception ex)
        {
            ErrorEvent?.Invoke(this, new("Error occurred in application. Inspect the exception object for details.", ex));
        }
        finally
        {
            ((IDisposable)this).Dispose();
        }
    }

    private async Task HandleClient(TcpClient client, Guid clientId, CancellationToken cancellationToken)
    {
        try
        {
            var certificate = this.certificate.Value;
            using var networkStream = client.GetStream();
            using var sslStream = new SslStream(networkStream, false);
            await sslStream.AuthenticateAsServerAsync(certificate, clientCertificateRequired: false,
                                                          enabledSslProtocols: SslProtocols.Tls13 | SslProtocols.Tls12,
                                                          checkCertificateRevocation: true);
            
            TlsHandShakeCompletedEvent?.Invoke(this, new() { ClientId = clientId });

            var handler = new ClientHandler(clientId, sslStream);
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await handler.ReceiveMessage().ConfigureAwait(false);
                    MessageReceivedEvent?.Invoke(this, new() { ClientId = clientId, Message = message ?? string.Empty });
                    await handler.SendMessage($"{clientId}: {message}").ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }
        catch (Exception ex)
        {
            ex.Data.Add("clientid", clientId);
            ErrorEvent?.Invoke(this, new("Error occurred while handling a client.", ex));
            ((IDisposable)this).Dispose();
        }
        finally
        {
            client.Close();
        }
    }

    void IDisposable.Dispose()
    {
        try
        {
            DisposeStartedEvent?.Invoke(this, new());
            if (listener is not null)
            {
                listener.Stop();
                ServerStoppedListeningEvent?.Invoke(this, new(listener.LocalEndpoint));
            }

            foreach (var client in connectedClients)
            {
                try
                {
                    client.Value.Close();
                    ClientDisconnectedEvent?.Invoke(this, new() { ClientId = client.Key });
                }
                catch (Exception ex)
                {
                    ex.Data.Add("clientid", client.Key);
                    ErrorEvent?.Invoke(this, new("Error occurred while closing a client.", ex));
                }
            }

            DisposeCompletedEvent?.Invoke(this, new());
        }
        catch (Exception ex)
        {
            ErrorEvent?.Invoke(this, new ErrorEventArgs("Error occurred while disposing. Resources might not have been reclaimed.", ex));
        }
        finally
        {
            disposed = true;
        }
    }
}
