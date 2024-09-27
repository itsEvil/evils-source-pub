using Shared;
using GameServer.Net.Interfaces;
using System.Numerics;

namespace GameServer.Net.Packets;
public readonly struct Move : IReceive {
    private readonly Vector2[] History;
    private readonly Vector2 Position;
    public Move(Reader r, Span<byte> b) {
        var len = r.UShort(b);
        History = new Vector2[len];
        for(int i =0; i < len; i++)
            History[i] = new Vector2(r.Float(b), r.Float(b));

        Position = new Vector2(r.Float(b), r.Float(b));
    }
    public void Handle(Client client) { 

        var player = client.Player;
        if (!player.World.ValidatePosition(Position))
        {
            SLog.Debug("Position for player {0} at {1} is invalid.", args: [player.Name, Position]);
            return;
        }

        client.Player.Position = Position;
        client.Player.OnMove();
    }
}

public readonly struct MoveAck(Vector2 position) : ISend {
    public ushort Id => (ushort)S2C.MoveAck;
    public readonly Vector2 Position = position;
    public void Write(Writer w, Span<byte> b) {
        w.Write(b, Position.X);
        w.Write(b, Position.Y);
    }
}
