using CameraSystem.Managers;
using CameraSystem.Models;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;

namespace CameraSystem.Patches;
[HarmonyPatch(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.GetTeam), typeof(ReferenceHub))]
internal sealed class RolesUtilsGetTeamPatch
{
    [HarmonyPrefix]
    private static bool Prefix(ReferenceHub referenceHub, ref Team __result)
    {
        if (!Player.TryGet(referenceHub, out Player player) ||
            !CameraManager.Instance.TryGetWatcher(player, out Watcher watcher))
            return true;

        __result = watcher.PlayerSnapshot.Role.GetTeam();
        return false;
    }
}