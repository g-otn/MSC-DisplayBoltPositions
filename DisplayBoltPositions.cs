using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DisplayBoltPositions
{
    public class DisplayBoltPositions : Mod
    {
        public override string ID => "DisplayBoltPositions"; // Your (unique) mod ID 
        public override string Name => "Display Bolt Positions"; // Your mod name
        public override string Author => "g-otn"; // Name of the Author (your name)
        public override string Version => "1.0.0"; // Version
        public override string Description => ""; // Short description of your mod


        SettingsCheckBox enabledCheckBox;

        SettingsCheckBox toolModeOnlyCheckBox;
        SettingsSlider updateIntervalSlider;
        SettingsCheckBox pulseCheckBox;

        SettingsSlider sizeSlider;
        SettingsColorPicker colorPicker;
        SettingsSlider pulseIntervalSlider;

        SettingsCheckBox showDetachedCheckBox;


        private static readonly string DetachedPartSuffix = "(Clone)";
        private static readonly string AttachedPartSuffix = "xxx)";
        private static readonly string PartTag = "PART";
        private static readonly string BoltObjectName = "BoltPM";
        private static readonly List<string> MiscPartNames = new List<string> { "wheel_regula" };


        private float _raycastCheckTimer = 0f;


        private IndicatorPool _indicatorPool = new IndicatorPool();

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }

        private void Mod_Settings()
        {
            Settings.AddHeader("General");
            enabledCheckBox = Settings.AddCheckBox("enabled", "Display indicators", true);
            Settings.AddText("Enables and disables the mod. When checked, opaque spheres will blink to indicate where bolts are positioned for the car part the player is looking at");


            Settings.AddHeader("Behavior");
            toolModeOnlyCheckBox = Settings.AddCheckBox("toolModeOnly", "Only display in tool mode (spanner)", true);
            showDetachedCheckBox = Settings.AddCheckBox("showDetached", "Display positions in detached parts", true);
            Settings.AddText("(does not work with every part)");


            Settings.AddHeader("Style");
            sizeSlider = Settings.AddSlider("size", "Indicator size", 1f, 30f, 4f, null, 1);
            colorPicker = Settings.AddColorPickerRGBA("color", "Indicator color", new Color32(0, 200, 255, 40));

            Settings.AddText("");
            pulseCheckBox = Settings.AddCheckBox("pulse", "Pulse animation", true, () => pulseIntervalSlider.SetVisibility(pulseCheckBox.GetValue()));
            pulseIntervalSlider = Settings.AddSlider("pulseInterval", "Pulse frequency", 1f, 20f, 7f, null, 0);

            Settings.AddHeader("Other");
            updateIntervalSlider = Settings.AddSlider("updateInterval", "Position update interval (milliseconds)", 5f, 1000f, 10f, null, 0);
            Settings.AddText("(low values may impact performance)");
        }

        private void Mod_OnLoad()
        {
            _indicatorPool.Start();
            ModConsole.Print("[DisplayBoltPositions] Mod loaded");
        }

        private void Mod_Update()
        {
            Pulse_Indicators();

            // Frame skipping every configured interval
            _raycastCheckTimer += Time.deltaTime;
            float interval = updateIntervalSlider.GetValue() / 1000;
            if (_raycastCheckTimer <= interval) return;
            _raycastCheckTimer = 0;


            // Required every early return, but also before placing active indicators
            // so we don't have to manage currently active indicator.
            // So it's cleaner to call it every time here
            _indicatorPool.Return_All_To_Pool();


            // Mod disabled in settings
            if (enabledCheckBox.GetValue() == false)
            {
                return;
            }

            // Disable outside of tool mode
            bool isInToolMode = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/2Spanner").activeInHierarchy;
            if (toolModeOnlyCheckBox.GetValue() && !isInToolMode)
            {
                return;
            }

            RaycastHit? partHit = Get_Closest_Car_Part_RaycastHit();

            // Not looking at part
            if (!partHit.HasValue || partHit.Value.collider == null)
            {
                return;
            }

            //ModConsole.Print($"partHit: ${partHit.Value.collider.gameObject.name}");

            Vector3[] boltPositions = Get_Bolts_Positions(partHit.Value.collider.gameObject);

            // Looking at part, but no bolts
            if (boltPositions.Length == 0)
            {
                return;
            }
            //ModConsole.Print("Bolts found:" + boltPositions.Length);

            // Bolts found, display indicators
            foreach (Vector3 p in boltPositions)
            {
                Display_Indicator(p);
            }
        }

        private void Pulse_Indicators()
        {
            List<GameObject> activeIndicators = _indicatorPool.Get_Active_Indicators();

            if (activeIndicators.Count == 0)
            {
                return;
            }

            Color color = colorPicker.GetValue();
            float frequency = pulseIntervalSlider.GetValue();
            float amplitude = 0.5f;
            float offset = 0.5f;
            float t = Mathf.Sin(Time.time * frequency);
            float newAlpha = Mathf.SmoothStep(0, color.a, t * amplitude + offset);
            color.a = newAlpha;

            foreach (GameObject indicator in _indicatorPool.Get_Active_Indicators())
            {

                Renderer sphereRenderer = indicator.GetComponent<Renderer>();
                sphereRenderer.material.SetColor(IndicatorShader.ColorProperty, color);
            }
        }

        private bool isPartName(string name)
        {
            return name.EndsWith(DetachedPartSuffix) || name.EndsWith(AttachedPartSuffix) || MiscPartNames.Contains(name);
        }

        private RaycastHit? Get_Closest_Car_Part_RaycastHit()
        {
            bool hasHitPartCollider = UnifiedRaycast.GetHitNames().Any(n => isPartName(n));

            //ModConsole.Print("hits:" + String.Join("->", 
            //    UnifiedRaycast.GetRaycastHits().Select((s) => 
            //    s.collider ? s.collider.gameObject.name + 
            //    $"({(s.collider.gameObject.name == BoltObjectName ? s.collider.transform.parent.name + ($"({s.collider.gameObject.transform.parent.parent.name})") : "(y)")})" : "(x)").ToArray()));

            if (!hasHitPartCollider)
            {
                return null;
            }

            RaycastHit[] partHits = UnifiedRaycast.GetRaycastHits().Where(h =>
            {
                GameObject obj = h.collider?.gameObject;
                if (obj == null) return false;
                bool hasPartTag = obj.CompareTag(PartTag);
                bool isDetached = hasPartTag;
                bool isPart = isPartName(obj.name);
                // When attached to the car, only the bolt is hit by the raycast.
                // Not ideal but at least we can use it
                bool isBolt = obj.name == BoltObjectName; 
                return (isPart && (showDetachedCheckBox.GetValue() || !isDetached)) || isBolt;
            }).ToArray();

            if (partHits.Length == 0)
            {
                //ModConsole.Print("no parts hit");
                return null;
            }

            // Closest part
            Array.Sort(partHits, (a, b) => a.distance.CompareTo(b.distance));
            RaycastHit partHit = partHits[0];

            //ModConsole.Print($"Hit: {partHit.collider.gameObject.name}");
            return partHit;
        }
        Vector3[] Get_Bolts_Positions(GameObject part)
        {
            List<PlayMakerFSM> bolts = new List<PlayMakerFSM>();

            bool isBolt = part.name == BoltObjectName;

            // Search parent if raycast hit is a bolt
            GameObject partToSearch = isBolt && part.transform.parent ? part.transform.parent.gameObject : part;

            PlayMakerFSM[] immediateChildren = partToSearch.GetComponentsInChildren<PlayMakerFSM>();
            foreach (PlayMakerFSM immediateChild in immediateChildren)
            {
                if (immediateChild.name == BoltObjectName)
                    bolts.Add(immediateChild);
            }

            //ModConsole.Print($"Immediate {part.name} child bolt found {immediateChildren.Length} - " + bolts.ToArray().Length);

            Transform childNamedBolts = partToSearch.transform.Find("Bolts");

            if (childNamedBolts != null)
            {
                PlayMakerFSM[] childNamedBoltsChildren = childNamedBolts.gameObject.GetComponentsInChildren<PlayMakerFSM>(true);
                //ModConsole.Print($"childNamedBolts.childCount: " + childNamedBolts.childCount + " " + childNamedBoltsChildren.Length);
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

        private void Display_Indicator(Vector3 position)
        {
            GameObject sphere = _indicatorPool.Get_Indicator();
            sphere.SetActive(true);

            if (!pulseCheckBox.GetValue())
            {
                Renderer sphereRenderer = sphere.GetComponent<Renderer>();
                sphereRenderer.material.SetColor(IndicatorShader.ColorProperty, colorPicker.GetValue());
            }

            sphere.transform.position = position;
            sphere.transform.localScale = (Vector3.one / 200) * sizeSlider.GetValue();
        }
    }
}
