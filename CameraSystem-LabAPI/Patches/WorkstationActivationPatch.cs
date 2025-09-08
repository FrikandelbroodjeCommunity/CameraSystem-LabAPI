using HarmonyLib;
using InventorySystem.Items.Firearms.Attachments;
using LabApi.Features.Wrappers;

namespace CameraSystem.Patches;

[HarmonyPatch(typeof(WorkstationController), nameof(WorkstationController.ServerInteract))]
public static class WorkstationActivationPatch
{
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static void OnEnterWorkstation(ReferenceHub ply, WorkstationController __instance)
    {
        EventHandlers.OnActivatingWorkstation(Player.Get(ply), __instance);
    }
}