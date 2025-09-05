using CameraSystem.Managers;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;

namespace CameraSystem.Patches;

[HarmonyPatch(typeof(Scp079ScannerSequence), nameof(Scp079ScannerSequence.ScanningPossible), MethodType.Getter)]
internal static class Scp079ScanningPossiblePatch
{
    [HarmonyPrefix]
    private static bool Prefix(Scp079ScannerSequence __instance, ref bool __result)
    {
        var player = Player.Get(__instance._teamSelector.Owner);
        if (player == null || !CameraManager.Instance.IsWatching(player))
        {
            return true;
        }

        __result = false;
        return false;
    }
}