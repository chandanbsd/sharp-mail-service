using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using SharpMail.Api;
using SharpMail.Api.Models;
using SharpMail.Api.Services;
using SharpMail.Core.Exceptions;
using SharpMail.Core.Extensions;
using SharpMail.Core.Models;
using SharpMail.Core.Services.Interfaces;
using SharpMail.Data;
using SharpMail.Data.Models;
using SharpMail.Renderer.Liquid.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSharpMail<MailpitSender>()
    .AddLiquidEmailTemplates(Path.Combine(AppContext.BaseDirectory, "EmailTemplates"));
var initialize = DemoStorage.Configure(builder);
var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("SharpMail demo API").DisableDefaultFonts());

app.Use(async (context, next) =>
{
    try { await next(); }
    catch (AttemptConflictException error) { await Results.Conflict(new { error.Message }).ExecuteAsync(context); }
    catch (ArgumentException error) { await Results.BadRequest(new { error.Message }).ExecuteAsync(context); }
});

app.MapGet("/", () => new { store = builder.Configuration["AuditProvider"], mailer = "local-mailpit" });
app.MapPost("/attempt-ids", (IEmailSubmissionService submission) => new { attemptId = submission.NewAttemptId() });
app.MapPost("/preview", (TemplateRequest request, ITemplateRenderer renderer, CancellationToken token) =>
    renderer.RenderAsync(request.TemplateKey, request.Model, token))
    .WithSummary("Preview a receipt without submitting it")
    .WithRequestExample(new TemplateRequest("orders/receipt", new() { ["orderNumber"] = "42", ["customerName"] = "Pat" }));
app.MapPut("/attempts/{id:guid}", async (Guid id, SubmitRequest request, IEmailSubmissionService submission, CancellationToken token) =>
{
    var result = await submission.SubmitAsync(id, request.CorrelationId, request.Message, token);
    return Results.Json(result, statusCode: result.OutcomePersisted ? 200 : 503);
}).WithSummary("Submit a finalized email to local Mailpit")
    .WithRequestExample(OpenApiExamples.Receipt)
    .Produces<SubmissionResult>().Produces(400).Produces(409).Produces<SubmissionResult>(503);
app.MapGet("/attempts/{id:guid}", async (Guid id, IEmailAuditService audit, CancellationToken token) =>
    await audit.GetAsync(id, token) is { } detail ? Results.Ok(detail) : Results.NotFound())
    .Produces<AuditDetail>().Produces(404);
app.MapPost("/delivery-events", async (DeliveryRequest request, IEmailAuditService audit, CancellationToken token) =>
{
    var added = await audit.RecordAsync(request.AttemptId, request.Transport, request.ProviderMessageId, request.Notification, token);
    return added.HasValue ? Results.Ok(new DeliveryReceipt(added.Value)) : Results.UnprocessableEntity(new { reason = "Unresolved attempt" });
}).WithSummary("Record a simulated delivery event for an existing attempt")
    .WithRequestExample(new DeliveryRequest(Guid.Empty, null, null,
        new DeliveryNotification("delivery-1", "To", 0, DeliveryKind.Delivered, DateTimeOffset.UnixEpoch)))
    .Produces<DeliveryReceipt>().Produces(400).Produces(422);

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
