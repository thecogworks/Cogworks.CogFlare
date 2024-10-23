namespace Cogworks.CogFlare.Core.Services;

public interface ILogService
{
    void Log(string message);
}

public class LogService : ILogService
{
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<LogService> _logger;

    public LogService(CogFlareSettings cogFlareSettings, ILogger<LogService> logger)
    {
        _cogFlareSettings = cogFlareSettings;
        _logger = logger;
    }

    public void Log(string message)
    {
        if (_cogFlareSettings.EnableLogging)
        {
            _logger.LogInformation(message);
        }
    }
}