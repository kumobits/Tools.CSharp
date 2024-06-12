using Microsoft.Extensions.Logging;
using H = Html2Markdown;
using R = ReverseMarkdown;

namespace Kumobits.Html2Markdown.CLI.Services;
public interface IHtmlToMarkdownConverter
{
    public string Convert(string html);
}

public class ReverseMarkdownConverter : IHtmlToMarkdownConverter
{
    private readonly R.Converter _converter;
    private readonly ILogger<ReverseMarkdownConverter> _logger;

    public ReverseMarkdownConverter(ILogger<ReverseMarkdownConverter> logger)
    {
        _converter = new R.Converter();
        _logger = logger;
    }

    public string Convert(string html)
    {
        var result = _converter.Convert(html);
        return result;
    }
}

public class Html2MarkdownConverter : IHtmlToMarkdownConverter
{
    private readonly H.Converter _converter;
    private readonly ILogger<ReverseMarkdownConverter> _logger;

    public Html2MarkdownConverter(ILogger<ReverseMarkdownConverter> logger)
    {
        _converter = new H.Converter();
        _logger = logger;
    }

    public string Convert(string html)
    {
        var result = _converter.Convert(html);
        return result;
    }
}

