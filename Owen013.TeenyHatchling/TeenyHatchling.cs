using OWML.ModHelper;
using OWML.Common;
using Harmony;

namespace TeenyHatchling
{
    public class TeenyHatchling : ModBehaviour
    {
        public static bool enableHighPitch;
        public static float sizeMultiplier;

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
            enableHighPitch = config.GetSettingsValue<bool>("High Pitched SFX when Small");
            sizeMultiplier = config.GetSettingsValue<float>("Size Multiplier");
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
            playerModel.transform.localScale = new UnityEngine.Vector3(0.1f, sizeMultiplier / 10, 0.1f);
            playerThruster.transform.localScale = new UnityEngine.Vector3(1f, sizeMultiplier, 1f);
            playerThruster.transform.localPosition = new UnityEngine.Vector3(0f, -1 + sizeMultiplier, 0f);
            playerCamera._origLocalPosition = new UnityEngine.Vector3(0f, -1f + 1.8496f * sizeMultiplier, 0.15f);
            playerMarshmallowStick.transform.localPosition = new UnityEngine.Vector3(0.25f, -1.8496f + 1.8496f * sizeMultiplier, 0.08f);
            // playerMarshmallowStick.transform.localRotation = new UnityEngine.Quaternion(0f, 0.0872f, 0f, -0.9962f);
            // Raise ship attach points, but only if the scene is the Solar System.
            if (LoadManager.s_currentScene == OWScene.SolarSystem)
            {
                var shipCockpit = FindObjectOfType<ShipCockpitController>();
                var shipLog = FindObjectOfType<ShipLogController>();
                ModHelper.Events.Unity.FireInNUpdates(() =>
                    shipCockpit._origAttachPointLocalPos = new UnityEngine.Vector3(0f, 2.1849f - 1.8496f * sizeMultiplier, 4.2307f),
                    60);
                shipLog._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * sizeMultiplier, 0f);
            }
            // If pitch shift is enabled then turn them on.
            if (enableHighPitch)
            {
                var playerSounds = FindObjectOfType<PlayerAudioController>();
                var playerBreathing = FindObjectOfType<PlayerBreathingAudio>();
                playerSounds._oneShotSleepingAtCampfireSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
                playerSounds._oneShotSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
                playerBreathing._asphyxiationSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
                playerBreathing._breathingLowOxygenSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
                playerBreathing._breathingSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
                playerBreathing._drowningSource.pitch = sizeMultiplier / sizeMultiplier / sizeMultiplier;
            }
            // If pitch shift is enabled then turn them off just in case they're already on.
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
                __instance._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * sizeMultiplier, 0);
            }
        }
    }
}
