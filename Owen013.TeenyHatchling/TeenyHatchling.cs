using OWML.ModHelper;
using OWML.Common;
using Harmony;



namespace TeenyHatchling
{
    public class TeenyHatchling : ModBehaviour
    {
        public static bool enableTeenyHatchling;

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"{nameof(TeenyHatchling)} is ready to go!", MessageType.Success);
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>
                ("OnStartLiftPlayer",
                typeof(Patches),
                nameof(Patches.AttachPointOnStartLiftPlayer));

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                Setup();
            };
        }

        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            enableTeenyHatchling = config.GetSettingsValue<bool>("Enable Smol Hatchling");
            Setup();
        }

        private void Setup()
        {
            if (LoadManager.s_currentScene != OWScene.SolarSystem
                    && LoadManager.s_currentScene != OWScene.EyeOfTheUniverse)
                    return;

            var playerBody = FindObjectOfType<PlayerBody>();
            var playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2");
            var thrusterEffect = playerBody.transform.Find("PlayerVFX");
            var playerCamera = FindObjectOfType<PlayerCameraController>();
            var stickRoot = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root");

            // ENABLING TeenyHatchling
            if (enableTeenyHatchling)
            {
                playerModel.transform.localScale = new UnityEngine.Vector3(0.1f, 0.05f, 0.1f);
                thrusterEffect.transform.localScale = new UnityEngine.Vector3(1f, 0.5f, 1f);
                thrusterEffect.transform.localPosition = new UnityEngine.Vector3(0f, -0.5f, 0f);
                playerCamera._origLocalPosition = new UnityEngine.Vector3(0f, 0.1f, 0.15f);
                stickRoot.transform.localPosition = new UnityEngine.Vector3(0.25f, -0.75f, 0.08f);
                stickRoot.transform.localRotation = new UnityEngine.Quaternion(-0.0868f, -0.0868f, -0.0076f, 0.9924f);
                if (LoadManager.s_currentScene != OWScene.SolarSystem) return;
                var cockpitController = FindObjectOfType<ShipCockpitController>();
                var logController = FindObjectOfType<ShipLogController>();
                ModHelper.Events.Unity.FireInNUpdates(() =>
                    cockpitController._origAttachPointLocalPos = new UnityEngine.Vector3(0f, 1.1f, 4.2307f) ,
                    60);
                logController._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 0.75f, 0f);
                }
            // DISABLING TeenyHatchling
            else
                {
                playerModel.transform.localScale = new UnityEngine.Vector3(0.1f, 0.1f, 0.1f);
                thrusterEffect.transform.localScale = new UnityEngine.Vector3(1f, 1f, 1f);
                thrusterEffect.transform.localPosition = new UnityEngine.Vector3(0f, 0f, 0f);
                playerCamera._origLocalPosition = new UnityEngine.Vector3(0f, 0.8496f, 0.15f);
                stickRoot.transform.localPosition = new UnityEngine.Vector3(0.25f, 0f, 0.08f);
                stickRoot.transform.localRotation = new UnityEngine.Quaternion(0f, 0.0872f, 0f, -0.9962f);
                if (LoadManager.s_currentScene != OWScene.SolarSystem) return;
                var cockpitController = FindObjectOfType<ShipCockpitController>();
                var logController = FindObjectOfType<ShipLogController>();
                ModHelper.Events.Unity.FireInNUpdates(() =>
                    cockpitController._origAttachPointLocalPos = new UnityEngine.Vector3(0f, 0.3353f, 4.2307f) ,
                    60);
                logController._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 0f, 0f);
            }
        }
        public static class Patches
        {
            public static void AttachPointOnStartLiftPlayer(GhostGrabController __instance)
            {
                if (enableTeenyHatchling)
                {
                    __instance._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 0.75f, 0);
                }
                else
                {
                    __instance._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 0f, 0);
                }
            }
        }
    }
}
