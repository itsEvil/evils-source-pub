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
public sealed class SubmergedWorld(uint worldId, WorldDesc worldDesc, bool isEmpty = false) : World(worldId, worldDesc, isEmpty)
{
    private readonly HashSet<Vector2UInt> m_Vents = [];

    private readonly WorldDesc.DrainInfo DrainDesc = worldDesc.Drain ?? throw new Exception($"WorldDesc.Drain is null for {worldDesc.Name}");

    private long m_NextDrainTime = 0;

    public override void Init(Map map)
    {
        base.Init(map);

        RequiresOxygen = DrainDesc.ResourceType == 0;

        var locations = GetRegionPositions(Region.AirVent);

        var span = CollectionsMarshal.AsSpan(locations);
        for (int i = 0; i < locations.Count; i++) {
            if (!m_Vents.Add(span[i])) {
                SLog.Warn("Already added vent at : {0}", args: [span[i]]);;
            }
        }
    }

    protected override void Update() {
        base.Update();

        if(Watch.ElapsedMilliseconds - m_NextDrainTime > 0) {
            m_NextDrainTime = Watch.ElapsedMilliseconds + DrainDesc.Frequency;

            if(DrainDesc.ResourceType == 0) {
                foreach(var (_, player) in Players) {
                    var position = new Vector2UInt(player.Position);
                    if (m_Vents.Contains(position)) {
                        SLog.Debug("Player {0} is on a vent at {1}", player.Name, position.ToString());
                        continue;
                    }

                    player.DrainHealth(DrainDesc.Amount);
                }
            } 
            else
            {
                foreach (var (_, player) in Players)
                {
                    var position = new Vector2UInt(player.Position);
                    if (m_Vents.Contains(position)) {
                        SLog.Debug("Player {0} is on a vent at {1}", player.Name, position.ToString());
                        continue;
                    }

                    player.DrainResource(DrainDesc.Amount);
                }
            }
        }
    }
}
