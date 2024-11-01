using GameServer.Core;
using Shared;
using Shared.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Game.Worlds;
public sealed class ArenaWorld(uint worldId, WorldDesc worldDesc, bool isEmpty = false) : World(worldId, worldDesc, isEmpty)
{
    private List<Vector2UInt> m_WeakSpawnPoints = [];
    private List<Vector2UInt> m_NormalSpawnPoints = [];
    private List<Vector2UInt> m_HardSpawnPoints = [];

    public override void Init(Map map)
    {
        base.Init(map);

        m_WeakSpawnPoints = GetRegionPositions(Region.WeakMonsterSpawn);
        m_NormalSpawnPoints = GetRegionPositions(Region.NormalMonsterSpawn);
        m_HardSpawnPoints = GetRegionPositions(Region.HardMonsterSpawn);
    }

    //0 == weak,
    //1 == normal,
    //2 == hard
    public Vector2UInt GetLocation(byte type = 0) {
        return type switch {
            1 => m_HardSpawnPoints[Random.Shared.Next(0, m_HardSpawnPoints.Count)],
            2 => m_NormalSpawnPoints[Random.Shared.Next(0, m_NormalSpawnPoints.Count)],
            _ => m_WeakSpawnPoints[Random.Shared.Next(0, m_WeakSpawnPoints.Count)]
        };
    }
}
