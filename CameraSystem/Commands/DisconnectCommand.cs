using System;
using System.Collections.Generic;
using CameraSystem.Managers;
using CommandSystem;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Utils;

namespace CameraSystem.Commands;

internal class DisconnectCommand : ICommand, IUsageProvider
{
    public string Command => "disconnect";
    public string Description => CameraSystem.Instance.Config?.Translations.DisconnectCommandDescription;
    public string[] Aliases { get; } = new[] { "d", "dc" };

    public string[] Usage { get; } = new[] { "%player%" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasPermissions(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Config?.Translations.NoPermission;
            return false;
        }

        if (arguments.Count == 0)
        {
            response = string.Format(CameraSystem.Instance.Config?.Translations.InvalidArguments, "1",
                this.DisplayCommandUsage());
            return false;
        }

        List<ReferenceHub> referenceHubs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out _);

        if (referenceHubs is null || referenceHubs.Count == 0)
        {
            response = CameraSystem.Instance.Config?.Translations.DisconnectNoPlayersFound;
            return false;
        }

        foreach (ReferenceHub referenceHub in referenceHubs)
        {
            var player = Player.Get(referenceHub);
            if (player == null)
            {
                continue;
            }

            try
            {
                CameraManager.Instance.Disconnect(player);
            }
            catch (Exception ex)
            {
                response = "Error while disconnecting player, see the server logs full the full error!";
                Logger.Error($"Error disconnecting player: {ex}");
                return false;
            }
        }

        response = CameraSystem.Instance.Config?.Translations.DisconnectSuccess;
        return true;
    }
}