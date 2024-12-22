#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SecureTcp.Client.App;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter server IP: ");
        var serverAddress = Console.ReadLine();
        Console.WriteLine("Enter Port: ");
        var port = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine();

        try
        {
            using var client = new TcpClient();
            Console.WriteLine($"Connecting to {serverAddress}:{port}...");
            await client.ConnectAsync(serverAddress, port);
            Console.WriteLine("Connected to the server.");

            using (NetworkStream networkStream = client.GetStream())
            using (SslStream sslStream = new SslStream(networkStream, false, ValidateServerCertificate))
            {
                // Authenticate the client (TLS handshake)
                await sslStream.AuthenticateAsClientAsync(serverAddress, null, SslProtocols.Tls13 | SslProtocols.Tls12, true);
                Console.WriteLine("TLS connection established.");

                await CommunicationLoopAsync(sslStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // Accept all certificates for testing purposes. Do NOT use this in production.
        // In production, validate the server certificate properly.
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        Console.WriteLine($"Certificate error: {sslPolicyErrors}");
        return false;
    }

    private static async Task CommunicationLoopAsync(SslStream sslStream)
    {
        using (StreamReader reader = new StreamReader(sslStream))
        using (StreamWriter writer = new StreamWriter(sslStream) { AutoFlush = true })
        {
            Console.WriteLine(await reader.ReadLineAsync()); // Initial server message

            while (true)
            {
                // Get user input
                Console.Write("You: ");

                string message = Console.ReadLine();

                if (string.IsNullOrEmpty(message) || message.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Disconnecting...");
                    break;
                }

                // Send message to the server
                await writer.WriteLineAsync(message);

                // Receive response from the server
                string serverResponse = await reader.ReadLineAsync();
                if (serverResponse == null)
                {
                    Console.WriteLine("Server disconnected.");
                    break;
                }

                Console.WriteLine(serverResponse);
            }
        }
    }
}


#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.