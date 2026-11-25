using System.Text.Json.Serialization;
using SharpMail;
using SharpMail.Api;
using SharpMail.Liquid;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSharpMail<MailpitSender>()
    .AddLiquidEmailTemplates(Path.Combine(AppContext.BaseDirectory, "EmailTemplates"));
var initialize = DemoStorage.Configure(builder);
var app = builder.Build();

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (AttemptConflictException error) { await Results.Conflict(new { error.Message }).ExecuteAsync(context); }
    catch (ArgumentException error) { await Results.BadRequest(new { error.Message }).ExecuteAsync(context); }
});

app.MapGet("/", () => new { store = builder.Configuration["AuditProvider"], mailer = "local-mailpit" });
app.MapPost("/attempt-ids", () => new { attemptId = EmailSubmission.NewAttemptId() });
app.MapPost("/preview", (TemplateRequest request, ITemplateRenderer renderer, CancellationToken token) =>
    renderer.RenderAsync(request.TemplateKey, request.Model, token));
app.MapPut("/attempts/{id:guid}", async (Guid id, SubmitRequest request, EmailSubmission submission, CancellationToken token) =>
{
    var result = await submission.SubmitAsync(id, request.CorrelationId, request.Message, token);
    return Results.Json(result, statusCode: result.OutcomePersisted ? 200 : 503);
});
app.MapGet("/attempts/{id:guid}", async (Guid id, EmailAudit audit, CancellationToken token) =>
    await audit.GetAsync(id, token) is { } detail ? Results.Ok(detail) : Results.NotFound());
app.MapPost("/delivery-events", async (DeliveryRequest request, EmailAudit audit, CancellationToken token) =>
{
    var added = await audit.RecordAsync(request.AttemptId, request.Transport, request.ProviderMessageId, request.Notification, token);
    return added.HasValue ? Results.Ok(new { appended = added.Value }) : Results.UnprocessableEntity(new { reason = "Unresolved attempt" });
});

using var startup = new CancellationTokenSource(TimeSpan.FromMinutes(3));
while (true)
{
    try { await initialize(app.Services, startup.Token); break; }
    catch (Exception error) when (!startup.IsCancellationRequested)
    {
        app.Logger.LogWarning("Waiting for audit storage ({ErrorType})", error.GetType().Name);
        await Task.Delay(TimeSpan.FromSeconds(3), startup.Token);
    }
}
await app.RunAsync();

internal sealed record TemplateRequest(string TemplateKey, Dictionary<string, string> Model);
internal sealed record SubmitRequest(string? CorrelationId, EmailMessage Message);
internal sealed record DeliveryRequest(Guid? AttemptId, string? Transport, string? ProviderMessageId, DeliveryNotification Notification);
