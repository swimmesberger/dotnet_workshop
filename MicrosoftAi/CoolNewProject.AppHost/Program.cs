var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithImage("ankane/pgvector")
    .WithImageTag("latest");

var catalogDb = postgres.AddDatabase("catalogdb");

// Services
var catalogApi = builder
    .AddProject<Projects.CoolNewProject_Api>("catalog-api")
    .WithReference(catalogDb);

// Apps
_ = builder.AddProject<Projects.CoolNewProject_WebApp>("webapp")
    .WithReference(catalogApi)
    .WithLaunchProfile("http");

builder.Build().Run();
