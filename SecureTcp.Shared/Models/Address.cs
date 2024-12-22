using System.Net;

namespace SecureTcp.Shared.Models;

public record Address(IPAddress Ip, uint Port);
