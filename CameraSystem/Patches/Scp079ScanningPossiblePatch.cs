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
        if (!Player.TryGet(__instance._teamSelector.Owner, out Player player) ||
            !CameraManager.Instance.IsWatching(player))
            return true;

        __result = false;
        return false;
    }
}
