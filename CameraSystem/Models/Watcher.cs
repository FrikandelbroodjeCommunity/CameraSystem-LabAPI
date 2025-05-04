using Exiled.API.Features;

namespace CameraSystem.Models;
internal sealed class Watcher
{
    internal Player Player { get; }
    internal PlayerSnapshot PlayerSnapshot { get; }
    internal Npc Npc { get; }

    internal Watcher(Player player)
    {
        Player = player;
        PlayerSnapshot = new(player);
        Npc = SpawnNpc();
    }

    private Npc SpawnNpc()
    {
        Npc npc = Npc.Spawn(PlayerSnapshot.CustomName, PlayerSnapshot.Role, PlayerSnapshot.Position);

        npc.CustomInfo = $"{PlayerSnapshot.CustomInfo}{Plugin.Instance.Translation.WatchingCamerasPostfix}";
        npc.Emotion = PlayerSnapshot.Emotion;
        npc.Health = PlayerSnapshot.Health;
        npc.ArtificialHealth = PlayerSnapshot.ArtificialHealth;
        npc.Scale = PlayerSnapshot.Scale;
        npc.InfoArea &= ~PlayerInfoArea.Badge;

        return npc;
    }
}