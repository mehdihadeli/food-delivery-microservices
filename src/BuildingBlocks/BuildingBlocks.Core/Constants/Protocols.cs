using Humanizer;

namespace BuildingBlocks.Core.Constants;

public static class Protocols
{
    public static readonly string Http = nameof(Http).Kebaberize();
    public static readonly string Https = nameof(Https).Kebaberize();
    public static readonly string HttpOrHttps = $"{Https}+{Http}";
    public static readonly string Tcp = nameof(Tcp).Kebaberize();
    public static readonly string OtlpGrpc = nameof(OtlpGrpc).Kebaberize();
    public static readonly string OtlpHttp = nameof(OtlpHttp).Kebaberize();
    public static readonly string Grpc = nameof(Grpc).Kebaberize();
}
