using System;
using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace CameraSystem.Patches;

[HarmonyPatch(typeof(WorkstationController), nameof(WorkstationController.ServerInteract))]
public static class WorkstationActivationPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool OnEnterWorkstation(ReferenceHub ply, WorkstationController __instance)
    {
        try
        {
            return EventHandlers.OnActivatingWorkstation(Player.Get(ply), __instance);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to connect player: {e}");
            return true;
        }
    }
}