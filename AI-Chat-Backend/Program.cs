using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Azure.AI.OpenAI;
using Azure.Identity;

// ---------------------------------------------------------------------
// 1. Load configuration (endpoint + deployment name come from user secrets)
// ---------------------------------------------------------------------
IConfigurationRoot config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string? endpoint = config["AZURE_OPENAI_ENDPOINT"];
string? deployment = config["AZURE_OPENAI_GPT_NAME"];

if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment))
{
    Console.WriteLine("Missing configuration. Run these commands first:");
    Console.WriteLine("  dotnet user-secrets set AZURE_OPENAI_ENDPOINT \"https://your-resource.openai.azure.com/\"");
    Console.WriteLine("  dotnet user-secrets set AZURE_OPENAI_GPT_NAME \"your-deployment-name\"");
    return;
}

// ---------------------------------------------------------------------
// 2. Create the chat client
//    Uses DefaultAzureCredential (Entra ID) so no API key is stored.
//    Run `az login` locally before running this app.
// ---------------------------------------------------------------------
IChatClient chatClient = new AzureOpenAIClient(
        new Uri(endpoint),
        new DefaultAzureCredential())
    .GetChatClient(deployment)
    .AsIChatClient();

// ---------------------------------------------------------------------
// 3. Conversation loop
//    We resend the full history each turn - LLMs are stateless per call.
// ---------------------------------------------------------------------
List<ChatMessage> history = new()
{
    new ChatMessage(ChatRole.System, "You are a friendly, concise assistant.")
};

Console.WriteLine("Chatbot ready. Type 'exit' to quit.\n");

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
        break;

    history.Add(new ChatMessage(ChatRole.User, input));

    var response = await chatClient.GetResponseAsync(history);
    Console.WriteLine($"AI: {response.Text}\n");

    history.Add(new ChatMessage(ChatRole.Assistant, response.Text));
}

Console.WriteLine("Goodbye!");
