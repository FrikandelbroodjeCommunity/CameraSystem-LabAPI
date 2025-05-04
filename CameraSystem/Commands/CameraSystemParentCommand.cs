using System;
using System.Linq;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CameraSystem.Commands;
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal sealed class CameraSystemParentCommand : ParentCommand
{
    public CameraSystemParentCommand() => LoadGeneratedCommands();

    public override string Command => "camerasystem";
    public override string Description => Plugin.Instance.Translation.CameraSystemCommandDescription;
    public override string[] Aliases { get; } = new[] { "cs", "camera" };

    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new ToggleWorkstationUsage());
        RegisterCommand(new Disconnect());
        RegisterCommand(new Toggle());
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = Plugin.Instance.Translation.ParentCommandHeader;

        foreach (ICommand command in AllCommands.Where(c => sender.CheckPermission($"camsys.{c.Command}")))
        {
            response += string.Format(Plugin.Instance.Translation.ParentCommandFormat,
                command.Command,
                string.Join(", ", command.Aliases),
                command.Description);
        }

        return false;
    }
}