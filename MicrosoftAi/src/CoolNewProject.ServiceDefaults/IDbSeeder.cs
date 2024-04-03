using Microsoft.EntityFrameworkCore;

namespace CoolNewProject.ServiceDefaults;

public interface IDbSeeder<in TContext> where TContext : DbContext {
    Task SeedAsync(TContext context);
}
