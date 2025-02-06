namespace CAP.Infrastructure;

public abstract class EndpointGroupBase {
    public string GroupName { get; }

    protected EndpointGroupBase() {
        GroupName = GetType().Name;
    }

    public abstract void Map(IEndpointRouteBuilder routeBuilder);
}
