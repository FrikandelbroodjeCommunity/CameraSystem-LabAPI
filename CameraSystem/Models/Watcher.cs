using Exiled.API.Features;
using MEC;
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
        Npc npc = Npc.Spawn(PlayerSnapshot.Nickname, PlayerSnapshot.Role, PlayerSnapshot.Position);

        string newCustomInfo = PlayerSnapshot.CustomInfo + CameraSystem.Instance.Translation.WatchingCamerasPostfix;
        if (!string.IsNullOrEmpty(newCustomInfo))
            npc.CustomInfo = newCustomInfo;

        npc.Health = PlayerSnapshot.Health;
        npc.ArtificialHealth = PlayerSnapshot.ArtificialHealth;

        npc.Rotation = PlayerSnapshot.Rotation;
        npc.Scale = PlayerSnapshot.Scale;
        npc.InfoArea &= ~PlayerInfoArea.Badge;

        if (!string.IsNullOrEmpty(PlayerSnapshot.CustomName))
            Timing.CallDelayed(0.1f, () =>
            {
                if (npc.ReferenceHub?.nicknameSync is not null)
                    npc.CustomName = PlayerSnapshot.CustomName;
            });

        return npc;
    }
}
