using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
    .ExcludeFromManifest()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("ACCEPT_EULA", "Y");

var externalApi = builder.AddProject<ExternalApi>("ExternalApi")
    .WithReference(seq)
    .WaitFor(seq);
builder.AddProject<WebApi>("WebApi").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);
builder.AddProject<Worker>("Worker").WithReference(externalApi).WithReference(seq).WaitFor(externalApi);

builder.Build().Run();