using UnityEngine;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using OWML.Utils;
using OWML.Common;

namespace SmolHatchling
{
    public class SmolHatchling : ModBehaviour
    {
        // Config vars
        public float height, radius;
        public bool enableHighPitch;

        // Mod vars
        public static SmolHatchling Instance;
        public OWScene scene;
        public static Vector3 playerScale;
        public PlayerBody playerBody;
        public PlayerCameraController playerCamera;
        public PlayerAudioController audioController;
        public static PlayerBreathingAudio breathingAudio;
        public CapsuleCollider playerCollider;
        public ShipCockpitController cockpitController;
        public ShipLogController logController;
        public PlayerCloneController cloneController;
        public EyeMirrorController mirrorController;
        public GameObject playerModel, playerThruster, playerMarshmallowStick;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Create patch for when character starts
            ModHelper.HarmonyHelper.AddPostfix<PlayerCharacterController>(
                "Start",
                typeof(Patches),
                nameof(Patches.CharacterStart));
            // Create ghost grabbing patch for when player is picked up by Stranger.
            ModHelper.HarmonyHelper.AddPostfix<GhostGrabController>(
                "OnStartLiftPlayer",
                typeof(Patches),
                nameof(Patches.GhostLiftedPlayer));
            // Create patch for when clone starts
            ModHelper.HarmonyHelper.AddPostfix<PlayerCloneController>(
                "Start",
                typeof(Patches),
                nameof(Patches.EyeCloneStart));
            // Create patch for when mirror image starts
            ModHelper.HarmonyHelper.AddPostfix<EyeMirrorController>(
                "Start",
                typeof(Patches),
                nameof(Patches.EyeMirrorStart));
            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);
        }

        // Runs whenever the config is changed
        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            height = config.GetSettingsValue<float>("Height (Default 1)");
            radius = config.GetSettingsValue<float>("Radius (Default 1)");
            enableHighPitch = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            Setup();
        }

        public void Setup()
        {
            scene = LoadManager.s_currentScene;
            if (scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse)
            {
                playerScale = new Vector3(radius, height, radius);
                playerBody = FindObjectOfType<PlayerBody>();
                playerCamera = FindObjectOfType<PlayerCameraController>();
                playerCollider = playerBody.GetComponent<CapsuleCollider>();
                playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2").gameObject;
                playerThruster = playerBody.transform.Find("PlayerVFX").gameObject;
                playerMarshmallowStick = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root").gameObject;
                audioController = FindObjectOfType<PlayerAudioController>();
                breathingAudio = FindObjectOfType<PlayerBreathingAudio>();
                if (scene == OWScene.SolarSystem)
                {
                    cockpitController = FindObjectOfType<ShipCockpitController>();
                    logController = FindObjectOfType<ShipLogController>();
                }
                else if (scene == OWScene.EyeOfTheUniverse)
                {
                    var cloneControllers = Resources.FindObjectsOfTypeAll<PlayerCloneController>();
                    cloneController = cloneControllers[0];
                    var mirrorControllers = Resources.FindObjectsOfTypeAll<EyeMirrorController>();
                    mirrorController = mirrorControllers[0];
                }
                ChangeSize();
            }
        }

        public void ChangeSize()
        {
            // Resize collider
            playerCollider.height = 2f * playerScale.y;
            float targetRadius = (playerScale.x + playerScale.z) / 2f * 0.5f;
            playerCollider.radius = Mathf.Min(playerCollider.height / 2, targetRadius);
            playerCollider.center = new Vector3(0f, playerScale.y - 1f, 0f);
            ModHelper.Console.WriteLine($"Height: {playerCollider.height} \n Radius: {playerCollider.radius}");
            // Smolify/beegify/regularify playermodel, camera, thrusters, and marshmallow stick.
            playerModel.transform.localScale = playerScale / 10f;
            playerThruster.transform.localScale = playerScale;
            playerThruster.transform.localPosition = new Vector3(0f, -1f + playerScale.y, 0f);
            playerCamera._origLocalPosition = new Vector3(0f, -1f + 1.8496f * playerScale.y, 0.15f * playerScale.z);
            playerMarshmallowStick.transform.localPosition =
                new Vector3(0.25f, -1.8496f + 1.8496f * playerScale.y, 0.08f - 0.15f + 0.15f * playerScale.z);
            // If pitch shift is enabled, then crank that pitch.
            if (enableHighPitch)
            {
                float pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
                audioController._oneShotSleepingAtCampfireSource.pitch = pitch;
                audioController._oneShotSource.pitch = pitch;
                breathingAudio._asphyxiationSource.pitch = pitch;
                breathingAudio._breathingLowOxygenSource.pitch = pitch;
                breathingAudio._breathingSource.pitch = pitch;
                breathingAudio._drowningSource.pitch = pitch;
            }
            // If pitch shift is disabled then set pitches to normal.
            else
            {
                audioController._oneShotSleepingAtCampfireSource.pitch = 1f;
                audioController._oneShotSource.pitch = 1f;
                breathingAudio._asphyxiationSource.pitch = 1f;
                breathingAudio._breathingLowOxygenSource.pitch = 1f;
                breathingAudio._breathingSource.pitch = 1f;
                breathingAudio._drowningSource.pitch = 1f;
            }
            // Offset attachments for pilot seat and ship log, but only if the scene is the Solar System!
            if (scene == OWScene.SolarSystem)
            {
                cockpitController._origAttachPointLocalPos =
                    new Vector3(0f, 2.1849f - 1.8496f * playerScale.y, 4.2307f + 0.15f - 0.15f * playerScale.z);
                logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
            }
            else if (scene == OWScene.EyeOfTheUniverse)
            {
                cloneController._playerVisuals.transform.localScale = playerScale / 10f;
                cloneController._signal._owAudioSource.pitch = breathingAudio._oneShotSource.pitch;
                mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10f;
            }
        }
    }

    public static class Patches
    {
        public static void CharacterStart(PlayerCharacterController __instance)
        {
            SmolHatchling.Instance.Setup();
        }

        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset =
                new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
        }

        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10f;
            if (SmolHatchling.Instance.enableHighPitch) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1f;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10f;
        }
    }
}