﻿namespace common;
public enum LoginStatus {
    Failed,
    Ok,
}
public enum CreateStatus {
    InvalidError,
    LimitReached,
    SkinUnavailable,
    Locked,
    Ok,
}
public enum RegisterStatus {
    InvalidError,
    NameTaken,
    EmailTaken,
    InvalidEmail,
    InvalidPassword,
    InvalidName,
    Ok,
}

public enum Ranks {
    None = 0,
    Donor_1 = 10,
    Staff = 50,
    Admin = 70,
    Owner = 100,
}
public enum WorldTypes { //Not linked to world Id's
    Generic,
    Nexus = -1,
    Vault = -2,
    Market = -3,
    Realm = -4,
}
public enum Currency {
    Fame,
    Gold,
    GuildFame,
}

public enum Region {
    None,
    Spawn,
    Regen,
    BlocksSight,
    Note,
    Enemy1,
    Enemy2,
    Enemy3,
    Enemy4,
    Enemy5,
    Enemy6,
    Decoration1,
    Decoration2,
    Decoration3,
    Decoration4,
    Decoration5,
    Decoration6,
    Trigger1,
    Callback1,
    Trigger2,
    Callback2,
    Trigger3,
    Callback3,
    VaultChest,
    GiftChest,
    Store1,
    Store2,
    Store3,
    Store4,
    VaultPortal,
    RealmPortal,
    GuildPortal,
}

public enum Terrain : byte {
    None = 0,
    Mountains,
    HighSand,
    HighPlains,
    HighForest,
    MidSand,
    MidPlains,
    MidForest,
    LowSand,
    LowPlains,
    LowForest,
    ShoreSand,
    ShorePlains,
    BeachTowels,
}

public enum ObjectClass {
    GameObject,
    Player,
    Equipment,
    Character,
    Projectile,
}

public enum ItemTypes {
    Any, //any item can go in here
    Staff,
    Spell,
    Robe,
    Ring,
}