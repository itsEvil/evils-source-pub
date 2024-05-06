using System.Numerics;

namespace game_server.Core.Entities;
public class Projectile(int id, int projectileType, int damage, Entity? owner) {
    public readonly int Id = id;
    public readonly int ObjectType = projectileType;
    public readonly Entity? Owner = owner;
    public readonly int Damage = damage;
}