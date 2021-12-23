﻿using OWML.ModHelper;
using OWML.Common;

namespace TeenyHatchling
{
    public class TeenyHatchling : ModBehaviour
    {
        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"{nameof(TeenyHatchling)} is loaded!", MessageType.Success);
            
            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                var playerModel = playerBody.transform.Find("Traveller_HEA_Player_v2");
                var playerCamera = FindObjectOfType<PlayerCameraController>();
                var stickRoot = playerBody.transform.Find("RoastingSystem").transform.Find("Stick_Root");
                var cockpitController = FindObjectOfType<ShipCockpitController>();
                var logController = FindObjectOfType<ShipLogController>();
                playerModel.transform.localScale = new UnityEngine.Vector3(0.1f, 0.05f, 0.1f);
                playerCamera._origLocalPosition = new UnityEngine.Vector3(0f, 0.1f, 0.15f);
                stickRoot.transform.localPosition = new UnityEngine.Vector3(0.25f, -0.75f, 0.08f);
                stickRoot.transform.localRotation = new UnityEngine.Quaternion(-0.0868f, -0.0868f, -0.0076f, 0.9924f);
                ModHelper.Events.Unity.FireInNUpdates(() => cockpitController._origAttachPointLocalPos = new UnityEngine.Vector3(0f, 1.1f, 4.2307f), 60);
                logController._attachPoint._attachOffset = new UnityEngine.Vector3(0f, 0.75f, 0f);
                ModHelper.Console.WriteLine($"All done!", MessageType.Success);
            };
        }
    }
}
