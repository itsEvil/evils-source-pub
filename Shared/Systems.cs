using Shared.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Shared;
public sealed class Systems : IDisposable
{
    private readonly ConcurrentDictionary<Type, List<ISystem>> m_Systems = [];
    private readonly Stopwatch Watch = Stopwatch.StartNew();
    public void Add<T>() where T : ISystem, new()
    {
        if (!m_Systems.TryGetValue(typeof(T), out var list))
        {
            list = [];
        }

        list.Add(new T());
        m_Systems[typeof(T)] = list;
    }
    public void Add(ISystem system)
    {
        var type = system.GetType();
        if (!m_Systems.TryGetValue(type, out List<ISystem> list))
        {
            list = [];
        }

        list.Add(system);
        m_Systems[type] = list;
    }
    public List<ISystem> Get(Type type)
    {
        if (m_Systems.TryGetValue(type, out var list))
            return list;

        return null;
    }
    public T Get<T>(Type type) where T : ISystem
    {
        List<ISystem> list = Get(type);
        if (list is null || list.Count == 0)
            return default;

        return (T)list.First();
    }
    public void Update()
    {
        //Loop all the systems
        //Check if their cooldowns are up and if so execute
        //Reduce retries on each execute if retries is not -1
        foreach (var (_, list) in m_Systems)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var system = list[i];
                if (Watch.ElapsedMilliseconds - system.LastRunTime > system.Cooldown)
                {
                    system.LastRunTime = Watch.ElapsedMilliseconds;

                    if (system.Retries != -1)
                    {
                        if (system.Retries > 0)
                        {
                            system.Retries--;
                            system.Execute();
                        }
                        else
                        {
                            list.RemoveAt(i);
                        }
                    }
                    else
                    {
                        system.Execute();
                    }
                }
            }
        }
    }
    public void Dispose()
    {
        foreach (var (_, list) in m_Systems)
            list.Clear();

        m_Systems.Clear();
    }
}
