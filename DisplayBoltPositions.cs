using HutongGames.PlayMaker;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace DisplayBoltPositions
{
    public class DisplayBoltPositions : Mod
    {
        public override string ID => "DisplayBoltPositions"; // Your (unique) mod ID 
        public override string Name => "Display Bolt Positions"; // Your mod name
        public override string Author => "g-otn"; // Name of the Author (your name)
        public override string Version => "1.0.0"; // Version
        public override string Description => ""; // Short description of your mod

        SettingsSlider sizeSlider;
        SettingsColorPicker colorPicker;
        SettingsCheckBox showDetachedCheckBox;

        private static readonly string DetachedPartSuffix = "(Clone)";
        private static string AttachedPartSuffix = "(xxxxx)";
        private static string PartTag = "PART";
        private static string BoltObjectName = "BoltPM";

        private int _raycastCheckFrameCount = 0;
        private static int RaycastCheckFrameInterval = 60; // How many frames to skip before running check\

        private IndicatorPool _indicatorPool = new IndicatorPool();

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("Behavior");
            showDetachedCheckBox = Settings.AddCheckBox("showDetached", "Display positions in detached parts", false);

            Settings.AddHeader("Style");
            sizeSlider = Settings.AddSlider("size", "Ping size", 2f, 50f, 5f, null, 0);
            colorPicker = Settings.AddColorPickerRGBA("color", "Ping color", new Color32(0, 200, 255, 255));
        }

        private void Mod_OnLoad()
        {
            _indicatorPool.Start();
            ModConsole.Print("[DisplayBoltPositions] Mod loaded");
        }

        private void Mod_Update()
        {
            //foreach (GameObject indicator in currentIndicators)
            //{
            //    float pulseValue = Mathf.Sin(Time.time * IndicatorPulseSpeed) * 0.5f + 0.5f;
            //    Color newColor = colorPicker.GetValue();
            //    newColor.a = pulseValue;

            //    Renderer sphereRenderer = indicator.GetComponent<Renderer>();
            //    sphereRenderer.material.SetColor("_Color", colorPicker.GetValue());
            //}

            // Frame skipping
            _raycastCheckFrameCount++;
            if (_raycastCheckFrameCount < RaycastCheckFrameInterval) return;
            _raycastCheckFrameCount = 0;

            RaycastHit? partHit = Get_Closest_Car_Part_RaycastHit();

            if (!partHit.HasValue || partHit.Value.collider == null)
            {
                _indicatorPool.Return_All_To_Pool();
                return;
            }

            Vector3[] boltPositions = Get_Bolts_Positions(partHit.Value.collider.gameObject);

            if (boltPositions.Length == 0)
            {
                _indicatorPool.Return_All_To_Pool();
                return;
            }
            ModConsole.Print("Bolts found:" + boltPositions.Length);

            _indicatorPool.Return_All_To_Pool();
            foreach (Vector3 p in boltPositions)
            {
                Create_Indicator(p);
            }

            // Update is called once per frame
            //new RaycastHit().collider.gameObject.GetComponents<PlayMakerFSM>("")
        }

        private bool isPartName(string name)
        {
            return name.EndsWith(DetachedPartSuffix) || name.EndsWith(AttachedPartSuffix);
        }

        private RaycastHit? Get_Closest_Car_Part_RaycastHit()
        {
            bool hasHitPartCollider = UnifiedRaycast.GetHitNames().Any(n => isPartName(n));


            if (!hasHitPartCollider)
            {
                return null;
            }

            RaycastHit[] partHits = UnifiedRaycast.GetRaycastHits().Where(h =>
            {
                GameObject obj = h.collider?.gameObject;
                if (obj == null) return false;
                bool isPart = isPartName(obj.name);
                bool isDetached = obj.CompareTag(PartTag);
                return isPart && (showDetachedCheckBox.GetValue() || !isDetached);
            }).ToArray();

            if (partHits.Length == 0)
            {
                ModConsole.Print("no parts hit");
                return null;
            }

            // Closest part
            Array.Sort(partHits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit partHit = partHits[0];

            ModConsole.Print($"Hit: {partHit.collider.gameObject.name}");
            return partHit;
        }
        Vector3[] Get_Bolts_Positions(GameObject part)
        {
            List<PlayMakerFSM> bolts = new List<PlayMakerFSM>();

            PlayMakerFSM[] immediateChildren = part.GetComponentsInChildren<PlayMakerFSM>();
            foreach (PlayMakerFSM immediateChild in immediateChildren)
            {
                if (immediateChild.name == BoltObjectName)
                    bolts.Add(immediateChild);
            }

            ModConsole.Print($"Immediate {part.name} child bolt found {immediateChildren.Length} - " + bolts.ToArray().Length);

            Transform childNamedBolts = part.transform.Find("Bolts");

            if (childNamedBolts != null)
            {
                PlayMakerFSM[] childNamedBoltsChildren = childNamedBolts.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
                ModConsole.Print($"childNamedBolts.childCount: " + childNamedBolts.childCount + " " + childNamedBoltsChildren.Length);
                foreach (PlayMakerFSM bolt in childNamedBoltsChildren)
                {

                    if (bolt.name == BoltObjectName)
                        bolts.Add(bolt);
                }
            }

            // Assumes every direct child is a bolt
            //PlayMakerFSM[] bolts = boltsTransform.GetComponentsInChildren<PlayMakerFSM>(true);



            return bolts.Select(b => b.transform.position).ToArray();
        }

        private void Create_Indicator(Vector3 position)
        {
            GameObject sphere = _indicatorPool.Get_Indicator();
            sphere.SetActive(true);

            Renderer sphereRenderer = sphere.GetComponent<Renderer>();
            sphereRenderer.material.SetColor(IndicatorShader.ColorProperty, colorPicker.GetValue());

            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * 0.01f * sizeSlider.GetValue();
        }
    }
}
