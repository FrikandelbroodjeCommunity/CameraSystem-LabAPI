using System;
using CameraSystem.Managers;
using CommandSystem;
using LabApi.Features.Permissions;

namespace CameraSystem.Commands;

internal class ToggleCommand : ICommand
{
    public string Command => "toggle";
    public string Description => CameraSystem.Instance.Config?.Translations.ToggleCommandDescription;
    public string[] Aliases { get; } = new[] { "t" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasPermissions(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Config?.Translations.NoPermission;
            return false;
        }

        if (CameraManager.Instance.IsCameraSystemEnabled)
        {
            CameraManager.Instance.Disable();
            response = CameraSystem.Instance.Config?.Translations.ToggleDisabled;
        }
        else
        {
            CameraManager.Instance.Enable();
            response = CameraSystem.Instance.Config?.Translations.ToggleEnabled;
        }

        return true;
    }
}