using System.Xml;
using UnityEngine;
using OWML.ModHelper;
using OWML.Common;

namespace SmolHatchling
{
    public class SmolHatchlingAPI
    {
        public float GetAnimSpeed()
        {
            return SmolHatchling.animSpeed;
        }
    }

    public class SmolHatchling : ModBehaviour
    {
        // Config vars
        float height, radius;
        public static float animSpeed;
        public bool enableHighPitch, enableStory;

        // Mod vars
        public static SmolHatchling Instance;
        OWScene scene;
        public static Vector3 playerScale;
        PlayerBody playerBody;
        PlayerCameraController cameraController;
        PlayerAnimController animController;
        PlayerAudioController audioController;
        static PlayerBreathingAudio breathingAudio;
        CapsuleCollider playerCollider;
        ShipCockpitController cockpitController;
        ShipLogController logController;
        PlayerCloneController cloneController;
        EyeMirrorController mirrorController;
        GameObject playerModel, playerThruster, playerMarshmallowStick;
        AssetBundle assets;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Load assets
            assets = ModHelper.Assets.LoadBundle("Assets/textassets");
            // Create patch for when character starts
            ModHelper.HarmonyHelper.AddPostfix<PlayerCharacterController>(
                "Start",
                typeof(Patches),
                nameof(Patches.CharacterStart));
            ModHelper.Console.WriteLine($"{nameof(SmolHatchling)} is ready to go!", MessageType.Success);
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

        public override object GetApi()
        {
            return new SmolHatchlingAPI();
        }

        // Runs whenever the config is changed
        public override void Configure(IModConfig config)
        {
            base.Configure(config);
            height = config.GetSettingsValue<float>("Height (Default 1)");
            radius = config.GetSettingsValue<float>("Radius (Default 1)");
            enableHighPitch = config.GetSettingsValue<bool>("Change Pitch Depending on Height");
            enableStory = config.GetSettingsValue<bool>("Enable Story");
            Setup();
        }

        public void Setup()
        {
            scene = LoadManager.s_currentScene;
            if (scene == OWScene.SolarSystem || scene == OWScene.EyeOfTheUniverse)
            {
                if (enableStory)
                {
                    playerScale = new Vector3(0.75f, 0.5f, 0.75f);
                    ModHelper.Events.Unity.FireInNUpdates(() => StorySetup(), 60);
                }
                else playerScale = new Vector3(radius, height, radius);
                animSpeed = Mathf.Pow(playerScale.z, -1);
                playerBody = FindObjectOfType<PlayerBody>();
                cameraController = FindObjectOfType<PlayerCameraController>();
                animController = FindObjectOfType<PlayerAnimController>();
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
            playerCollider.height = 2 * playerScale.y;
            float targetRadius = (playerScale.x + playerScale.z) / 2 * 0.5f;
            playerCollider.radius = Mathf.Min(playerCollider.height / 2, targetRadius);
            playerCollider.center = new Vector3(0, playerScale.y - 1, 0);
            ModHelper.Console.WriteLine($"Height: {playerCollider.height} \n Radius: {playerCollider.radius}");
            // Change speed
            animController._animator.speed = animSpeed;
            // Smolify/beegify/regularify playermodel, camera, thrusters, and marshmallow stick.
            playerModel.transform.localScale = playerScale / 10;
            playerThruster.transform.localScale = playerScale;
            playerThruster.transform.localPosition = new Vector3(0f, -1 + playerScale.y, 0);
            cameraController._origLocalPosition = new Vector3(0f, -1 + 1.8496f * playerScale.y, 0.15f * playerScale.z);
            playerMarshmallowStick.transform.localPosition =
                new Vector3(0.25f, -1.8496f + 1.8496f * playerScale.y, 0.08f - 0.15f + 0.15f * playerScale.z);
            // If pitch shift is enabled, then crank that pitch.
            if (enableHighPitch)
            {
                float pitch = 0.5f * Mathf.Pow(playerScale.y, -1) + 0.5f;
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
                audioController._oneShotSleepingAtCampfireSource.pitch = 1;
                audioController._oneShotSource.pitch = 1;
                breathingAudio._asphyxiationSource.pitch = 1;
                breathingAudio._breathingLowOxygenSource.pitch = 1;
                breathingAudio._breathingSource.pitch = 1;
                breathingAudio._drowningSource.pitch = 1;
            }
            // Offset attachments for pilot seat and ship log, but only if the scene is the Solar System!
            if (scene == OWScene.SolarSystem)
            {
                cockpitController._origAttachPointLocalPos =
                    new Vector3(0, 2.1849f - 1.8496f * playerScale.y, 4.2307f + 0.15f - 0.15f * playerScale.z);
                logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
            }
            else if (scene == OWScene.EyeOfTheUniverse)
            {
                cloneController._playerVisuals.transform.localScale = playerScale / 10;
                cloneController._signal._owAudioSource.pitch = breathingAudio._oneShotSource.pitch;
                mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
            }
        }

        public void StorySetup()
        {
            var dialogueTrees = FindObjectsOfType<CharacterDialogueTree>();
            for (var i = 0; i < dialogueTrees.Length; ++i)
            {
                CharacterDialogueTree dialogueTree = dialogueTrees[i];
                string dialogueName = dialogueTree._characterName;
                TextAsset textAsset = assets.LoadAsset<TextAsset>(dialogueName);
                if (!assets.Contains(dialogueName)) continue;
                dialogueTree.SetTextXml(textAsset);
                AddTranslations(textAsset.ToString());
                dialogueTree.OnDialogueConditionsReset();
                ModHelper.Console.WriteLine($"Replaced dialogue for {dialogueName}");
            }
        }

        private void AddTranslations(string xml)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("DialogueTree");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("DialogueNode");
            string NameField = xmlNode.SelectSingleNode("NameField").InnerText;
            var translationTable = TextTranslation.Get().m_table.theTable;
            translationTable[NameField] = NameField;
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                var name = xmlNode2.SelectSingleNode("Name").InnerText;

                XmlNodeList xmlText = xmlNode2.SelectNodes("Dialogue/Page");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[name + pageData.InnerText] = pageData.InnerText;
                }
                xmlText = xmlNode2.SelectNodes("DialogueOptionsList/DialogueOption/Text");
                foreach (object Page in xmlText)
                {
                    XmlNode pageData = (XmlNode)Page;
                    translationTable[NameField + name + pageData.InnerText] = pageData.InnerText;

                }
            }
        }
    }

    public static class Patches
    {
        public static void CharacterStart()
        {
            SmolHatchling.Instance.Setup();
        }

        public static void GhostLiftedPlayer(GhostGrabController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            // Offset attachment so that camera is where it normally is
            __instance._attachPoint._attachOffset =
                new Vector3(0, 1.8496f - 1.8496f * playerScale.y, 0.15f - 0.15f * playerScale.z);
        }

        public static void EyeCloneStart(PlayerCloneController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            float pitch;
            __instance._playerVisuals.transform.localScale = playerScale / 10;
            if (SmolHatchling.Instance.enableHighPitch) pitch = 0.5f * Mathf.Pow(playerScale.y, -1f) + 0.5f;
            else pitch = 1;
            __instance._signal._owAudioSource.pitch = pitch;
        }

        public static void EyeMirrorStart(EyeMirrorController __instance)
        {
            Vector3 playerScale = SmolHatchling.playerScale;
            __instance._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = playerScale / 10;
        }
    }
}