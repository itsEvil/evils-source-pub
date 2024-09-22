namespace Shared;

public sealed class DeferAction(Action action) : IDisposable {
    private readonly Action _action = action;
    public bool Cancel = false;
    public void Dispose() {
        if(Cancel) 
            return;

        _action?.Invoke();
    }
}
