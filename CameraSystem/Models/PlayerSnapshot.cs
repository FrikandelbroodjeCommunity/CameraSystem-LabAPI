using System.Collections.Generic;
using System.Linq;
using CameraSystem.Managers;
using CustomPlayerEffects;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using UnityEngine;

namespace CameraSystem.Models;

internal class PlayerSnapshot
{
    internal Dictionary<StatusEffectBase, (byte Intensity, float Duration)> ActiveEffects { get; }
    internal Dictionary<ItemType, ushort> Ammo { get; }
    internal float ArtificialHealth { get; }
    internal string CustomInfo { get; }
    internal string CustomName { get; }
    internal EmotionPresetType Emotion { get; }
    internal float Health { get; }
    internal Vector3 Position { get; }
    internal RoleTypeId Role { get; }
    internal Vector3 Scale { get; }
    internal Quaternion Rotation { get; }
    internal string Nickname { get; }
    internal int Scp330Uses { get; }
    internal bool Scp1344Equipped { get; }

    internal PlayerSnapshot(Player player)
    {
        ActiveEffects = player.ActiveEffects.ToDictionary(
            effect => effect,
            effect => (effect.Intensity, effect.Duration)
        );
        Ammo = player.Ammo.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        ArtificialHealth = player.ArtificialHealth;
        CustomName = player.DisplayName == player.Nickname ? null : player.DisplayName;
        CustomInfo = player.CustomInfo;
        Emotion = EmotionSync.Database[player.ReferenceHub];
        Health = player.Health;
        Position = player.Position;
        Role = player.Role;
        Scale = player.Scale;
        Rotation = player.Rotation;
        Nickname = player.Nickname;

        if (CameraManager.Scp330Interobject != null)
        {
            Scp330Uses = CameraManager.Scp330Interobject.PreviousUses
                .Count(x => x.LifeIdentifier == player.RoleBase.UniqueLifeIdentifier);
        }

        foreach (var item in player.Items.Where(x => x.Type == ItemType.SCP1344))
        {
            var goggles = (Scp1344Item)item;
            if (!goggles.IsWorn) continue;

            Scp1344Equipped = true;
            break;
        }
    }
}