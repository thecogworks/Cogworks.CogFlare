namespace Cogworks.CogFlare.Core.Services;

public interface ICogFlareLogService
{
    void Log(string message);
}

public class CogFlareLogService : ICogFlareLogService
{
    private readonly CogFlareSettings _cogFlareSettings;
    private readonly ILogger<CogFlareLogService> _logger;

    public CogFlareLogService(CogFlareSettings cogFlareSettings, ILogger<CogFlareLogService> logger)
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