using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;

namespace CameraSystem.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal sealed class CameraSystemParentCommand : ParentCommand
{
    public CameraSystemParentCommand() => LoadGeneratedCommands();

    public override string Command => "camerasystem";
    public override string Description => CameraSystem.Instance.Config?.Translations.CameraSystemCommandDescription;
    public override string[] Aliases { get; } = new[] { "cs", "camera" };

    internal const string PermissionPrefix = "camsys.";

    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new ToggleWorkstationUsageCommand());
        RegisterCommand(new DisconnectCommand());
        RegisterCommand(new ToggleCommand());
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = CameraSystem.Instance.Config?.Translations.ParentCommandHeader;

        foreach (var command in AllCommands.Where(c => sender.HasPermissions(PermissionPrefix + c.Command)))
        {
            response += string.Format(
                CameraSystem.Instance.Config?.Translations.ParentCommandFormat ?? "",
                command.Command,
                string.Join(", ", command.Aliases),
                command.Description
            );
        }

        return false;
    }
}