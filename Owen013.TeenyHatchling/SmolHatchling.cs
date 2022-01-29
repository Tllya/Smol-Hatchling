using OWML.ModHelper;
using OWML.Common;

namespace SmolHatchling
{
    public class SmolHatchling : ModBehaviour
    {
        // Configuration vars
        public static float tolness;            // This is a float
        public static float thiccness;          // This is also a float
        public static float chub;               // This is a float too!
        public static bool enableHighPitch;     // This is not a float :(

        private void Start()
        {
            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);

            // Create ghost grabbing patch for when player is picked up by Stranger.
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>
                ("OnStartLiftPlayer",
                typeof(Patches),
                nameof(Patches.AttachPointOnStartLiftPlayer));

            // Upon scene loading, set up right away!
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                ChangeSize();
            };
        }

        // This function is cool! It runs whenever the config is changed!
        public override void Configure(IModConfig config)   // This was a pain in the **** to get working
        {
            base.Configure(config);     // I don't even know what this bucko does
            enableHighPitch = config.GetSettingsValue<bool>("Change Pitch Depending on Tolness");
            tolness = config.GetSettingsValue<float>("Tolness Multiplier");
            thiccness = config.GetSettingsValue<float>("Thiccness Multiplier");     // Dummy thicc hatchling
            chub = config.GetSettingsValue<float>("Chub Multiplier");
            ChangeSize();
        }

        private void ChangeSize()   // Hearthian inflation O_O
        {
            // If the current scene isn't the Solar System or the Eye, don't do anything.
            if (LoadManager.s_currentScene != OWScene.SolarSystem
                    && LoadManager.s_currentScene != OWScene.EyeOfTheUniverse)
                    return;
            var playerBody = FindObjectOfType<PlayerBody>();    
            var playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2");     // I wanna see Traveller_HEA_Player v1
            var playerThruster = playerBody.transform.Find("PlayerVFX");
            var playerCamera = FindObjectOfType<PlayerCameraController>();
            var playerMarshmallowStick = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root");

            // Smolify/beegify/regularify playermodel, camera, thrusters, and marshmallow stick.
            playerModel.transform.localScale = new UnityEngine.Vector3(thiccness / 10, tolness / 10, chub / 10);
            playerThruster.transform.localScale = new UnityEngine.Vector3(thiccness, tolness, chub);
            playerThruster.transform.localPosition = new UnityEngine.Vector3(0f, -1 + tolness, 0f);
            playerCamera._origLocalPosition = new UnityEngine.Vector3(0f , -1f + 1.8496f * tolness , 0.15f * chub);
            playerMarshmallowStick.transform.localPosition =
                new UnityEngine.Vector3(0.25f, -1.8496f + 1.8496f * tolness, 0.08f - 0.15f + 0.15f * chub);
            //Cancel// playerMarshmallowStick.transform.localRotation = new UnityEngine.Quaternion(0f, 0.0872f, 0f, -0.9962f);

            // Offset attachments for pilot seat and ship log, but only if the scene is the Solar System!
            if (LoadManager.s_currentScene == OWScene.SolarSystem)
            {
                var shipCockpit = FindObjectOfType<ShipCockpitController>();
                var shipLog = FindObjectOfType<ShipLogController>();
                ModHelper.Events.Unity.FireInNUpdates(() =>
                    shipCockpit._origAttachPointLocalPos =
                        new UnityEngine.Vector3(0f, 2.1849f - 1.8496f * tolness, 4.2307f + 0.15f - 0.15f * chub), 60);
                shipLog._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * tolness, 0.15f - 0.15f * chub);
            }
            // If pitch shift is enabled, then crank that pitch.
            if (enableHighPitch)
            {
                var playerSounds = FindObjectOfType<PlayerAudioController>();
                var playerBreathing = FindObjectOfType<PlayerBreathingAudio>();
                playerSounds._oneShotSleepingAtCampfireSource.pitch = tolness / tolness / tolness;
                playerSounds._oneShotSource.pitch = tolness / tolness / tolness;
                playerBreathing._asphyxiationSource.pitch = tolness / tolness / tolness;
                playerBreathing._breathingLowOxygenSource.pitch = tolness / tolness / tolness;
                playerBreathing._breathingSource.pitch = tolness / tolness / tolness;
                playerBreathing._drowningSource.pitch = tolness / tolness / tolness;
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
                // Offset attachment so that camera is where it normally is
                __instance._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 1.8496f - 1.8496f * tolness, 0.15f - 0.15f * chub);
            }
        }
    }
}