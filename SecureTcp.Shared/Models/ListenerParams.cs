using System.Net;
using System.Security.Authentication;

namespace SecureTcp.Shared.Models;
public record ListenerParams(
    Address Address,
    CertificateInfo CertificateInfo,
    SslProtocols EnabledProtocols,
    bool ClientCertificateRequired,
    bool CheckCertificateRevocation);