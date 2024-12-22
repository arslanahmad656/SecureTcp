namespace SecureTcp.Shared.Events;
public class ClientEventArgsBase : EventArgs
{
    public required Guid ClientId { get; init; }
}
