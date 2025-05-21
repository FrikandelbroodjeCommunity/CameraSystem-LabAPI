using CameraSystem.Managers;
using CameraSystem.Models;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;

namespace CameraSystem.Patches;
[HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), typeof(ReferenceHub))]
internal static class RolesUtilsGetTeamPatch
{
    [HarmonyPrefix]
    private static bool Prefix(ReferenceHub hub, ref Team __result)
    {
        if (!Player.TryGet(hub, out Player player) ||
            !CameraManager.Instance.TryGetWatcher(player, out Watcher watcher))
            return true;

        __result = watcher.PlayerSnapshot.Role.GetTeam();
        return false;
    }
}