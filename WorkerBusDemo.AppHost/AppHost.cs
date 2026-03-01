using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq", 5341)
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var externalApi = builder.AddProject<ExternalApi>("ExternalApi", launchProfileName: "http")
    .WithReference(seq)
    .WaitFor(seq);
builder.AddProject<WebApi>("WebApi", launchProfileName: "http").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);
builder.AddProject<Worker>("Worker").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);
await builder.Build().RunAsync();