using System;
using CameraSystem.Configs;
using CameraSystem.Managers;
using Exiled.API.Features;

namespace CameraSystem;
public sealed class Plugin : Plugin<Config, Translation>
{
    public override string Author => "Jiraya";
    public override string Name => "CameraSystem";
    public override Version Version { get; } = new(1, 0, 7);

    internal static Plugin Instance { get; private set; }
    internal CameraManager CameraManager { get; private set; }

    public override void OnEnabled()
    {
        Instance = this;

        CameraManager = new CameraManager();
        EventHandlers.Register();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        EventHandlers.Unregister();

        CameraManager.Dispose();
        CameraManager = null;

        Instance = null;

        base.OnDisabled();
    }
}