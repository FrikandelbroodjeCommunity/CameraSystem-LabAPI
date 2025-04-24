using System.ComponentModel;
using UnityEngine;

namespace CameraSystem.Models;
public sealed class WorkstationConfig
{
    public WorkstationConfig()
    {
    }

    public WorkstationConfig(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }

    [Description("The position where the workstation should spawn")]
    public Vector3 Position { get; set; }

    [Description("The rotation of the workstation")]
    public Vector3 Rotation { get; set; }

    [Description("The scale of the workstation")]
    public Vector3 Scale { get; set; }
}