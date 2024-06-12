namespace Kumobits.Html2Markdown.CLI.Core;

public class AppConfig
{
    private static string _settingsTextContent = string.Empty;

    public AppConfig()
    {
        // Read from the settings.txt file, this file has the "KEY=VALUE" format
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "settings.txt");
        GuardFileExists(fullPath);
        _settingsTextContent = File.ReadAllText(fullPath).TrimStart().TrimEnd();

        OPENAI_API_KEY = GetSettingValue<string>(nameof(OPENAI_API_KEY))!;
        ANTHROPIC_API_KEY = GetSettingValue<string>(nameof(ANTHROPIC_API_KEY))!;
        HTML2MARKDOWN_STRATEGY = GetSettingValue<string>(nameof(HTML2MARKDOWN_STRATEGY))!; // ReverseMarkdown | Html2Markdown
        CHAT_PROVIDER = GetSettingValue<string>(nameof(CHAT_PROVIDER))!; // anthropic | openai
        AI_MAX_TOKENS = GetSettingValue<string>(nameof(AI_MAX_TOKENS))!; // step1,step2,step3
        AI_TEMPERATURE = GetSettingValue<string>(nameof(AI_TEMPERATURE))!; // step1,step2,step3
        PROMPT_STEPS = GetSettingValue<string>(nameof(PROMPT_STEPS))!; // step1,step2,step3
    }

    // Properties

    public string OPENAI_API_KEY { get; }
    public string ANTHROPIC_API_KEY { get; }
    public string HTML2MARKDOWN_STRATEGY { get; }
    public string CHAT_PROVIDER { get; }
    public string AI_MAX_TOKENS { get; }
    public string AI_TEMPERATURE { get; }
    public string PROMPT_STEPS { get; }
    public string CHAT_SYSTEM_INSTRUCTION { get; set; }

    // Parsed

    public string[] PROMPT_STEPS_PARSED => PROMPT_STEPS.Split(",");
    public int AI_MAX_TOKENS_PARSED => Convert.ToInt32(AI_MAX_TOKENS);
    public decimal AI_TEMPERATURE_PARSED => Convert.ToDecimal(AI_TEMPERATURE);

    // Helpers

    private T? GetSettingValue<T>(string key)
    {
        return GetSettingValueFromTextFile<T>(key) ?? GetEnvVariableValue<T>(key);
    }

    /// <summary>
    /// Reads from the settings.txt file and returns the value of the setting
    /// </summary>
    private T GetSettingValueFromTextFile<T>(string settingName)
    {
        var settingLine = _settingsTextContent
            .Split(Environment.NewLine)
            .FirstOrDefault(x => x.StartsWith(settingName));

        if (settingLine == null)
        {
            return default;
        }

        var settingValue = settingLine.Split("=")[1].Trim();

        // We support having comments in the text file. The comments are prefixed with "//". We need to remove the // and everything that comes after it.
        var settingValueWithoutComments = settingValue.Split("//")[0].TrimEnd();

#pragma warning disable CS8603 // Possible null reference return.

        if (string.IsNullOrEmpty(settingValueWithoutComments))
            return default;

        try
                {
            return (T) Convert.ChangeType(settingValueWithoutComments, typeof(T));
        }
        catch
        {

            return default;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public static T? GetEnvVariableValue<T>(string settingName)
    {
        // Read Env variable
        var settingValue = Environment.GetEnvironmentVariable(settingName);

        if (string.IsNullOrEmpty(settingValue))
            return default;

        try
        {
            return (T) Convert.ChangeType(settingValue, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    private void GuardFileExists(string fullFilePath)
    {
        if (!File.Exists(fullFilePath))
            throw new FileNotFoundException($"The file {fullFilePath} was not found.");
    }
}
