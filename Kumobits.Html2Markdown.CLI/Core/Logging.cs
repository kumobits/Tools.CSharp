using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace Kumobits.Html2Markdown.CLI.Core;
public class Logging
{
    public static Logger ConfigureSerilogForApps(ServiceCollection services)
    {
        var serilogConfig = CreateSerilogConfig();
        var seriLogger = serilogConfig.CreateLogger();
        services.AddLogging(x => x.AddSerilog(seriLogger));
        seriLogger.Information("== BOOTING APPLICATION ==");
        return seriLogger;
    }

    /// <summary>
    /// See https://github.com/serilog/serilog-expressions
    /// </summary>
    private static LoggerConfiguration CreateSerilogConfig()
    {
        // -- Serilog templates:
        const string coreTemplate1 = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        // with: .WriteTo.Console(theme: AnsiConsoleTheme.Code)

        // -- Serilog.Expressions templates:

        // Timestamp, log level, trace id, span id, message,category context, exception
        const string expressionTemplate1 = "[{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m} {#if SourceContext is not null} ({SourceContext}){#end}\n{@x}";
        //const string expressionTemplate2 = "{Coalesce(Coalesce(Name, Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)), 'Program')}: [{@t:HH:mm:ss} {@l:u3}{#if @tr is not null} ({substring(@tr,0,4)}:{substring(@sp,0,4)}){#end}] {@m}\n{@x}";
        var loggerConf = new LoggerConfiguration();

        loggerConf.WriteTo.Console(new ExpressionTemplate(
            expressionTemplate1,
            theme: TemplateTheme.Code
            // encoder: Json // Ansi // etc..
            ), standardErrorFromLevel: LogEventLevel.Error)

            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Reduce ASP.Net built-in HttpRequest noise
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Information)
            .MinimumLevel.Information();

        return loggerConf;
    }
}
