namespace CoolNewProject.Web.MinimalApi; 

public interface IEndpoint {
    void AddRoute(IEndpointRouteBuilder app);
}