using Genocs.HTTP;
using Genocs.HTTP.Configurations;

namespace Genocs.LoadBalancing.Fabio.Http;

internal sealed class FabioHttpClient : GenocsHttpClient, IFabioHttpClient
{
    public FabioHttpClient(HttpClient client, HttpClientOptions options, IHttpClientSerializer serializer,
        ICorrelationContextFactory correlationContextFactory, ICorrelationIdFactory correlationIdFactory)
        : base(client, options, serializer, correlationContextFactory, correlationIdFactory)
    {
    }
}