namespace CoolNewProject.Web.Endpoints; 

/// <summary>
/// Simple endpoint "module"
/// </summary>
public interface IEndpointProvider {
    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app);
}