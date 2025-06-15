using System;
using System.Collections.Generic;
using CameraSystem.Managers;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Utils;

namespace CameraSystem.Commands;
internal class DisconnectCommand : ICommand, IUsageProvider
{
    public string Command => "disconnect";
    public string Description => CameraSystem.Instance.Translation.DisconnectCommandDescription;
    public string[] Aliases { get; } = new[] { "d", "dc" };

    public string[] Usage { get; } = new[] { "%player%" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Translation.NoPermission;
            return false;
        }

        if (arguments.Count == 0)
        {
            response = string.Format(CameraSystem.Instance.Translation.InvalidArguments, "1", this.DisplayCommandUsage());
            return false;
        }

        List<ReferenceHub> referenceHubs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out _);

        if (referenceHubs is null || referenceHubs.Count == 0)
        {
            response = CameraSystem.Instance.Translation.DisconnectNoPlayersFound;
            return false;
        }

        foreach (ReferenceHub referenceHub in referenceHubs)
        {
            if (!Player.TryGet(referenceHub, out Player player))
                continue;

            try
            {
                CameraManager.Instance.Disconnect(player);
            }
            catch (Exception ex)
            {
                response = string.Empty;
                Log.Error($"Error disconnecting player: {ex}");
                return false;
            }
        }

        response = CameraSystem.Instance.Translation.DisconnectSuccess;
        return true;
    }
}
