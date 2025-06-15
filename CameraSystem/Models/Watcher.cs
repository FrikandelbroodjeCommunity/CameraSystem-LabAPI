using Exiled.API.Features;
using Mirror;
using PlayerRoles;

namespace CameraSystem.Models;
internal class Watcher
{
    internal Player Player { get; }
    internal PlayerSnapshot PlayerSnapshot { get; }
    internal Npc? Npc { get; }

    internal Watcher(Player player)
    {
        Player = player;
        PlayerSnapshot = new(player);
        Npc = SpawnNpc();
    }

    internal void DestroyNpc()
    {
        if (Npc is null)
            return;

        RoundSummary.singleton.OnServerRoleSet(Npc.ReferenceHub, RoleTypeId.None, RoleChangeReason.Destroyed);
        NetworkServer.DestroyPlayerForConnection(Npc.NetworkIdentity.connectionToClient);
        Npc.Destroy();
    }

    private Npc SpawnNpc()
    {
        Npc npc = Npc.Spawn(PlayerSnapshot.CustomName, PlayerSnapshot.Role, PlayerSnapshot.Position);

        npc.CustomInfo = $"{PlayerSnapshot.CustomInfo}{CameraSystem.Instance.Translation.WatchingCamerasPostfix}";
        npc.Emotion = PlayerSnapshot.Emotion;
        npc.Health = PlayerSnapshot.Health;
        npc.ArtificialHealth = PlayerSnapshot.ArtificialHealth;

        npc.Scale = PlayerSnapshot.Scale;
        npc.InfoArea &= ~PlayerInfoArea.Badge;
        npc.CustomName = PlayerSnapshot.CustomName;

        return npc;
    }
}
