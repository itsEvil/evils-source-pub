namespace Shared.Interfaces;
public interface ISystem
{
    /// <summary>
    /// Cooldown before <see cref="ITask.Execute()"/> gets called in milliseconds.
    /// </summary>
    public int Cooldown { get; init; } //0 == runs each frame
    /// <summary>
    /// Amount of times <see cref="ITask.Execute()"/> gets called.
    /// </summary>
    public int Retries { get; set; } //-1 == retries forever
    /// <summary>
    /// Backend variable, ignore.
    /// </summary>
    public long LastRunTime { get; set; }
    /// <summary>
    /// Method that gets called after cooldown goes out
    /// </summary>
    public void Execute();
}
