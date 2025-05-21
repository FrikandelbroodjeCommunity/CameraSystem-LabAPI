using System;
using CameraSystem.Managers;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using InventorySystem.Items.Firearms.Attachments;
using UnityEngine;

namespace CameraSystem.Commands;
internal class ToggleWorkstationUsage : ICommand
{
    public string Command => "toggleworkstationusage";
    public string Description => Plugin.Instance.Translation.ToggleWorkstationUsageDescription;
    public string[] Aliases { get; } = new[] { "twu" };

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission($"camsys.{Command}"))
        {
            response = string.Format(Plugin.Instance.Translation.NoPermission, $"camsys.{Command}");
            return false;
        }

        if (!Player.TryGet(sender, out Player player))
        {
            response = Plugin.Instance.Translation.NotPlayer;
            return false;
        }

        if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 5f))
        {
            response = Plugin.Instance.Translation.NoWorkstationFound;
            return false;
        }

        WorkstationController workstationController = hit.collider.GetComponentInParent<WorkstationController>();

        if (workstationController is null)
        {
            response = Plugin.Instance.Translation.NoWorkstationFound;
            return false;
        }

        if (CameraManager.Instance.WorkstationControllers.Contains(workstationController))
        {
            CameraManager.Instance.WorkstationControllers.Remove(workstationController);
            response = Plugin.Instance.Translation.WorkstationRemoved;
            return true;
        }

        CameraManager.Instance.WorkstationControllers.Add(workstationController);

        Room room = Room.Get(workstationController.transform.position);
        Vector3 localPos = room?.Transform.InverseTransformPoint(workstationController.transform.position) ?? Vector3.zero;
        Vector3 localRot = (Quaternion.Inverse(room?.transform.rotation ?? Quaternion.identity) *
                            workstationController.transform.rotation).eulerAngles;

        response = $"Workstation added to managed list.\n" +
                   $"Preset configuration (copy to config):\n" +
                   $"room_type: {room?.Type}\n" +
                   $"local_position:\n" +
                   $"  x: {localPos.x:0.###}\n" +
                   $"  y: {localPos.y:0.###}\n" +
                   $"  z: {localPos.z:0.###}\n" +
                   $"local_rotation:\n" +
                   $"  x: {localRot.x:0.###}\n" +
                   $"  y: {localRot.y:0.###}\n" +
                   $"  z: {localRot.z:0.###}\n" +
                   $"scale:\n" +
                   $"  x: {workstationController.transform.localScale.x:0.###}\n" +
                   $"  y: {workstationController.transform.localScale.y:0.###}\n" +
                   $"  z: {workstationController.transform.localScale.z:0.###}";

        return true;
    }
}