using ChatApp.Common;

namespace ChatApp.Api.Infrastructure;

public abstract class EndpointGroupBase {
    public string GroupName { get; }

    protected EndpointGroupBase() {
        string groupName = GetType().Name;
        if (groupName.EndsWith("Endpoint")) {
            groupName = groupName[..^8];
        }
        GroupName = groupName.ToKebabCase();
    }

    public abstract void Map(IEndpointRouteBuilder routeBuilder);
}
