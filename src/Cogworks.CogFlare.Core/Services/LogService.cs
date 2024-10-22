namespace Cogworks.CogFlare.Core.Services;

public interface ILogService<T>
{
    void Log(string message);
}

public class LogService<T> : ILogService<T>
{
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<T> _logger;

    public LogService(CogFlareSettings cogFlareSettings, ILogger<T> logger)
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