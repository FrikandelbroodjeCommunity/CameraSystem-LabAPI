using System;
using System.Collections.Generic;
using CameraSystem.Models;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using InventorySystem.Items.Firearms.Attachments;
using Mirror;
using PlayerRoles;

namespace CameraSystem.Managers;
internal sealed class CameraManager : IDisposable
{
    internal static CameraManager Instance => Plugin.Instance.CameraManager;

    public List<WorkstationController> WorkstationControllers { get; set; } = new();

    private readonly List<Watcher> _watchers = new();

    internal void Connect(Player player)
    {
        if (IsWatching(player))
            return;

        Watcher watcher = new Watcher(player);
        _watchers.Add(watcher);

        player.Role.Set(RoleTypeId.Scp079, RoleSpawnFlags.None);

        player.ShowHint(Plugin.Instance.Translation.ConnectionSuccessMessage);
    }

    internal void Disconnect(Player player, DamageHandler damageHandler = null)
    {
        if (!TryGetWatcher(player, out Watcher watcher))
            return;

        try
        {
            player.Role.Set(watcher.PlayerSnapshot.Role, RoleSpawnFlags.None);
            player.Position = watcher.PlayerSnapshot.Position;
            player.Emotion = watcher.PlayerSnapshot.Emotion;
            player.ArtificialHealth = watcher.PlayerSnapshot.ArtificialHealth;
            player.Health = watcher.PlayerSnapshot.Health;
            player.SetAmmo(watcher.PlayerSnapshot.Ammo);

            foreach (KeyValuePair<EffectType, (byte Intensity, float Duration)> kvp in watcher.PlayerSnapshot.ActiveEffects)
            {
                player.EnableEffect(kvp.Key, kvp.Value.Intensity, kvp.Value.Duration);
            }

            if (damageHandler != null && player.IsAlive)
            {
                player.Hurt(damageHandler);
            }

            player.ShowHint(Plugin.Instance.Translation.DisconnectionMessage);
        }
        catch (Exception e)
        {
            Log.Error($"Error during disconnect: {e}");
        }
        finally
        {
            if (watcher.Npc != null)
            {
                RoundSummary.singleton.OnServerRoleSet(watcher.Npc.ReferenceHub, RoleTypeId.None, RoleChangeReason.Destroyed);
                NetworkServer.DestroyPlayerForConnection(watcher.Npc.NetworkIdentity.connectionToClient);
            }

            _watchers.Remove(watcher);
        }
    }

    internal bool IsWatching(Player player) => _watchers.Exists(watcher => watcher.Player == player);

    internal bool IsWatching(Npc npc) => _watchers.Exists(watcher => watcher.Npc == npc);

    internal bool TryGetWatcher(Player player, out Watcher watcher)
    {
        watcher = _watchers.Find(w => w.Player == player);
        return watcher != null;
    }

    internal bool TryGetWatcher(Npc npc, out Watcher watcher)
    {
        watcher = _watchers.Find(w => w.Npc == npc);
        return watcher != null;
    }

    internal void ForceDisconnect(Player player)
    {
        if (!TryGetWatcher(player, out Watcher watcher))
            return;

        if (watcher.Npc.IsAlive)
        {
            watcher.Npc.Kill(DamageType.Unknown);

            RoundSummary.singleton.OnServerRoleSet(watcher.Npc.ReferenceHub, RoleTypeId.None, RoleChangeReason.Destroyed);
            NetworkServer.DestroyPlayerForConnection(watcher.Npc.NetworkIdentity.connectionToClient);
        }

        _watchers.Remove(watcher);
    }

    public void Dispose()
    {
        foreach (Watcher watcher in _watchers.ToArray())
        {
            try
            {
                if (watcher.Player != null)
                    Disconnect(watcher.Player);

                if (watcher.Npc != null && watcher.Npc.IsAlive)
                {
                    watcher.Npc.Kill(DamageType.Unknown);
                    NetworkServer.Destroy(watcher.Npc.GameObject);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error disposing watcher: {e}");
            }
        }

        _watchers.Clear();
        WorkstationControllers.Clear();

        GC.SuppressFinalize(this);
    }
}