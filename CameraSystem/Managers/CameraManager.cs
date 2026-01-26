using System;
using System.Collections.Generic;
using System.Linq;
using CameraSystem.Models;
using CustomPlayerEffects;
using Footprinting;
using Interactables.Interobjects;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Usables.Scp1344;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.PlayableScps.Scp079;
using PlayerStatsSystem;
using UnityEngine;
using Utils.NonAllocLINQ;
using Logger = LabApi.Features.Console.Logger;
using Scp1344Item = LabApi.Features.Wrappers.Scp1344Item;

namespace CameraSystem.Managers;

internal sealed class CameraManager : IDisposable
{
    internal static CameraManager Instance => CameraSystem.Instance.CameraManager;

    internal bool IsCameraSystemEnabled { get; private set; } =
        CameraSystem.Instance.Config.IsCameraSystemEnabledByDefault;

    internal static Scp330Interobject Scp330Interobject;
    internal List<WorkstationController> WorkstationControllers { get; set; } = new();

    private readonly List<Watcher> _watchers = new();
    private bool _disposed;

    internal void Connect(Player player)
    {
        if (IsWatching(player))
            return;

        Watcher watcher = new(player);
        _watchers.Add(watcher);

        player.SetRole(RoleTypeId.Scp079, RoleChangeReason.None, flags: RoleSpawnFlags.None);
        player.SendHint(CameraSystem.Instance.Config.Translations.ConnectionSuccessMessage, 7);

        player.CurrentItem = null;
    }

    internal void Disconnect(Player player, DamageHandlerBase damageHandler = null)
    {
        if (!TryGetWatcher(player, out var watcher) || watcher.Player.IsDestroyed)
        {
            return;
        }

        try
        {
            watcher.Player.SetRole(watcher.PlayerSnapshot.Role, RoleChangeReason.None, flags: RoleSpawnFlags.None);
            watcher.Player.Rotation = watcher.PlayerSnapshot.Rotation;
            watcher.Player.Position = watcher.PlayerSnapshot.Position;
            watcher.Player.ReferenceHub.ServerSetEmotionPreset(watcher.PlayerSnapshot.Emotion);
            watcher.Player.ArtificialHealth = watcher.PlayerSnapshot.ArtificialHealth;
            watcher.Player.Health = watcher.PlayerSnapshot.Health;

            if (watcher.PlayerSnapshot.EquippedItem.HasValue)
            {
                var item = watcher.Player.Items.FirstOrDefault(x =>
                    x.Serial == watcher.PlayerSnapshot.EquippedItem.Value);

                if (item != null)
                {
                    watcher.Player.CurrentItem = item;
                }
            }

            if (Scp330Interobject != null)
            {
                var current330Count = Scp330Interobject.PreviousUses
                    .Count(x => x.LifeIdentifier == watcher.Player.RoleBase.UniqueLifeIdentifier);
                for (var i = current330Count; i < watcher.PlayerSnapshot.Scp330Uses; i++)
                {
                    Scp330Interobject.PreviousUses.Add(new Footprint(watcher.Player.ReferenceHub));
                }
            }

            if (watcher.PlayerSnapshot.Scp1344Equipped)
            {
                var instance = (Scp1344Item)watcher.Player.Items.First(x => x.Type == ItemType.SCP1344);
                instance.Status = Scp1344Status.Active;
            }

            foreach (var pair in watcher.PlayerSnapshot.Ammo)
            {
                watcher.Player.SetAmmo(pair.Key, pair.Value);
            }

            watcher.Player.DisableEffect<SpawnProtected>();
            foreach (var kvp in watcher.PlayerSnapshot.ActiveEffects)
            {
                watcher.Player.EnableEffect(kvp.Key, kvp.Value.Intensity, kvp.Value.Duration);
            }

            if (damageHandler is not null && player.IsAlive)
            {
                Timing.CallDelayed(Timing.WaitForOneFrame, () => { watcher.Player.Damage(damageHandler); });
            }

            watcher.Player.SendHint(CameraSystem.Instance.Config.Translations.DisconnectionMessage, 7);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error during disconnect: {ex}");
        }
        finally
        {
            _watchers.Remove(watcher);
            watcher.DestroyNpc();
        }
    }

    internal void DisconnectAll(DamageHandlerBase damageHandler = null)
    {
        foreach (var watcher in _watchers.ToArray())
        {
            Disconnect(watcher.Player, damageHandler);
        }
    }

    internal void Enable() => IsCameraSystemEnabled = true;

    internal void Disable()
    {
        IsCameraSystemEnabled = false;

        foreach (var player in _watchers.Select(w => w.Player).ToArray())
        {
            if (player.RoleBase is not Scp079Role pcRole)
            {
                ForceDisconnect(player);
                continue;
            }

            if (pcRole.SubroutineModule.TryGetSubroutine(out Scp079LostSignalHandler lostSignalHandler))
            {
                lostSignalHandler.ServerLoseSignal(3);
            }

            player.SendHint(CameraSystem.Instance.Config.Translations.CameraSystemDisabledMessage, 7);

            Timing.CallDelayed(3f, () => Disconnect(player));
        }
    }

    internal bool IsWatching(Player player)
    {
        return _watchers.Any(watcher => watcher.Player == player || watcher.Npc == player.ReferenceHub);
    }

    internal bool IsWatcher(Player player) => _watchers.Any(x => x.Player == player);

    internal bool TryGetWatcher(Player player, out Watcher watcher)
    {
        watcher = _watchers.Find(w => w.Player == player || w.Npc == player.ReferenceHub);
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
        var workstation = UnityEngine.Object.Instantiate(prefab, position, rotation);
        workstation.transform.localScale = scale;
        NetworkServer.Spawn(workstation);

        if (!workstation.TryGetComponent(out WorkstationController workstationController))
        {
            Logger.Error($"WorkstationController missing on spawned workstation (ID: {workstation.GetInstanceID()}).");
            return;
        }

        WorkstationControllers.Add(workstationController);
        Logger.Debug($"Successfully spawned workstation (ID: {workstation.GetInstanceID()}).");
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
                    if (!watcher.Player.IsDestroyed)
                        Disconnect(watcher.Player);

                    if (watcher.Npc is not null && watcher.Npc.IsAlive())
                    {
                        watcher.NpcPlayer.Kill();
                        NetworkServer.Destroy(watcher.Npc.gameObject);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error disposing watcher: {ex}");
                }
            }

            _watchers.Clear();
            WorkstationControllers.Clear();
        }

        _disposed = true;
    }

    ~CameraManager() => Dispose(false);
}