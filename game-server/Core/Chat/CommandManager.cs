using common;
using game_server.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game_server.Core.Chat;
public static class CommandManager
{
    private readonly static Dictionary<string, ICommand> Commands = new() {
        { "help", new HelpCommand() },
        { "ban", new BanCommand() },
        { "kick", new KickCommand() },
    };
    public static void Execute(Player player, string rawText) {
        var words = rawText[1..].Split(' ');
        var inputWords = new string[words.Length - 1];
        for(int i = 0; i< words.Length; i++) {
            inputWords[i - 1] = words[i];
        }
        var inputCommand = words[0];

        if(!Commands.TryGetValue(inputCommand, out ICommand icommand)) {
            player.SendError("Unknown Command");
            return;
        }

        if(icommand is null || icommand is not Command command) {
            SLog.Error("Command::{0}::IsNotClassCommand", inputCommand);
            player.SendError("Unknown Command");
            return;
        }

        if ((int)player.Rank < (int)command.RankRequired) {
            player.SendError("Unknown Command");
            return;
        }

        if(inputWords.Length < command.RequiredArguments) {
            player.SendError($"Usage: {command.UseCase}");
            return;
        }

        try {
            if(!command.Execute(player, inputWords)) {
                player.SendError($"Usage: {command.UseCase}");
            }
        } catch(Exception e) {
            SLog.Error(e);
        }

        if(player.Client is not null && player.Client.Account is not null && player.Client.Manager.Transaction is not null)
            player.Client.Account.FlushAsync(player.Client.Manager.Transaction);
    }
}
public interface ICommand
{
    public bool Execute(Player player, string[] arguments); //Return true if successful else false and UseCase will be sent to player
}

/// <summary>
/// Base class for command system.
/// User should inherit this class and add the new class below in CommandManager.Commands dictionary.
/// </summary>
/// <param name="name">Text that player has to type to run the command.</param>
/// <param name="rankRequired">Rank the player needs to have access to the command.</param>
/// <param name="requiredArguments">Minimum amount of arguments that have to exist else useCase gets sent to player.</param>
/// <param name="listCommand">Allows to force hide the command if value is false regardless of rank.</param>
/// <param name="useCase">Message that gets printed to player if command fails.</param>
/// <param name="aliases">Shorthand to run the command.</param>
public abstract class Command(string name, Ranks rankRequired = Ranks.None, int requiredArguments = 0, bool listCommand = true, string useCase = "/<command_name>", params string[] aliases) : ICommand
{
    public readonly string Name = name;
    public readonly Ranks RankRequired = rankRequired;
    public readonly int RequiredArguments = requiredArguments;
    public readonly bool ListCommand = listCommand;
    public readonly string UseCase = useCase;
    public readonly string[] Aliases = aliases;
    public abstract bool Execute(Player player, string[] arguments);
}
