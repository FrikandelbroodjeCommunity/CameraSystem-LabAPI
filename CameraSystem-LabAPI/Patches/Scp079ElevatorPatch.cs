using CameraSystem.Managers;
using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp079;

namespace CameraSystem.Patches;

[HarmonyPatch]
public class Scp079ElevatorPatch
{
    [HarmonyPatch(typeof(Scp079ElevatorStateChanger), nameof(Scp079ElevatorStateChanger.ServerProcessCmd))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool On079InteractElevator(Scp079ElevatorStateChanger __instance)
    {
        var owner = Player.Get(__instance.Owner);
        return owner == null || !CameraManager.Instance.IsWatching(owner);
    }
}