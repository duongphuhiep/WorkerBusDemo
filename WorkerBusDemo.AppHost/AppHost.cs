using Projects;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", 5341)
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var externalApi = builder.AddProject<ExternalApi>("ExternalApi", launchProfileName: "http")
    .WithReference(seq)
    .WaitFor(seq);
var webApi = builder.AddProject<WebApi>("WebApi", launchProfileName: "http").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);
builder.AddProject<Worker>("Worker").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);

// Register services with the API Reference
var scalar = builder.AddScalarApiReference();
scalar
    .WithApiReference(webApi)
    .WithApiReference(externalApi);

await builder.Build().RunAsync();