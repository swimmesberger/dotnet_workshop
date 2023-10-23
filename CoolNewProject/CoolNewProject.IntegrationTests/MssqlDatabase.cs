using Testcontainers.SqlEdge;

namespace CoolNewProject.IntegrationTests; 


public sealed class MssqlDatabase : IAsyncLifetime {
    private SqlEdgeContainer Container { get; } = new SqlEdgeBuilder().Build();
    public string ConnectionString => Container.GetConnectionString();

    public Task InitializeAsync() {
        return Container.StartAsync();
    }

    public Task DisposeAsync() {
        return Container.DisposeAsync().AsTask();
    }
}