using common;
using game_server.Core.Entities;

namespace game_server.Core.Chat;
//Make sure to add new commands in CommandManager.cs
//Format for command arguments
//
//Arguments with these '<>' mean that they are required
//Arguments with these '[]' mean that they are optional
public sealed class HelpCommand() : Command("help", Ranks.None, 0, true, "/help [command]", "h") {
    public override bool Execute(Player player, string[] arguments) {
        SLog.Debug("HelpCommand::Execute::{0}::{1}", player.Name, string.Join(',', arguments));

        return true;
    }
}