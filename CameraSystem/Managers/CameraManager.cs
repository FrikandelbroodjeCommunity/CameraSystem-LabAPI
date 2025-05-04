using System;
using System.Collections.Generic;
using CameraSystem.Models;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.DamageHandlers;
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
            watcher.Player.Role.Set(watcher.PlayerSnapshot.Role, RoleSpawnFlags.None);
            watcher.Player.Position = watcher.PlayerSnapshot.Position;
            watcher.Player.Emotion = watcher.PlayerSnapshot.Emotion;
            watcher.Player.ArtificialHealth = watcher.PlayerSnapshot.ArtificialHealth;
            watcher.Player.Health = watcher.PlayerSnapshot.Health;
            watcher.Player.SetAmmo(watcher.PlayerSnapshot.Ammo);

            foreach (KeyValuePair<EffectType, (byte Intensity, float Duration)> kvp in watcher.PlayerSnapshot.ActiveEffects)
            {
                watcher.Player.EnableEffect(kvp.Key, kvp.Value.Intensity, kvp.Value.Duration);
            }

            if (damageHandler != null && player.IsAlive)
            {
                watcher.Player.Hurt(damageHandler);
            }

            watcher.Player.ShowHint(Plugin.Instance.Translation.DisconnectionMessage);
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

    internal void Enable() => IsCameraSystemEnabled = true;

    internal void Disable()
    {
        IsCameraSystemEnabled = false;

        foreach (Watcher watcher in _watchers.ToArray())
        {
            if (watcher.Player.Role is not Scp079Role scp079Role)
            {
                ForceDisconnect(watcher.Player);
                continue;
            }

            scp079Role.LoseSignal(3f);
            watcher.Player.ShowHint(Plugin.Instance.Translation.CameraSystemDisabledMessage);

            Timing.CallDelayed(3f, () => Disconnect(watcher.Player));
        }
    }

    internal bool IsWatching(Player player)
    {
        return _watchers.Any(watcher => watcher.Player == player || watcher.Npc == player);
    }

    internal bool TryGetWatcher(Player player, out Watcher watcher)
    {
        watcher = _watchers.Find(w => w.Player == player || w.Npc == player);
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
    }
}