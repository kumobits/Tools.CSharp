using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;
using Anthropic.SDK;
using Kumobits.Html2Markdown.CLI.Core;
using Microsoft.Extensions.Logging;

namespace Kumobits.Html2Markdown.CLI.Services;

public interface IChatService
{
    Task<string> Answer(string promptTemplate, string input);
}

public class AnthropicChatService : IChatService
{
    private readonly AppConfig appConfig;
    private readonly ILogger<AnthropicChatService> logger;

    public AnthropicChatService(AppConfig appConfig, ILogger<AnthropicChatService> logger)
    {
        this.appConfig = appConfig;
        this.logger = logger;
    }

    public async Task<string> Answer(string promptTemplate, string input)
    {

        var fullMessage = promptTemplate.Replace("{{INPUT}}", input);
        var client = new AnthropicClient(new APIAuthentication(appConfig.ANTHROPIC_API_KEY));
        var messages = new List<Message>()
        {
            new(RoleType.User, appConfig.CHAT_SYSTEM_INSTRUCTION),
            new(RoleType.Assistant, "Understood."),
            new(RoleType.User, fullMessage),
        };

        var parameters = new MessageParameters()
        {
            Messages = messages,
            MaxTokens = 1024,
            Model = AnthropicModels.Claude3Sonnet,
            Stream = false,
            Temperature = 1.0m,

        };

        try
        {
            var firstResult = await client.Messages.GetClaudeMessageAsync(parameters);
            return firstResult.Message.ToString();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching message from Anthropic, Message: {ex.Message}");
            throw;
        }


        ////print result
        //Console.WriteLine(firstResult.Message.ToString());

        ////add assistant message to chain for second call
        //messages.Add(firstResult.Message);

        ////ask followup question in chain
        //messages.Add(new Message(RoleType.User, "Who were the starting pitchers for the Dodgers?"));

        //var finalResult = await client.Messages.GetClaudeMessageAsync(parameters);

        ////print result
        //return finalResult.Message.ToString();
    }
}

internal class OpenAIChatService : IChatService
{
    public Task<string> Answer(string promptTemplate, string input)
    {
        throw new NotImplementedException();
    }
}
