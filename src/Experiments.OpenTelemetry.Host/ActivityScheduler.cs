using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Experiments.OpenTelemetry.Host;

internal sealed class ActivityScheduler : IObservable<ActivityDescriptor>, IDisposable
{
    private static ActivityScheduler? _instance;

    private bool _disposed;
    private readonly Subject<ActivityDescriptor> _activitySubject;
    private readonly BlockingCollection<ActivityDescriptor> _activityQueue;

    private ActivityScheduler(int queueLimit = 100, CancellationToken cancellationToken = default)
    {
        _activityQueue = new(queueLimit);
        _activitySubject = new Subject<ActivityDescriptor>();

        Task.Factory.StartNew(
            () =>
            {
                foreach (var descriptor in _activityQueue)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _activitySubject.OnNext(descriptor);
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        )
        .ContinueWith(t =>
        {
            Console.WriteLine(t.Exception!.ToString());
            _activitySubject.OnError(t.Exception!);
        },
        default, TaskContinuationOptions.NotOnFaulted, TaskScheduler.Default);
    }

    public static ActivityScheduler Instance => _instance ?? throw new InvalidOperationException();

    public static void Init(int queueLimit = 100, CancellationToken cancellationToken = default) => _instance = new(queueLimit, cancellationToken);

    public void Dispose()
    {
        if (_disposed) { return; }

        _activitySubject.Dispose();
        _activityQueue.Dispose();

        _disposed = true;
    }

    public void QueueActivity(ActivityDescriptor descriptor) => _activityQueue.Add(descriptor);

    public IDisposable Subscribe(IObserver<ActivityDescriptor> observer) => _activitySubject.Subscribe(observer);
}
