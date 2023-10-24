namespace CoolNewProject.Web.Endpoints; 

// see: EndpointServiceExtensions for explanation
public interface IEndpoint {
    void AddRoute(IEndpointRouteBuilder app);
}