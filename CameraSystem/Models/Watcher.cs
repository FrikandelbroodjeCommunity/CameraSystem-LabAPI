using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using UnityEngine;

namespace CameraSystem.Models;

internal class Watcher
{
    internal Player Player { get; }
    internal PlayerSnapshot PlayerSnapshot { get; }
    internal ReferenceHub Npc { get; }
    internal Player NpcPlayer => Player.Get(Npc);

    internal Watcher(Player player)
    {
        Player = player;
        PlayerSnapshot = new PlayerSnapshot(player);
        Npc = SpawnNpc();
    }

    internal void DestroyNpc()
    {
        if (Npc != null)
        {
            NetworkServer.Destroy(Npc.gameObject);
        }
    }

    private ReferenceHub SpawnNpc()
    {
        var hub = DummyUtils.SpawnDummy($"{PlayerSnapshot.Nickname} (cams)");
        var npcPlayer = Player.Get(hub);
        npcPlayer.SetRole(PlayerSnapshot.Role, RoleChangeReason.None, RoleSpawnFlags.None);
        npcPlayer.Position = PlayerSnapshot.Position + Vector3.up * 0.1f;

        var newCustomInfo = PlayerSnapshot.CustomInfo +
                            CameraSystem.Instance.Config?.Translations.WatchingCamerasPostfix;
        if (!string.IsNullOrEmpty(newCustomInfo))
        {
            npcPlayer.CustomInfo = newCustomInfo;
        }

        npcPlayer.Health = PlayerSnapshot.Health;
        npcPlayer.ArtificialHealth = PlayerSnapshot.ArtificialHealth;

        npcPlayer.Rotation = PlayerSnapshot.Rotation;
        npcPlayer.Scale = PlayerSnapshot.Scale;
        npcPlayer.InfoArea &= ~PlayerInfoArea.Badge;

        if (!string.IsNullOrEmpty(PlayerSnapshot.CustomName))
        {
            Timing.CallDelayed(0.1f, () => { npcPlayer.DisplayName = $"{PlayerSnapshot.CustomName} (cams)"; });
        }

        return hub;
    }
}