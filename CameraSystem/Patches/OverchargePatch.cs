using HarmonyLib;
using MEC;
using PlayerRoles.PlayableScps.Scp079;

namespace CameraSystem.Patches;

[HarmonyPatch]
public class OverchargePatch
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge))]
    [HarmonyPrefix]
    public static void OnOvercharge()
    {
        EventHandlers.Disabled = true;
        Timing.CallDelayed(CameraSystem.Instance.Config.RecontainmentTimeout, () => { EventHandlers.Disabled = false; });
    }
}