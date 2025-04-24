using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CameraSystem.Configs;
public sealed class Translation : ITranslation
{
    [Description("The text appended to a player's custom info when they're watching cameras")]
    public string WatchingCamerasPostfix { get; set; } = "\nWatching Security Cameras";


    [Description("The message shown when a player successfully connects to the camera system")]
    public string ConnectionSuccessMessage { get; set; } = "\n\n<color=#FAFF86><size=21><b>You have successfully connected to the security camera system.\nPress E to exit.</b></size></color>";

    [Description("The message shown when a player disconnects from the camera system")]
    public string DisconnectionMessage { get; set; } = "\n\n<color=#FAFF86><size=21><b>You have disconnected from the security camera system.</b></size></color>";

}