using System;
using CameraSystem.Configs;
using CameraSystem.Managers;
using Exiled.API.Features;
using HarmonyLib;

namespace CameraSystem;
public class Plugin : Plugin<Config, Translation>
{
    public override string Author => "Jiraya";
    public override string Name => "CameraSystem";
    public override Version Version { get; } = new(1, 1, 0);

    internal static Plugin Instance { get; private set; }
    internal CameraManager CameraManager { get; private set; }

    private Harmony _harmony;

    public override void OnEnabled()
    {
        Instance = this;

        CameraManager = new();
        EventHandlers.Register();

        _harmony = new($"com.{Author}.{Name}.{DateTime.Now.Ticks}");
        _harmony.PatchAll();

        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        _harmony.UnpatchAll();
        _harmony = null;

        EventHandlers.Unregister();

        CameraManager.Dispose();
        CameraManager = null;

        Instance = null;

        base.OnDisabled();
    }
}