using System;
using CameraSystem.Managers;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using InventorySystem.Items.Firearms.Attachments;
using UnityEngine;

namespace CameraSystem.Commands;
internal class ToggleWorkstationUsageCommand : ICommand
{
    public string Command => "toggleworkstationusage";
    public string Description => CameraSystem.Instance.Translation.ToggleWorkstationUsageDescription;
    public string[] Aliases { get; } = new[] { "twu" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission(CameraSystemParentCommand.PermissionPrefix + Command))
        {
            response = CameraSystem.Instance.Translation.NoPermission;
            return false;
        }

        if (!Player.TryGet(sender, out Player player))
        {
            response = CameraSystem.Instance.Translation.NotPlayer;
            return false;
        }

        if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 5f))
        {
            response = CameraSystem.Instance.Translation.NoWorkstationFound;
            return false;
        }

        WorkstationController workstationController = hit.collider.GetComponentInParent<WorkstationController>();

        if (workstationController is null)
        {
            response = CameraSystem.Instance.Translation.NoWorkstationFound;
            return false;
        }

        if (CameraManager.Instance.WorkstationControllers.Contains(workstationController))
        {
            CameraManager.Instance.WorkstationControllers.Remove(workstationController);
            response = CameraSystem.Instance.Translation.WorkstationRemoved;
            return true;
        }

        CameraManager.Instance.WorkstationControllers.Add(workstationController);

        Room room = Room.Get(workstationController.transform.position);
        Vector3 localPos = room?.Transform.InverseTransformPoint(workstationController.transform.position) ?? Vector3.zero;
        Vector3 localRot = (Quaternion.Inverse(room?.transform.rotation ?? Quaternion.identity) *
                            workstationController.transform.rotation).eulerAngles;

        response = $"""
                    Workstation added to managed list.
                    Preset configuration (copy to config):
                    room_type: {room?.Type}
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
