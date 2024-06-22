namespace NYTimes_HTTPServer_Reactive.ReactiveModels;

public class Unsubscriber<T> : IDisposable
{
    private readonly IObserver<T> _observer;
    private readonly List<IObserver<T>> _observers;

    public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _observers.Remove(_observer);
    }
}