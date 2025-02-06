namespace ChatApp.Common;

public static class Disposable {
    public static IDisposable Empty() => EmptyDisposable.Instance;

    public static IDisposable From(Action action) => new FunctionRefDisposable(action);

    private sealed class FunctionRefDisposable : IDisposable {
        private readonly Action _action;

        public FunctionRefDisposable(Action action) {
            _action = action;
        }

        public void Dispose() => _action.Invoke();
    }

    private sealed class EmptyDisposable : IDisposable {
        public static readonly IDisposable Instance = new EmptyDisposable();

        public void Dispose() { }
    }
}
