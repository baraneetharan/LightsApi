using Azure;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<LightContext>(opt => opt.UseInMemoryDatabase("LightList"));
builder.Services.AddScoped<LightPlugin>();

// Load the .env file
Env.Load(".env");
string githubKey = Env.GetString("GITHUB_KEY");
builder.Services.AddSingleton(sp =>
{
    var endpoint = new Uri("https://models.inference.ai.azure.com");
    var credential = new AzureKeyCredential("githubkey");
    var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("gpt-4o-mini", endpoint.ToString(), githubKey).Build();
    return kernel;
}
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();