using System;
using CameraSystem.Managers;
using CommandSystem;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace CameraSystem.Commands;

internal class ToggleWorkstationUsageCommand : ICommand
{
    public string Command => "toggleworkstationusage";
    public string Description => CameraSystem.Instance.Config?.Translations.ToggleWorkstationUsageDescription;
    public string[] Aliases { get; } = new[] { "twu" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.HasPermissions(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Config?.Translations.NoPermission;
            return false;
        }

        var player = Player.Get(sender);
        if (player == null)
        {
            response = CameraSystem.Instance.Config?.Translations.NotPlayer;
            return false;
        }

        if (!Physics.Raycast(player.Camera.position, player.Camera.forward, out var hit, 5f))
        {
            response = CameraSystem.Instance.Config?.Translations.NoWorkstationFound;
            return false;
        }

        WorkstationController workstationController = hit.collider.GetComponentInParent<WorkstationController>();

        if (workstationController is null)
        {
            response = CameraSystem.Instance.Config?.Translations.NoWorkstationFound;
            return false;
        }

        if (CameraManager.Instance.WorkstationControllers.Contains(workstationController))
        {
            CameraManager.Instance.WorkstationControllers.Remove(workstationController);
            response = CameraSystem.Instance.Config?.Translations.WorkstationRemoved;
            return true;
        }

        CameraManager.Instance.WorkstationControllers.Add(workstationController);

        var room = Room.GetRoomAtPosition(workstationController.transform.position);
        var localPos = room?.Transform.InverseTransformPoint(workstationController.transform.position) ??
                       Vector3.zero;
        var localRot = (Quaternion.Inverse(room?.Transform.rotation ?? Quaternion.identity) *
                        workstationController.transform.rotation).eulerAngles;

        response = $"""
                    Workstation added to managed list.
                    Preset configuration (copy to config):
                    room_type: {room?.Name}
                    local_position:
                      x: {localPos.x:0.###}
                      y: {localPos.y:0.###}
                      z: {localPos.z:0.###}
                    local_rotation:
                      x: {localRot.x:0.###}
                      y: {localRot.y:0.###}
                      z: {localRot.z:0.###}
                    scale:
                      x: {workstationController.transform.localScale.x:0.###}
                      y: {workstationController.transform.localScale.y:0.###}
                      z: {workstationController.transform.localScale.z:0.###}
                    """;

        return true;
    }
}