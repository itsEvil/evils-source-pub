using GameServer.Core.Options;

namespace GameServer.Core;
public sealed class Builder
{
    private AppOptions m_Options;
    public static Builder Create() => new();
    public void AddOptions<T>(Action<T> options = null) where T : AppOptions, new()
    {
        m_Options ??= new T();
        options?.Invoke((T)m_Options);
    }
    public Application Build() {
        var ret = Application.Get();
        m_Options ??= new AppOptions();
        ret.Awake(m_Options);
        return ret;
    }
}
