using common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Networking;
public class SyncedVariable<T>(StatType key, T startingValue) {
    public static readonly SyncedVariable<int> EmptyInt = new(StatType.Nothing, 0);
    public static readonly SyncedVariable<string> EmptyStr = new(StatType.Nothing, "");
    public bool IsEmpty => Key == StatType.Nothing;
    public readonly StatType Key = key;
    public T Value = startingValue;
    /// <summary>
    /// Read-Only operation, use any index if the type T is not an array.
    /// </summary>
    public T this[int index] {
        get {
            SLog.Debug("IsArray::{0}", GetType().IsArray);
            if (GetType().IsArray) {
                return this[index];
            }
            else return this.Value;
        }
    }
    public override string ToString() {
        return Value?.ToString() ?? "Unknown-Value";
    }
}

