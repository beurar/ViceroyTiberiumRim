﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Collections;
using TeleCore;

namespace TiberiumRim
{
    public class TNW_Pipe : FXBuilding
    {
        public override Graphic Graphic => TiberiumContent.TiberiumNetworkPipes;
        public CompNetwork CompTNW => this.TryGetComp<CompNetwork>();

        public new void DynamicDrawPhase(DrawPhase drawPhase)
        {
            base.DynamicDrawPhase(drawPhase);
            if (CompTNW != null)
            {
                Color color = Color.magenta ;
                TiberiumContent.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(this.DrawPos + Altitudes.AltIncVect, Rotation, this);
            }
        }
    }

    /*
    public class TNW_PipeDynamic : FXBuilding, IContainerHolder
    {
        public TiberiumContainer Container;

        public List<TNW_PipeDynamic> Neighbors =  new List<TNW_PipeDynamic>() { null, null, null, null };

        public float transferVal = 0.125f;

        //private double containedValue = 0;
        //private double maxValue = 100;

        private float cachedPercent = 0;
        private bool debugProduce = false;

        private float smallest = 999;
        private float biggest = 0;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            Container = new TiberiumContainer(1, new List<TiberiumValueType>() {TiberiumValueType.Green}, this, this);
            ConnectToNeighbors();
        }

        private void ConnectToNeighbors()
        {
            for (int i = 0; i < 4; i++)
            {
                if(!Neighbors[i].DestroyedOrNull()) continue;
                var b = (GenAdj.CardinalDirections[i] + Position).GetFirstBuilding(Map);
                if (b is TNW_PipeDynamic t)
                {
                    Neighbors[i] = t;
                    t.ConnectToNeighbors();
                }
            }
        }


        public override void Tick()
        {
            base.Tick();
            if (debugProduce)
            {
                Container.TryAddValue(TiberiumValueType.Green, 1, out float actualValue);
                cachedPercent = Container.StoredPercent;
                
                containedValue += 100;
                if (containedValue > maxValue)
                    containedValue = maxValue;
                cachedPercent = containedValue / maxValue;
                
            }

            if (Container.TotalStorage >= 0.25f)
            {
                for (int i = 0; i < 4; i++)
                {
                    var neighbor = Neighbors[i];
                    if (neighbor != null && neighbor.WantsFrom(this))
                    {
                        Give(ThroughPut(neighbor), neighbor);
                    }
                }
            }
        }

        private float ThroughPut(TNW_PipeDynamic other)
        {
            var weight = (Container.TotalStorage - other.Container.TotalStorage) / 2f;
            if (weight < smallest)
                smallest = weight;
            if (weight > biggest)
                biggest = weight;
            return weight >= transferVal ? weight : 0;
            
            var wantingNeighbors = Neighbors.Where(n => n != null && n.WantsFrom(this));
            if (!wantingNeighbors.Any()) return 0f;

            var half = containedValue / 2;
            var 
            var weight = containedValue - wantingNeighbors.

            var weight = (Container.TotalStorage - wantingNeighbors.Sum(n => n.Container.TotalStorage)) / (float)(Math.Pow(2, wantingNeighbors.Count()));

            if (weight < smallest)
                smallest = weight;
            if (weight > biggest)
                biggest = weight;

            return weight;
            
        }

        public void Give(float weight, TNW_PipeDynamic to)
        {
            to.Receive(weight, this);
            cachedPercent = Container.StoredPercent;
        }

        public void Receive(float weight, TNW_PipeDynamic from)
        {
            
            from.Container.TryTransferTo(this.Container, TiberiumValueType.Green, weight);
            cachedPercent = Container.StoredPercent;
        }

        private bool WantsFrom(TNW_PipeDynamic other)
        {
            return cachedPercent < other.cachedPercent;
        }

        public override string GetInspectString()
        {
            string str = "Smallest ThroughPut: " + smallest + "\n" + "Biggest Throughput: " + biggest;
            return str;
            return base.GetInspectString();
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                Vector3 v2 = v + new Vector3(0, 15f, 0);
                Vector3 v3 = v - new Vector3(0, 15f, 0);
                GenMapUI.DrawThingLabel(v3, Container.TotalStorage.ToString(), Color.green);
                GenMapUI.DrawThingLabel(v, cachedPercent.ToStringPercent(), Color.white);
                GenMapUI.DrawThingLabel(v2, Container.StoredPercent.ToString() + "p", Color.yellow);
            }
        }

        public override void Draw()
        {
            Color color = Container.Color;
            TiberiumContent.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(DrawPos + Altitudes.AltIncVect, Rotation, this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Switch Produce",
                    action = delegate
                    {
                        debugProduce = !debugProduce;
                    }
                };
            }
        }

        public void Notify_ContainerFull()
        { }
    }
    */
    /*
    public class TNW_Pipe2 : FXBuilding, IContainerHolder
    {
        public TiberiumNetworkBuilding DirectParent;
        public IntVec3 ParentCell = IntVec3.Invalid;
        public int transferAmount = 6;
        public PipeMode pipeMode = PipeMode.Flow;

        public TiberiumContainer Container;

        //public FluidBox FluidBox;

        public TNW_Pipe from;
        private Rot4 leakDirection = Rot4.Invalid;
        private bool leaking = false;

        //Debug
        private bool debugProduce = false;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            AdaptToAdjacent();
            UpdatePipeMode();
            ConnectedPipes.ForEach(p => p.UpdatePipeMode());
        }

        public void AdaptToAdjacent()
        {
            NetworkMode nMode;
            StoreMode sMode;
            if (!DirectParent.DestroyedOrNull())
            {
                if (StructureSet.Pipes.Any())
                {
                    nMode = StructureSet.Pipes.Select(p => p.Network.NetworkMode).FirstOrDefault();
                    sMode = StructureSet.Pipes.Select(p => p.Container.mode).FirstOrDefault();
                }
                else
                {
                    nMode = DirectParent.Network.NetworkMode;
                    sMode = DirectParent.Container.mode;
                }
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!DirectParent.DestroyedOrNull())
            {
                if (DirectParent.def.IOMode != TNWBIOMode.Static)
                {
                    DirectParent.InputCells.Remove(Position);
                    DirectParent.OutputCells.Remove(Position);
                }
            }
            base.Destroy(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (!leaking)
            {
                float probability = 0.15f;
                probability *= Mathf.Lerp(1f, 0f, 1f * dinfo.ArmorPenetrationInt);
                if (HitPoints < ((float)MaxHitPoints * 0.75f) && TRUtils.Chance(0.15f))
                {
                    leaking = true;
                    leakDirection = Rot4.FromAngleFlat(dinfo.Angle);
                }
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public void UpdatePipeMode()
        {
            if (!DirectParent.DestroyedOrNull())
            {
                TNWBIOMode mode = DirectParent.def.IOMode;
                switch (mode)
                {
                    case TNWBIOMode.Static:
                        {
                            if (DirectParent.InputCells.Contains(Position))
                            {
                                pipeMode = PipeMode.Input;
                            }
                            else if (DirectParent.OutputCells.Contains(Position))
                            {
                                pipeMode = PipeMode.Output;
                            }
                            return;
                        }
                    case TNWBIOMode.Dynamic:
                        {
                            if (DirectParent.InputCells.Any())
                            {
                                SetMode(PipeMode.Output);
                            }
                            else
                            {
                                SetMode(PipeMode.Input);
                            }
                            return;
                        }
                    case TNWBIOMode.ForceInput:
                        {
                            SetMode(PipeMode.Input);
                            return;
                        }
                    case TNWBIOMode.ForceOutput:
                        {
                            SetMode(PipeMode.Output);
                            return;
                        }
                }
            }
            else if (ConnectedPipes.Count == 2)
            {
                pipeMode = PipeMode.Flow;
            }
            else if (ConnectedPipes.Count > 2)
            {
                pipeMode = PipeMode.Intersection;
            }
        }

        public void SetMode(PipeMode mode)
        {
            pipeMode = mode;
            switch (mode)
            {
                case PipeMode.Flow:                  
                case PipeMode.Intersection:
                    break;
                case PipeMode.Input:
                    {
                        if (!DirectParent.InputCells.Contains(Position))
                        {
                            DirectParent.InputCells.Add(Position);
                            DirectParent.OutputCells.Remove(Position);
                        }
                        return;
                    }
                case PipeMode.Output:
                    {
                        if (!DirectParent.OutputCells.Contains(Position))
                        {
                            DirectParent.OutputCells.Add(Position);
                            DirectParent.InputCells.Remove(Position);
                        }
                        return;
                    }
            }
        }

        public void UpdateDirectParent(TiberiumNetworkBuilding newParent, IntVec3 pos)
        {
            DirectParent = newParent;
            ParentCell = pos;
        }

        public void Notify_DirectParentGone(TiberiumNetworkBuilding tnwb)
        {
            if (DirectParent == tnwb)
            {
                StructureSet.RemoveStructure(tnwb);
                DirectParent = null;
                ParentCell = IntVec3.Invalid;
            }
        }

        public override void Tick()
        {
            //base.Tick();
            if (leaking && Container.StoredPct > 0)
            {
                if (HitPoints == MaxHitPoints)
                {
                    leaking = false;
                }
                else
                {
                    Color color = Container.Color;
                    TRUtils.ThrowTiberiumLeak(Map, Position, leakDirection, color);
                    foreach (TiberiumValueType valueType in Container.AllStoredTypes)
                    {
                        Container.RemoveValue(valueType, 1, out int removed);
                    }
                }
            }
            if (debugProduce)
            {
                Container.TryAddValue(TiberiumValueType.Green, 20, out int excess);
            }
            if (Find.TickManager.TicksGame % 5 == 0 && !Container.Empty)
            {
                if (!DirectParent.DestroyedOrNull())
                {
                    if (pipeMode == PipeMode.Input)
                    {
                        foreach (TiberiumValueType valueType in Container.AllStoredTypes)
                        {
                            Container.TryTransferTo(DirectParent.Container, valueType, 5);
                        }
                    }
                    if (pipeMode == PipeMode.Output)
                    {
                        foreach (TiberiumValueType valueType in DirectParent.Container.AllStoredTypes)
                        {
                            DirectParent.Container.TryTransferTo(Container, valueType, 5);
                        }
                    }
                }

                foreach (TNW_Pipe pipe in ConnectedPipes)
                {
                    if (pipe != from && Container.GetTotalStorage >= 2)
                    {
                        int num = ThroughPut(pipe.Container);
                        if (num > 0)
                        {
                            pipe.from = this;
                            int weight = num / Container.AllStoredTypes.Count;
                            foreach (TiberiumValueType valueType in Container.AllStoredTypes)
                            {
                                Container.TryTransferTo(pipe.Container, valueType, weight);
                            }
                        }
                    }
                }
                //from = null;
            }
        }

        public int ThroughPut(TiberiumContainer other)
        {
            int val = (Container.GetTotalStorage - other.GetTotalStorage) / 2;
            if (Container.StoredPct != other.StoredPct)
            {
                val = val > 0 ? val : 1;
            }
            return val;
        }
        
        public bool IsIOPipe
        {
            get
            {
                return pipeMode == PipeMode.Input || pipeMode == PipeMode.Output;
            }
        }

        public override IEnumerable<IntVec3> ConnectableCells => new List<IntVec3>() { this.Position };

        public override Graphic Graphic
        {
            get
            {
                return TiberiumGraphics.TiberiumNetworkPipes;
            }
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                Vector3 v2 = v + new Vector3(0, 15f, 0);
                Vector3 v3 = v - new Vector3(0, 15f, 0);
                GenMapUI.DrawThingLabel(v3, Container.GetTotalStorage.ToString(), Color.green);
                GenMapUI.DrawThingLabel(v, Container.StoredPct.ToStringPercent(), Color.white);
                GenMapUI.DrawThingLabel(v2, Container.Pressure.ToString() + "p", Color.yellow);
            }
        }

        public override void Draw()
        {
            base.Draw();
            GenDraw.DrawFieldEdges(new List<IntVec3>() { ParentCell }, Color.cyan);
            if (Container.StoredPct > 0f)
            {
                Color color = Container.Color;
                TiberiumGraphics.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(this.DrawPos + Altitudes.AltIncVect, Rotation, this);
            }

            if (Find.Selector.IsSelected(this))
            {
                GenDraw.DrawTargetHighlight(from);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.GetInspectString().TrimEndNewlines());
            if (DebugSettings.godMode)
            {
                sb.AppendLine("PipeMode: " + pipeMode);
                sb.AppendLine("DirectParent: " + DirectParent);
            }
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (!DirectParent.DestroyedOrNull() && DirectParent.def.IOMode == TNWBIOMode.Dynamic)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Switch Mode",
                    action = delegate ()
                    {
                        if (pipeMode == PipeMode.Input)
                        {
                            SetMode(PipeMode.Output);
                        }
                        else
                        {
                            SetMode(PipeMode.Input);
                        }
                    }
                };
            }

            #region Big Container Func
            yield return new Command_Action
            {
                defaultLabel = "ContainerSwitch_TR".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    
                    list.Add(new FloatMenuOption("SetRGB", delegate ()
                    {
                        var des = new Designator_SetStorageMode();
                        des.mode = StoreMode.RGB;
                        des.ProcessInput(null);
                        //Container.mode = StoreMode.RGB;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("SetRed", delegate ()
                    {
                        Container.mode = StoreMode.Red;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("SetGreen", delegate ()
                    {
                        Container.mode = StoreMode.Green;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("SetBlue", delegate ()
                    {
                        Container.mode = StoreMode.Blue;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("SetGas", delegate ()
                    {
                        Container.mode = StoreMode.Gas;
                        UpdateConnections();
                    }));
                    list.Add(new FloatMenuOption("SetSludge", delegate ()
                    {
                        Container.mode = StoreMode.Sludge;
                        UpdateConnections();
                    }));
                    FloatMenu menu = new FloatMenu(list);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            };
            #endregion

            if (DebugSettings.godMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Switch Produce",
                    action = delegate
                    {
                        debugProduce = !debugProduce;
                    }
                };
            }
        }
    }
    */

    /*
    public class TNW_Pipe2 : TiberiumNetworkBuilding
    {
        [Unsaved]
        public TNW_Pipe2 lastPusher;

        private Rot4 leakDirection = Rot4.Invalid; 
        private int tickOffset = 0;
        private bool leaking = false;
        private bool debugProduce = false;
        private float transferAmt = 1f;
        private PipeMode PipeMode = PipeMode.Flow;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            tickOffset = TRUtils.Range(0, 10); 
            if (!ConnectedStructures.All(t => t is TNW_Pipe))
            {
                PipeMode = PipeMode.Entry;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            float probability = 0.15f;
            probability *= Mathf.Lerp(1f, 0f, 1f * dinfo.ArmorPenetrationInt);
            if(HitPoints < ((float)MaxHitPoints *0.75f) && TRUtils.Chance(0.15f))
            {
                leaking = true;
                leakDirection = Rot4.FromAngleFlat(dinfo.Angle);
            }
            base.PostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Tick()
        {
            if (leaking && Container.StoredPct > 0)
            {
                if (HitPoints == MaxHitPoints)
                {
                    leaking = false;
                }
                else
                {
                    Color color = Container.Color;
                    color.a = 1f;
                    TRUtils.ThrowTiberiumLeak(Map, Position, leakDirection, color);
                }
            }
            if (Find.TickManager.TicksGame % 100 == 0)
            {
                if (debugProduce)
                {
                    Container.TryAddValue(TiberiumValueType.Green, 5f, out float excess);
                }
                if (leaking && Container.StoredPct > 0)
                {
                    Vector2 dir = leakDirection.AsVector2;
                    IntVec3 cell = Position + new IntVec3((int)dir.x, 0, (int)dir.y);
                    FilthMaker.MakeFilth(cell, Map, TiberiumDefOf.FilthTibLiquid);
                    TiberiumLeak leak = (TiberiumLeak)cell.GetThingList(Map).Find(t => t is TiberiumLeak);
                    leak?.Setup(Container.Color);
                    foreach (TiberiumValueType valueType in Container.AllStoredTypes)
                    {
                        Container.RemoveValue(valueType, 2.33f, out float removed);
                    }
                }
                if (ConnectedPipes.All(p => p.DestroyedOrNull()))
                {
                    return;
                }
                //ConnectedPipes.Shuffle();
                float weight = transferAmt / (float)Container.AllStoredTypes.Count;
                //float max = ConnectedPipes.Max(p => p.Container.StoredPct);
                bool hasPusher = ConnectedPipes.Any(p => Container. StoredPct >= Container.StoredPct);
                bool isPusher = ConnectedPipes.Any(p => p.Container.StoredPct < Container.StoredPct) && Container.GetTotalStorage > transferAmt;
                foreach(TiberiumNetworkBuilding b in ConnectedStructures)
                {
                    if ((hasPusher || isPusher) && b != lastPusher)
                    {
                        if (Container.GetTotalStorage > 0 && b.Container.StoredPct < Container.StoredPct)
                        {
                            foreach (TiberiumValueType valueType in Container.AllStoredTypes)
                            {
                                if (Container.TryTransferTo(b.Container, valueType, weight))
                                {
                                    if (b is TNW_Pipe2)
                                    {
                                        (b as TNW_Pipe2).lastPusher = this;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public float GeneralConcentration
        {
            get
            {
                float total = this.Container.GetTotalStorage + ConnectedPipes.Sum(p => p.Container.GetTotalStorage);
                float max = (ConnectedPipes.Count() + 1) * 20f;
                return total / max;
            }
        }

        private void CondLog(string s)
        {
            if (Find.Selector.IsSelected(this))
            {
                Log.Message(s);
            }
        }

        public override Graphic Graphic
        {
            get
            {
              return TiberiumGraphics.TiberiumNetworkPipes;
            }
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                Vector3 v2 = v + new Vector3(0, 15f, 0);
                GenMapUI.DrawThingLabel(v, Container.GetTotalStorage.ToString(), Color.white);
                GenMapUI.DrawThingLabel(v2, GeneralConcentration.ToStringPercent(), Color.yellow);
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (Container.StoredPct > 0f)
            {
                Color color = Container.Color;
                TiberiumGraphics.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(this.DrawPos + Altitudes.AltIncVect, Rotation, this);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            if (leaking)
            {
                sb.AppendLine("Leaking!");
            }
            sb.AppendLine("Pipes: ");
            foreach (TNW_Pipe p in ConnectedPipes)
            {
                if (p.Spawned)
                {
                    sb.AppendLine(p.ToString());
                }
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (ConnectedStructures.Any(p => !(p is TNW_Pipe)))
            {
                yield return new Command_Action
                {
                    defaultLabel = "SetInput_TR".Translate(),
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {

                    }
                };
            }

            yield return new Designator_Build(TiberiumDefOf.TiberiumPipe);

            yield return new Command_Action
            {
                defaultLabel = "Switch Produce",
                action = delegate
                {
                    debugProduce = !debugProduce;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Switch Leak",
                action = delegate
                {
                    leaking = !leaking;
                    leakDirection = Rot4.Random;
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Container Options",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("Add RGB", delegate ()
                    {
                        Container.TryAddValue(TiberiumValueType.Red, 6.3f, out float ex);
                        Container.TryAddValue(TiberiumValueType.Blue, 6.3f, out ex);
                        Container.TryAddValue(TiberiumValueType.Green, 6.3f, out ex);
                    }));
                    list.Add(new FloatMenuOption("Add Green", delegate ()
                    {
                        Container.TryAddValue(TiberiumValueType.Green, 5, out float ex);
                    }));
                    list.Add(new FloatMenuOption("Add Blue", delegate ()
                    {
                        Container.TryAddValue(TiberiumValueType.Blue, 5, out float ex);
                    }));
                    list.Add(new FloatMenuOption("Add Red", delegate ()
                    {
                        Container.TryAddValue(TiberiumValueType.Red, 5, out float ex);
                    }));
                    list.Add(new FloatMenuOption("Clear", delegate ()
                    {
                        Container.Clear();
                    }));
                    FloatMenu menu = new FloatMenu(list);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                }
            };
        }
    }
    */
}
