using System.ComponentModel;
using Exiled.API.Enums;
using UnityEngine;

namespace CameraSystem.Models;
public sealed class PresetConfig
{
    [Description("The room type where the workstation should be spawned (e.g. HczArmory, EzIntercom)")]
    public RoomType RoomType { get; set; }

    [Description("Local position relative to the room")]
    public Vector3 LocalPosition { get; set; }

    [Description("Local rotation relative to the room")]
    public Vector3 LocalRotation { get; set; }

    [Description("Scale of the workstation")]
    public Vector3 Scale { get; set; }

    public PresetConfig()
    {
    }

    public PresetConfig(RoomType roomType,
        Vector3 localPosition,
        Vector3 localRotation,
        Vector3 scale)
    {
        RoomType = roomType;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        Scale = scale;
    }
}