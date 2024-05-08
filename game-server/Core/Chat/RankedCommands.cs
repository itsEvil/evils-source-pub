using common;
using game_server.Core.Entities;

namespace game_server.Core.Chat;
//Make sure to add new commands in CommandManager.cs
//Format for command arguments
//
//Arguments with these '<>' mean that they are required
//Arguments with these '[]' mean that they are optional
public sealed class BanCommand() : Command("ban", Ranks.Admin, 1, true, "/ban <player_name> [time in minutes]", "b") {
    public override bool Execute(Player player, string[] arguments) {
        SLog.Debug("BanCommand::Execute::{0}::{1}", player.Name, string.Join(',', arguments));

        if (arguments.Length > 2)
            return false;

        return true;
    }
}
public sealed class KickCommand() : Command("kick", Ranks.Staff, 1, true, "/kick <player_name>", "k") {
    public override bool Execute(Player player, string[] arguments) {
        SLog.Debug("KickCommand::Execute::{0}::{1}", player.Name, string.Join(',', arguments));
        if (arguments.Length > 1)
            return false;

        return true;
    }
}