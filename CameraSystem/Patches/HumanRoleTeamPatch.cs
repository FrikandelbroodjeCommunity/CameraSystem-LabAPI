using CameraSystem.Managers;
using CameraSystem.Models;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;

namespace CameraSystem.Patches;
[HarmonyPatch(typeof(HumanRole), nameof(HumanRole.Team), MethodType.Getter)]
internal static class HumanRoleTeamPatch
{
    [HarmonyPrefix]
    private static bool Prefix(HumanRole __instance, ref Team __result)
    {
        if (!Player.TryGet(__instance._lastOwner, out Player player) ||
            !CameraManager.Instance.TryGetWatcher(player, out Watcher watcher))
            return true;

        __result = watcher.PlayerSnapshot.Role.GetTeam();
        return false;
    }
}