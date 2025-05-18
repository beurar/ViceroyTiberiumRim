using System.Collections.Generic;
using System.Linq;
using TeleCore.FlowCore;
using TeleCore;
using Verse;
using TiberiumRim;
using System;
using UnityEngine;
namespace TiberiumRim
{
    public class Comp_TiberiumContainer : ThingComp
    {
        public FlowVolumeConfig<NetworkValueDef> VolumeConfig;
        private readonly Dictionary<NetworkValueDef, double> stored = new Dictionary<NetworkValueDef, double>();

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (props is CompProperties_TiberiumContainer typed)
            {
                VolumeConfig = typed.volumeConfig;
            }

            // Init all allowed values to zero
            foreach (var val in Allowed)
            {
                if (!stored.ContainsKey(val))
                    stored[val] = 0;
            }
        }

        public IEnumerable<NetworkValueDef> Allowed => VolumeConfig?.AllowedValues ?? Enumerable.Empty<NetworkValueDef>();
        public double Capacity => VolumeConfig?.capacity ?? 0;
        public bool InfiniteSource => VolumeConfig?.infiniteSource ?? false;
        public bool DropContents => VolumeConfig?.dropContents ?? false;

        // ──────────────── UTILITIES ────────────────

        public double Get(NetworkValueDef def)
        {
            return stored.TryGetValue(def, out var val) ? val : 0;
        }

        public double TotalStored => stored.Values.Sum();

        public double SpaceLeft => InfiniteSource ? double.PositiveInfinity : Math.Max(0, Capacity - TotalStored);

        public bool IsEmpty => TotalStored <= 0.0001;
        public bool IsFull => !InfiniteSource && TotalStored >= Capacity - 0.0001;

        public bool CanAccept(NetworkValueDef def, double amount = 1)
        {
            if (!Allowed.Contains(def)) return false;
            if (InfiniteSource) return true;
            return Get(def) + amount <= Capacity;
        }

        public bool TryAdd(NetworkValueDef def, double amount)
        {
            if (!CanAccept(def, amount)) return false;

            stored[def] = Get(def) + amount;
            return true;
        }

        public bool TryTake(NetworkValueDef def, double amount)
        {
            double available = Get(def);
            if (available < amount) return false;

            stored[def] = available - amount;
            return true;
        }

        public float StoredPercent
        {
            get
            {
                if (InfiniteSource) return 1f;
                if (Capacity <= 0.001f) return 0f;
                return Mathf.Clamp01((float)(TotalStored / Capacity));
            }
        }

        public Color DominantColor
        {
            get
            {
                if (IsEmpty) return Color.black;

                float total = (float)TotalStored;
                float r = 0, g = 0, b = 0;

                foreach (var kv in stored)
                {
                    var valueDef = kv.Key;
                    float amount = (float)kv.Value;

                    if (amount <= 0f) continue;

                    Color color = valueDef.valueColor;
                    float weight = amount / total;

                    r += color.r * weight;
                    g += color.g * weight;
                    b += color.b * weight;
                }

                return new Color(r, g, b);
            }
        }

        public IEnumerable<Thing> GetThingsToDropOnRupture()
        {
            List<Thing> result = new List<Thing>();

            foreach (var kv in StoredValues)
            {
                var valueDef = kv.Key;
                var amount = kv.Value;

                if (amount <= 0) continue;
                if (valueDef.ThingDroppedFromContainer == null) continue;

                float ratio = Mathf.Max(0.0001f, valueDef.ValueToThingRatio);
                int count = Mathf.FloorToInt((float)(amount / ratio));
                if (count <= 0) continue;

                ThingDef dropDef = valueDef.ThingDroppedFromContainer;
                Thing drop = ThingMaker.MakeThing(dropDef);
                drop.stackCount = count;

                result.Add(drop);
            }

            return result;
        }

        public IEnumerable<Gizmo> GetGizmos()
        {
            if (Capacity > 0)
            {
                if (Find.Selector.NumSelected == 1 && Find.Selector.IsSelected(parent))
                {
                    yield return new Gizmo_TiberiumContainer
                    {
                        container = this
                    };
                }

                if (Prefs.DevMode && DebugSettings.godMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEBUG: Container Options",
                        icon = TiberiumContent.ContainMode_TripleSwitch, // replace with any dev icon
                        action = delegate
                        {
                            List<FloatMenuOption> list = new List<FloatMenuOption>();

                            foreach (var def in Allowed)
                            {
                                var localDef = def;
                                list.Add(new FloatMenuOption($"Add {localDef.label.CapitalizeFirst()}", () =>
                                {
                                    TryAdd(localDef, 500);
                                }));
                            }

                            list.Add(new FloatMenuOption("Clear Container", () =>
                            {
                                foreach (var def in Allowed.ToList())
                                {
                                    TryTake(def, Get(def)); // Take all
                                }
                            }));

                            FloatMenu menu = new FloatMenu(list)
                            {
                                vanishIfMouseDistant = true
                            };
                            Find.WindowStack.Add(menu);
                        }
                    };
                }
            }
        }

        internal Comp_TiberiumContainer MakeCopy(PortableContainer target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var copy = new Comp_TiberiumContainer
            {
                parent = target,
                VolumeConfig = this.VolumeConfig
            };

            // Manually initialize stored dictionary
            foreach (var def in Allowed)
            {
                double amount = Get(def);
                if (amount > 0)
                    copy.stored[def] = amount;
            }

            return copy;
        }


        public IEnumerable<KeyValuePair<NetworkValueDef, double>> StoredValues => stored;
    }


    public class CompProperties_TiberiumContainer : CompProperties
    {
        public FlowVolumeConfig<NetworkValueDef> volumeConfig;

        public CompProperties_TiberiumContainer()
        {
            this.compClass = typeof(Comp_TiberiumContainer);
        }
    }

    public class Gizmo_TiberiumContainer : Gizmo
    {

        private static bool optionToggled = false;
        public Comp_TiberiumContainer container;

        public Gizmo_TiberiumContainer()
        {
            this.Order = -200f;
        }

        public override float GetWidth(float maxWidth)
        {
            return optionToggled ? 310 : 150f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect MainRect = new Rect(topLeft.x, topLeft.y, 150, 75f);
            Find.WindowStack.ImmediateWindow(145356798, MainRect, WindowLayer.GameUI, delegate
            {
                Rect rect = MainRect.AtZero().ContractedBy(5f);
                Rect optionRect = new Rect(rect.xMax - 15, rect.y, 15, 15);
                bool mouseOver = Mouse.IsOver(rect);
                GUI.color = mouseOver ? Color.cyan : Color.white;
                Widgets.DrawTextureFitted(optionRect, TexButton.Info, 1f);
                GUI.color = Color.white;

                if (Widgets.ButtonInvisible(rect))
                    optionToggled = !optionToggled;

                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect, "TR_ContainerContent".Translate());

                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, Math.Round(container.TotalStored, 0) + "/" + container.Capacity);
                Text.Anchor = 0;

                Rect rect2 = rect.BottomHalf();
                Rect rect3 = rect2.BottomHalf();
                GUI.BeginGroup(rect3);
                Rect BGRect = new Rect(0, 0, rect3.width, rect3.height);
                Rect BarRect = BGRect.ContractedBy(2.5f);
                float xPos = 0f;

                Widgets.DrawBoxSolid(BGRect, new Color(0.05f, 0.05f, 0.05f));
                Widgets.DrawBoxSolid(BarRect, new Color(0.25f, 0.25f, 0.25f));

                foreach (var kv in container.StoredValues)
                {
                    NetworkValueDef def = kv.Key;
                    double value = kv.Value;
                    if (value <= 0) continue;

                    float percent = (float)(value / container.Capacity);
                    float width = BarRect.width * percent;
                    Rect typeRect = new Rect(2.5f + xPos, BarRect.y, width, BarRect.height);
                    xPos += width;

                    Color color = def.valueColor;
                    Widgets.DrawBoxSolid(typeRect, color);
                }

                GUI.EndGroup();

                if (optionToggled)
                {
                    Rect Main2 = new Rect(topLeft.x + 160, topLeft.y, 150, 75f);
                    DrawOptions(Main2);
                }
            }, true, false, 1f);

            return new GizmoResult(GizmoState.Clear);
        }

        public void DrawOptions(Rect inRect)
        {
            Find.WindowStack.ImmediateWindow(1453564358, inRect, WindowLayer.GameUI, delegate
            {
                // Optional: Add more diagnostics or toggle controls here
            }, true, false, 1f);
        }
    }
}