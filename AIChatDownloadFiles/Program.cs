using AIChatDownloadFiles.Components;
using AIChatDownloadFiles.Services;
using Azure.AI.OpenAI;
using DevExpress.AIIntegration;
using DevExpress.AIIntegration.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.FileProviders;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Containers;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});

var openAiServiceSettings = builder.Configuration.GetSection("OpenAISettings").Get<OpenAIServiceSettings>();
if(openAiServiceSettings == null ||
    string.IsNullOrEmpty(openAiServiceSettings.Key) || openAiServiceSettings.Key == "your_key" ||
    string.IsNullOrEmpty(openAiServiceSettings.DeploymentName))
    throw new InvalidOperationException("Specify the OpenAI key and deployment name in the 'appsettings.json' file.");
var openAIClient = new OpenAIClient(openAiServiceSettings.Key);

#pragma warning disable OPENAI001
AIAgent agent = openAIClient.GetResponsesClient().AsAIAgent(
    model: openAiServiceSettings.DeploymentName,
    instructions: "You are a helpful assistant that can generate files using code.",
        name: "CodeInterpreterAgent",
    tools: [new HostedCodeInterpreterTool()]
);
builder.Services.AddSingleton(agent.AsIChatResponseProvider());

builder.Services.AddDevExpressAI();

builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.MapGet("/downloads/{containerId}/{fileId}/{fileName}", async (string containerId, string fileId, string fileName) => {
    BinaryData fileData = await openAIClient.GetContainerClient().DownloadContainerFileAsync(
        containerId,
        fileId);

    var contentType = "application/octet-stream";
    return Results.File(fileData.ToArray(), contentType, fileName);
});
#pragma warning restore OPENAI001

app.Run();