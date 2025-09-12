using CameraSystem.Managers;
using HarmonyLib;
using Interactables.Interobjects;

namespace CameraSystem.Patches;

[HarmonyPatch]
public static class Scp330InterobjectPatch
{
    [HarmonyPatch(typeof(Scp330Interobject), nameof(Scp330Interobject.ServerInteract))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void On330Interact(Scp330Interobject __instance)
    {
        CameraManager.Scp330Interobject = __instance;
    }
}