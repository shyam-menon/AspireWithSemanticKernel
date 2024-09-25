using AspireSample.Web;
using AspireSample.Web.Components;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var useOpenAPI = true;
string? apiKey = string.Empty;

if (useOpenAPI)
{
    // Get the Open AI API key from environment variables
    apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("The OpenAI API key is not set in the environment variables.");
    }

    //Add OpenAI chat completion service
    builder.Services.AddKernel().AddOpenAIChatCompletion(
        modelId: "gpt-4o-mini",
        apiKey: apiKey
    );
}
else
{
    // Get the Azure API key from environment variables
    apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY");

    if (string.IsNullOrEmpty(apiKey))
    {
        Console.WriteLine("Please set the AZURE_API_KEY environment variable.");
        return;
    }

    var endpoint = Environment.GetEnvironmentVariable("AZURE_ENDPOINT");
    if (string.IsNullOrEmpty(endpoint))
    {
        Console.WriteLine("Please set the AZURE_ENDPOINT environment variable.");
        return;
    }

    //Add Azure Open AI chat completion service
    builder.Services.AddKernel().AddAzureOpenAIChatCompletion("GPT-4o",
    endpoint,
    apiKey,
    "GPT-4o");

}


//Create a theme plugin from the ThemePlugin class
builder.Services.AddScoped(sp => KernelPluginFactory.CreateFromType<ThemePlugin>(serviceProvider: sp));

builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
