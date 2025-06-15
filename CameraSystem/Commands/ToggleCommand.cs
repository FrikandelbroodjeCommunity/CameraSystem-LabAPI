using System;
using CameraSystem.Managers;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CameraSystem.Commands;
internal class ToggleCommand : ICommand
{
    public string Command => "toggle";
    public string Description => CameraSystem.Instance.Translation.ToggleCommandDescription;
    public string[] Aliases { get; } = new[] { "t" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Translation.NoPermission;
            return false;
        }

        if (CameraManager.Instance.IsCameraSystemEnabled)
        {
            CameraManager.Instance.Disable();
            response = CameraSystem.Instance.Translation.ToggleDisabled;
        }
        else
        {
            CameraManager.Instance.Enable();
            response = CameraSystem.Instance.Translation.ToggleEnabled;
        }

        return true;
    }
}
