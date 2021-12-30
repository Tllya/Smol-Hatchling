using OWML.ModHelper;
using OWML.Common;
using Harmony;

namespace TeenyHatchling
{
    public class TeenyHatchling : ModBehaviour
    {
        public static bool enableHighPitch;
        public static float height;
        public static float length;
        public static float width;

        private void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(TeenyHatchling)} is ready to go!", MessageType.Success);
            // Create ghost grabbing patch for when player is picked up by Stranger.
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>
                ("OnStartLiftPlayer",
                typeof(Patches),
                nameof(Patches.AttachPointOnStartLiftPlayer));

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                Setup();
            };
        }

        public override void Configure(IModConfig config)
        {
            // Whenever the config is changed, change the configuration bools and run the setup function.
            base.Configure(config);
            enableHighPitch = config.GetSettingsValue<bool>("Change Pitch Depending on Tolness");
            height = config.GetSettingsValue<float>("Tolness Multiplier");
            length = config.GetSettingsValue<float>("Chub Multiplier");
            width = config.GetSettingsValue<float>("Thiccness Multiplier");
            Setup();
        }

        private void Setup()
        {
            // If the loaded scene is not the Solar System or the Eye then cancel this function.
            if (LoadManager.s_currentScene != OWScene.SolarSystem
                    && LoadManager.s_currentScene != OWScene.EyeOfTheUniverse)
                    return;
            var playerBody = FindObjectOfType<PlayerBody>();
            var playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2");
            var playerThruster = playerBody.transform.Find("PlayerVFX");
            var playerCamera = FindObjectOfType<PlayerCameraController>();
            var playerMarshmallowStick = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root");
            // Smolify playermodel, camera, thrusters, and marshmallow stick.
            playerModel.transform.localScale = new UnityEngine.Vector3(width / 10, height / 10, length / 10);
            playerThruster.transform.localScale = new UnityEngine.Vector3(width, height, length);
            playerThruster.transform.localPosition = new UnityEngine.Vector3(0f, -1 + height, 0f);
            playerCamera._origLocalPosition = new UnityEngine.Vector3(0f, -1f + 1.8496f * height, 0.15f * length);
            playerMarshmallowStick.transform.localPosition = new UnityEngine.Vector3(0.25f, -1.8496f + 1.8496f * height, 0.08f + 0.15f * length);
            // playerMarshmallowStick.transform.localRotation = new UnityEngine.Quaternion(0f, 0.0872f, 0f, -0.9962f);
            // Raise ship attach points, but only if the scene is the Solar System.
            if (LoadManager.s_currentScene == OWScene.SolarSystem)
            {
                var shipCockpit = FindObjectOfType<ShipCockpitController>();
                var shipLog = FindObjectOfType<ShipLogController>();
                ModHelper.Events.Unity.FireInNUpdates(() =>
                    shipCockpit._origAttachPointLocalPos = new UnityEngine.Vector3(0f, 2.1849f - 1.8496f * height, 4.2307f - 0.15f * length),
                    60);
                shipLog._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * height, 0f - 0.15f * length);
            }
            // If pitch shift is enabled then turn them on.
            if (enableHighPitch)
            {
                var playerSounds = FindObjectOfType<PlayerAudioController>();
                var playerBreathing = FindObjectOfType<PlayerBreathingAudio>();
                playerSounds._oneShotSleepingAtCampfireSource.pitch = height / height / height;
                playerSounds._oneShotSource.pitch = height / height / height;
                playerBreathing._asphyxiationSource.pitch = height / height / height;
                playerBreathing._breathingLowOxygenSource.pitch = height / height / height;
                playerBreathing._breathingSource.pitch = height / height / height;
                playerBreathing._drowningSource.pitch = height / height / height;
            }
            // If pitch shift is disabled then set pitches to normal.
            else
            {
                var playerSounds = FindObjectOfType<PlayerAudioController>();
                var playerBreathing = FindObjectOfType<PlayerBreathingAudio>();
                playerSounds._oneShotSleepingAtCampfireSource.pitch = 1f;
                playerSounds._oneShotSource.pitch = 1f;
                playerBreathing._asphyxiationSource.pitch = 1f;
                playerBreathing._breathingLowOxygenSource.pitch = 1f;
                playerBreathing._breathingSource.pitch = 1f;
                playerBreathing._drowningSource.pitch = 1f;
            }
        }

        public static class Patches
        {
            public static void AttachPointOnStartLiftPlayer(GhostGrabController __instance)
            {
                // If hatchling is smol, then use raised attach point.
                __instance._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * height, 0.15f - 0.15f * length);
            }
        }
    }
}
