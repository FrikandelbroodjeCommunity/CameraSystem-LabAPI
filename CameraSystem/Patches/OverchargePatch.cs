using HarmonyLib;
using MEC;
using PlayerRoles.PlayableScps.Scp079;

namespace CameraSystem.Patches;

[HarmonyPatch]
public class OverchargePatch
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge))]
    [HarmonyPrefix]
    public static void OnOvercharge(Scp079Recontainer __instance)
    {
        if (CameraSystem.Instance.Config.RecontainmentTimeout <= -2) return;

        var delay = CameraSystem.Instance.Config.RecontainmentTimeout == -1
            ? __instance._lockdownDuration
            : CameraSystem.Instance.Config.RecontainmentTimeout;

        EventHandlers.Disabled = true;
        Timing.CallDelayed(delay, () => { EventHandlers.Disabled = false; });
    }
}