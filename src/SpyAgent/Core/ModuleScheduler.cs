namespace SpyAgent.Core;

internal sealed class ModuleScheduler
{
    private readonly DataAggregator _aggregator;
    private readonly List<ScheduledModule> _modules = [];
    private readonly List<Task> _runningTasks = [];

    public ModuleScheduler(DataAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    public void Register(ISpyModule module, int intervalSeconds)
    {
        _modules.Add(new ScheduledModule
        {
            Module = module,
            Interval = TimeSpan.FromSeconds(intervalSeconds)
        });
    }

    public void StartAll(CancellationToken ct)
    {
        foreach (var scheduled in _modules)
        {
            var task = RunModuleLoopAsync(scheduled, ct);
            _runningTasks.Add(task);
        }
    }

    private async Task RunModuleLoopAsync(ScheduledModule scheduled, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var data = await scheduled.Module.CollectAsync();
                if (data != null)
                    _aggregator.Add(scheduled.Module.ModuleName, data);
            }
            catch (Exception ex)
            {
                _aggregator.AddError(scheduled.Module.ModuleName, ex.Message);
            }

            try
            {
                await Task.Delay(scheduled.Interval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public int ModuleCount => _modules.Count;
}

internal sealed class ScheduledModule
{
    public required ISpyModule Module { get; init; }
    public required TimeSpan Interval { get; init; }
}

internal interface ISpyModule
{
    string ModuleName { get; }
    Task<byte[]?> CollectAsync();
}
