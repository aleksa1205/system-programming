namespace NYTimes_HTTPServer_Reactive.ReactiveModels;

public static class ObserverUtility<T>
{
    public static void NotifyOnNext(List<IObserver<T>> observers, T value)
    {
        foreach (var observer in observers)
            observer.OnNext(value);
    }

    public static void NotifyOnCompleted(List<IObserver<T>> observers)
    {
        foreach (var observer in observers)
            observer.OnCompleted();
    }

    public static void NotifyOnError(List<IObserver<T>> observers, Exception ex)
    {
        foreach (var observer in observers)
            observer.OnError(ex);
    }

    public static void NotifyOnNextOnCompleted(List<IObserver<T>> observers, T value)
    {
        foreach (var observer in observers)
        {
            observer.OnNext(value);
            observer.OnCompleted();
        }
    }
}