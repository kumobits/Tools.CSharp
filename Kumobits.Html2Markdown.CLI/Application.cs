using Kumobits.Html2Markdown.CLI.Services;
using Microsoft.Extensions.Logging;

namespace Kumobits.Html2Markdown.CLI;
public class Application
{
    private readonly WebClient _webFetcher;
    private readonly IHtmlToMarkdownConverter _converter;
    private readonly IChatService _chatProvider;
    private readonly ILogger<Application> _logger;

    public Application(WebClient webFetcher, IHtmlToMarkdownConverter converter, IChatService chatProvider, ILogger<Application> logger)
    {
        this._webFetcher = webFetcher;
        this._converter = converter;
        this._chatProvider = chatProvider;
        this._logger = logger;
    }

    public async Task Execute(string filePrefix, string[] urls, string[] promptSteps)
    {
        GuardPromptStepsAreValid(promptSteps);

        // 1. Get markdown from URL
        // 2. Clean that markdown using 

        var i = 0;

        // For each URL, do this
        foreach (var url in urls)
        {
            i++;
            _logger.LogInformation($"Starting: {url}");
            var html = _webFetcher.FetchHtmlFromUrl(url);
            if (string.IsNullOrEmpty(html))
                continue;

            var lastOutput = _converter.Convert(html);

            // Process the steps. Each step sends two things into the AiAssistant: the output from the previous step, and the prompt of the current step
            foreach (var promptStepFile in promptSteps)
            {
                _logger.LogInformation($"Executing step: {promptStepFile}");
                var promptStepContent = ReadPromptStepContent(promptStepFile);
                lastOutput = await _chatProvider.Answer(promptStepContent, lastOutput);
            }
            // Save last markdown output
            _logger.LogInformation($"Finished: {url}");
            SaveResult(i, filePrefix, url, lastOutput);
        }
    }

    private void SaveResult(int i, string filePrefix, string url, string lastOutput)
    {
        var fileName = $"{filePrefix}_{i}";
        File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Output", $"{url}.md"), lastOutput);
    }

    private void GuardPromptStepsAreValid(string[] promptSteps)
    {
        foreach (var promptStep in promptSteps)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "PromptSteps", $"{promptStep}");
            // If file doesnt exist, throw
            if (!File.Exists(fullPath))
                throw new ArgumentException($"Prompt step {promptStep} does not exist! Path: {fullPath}");

            var promptStepContent = ReadPromptStepContent(promptStep);
            // Ensure we have "{{INPUT}}" in the prompt step
            if (!promptStepContent.Contains("{{INPUT}}"))
                throw new ArgumentException($"Prompt step {promptStep} does not contain '{{INPUT}}' in the file!");
        }
    }

    private string ReadPromptStepContent(string promptStep)
    {
        return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "PromptSteps", $"{promptStep}"));
    }
}
