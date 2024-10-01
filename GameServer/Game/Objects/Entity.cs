using GameServer.Game.Worlds;
using Shared;
using Shared.GameData;
using System.Numerics;

namespace GameServer.Game.Objects;
public class Entity {
    public const uint EFFECT_COUNT = 7;
    private const float _MOVE_THRESHOLD = 0.4f;
    private const float _MIN_MOVE_SPEED = 0.004f;
    private const float _MAX_MOVE_SPEED = 0.0096f;
    private const float _MIN_ATTACK_FREQ = 0.0015f;
    private const float _MAX_ATTACK_FREQ = 0.008f;
    private const float _MIN_ATTACK_MULT = 0.5f;
    private const float _MAX_ATTACK_MULT = 2f;
    private const float _MAX_SINK_LEVEL = 18f;


    public readonly uint UniqueId;
    public readonly uint ObjectId;

    public bool IsDead = false;
    public bool IsInvulnerable = false;
    public uint MaxHp;
    public uint Hp;

    public World World;
    public Vector2 Position;

    private readonly string PrivName = "";

    public int OwnerId = -1;

    protected readonly Dictionary<StatType, object> Stats = []; 

    private readonly Dictionary<ConditionEffect, KeyValuePair<ushort, ushort>> Effects = [];
    //KVP Format
    //Key = duration
    //Value = stack count

    public Entity(uint uniqueId, uint objectId) {
        UniqueId = uniqueId;
        ObjectId = objectId;

        PrivName = string.Format("{0}-{1}", UniqueId, ObjectId);

        InitEffects();
    }
    private void PreExport() {
        Stats[StatType.Health] = Hp;
        Stats[StatType.MaxHealth] = MaxHp;

        Export(Stats);
    }

    public Task Tick() {
        //Tick behaviours
        //Tick inherited
        PreExport();
        Update();


        return Task.CompletedTask;
    }
    protected virtual void Update() { } 
    protected virtual void Export(Dictionary<StatType, object> stats) { }
    public void Enter(World world) {
        World = world;
        OnEnterWorld(world);
    }
    protected virtual void OnEnterWorld(World world) { }
    private void InitEffects()
    {
        var span = Game.Effects.Array.AsSpan();
        for (int i = 0; i < span.Length; i++)
            Effects[span[i]] = new KeyValuePair<ushort, ushort>(0, 0);
    }
    public bool TryGetEffect(ConditionEffect effect, out KeyValuePair<ushort, ushort> kvp) => Effects.TryGetValue(effect, out kvp);
    public float GetMovementSpeed(float speed = 50f) {
        if (TryGetEffect(ConditionEffect.Paralyzed, out var paralyzed) && paralyzed.Key > 0)
        {
            var stat = (Math.Max(0, speed - Game.Effects.GetBaseValue(ConditionEffect.Paralyzed)));
#if DEBUG
            SLog.Debug("{0} is paralyzed, new speed {1}", args: [PrivName, stat]);
#endif
            return _MIN_MOVE_SPEED + stat / 75.0f * (_MAX_MOVE_SPEED - _MIN_MOVE_SPEED);
        }

        if (TryGetEffect(ConditionEffect.Slowness, out var slowness) && slowness.Key > 0)
        {
            var slownessModifier = 1 - Game.Effects.GetBaseValue(ConditionEffect.Slowness) * slowness.Value; //.15f * stacks
            var stat = Math.Max(0, speed * slownessModifier);
#if DEBUG
            SLog.Debug("{0} has slowness {1}, new speed is {2}!", args: [PrivName, slownessModifier, stat]);
#endif
            return _MIN_MOVE_SPEED + stat / 75.0f * (_MAX_MOVE_SPEED - _MIN_MOVE_SPEED);
        }

        if (TryGetEffect(ConditionEffect.Swiftness, out var swiftness) && swiftness.Key > 0)
        {
            var swiftnessModifier = 1 + Game.Effects.GetBaseValue(ConditionEffect.Swiftness) * swiftness.Value; //.15f * stacks
            var stat = Math.Max(0, speed * swiftnessModifier);
#if DEBUG
            SLog.Debug("{0} has swiftness by {1}, new speed is {2}!", args: [PrivName, swiftnessModifier, stat]);
#endif
            return _MIN_MOVE_SPEED + stat / 75.0f * (_MAX_MOVE_SPEED - _MIN_MOVE_SPEED);
        }

        return _MIN_MOVE_SPEED + speed / 75.0f * (_MAX_MOVE_SPEED - _MIN_MOVE_SPEED);
    }
}
