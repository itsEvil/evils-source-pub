using Shared.Redis.Models;
using StackExchange.Redis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

//Copied from TK / NR.CORE

namespace Shared.Redis;
public abstract class RedisObject
{
    public RedisObject(IDatabase db, string key, string? field = null, bool isAsync = false) {
        Key = key;
        Database = db;

        _isAsync = isAsync;
        _key = key;

        if (isAsync)
            return;

        if (field == null)
            _entries = db.HashGetAll(key).ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        else
        {
            var entry = new HashEntry[] { new HashEntry(field, db.HashGet(key, field)) };

            _entries = entry.ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        }
    }

    private Dictionary<RedisValue, KeyValuePair<byte[], bool>> _entries = [];
    private bool _isAsync;
    private string _key = "";
    private List<HashEntry> _update = [];

    public IEnumerable<RedisValue> AllKeys => _entries.Keys;

    public IDatabase Database { get; private set; }

    public bool IsNull => _entries.Count == 0;

    public string Key { get; private set; } = "";

    public Task FlushAsync(ITransaction transaction = null)
    {
        ReadyFlush();
        return transaction == null ? Database.HashSetAsync(Key, _update.ToArray()) : transaction.HashSetAsync(Key, _update.ToArray());
    }

    public void Reload(string field = null)
    {
        if (field != null && _entries != null)
        {
            _entries[field] = new KeyValuePair<byte[], bool>(Database.HashGet(Key, field), false);

            return;
        }

        _entries = Database.HashGetAll(Key).ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
    }

    public async Task ReloadAsync(ITransaction trans = null, string field = null)
    {
        if (field != null && _entries != null)
        {
            var tf = trans != null ? trans.HashGetAsync(Key, field) : Database.HashGetAsync(Key, field);

            try
            {
                await tf;

                _entries[field] = new KeyValuePair<byte[], bool>(tf.Result, false);
            }
            catch { }

            return;
        }

        var t = trans != null ? trans.HashGetAllAsync(Key) : Database.HashGetAllAsync(Key);

        try
        {
            await t;

            _entries = t.Result.ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        }
        catch { }
    }

    protected async void GetAllEntriesAsync()
    {
        var result = await Database.HashGetAllAsync(_key);

        _entries = result.ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
    }

    protected T GetValue<T>(RedisValue key, T def = default)
    {
        if (key.IsNullOrEmpty || !_entries.TryGetValue(key, out KeyValuePair<byte[], bool> val) || val.Key == null)
            return def;

        if (typeof(T) == typeof(int))
            try { return (T)(object)int.Parse(Encoding.UTF8.GetString(val.Key)); }
            catch (OverflowException) { return (T)(object)int.MaxValue; }

        if (typeof(T) == typeof(uint))
            try { return (T)(object)uint.Parse(Encoding.UTF8.GetString(val.Key)); }
            catch (OverflowException) { return (T)(object)uint.MaxValue; }

        if (typeof(T) == typeof(ushort))
            try { return (T)(object)ushort.Parse(Encoding.UTF8.GetString(val.Key)); }
            catch (OverflowException) { return (T)(object)ushort.MaxValue; }

        if (typeof(T) == typeof(float))
            try { return (T)(object)float.Parse(Encoding.UTF8.GetString(val.Key)); }
            catch (OverflowException) { return (T)(object)float.MaxValue; }

        if (typeof(T) == typeof(bool))
            return (T)(object)(val.Key[0] != 0);

        if (typeof(T) == typeof(DateTime))
            return (T)(object)DateTime.FromBinary(BitConverter.ToInt64(val.Key, 0));

        if (typeof(T) == typeof(byte[]))
            return (T)(object)val.Key;

        if (typeof(T) == typeof(ushort[]))
        {
            var ret = new ushort[val.Key.Length / 2];

            Buffer.BlockCopy(val.Key, 0, ret, 0, val.Key.Length);

            return (T)(object)ret;
        }

        if (typeof(T) == typeof(int[]) || typeof(T) == typeof(uint[]))
        {
            var ret = new int[val.Key.Length / 4];

            Buffer.BlockCopy(val.Key, 0, ret, 0, val.Key.Length);

            return (T)(object)ret;
        }

        if (typeof(T) == typeof(string))
            return (T)(object)Encoding.UTF8.GetString(val.Key);

        if (typeof(T) == typeof(ItemData[]))
        {
            var reader = new Reader();
            var buff = val.Key.AsSpan();
            _ = reader.Byte(buff); //version
            //if(version == 0)
            return (T)(object)ReadItemDataVersionZero(reader, buff);
        }

        if (typeof(T) == typeof(string[]))
            return (T)(object)JsonSerializer.Serialize(val.Key, SourceGenerationContext.Default.StringArray);

        throw new NotSupportedException();
    }
    private static ItemData[] ReadItemDataVersionZero(Reader reader, Span<byte> buff) {
        var length = reader.UShort(buff);
        ItemData[] arr = new ItemData[length];
        for (int i = 0; i < arr.Length; i++) {
            arr[i] = new ItemData(
                reader.UInt(buff),
                reader.UInt(buff),
                reader.UInt(buff)
            );
        }
        return arr;
    }

    protected byte[] GetValueRaw(RedisValue key)
    {
        if (!_entries.TryGetValue(key, out KeyValuePair<byte[], bool> val))
            return null;

        if (val.Key == null)
            return null;

        return (byte[])val.Key.Clone();
    }

    protected void Init(IDatabase db, string key, string field = null, bool isAsync = false)
    {
        Key = key;
        Database = db;

        _isAsync = isAsync;
        _key = key;

        if (isAsync)
            return;

        if (field == null)
            _entries = db.HashGetAll(key).ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        else
        {
            var entry = new HashEntry[] { new HashEntry(field, db.HashGet(key, field)) };

            _entries = entry.ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        }
    }

    protected void InitTwo(IDatabase db, string key, string[] field = null, bool isAsync = false)
    {
        Key = key;
        Database = db;

        _isAsync = isAsync;
        _key = key;

        if (isAsync)
            return;

        if (field == null)
            _entries = db.HashGetAll(key).ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        else
        {
            var entry = new HashEntry[]
            {
                new HashEntry(field[0], db.HashGet(key, field[0])),
                new HashEntry(field[1], db.HashGet(key, field[1]))
            };

            _entries = entry.ToDictionary(x => x.Name, x => new KeyValuePair<byte[], bool>(x.Value, false));
        }
    }
    private static readonly int ItemDataSize = Marshal.SizeOf<ItemData>();
    protected void SetValue<T>(RedisValue key, T val)
    {
        if (val == null)
            return;

        byte[] buff;

        if (typeof(T) == typeof(int) || typeof(T) == typeof(uint) || typeof(T) == typeof(ushort) || typeof(T) == typeof(string) || typeof(T) == typeof(float))
            buff = Encoding.UTF8.GetBytes(val.ToString());
        else if (typeof(T) == typeof(bool))
            buff = new byte[] { (byte)((bool)(object)val ? 1 : 0) };
        else if (typeof(T) == typeof(DateTime))
            buff = BitConverter.GetBytes(((DateTime)(object)val).ToBinary());
        else if (typeof(T) == typeof(byte[]))
            buff = (byte[])(object)val;
        else if (typeof(T) == typeof(ushort[]))
        {
            var v = (ushort[])(object)val;

            buff = new byte[v.Length * 2];

            Buffer.BlockCopy(v, 0, buff, 0, buff.Length);
        }
        else if (typeof(T) == typeof(int[]) || typeof(T) == typeof(uint[]))
        {
            var v = (int[])(object)val;

            buff = new byte[v.Length * 4];

            Buffer.BlockCopy(v, 0, buff, 0, buff.Length);
        }
        else if(typeof(T) == typeof(string[]))
        {
            var arr = (string[])(object)val;
            buff = Encoding.UTF8.GetBytes(arr.ToJson());
        }
        else if (typeof(T) == typeof(ItemData[]))
        {
            var arr = (ItemData[])(object)val;
            var size = ItemDataSize * arr.Length;

            buff = new byte[size + 3];
            var writer = new Writer();
            writer.Write(buff, (byte)0); //item data version
            writer.Write(buff, (ushort)arr.Length);
            for (int i = 0; i < arr.Length; i++)
                arr[i].Write(writer, buff);
        }
        else
            throw new NotSupportedException();

        if (!_entries.ContainsKey(Key) || _entries[Key].Key == null || !buff.SequenceEqual(_entries[Key].Key))
            _entries[key] = new KeyValuePair<byte[], bool>(buff, true);
    }

    private void ReadyFlush()
    {
        if (_update == null)
            _update = new List<HashEntry>();

        _update.Clear();

        foreach (var name in _entries.Keys.ToList())
            if (_entries[name].Value)
                _update.Add(new HashEntry(name, _entries[name].Key));

        foreach (var update in _update)
            _entries[update.Name] = new KeyValuePair<byte[], bool>(_entries[update.Name].Key, false);
    }
}
