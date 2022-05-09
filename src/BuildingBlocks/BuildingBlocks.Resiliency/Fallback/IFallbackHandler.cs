using MediatR;

namespace BuildingBlocks.Resiliency.Fallback;

public interface IFallbackHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleFallbackAsync(TRequest request, CancellationToken cancellationToken);
}
