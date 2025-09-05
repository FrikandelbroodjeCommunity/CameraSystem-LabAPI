using CameraSystem.Managers;
using CameraSystem.Models;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace CameraSystem.Patches;

[HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), typeof(ReferenceHub))]
internal static class RolesUtilsGetTeamPatch
{
    [HarmonyPrefix]
    private static bool Prefix(ReferenceHub hub, ref Team __result)
    {
        var player = Player.Get(hub);
        if (player == null || !CameraManager.Instance.TryGetWatcher(player, out Watcher watcher))
        {
            return true;
        }

        __result = watcher.PlayerSnapshot.Role.GetTeam();
        return false;
    }
}