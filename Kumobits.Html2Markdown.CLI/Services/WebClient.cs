using Microsoft.Extensions.Logging;

namespace Kumobits.Html2Markdown.CLI.Services;
public class WebClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebClient> _logger;

    public WebClient(HttpClient httpClient, ILogger<WebClient> logger)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient = httpClient;
        _logger = logger;
    }

    public string? FetchHtmlFromUrl(string url)
    {
        try
        {
            var response = _httpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().Result;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching HTML from {url}, Message: {ex.Message}");
            return null;
        }
    }
}
