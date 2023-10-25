using System.Diagnostics.CodeAnalysis;

namespace CoolNewProject.Web.Endpoints; 

// marker interface for API endpoints - the concrete used method is determined by a heuristic so only add a single public method
// prefer methods like Handle/HandleAsync
[SuppressMessage("ReSharper", "InconsistentNaming")]
public interface IEndpoint {
    public interface WithRequest<TRequest> {
        public interface WithoutResult : IEndpoint, WithRequest<TRequest> {
            public Task HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }
        
        public interface WithResult<TResponse> : IEndpoint, WithRequest<TRequest> {
            public Task<TResponse> HandleAsync(
                TRequest request,
                CancellationToken cancellationToken = default
            );
        }
    }
    
    public interface WithoutRequest {
        public interface WithoutResult : IEndpoint, WithoutRequest {
            public Task HandleAsync(
                CancellationToken cancellationToken = default
            );
        }
        
        public interface WithResult<TResponse> : IEndpoint, WithoutRequest {
            public Task<TResponse> HandleAsync(
                CancellationToken cancellationToken = default
            );
        }
    }
    
    public interface WithContext : IEndpoint {
        public Task HandleAsync(HttpContext context);
    }
}