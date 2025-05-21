using System;
using CameraSystem.Managers;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CameraSystem.Commands;
internal class Toggle : ICommand
{
    public string Command => "toggle";
    public string Description => Plugin.Instance.Translation.ToggleCommandDescription;
    public string[] Aliases { get; } = new[] { "t" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"camsys.{Command}"))
        {
            response = string.Format(Plugin.Instance.Translation.NoPermission, $"camsys.{Command}");
            return false;
        }

        if (CameraManager.Instance.IsCameraSystemEnabled)
        {
            CameraManager.Instance.Disable();
            response = Plugin.Instance.Translation.ToggleDisabled;
        }
        else
        {
            CameraManager.Instance.Enable();
            response = Plugin.Instance.Translation.ToggleEnabled;
        }

        return true;
    }
}