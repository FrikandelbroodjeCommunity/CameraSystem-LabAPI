using System;
using System.Collections.Generic;
using System.Linq;
using CameraSystem.Models;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
using Exiled.API.Features.Items;
using Exiled.API.Features.Roles;
using InventorySystem.Items.Firearms.Attachments;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CameraSystem.Managers;
internal sealed class CameraManager : IDisposable
{
    internal static CameraManager Instance => Plugin.Instance.CameraManager;

    internal bool IsCameraSystemEnabled { get; private set; } = Plugin.Instance.Config.IsCameraSystemEnabledByDefault;
    internal List<WorkstationController> WorkstationControllers { get; set; } = new();

    private readonly List<Watcher> _watchers = new();
    private bool _disposed;

    internal void Connect(Player player)
    {
        if (IsWatching(player))
            return;

        Watcher watcher = new(player);
        _watchers.Add(watcher);

        player.Role.Set(RoleTypeId.Scp079, RoleSpawnFlags.None);
        player.CurrentItem = null;

        player.ShowHint(Plugin.Instance.Translation.ConnectionSuccessMessage);
    }

    internal void Disconnect(Player player, DamageHandler damageHandler = null)
    {
        if (!TryGetWatcher(player, out Watcher watcher))
            return;

        try
        {
            watcher.Player.Role.Set(watcher.PlayerSnapshot.Role, RoleSpawnFlags.None);
            watcher.Player.Position = watcher.PlayerSnapshot.Position;
            watcher.Player.Emotion = watcher.PlayerSnapshot.Emotion;
            watcher.Player.ArtificialHealth = watcher.PlayerSnapshot.ArtificialHealth;
            watcher.Player.Health = watcher.PlayerSnapshot.Health;
            watcher.Player.SetAmmo(watcher.PlayerSnapshot.Ammo);

            foreach (KeyValuePair<EffectType, (byte Intensity, float Duration)> kvp in watcher.PlayerSnapshot.ActiveEffects)
                watcher.Player.EnableEffect(kvp.Key, kvp.Value.Intensity, kvp.Value.Duration);

            if (damageHandler is not null && player.IsAlive)
                watcher.Player.Hurt(damageHandler);

            watcher.Player.ShowHint(Plugin.Instance.Translation.DisconnectionMessage);
        }
        catch (Exception ex)
        {
            Log.Error($"Error during disconnect: {ex}");
        }
        finally
        {
            _watchers.Remove(watcher);
            watcher.DestroyNpc();
        }
    }

    internal void Enable() => IsCameraSystemEnabled = true;

    internal void Disable()
    {
        IsCameraSystemEnabled = false;

        foreach (Player player in _watchers.Select(w => w.Player).ToArray())
        {
            if (player.Role is not Scp079Role scp079Role)
            {
                ForceDisconnect(player);
                continue;
            }

            scp079Role.LoseSignal(3f);
            player.ShowHint(Plugin.Instance.Translation.CameraSystemDisabledMessage);

            Timing.CallDelayed(3f, () => Disconnect(player));
        }
    }

    internal bool IsWatching(Player player)
    {
        return _watchers.Any(watcher => watcher.Player == player || watcher.Npc == player);
    }

    internal bool TryGetWatcher(Player player, out Watcher watcher)
    {
        watcher = _watchers.Find(w => w.Player == player || w.Npc == player);
        return watcher is not null;
    }

    internal void ForceDisconnect(Player player)
    {
        if (!TryGetWatcher(player, out Watcher watcher))
            return;

        ForceDisconnect(watcher);
    }

    internal void ForceDisconnect(Watcher watcher)
    {
        _watchers.Remove(watcher);
        watcher.DestroyNpc();
    }

    internal void CreateWorkstation(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject workstation = UnityEngine.Object.Instantiate(prefab, position, rotation);
        workstation.transform.localScale = scale;
        NetworkServer.Spawn(workstation);

        if (!workstation.TryGetComponent(out WorkstationController workstationController))
        {
            Log.Error($"WorkstationController missing on spawned workstation (ID: {workstation.GetInstanceID()}).");
            return;
        }

        WorkstationControllers.Add(workstationController);
        Log.Debug($"Successfully spawned workstation (ID: {workstation.GetInstanceID()}).");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            foreach (Watcher watcher in _watchers.ToArray())
            {
                try
                {
                    if (watcher.Player is not null)
                        Disconnect(watcher.Player);

                    if (watcher.Npc is not null && watcher.Npc.IsAlive)
                    {
                        watcher.Npc.Kill(DamageType.Unknown);
                        NetworkServer.Destroy(watcher.Npc.GameObject);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error disposing watcher: {ex}");
                }
            }

            _watchers.Clear();
            WorkstationControllers.Clear();
        }

        _disposed = true;
    }

    ~CameraManager() => Dispose(false);
}