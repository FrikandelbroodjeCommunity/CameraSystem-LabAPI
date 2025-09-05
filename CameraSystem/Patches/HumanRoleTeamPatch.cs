using CameraSystem.Managers;
using CameraSystem.Models;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace CameraSystem.Patches;

[HarmonyPatch(typeof(HumanRole), nameof(HumanRole.Team), MethodType.Getter)]
internal static class HumanRoleTeamPatch
{
    [HarmonyPrefix]
    private static bool Prefix(HumanRole __instance, ref Team __result)
    {
        var player = Player.Get(__instance._lastOwner);
        if (player == null || !CameraManager.Instance.TryGetWatcher(player, out Watcher watcher))
        {
            return true;
        }

        __result = watcher.PlayerSnapshot.Role.GetTeam();
        return false;
    }
}