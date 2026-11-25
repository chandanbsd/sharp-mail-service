var builder = DistributedApplication.CreateBuilder(args);

var storageKey = Convert.ToBase64String("sharp-mail-local-emulator-account"u8);

var mailpit = builder.AddContainer("mailpit", "docker.io/axllent/mailpit", "latest")
    .WithEnvironment("MP_SMTP_ALLOWED_RECIPIENTS", "@demo[.]invalid$")
    .WithEnvironment("MP_SMTP_DISABLE_RDNS", "true")
    .WithEndpoint(targetPort: 1025, name: "smtp", scheme: "tcp")
    .WithHttpEndpoint(port: 18025, targetPort: 8025, name: "inbox");

var sqlServer = builder.AddSqlServer("sql")
    .WithImage("mssql/azure-sql-db-server", "latest")
    .WithContainerRuntimeArgs("--restart=on-failure:3")
    .WithDataVolume("sharpmail-azuresql");
var sql = sqlServer.AddDatabase("SharpMail");
var blobs = builder.AddContainer("blobs", "mcr.microsoft.com/azure-storage/azurite", "latest")
    .WithArgs("azurite-blob", "--blobHost", "0.0.0.0", "--location", "/data", "--disableProductStyleUrl", "--skipApiVersionCheck")
    .WithEnvironment("AZURITE_ACCOUNTS", $"sharpmail:{storageKey}")
    .WithVolume("sharpmail-blobs", "/data")
    .WithHttpEndpoint(targetPort: 10000, name: "blob");

var cosmos = builder.AddContainer("cosmos", "mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator", "vnext-latest")
    .WithArgs("--protocol", "https")
    .WithVolume("sharpmail-cosmos", "/data")
    .WithHttpsEndpoint(targetPort: 8081, name: "gateway");

AddApi("sql", 18081).WithReference(sql).WaitFor(sql);
AddApi("blobs", 18082).WithEnvironment("StoreEndpoint", blobs.GetEndpoint("blob"))
    .WithEnvironment("StorageKey", storageKey).WaitFor(blobs);
AddApi("cosmos", 18083).WithEnvironment("StoreEndpoint", cosmos.GetEndpoint("gateway")).WaitFor(cosmos);

builder.Build().Run();

IResourceBuilder<ContainerResource> AddApi(string store, int port) =>
    builder.AddDockerfile($"api-{store}", "../..", "demo/SharpMail.Api/Dockerfile")
        .WithHttpEndpoint(port: port, targetPort: 8080)
        .WithEnvironment("AuditProvider", store)
        .WithEnvironment("MailpitEndpoint", mailpit.GetEndpoint("smtp"))
        .WaitFor(mailpit);
