namespace SecureTcp.Shared.Events;
public class MessageReceivedEventArgs : ClientEventArgsBase
{
    public required string Message { get; init; }
}
