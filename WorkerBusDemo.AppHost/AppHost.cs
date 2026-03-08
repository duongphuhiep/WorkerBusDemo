using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", 5341)
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var sqlPassword = builder.AddParameter("sql-password", "Password123!");
var sqlServer = builder.AddSqlServer(
        "sqlserver",
        sqlPassword,
        14333
    )
    .WithImage("azure-sql-edge")
    .WithImageTag("latest")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("workerbusdemo-sqlserver-data");

var sqlCreationScript = await File.ReadAllTextAsync("Northwind.sql");
var northwindDb = sqlServer
    .AddDatabase("northwind", "northwind")
    .WithCreationScript(sqlCreationScript);

var externalApi = builder.AddProject<ExternalApi>("ExternalApi", "http")
    .WithReference(seq)
    .WaitFor(seq);
var webApi = builder.AddProject<WebApi>("WebApi", "http")
    .WithReference(externalApi)
    .WithReference(seq)
    .WithReference(sqlServer)
    .WithReference(northwindDb)
    .WaitFor(externalApi)
    .WaitFor(sqlServer)
    .WaitFor(northwindDb);
builder.AddProject<Worker>("Worker")
    .WithReference(externalApi)
    .WithReference(seq)
    .WithReference(sqlServer)
    .WithReference(northwindDb)
    .WaitFor(externalApi)
    .WaitFor(sqlServer)
    .WaitFor(northwindDb);

// Register services with the API Reference
var scalar = builder.AddScalarApiReference();
scalar
    .WithApiReference(webApi)
    .WithApiReference(externalApi);

await builder.Build().RunAsync();