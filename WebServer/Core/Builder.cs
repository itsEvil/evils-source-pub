using WebServer.Core.Options;

namespace WebServer.Core;
public sealed class Builder
{
    private AppOptions m_Options;
    public static Builder Create() => new();
    public void AddOptions<T>(Action<T> options = null) where T : AppOptions, new()
    {
        m_Options ??= new T();
        options?.Invoke((T)m_Options);
    }
    public T Build<T>() where T : Application, new()
    {
        var ret = new T();
        m_Options ??= new AppOptions();
        ret.Awake(m_Options);
        return ret;
    }
}
