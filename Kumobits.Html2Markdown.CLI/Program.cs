using Kumobits.Html2Markdown.CLI.Core;
using Kumobits.Html2Markdown.CLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Kumobits.Html2Markdown.CLI;

internal class Program
{
    private ServiceProvider _serviceProvider = null!;
    private bool _isProduction = false;
    private Logger _logger = null!;
    private AppConfig _config;

    private static void Main(string[] args)
    {
        var program = new Program();
        program
            .Run()
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }

    private async Task Run()
    {
        var services = new ServiceCollection();
        var isProduction = false;
        var systemInstructionsFullPath = Path.Combine(Directory.GetCurrentDirectory(), "system_instructions.md");
        var settingsFileFullPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.txt");
        var inputUrlsPath = Path.Combine(Directory.GetCurrentDirectory(), "input_urls.txt");
        var config = new AppConfig();
        var logger = Logging.ConfigureSerilogForApps(services);

        _config = config;

        GuardAppConfigCorrect(config, logger);
        RegisterServices(services, config, logger);
        GuardFileExists(systemInstructionsFullPath);
        GuardFileExists(settingsFileFullPath);
        GuardFileExists(inputUrlsPath);

        config.CHAT_SYSTEM_INSTRUCTION = File.ReadAllText(systemInstructionsFullPath);

        _serviceProvider = services.BuildServiceProvider();
        _isProduction = isProduction;
        _logger = logger;

        await RunApplication();
    }

    private async Task RunApplication()
    {
        await Task.CompletedTask;

        var app = _serviceProvider.GetRequiredService<Application>();
        var fileOutputPrefix = DateTime.Now.ToString("yyyy-MM-dd HH-mm ss");
        var urls = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory(), "input_urls.txt"));
        var promptSteps = _config.PROMPT_STEPS_PARSED;

        GuardValidUris(urls);

        await app.Execute(fileOutputPrefix, urls, promptSteps);

        _logger.Information($"Executing !");
    }

    private void GuardValidUris(string[] urls)
    {
        foreach (var url in urls)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                throw new Exception($"The URL {url} is not a valid URL.");
        }
    }

    private static void RegisterServices(ServiceCollection services, AppConfig config, Logger logger)
    {
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton<WebClient>();
        services.AddSingleton<AppConfig>(config);
        services.AddSingleton<Application>();
        services.AddHttpClient();

        // Html to Markdown Strategy
        if (config.HTML2MARKDOWN_STRATEGY == "ReverseMarkdown")
        {
            logger.Information("Using ReverseMarkdown when converting HTML to Markdown");
            services.AddScoped<IHtmlToMarkdownConverter, ReverseMarkdownConverter>();
        }
        else
        {
            logger.Information("Using Html2Markdown when converting HTML to Markdown");
            services.AddScoped<IHtmlToMarkdownConverter, Html2MarkdownConverter>();
        }

        // Chat Provider
        if (config.CHAT_PROVIDER == "openai")
        {
            logger.Information("Using OpenAI as the AI provider");
            services.AddScoped<IChatService, OpenAIChatService>();
        }
        else
        {
            logger.Information("Using Anthropic as the AI provider");
            services.AddScoped<IChatService, AnthropicChatService>();
        }
    }

    private void GuardFileExists(string fullPath)
    {
        if (!File.Exists(fullPath))
            throw new Exception($"The file {fullPath} is missing. Please add it to the same folder as this file. .");
    }

    private void GuardAppConfigCorrect(AppConfig config, Logger logger)
    {
        if (string.IsNullOrEmpty(config.CHAT_PROVIDER))
            throw new Exception("CHAT_PROVIDER is not set in the configuration file");
        if (string.IsNullOrEmpty(config.HTML2MARKDOWN_STRATEGY))
            throw new Exception("HTML2MARKDOWN_STRATEGY is not set in the configuration file");
        if (string.IsNullOrEmpty(config.OPENAI_API_KEY) && config.CHAT_PROVIDER == "openai")
            throw new Exception("Your CHAT_PROVIDER is configured to 'openai', but OPENAI_API_KEY is not set in the configuration file. Go and get a key!");
        if (string.IsNullOrEmpty(config.ANTHROPIC_API_KEY) && config.CHAT_PROVIDER == "anthropic")
            throw new Exception("Your CHAT_PROVIDER is configured to 'anthropic', but ANTHROPIC_API_KEY is not set in the configuration file. Go and get a key!");
        if (string.IsNullOrEmpty(config.PROMPT_STEPS))
            throw new Exception("PROMPT_STEPS is not set in the configuration file. Example: 'PROMPT_STEPS=clean_markdown.md,extract_wisdom.md'");
        if (string.IsNullOrEmpty(config.AI_MAX_TOKENS))
            throw new Exception("AI_MAX_TOKENS is not properly set in the configuration file. Example: 'AI_MAX_TOKENS=4096'");
        if (string.IsNullOrEmpty(config.AI_TEMPERATURE))
            throw new Exception("AI_TEMPERATURE is not properly set in the configuration file. Example: 'AI_TEMPERATURE=0.1'");

        if (config.PROMPT_STEPS_PARSED == null || config.PROMPT_STEPS_PARSED.Length == 0)
            throw new Exception("PROMPT_STEPS is not set in the configuration file. Example: 'PROMPT_STEPS=clean_markdown.md,extract_wisdom.md'");
        if (config.PROMPT_STEPS_PARSED.Length < 1)
            throw new Exception("PROMPT_STEPS must have at least one step! Example: 'PROMPT_STEPS=clean_markdown.md,extract_wisdom.md'");

        if (config.AI_MAX_TOKENS_PARSED < 1000)
            throw new Exception("AI_MAX_TOKENS misconfigured. It must be numeric and anything below 1000 is not accepted.");
        if (config.AI_TEMPERATURE_PARSED > 1)
            throw new Exception("AI_TEMPERATURE misconfigured. It cannot be higher than 1.0. Optimal is between 0.0 and 0.4");
    }
}
