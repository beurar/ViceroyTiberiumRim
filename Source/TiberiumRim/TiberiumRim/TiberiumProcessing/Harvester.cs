using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;

namespace TiberiumRim
{
    public enum HarvestMode
    {
        Nearest,
        Value,
        Moss,
    }

    public enum HarvesterPriority
    {
        Drafted,
        Harvest,
        Unload,
        Idle
    }

    public class HarvesterKindDef : MechanicalPawnKindDef
    {
        public float unloadValue = 0.125f;
        public float harvestValue = 0.125f;
        public float explosionRadius = 7;
        public ThingDef wreckDef;
    }

    public class Harvester : MechanicalPawn, IContainerHolder
    {
        public new HarvesterKindDef kindDef => (HarvesterKindDef) base.kindDef;
        protected Comp_TiberiumContainer container;

        //Data
        private IntVec3 lastKnownPos;
        private Comp_TiberiumRefinery compRefinery;

        //Settings
        private bool forceReturn = false;
        private HarvestMode harvestMode = HarvestMode.Nearest;
        private TiberiumProducer preferredProducer;
        private TiberiumCrystalDef preferredType;

        public Comp_TiberiumRefinery RefineryComp => compRefinery;
        public Comp_TiberiumContainer ContainerComp => container;
        public Building Refinery
        {
            get => ParentBuilding;
            set => ParentBuilding = value;
        }

        public bool AtRefinery => Position == Refinery?.InteractionCell;

        public bool AnyAvailableRefinery(out Building building)
        {
            building = null;
            return false;
        }

        public IntVec3 IdlePos => RefineryComp?.PositionFor(this) ?? lastKnownPos;

        public HarvestMode HarvestMode
        {
            get => harvestMode;
            private set => harvestMode = value;
        }
        public TiberiumProducer PreferredProducer
        {
            get => preferredProducer;
            private set => preferredProducer = value;
        }
        public TiberiumCrystalDef PreferredType
        {
            get => preferredType;
            private set => preferredType = value;
        }

        public bool ForceReturn
        {
            get => forceReturn;
            private set => forceReturn = value;
        }

        //User Control
        private bool ShouldReturnToIdle => ForceReturn || RefineryComp.RecallHarvesters;
        public bool PlayerInterrupt => ShouldReturnToIdle || Drafted;

        //Data Bools
        private bool ContainerFull => ContainerComp.IsFull;

        private bool HasAvailableTiberium => HarvestMode == HarvestMode.Moss ? TiberiumManager.MossAvailable() : TiberiumManager.TiberiumAvailable();

        //Priority Bools
        private bool ShouldIdle    => ContainerComp.IsEmpty && (!HasAvailableTiberium || (ContainerComp.IsFull && RefineryComp.Container.IsFull));
        private bool ShouldHarvest => !ContainerFull && HasAvailableTiberium;//CurrentPriority == HarvesterPriority.Harvest;
        private bool ShouldUnload  => ContainerFull || !HasAvailableTiberium;

        private bool CanHarvest => !IsUnloading; // !ContainerFull && HasAvailableTiberium;
        private bool CanUnload  => ContainerComp.IsEmpty == false && RefineryComp.CanBeRefinedAt;

        public bool IsHarvesting
        {
            get
            {
                if (CurJob == null)
                    return false;
                if (jobs.curDriver is JobDriver_HarvestTiberium harvest)
                    return true;
                return false;
            }
        }

        public bool IsUnloading
        {
            get
            {
                if (CurJob == null)
                    return false;
                if (jobs.curDriver is JobDriver_UnloadAtRefinery unload)
                    return true;
                return false;
            }
        }

        public HarvesterPriority CurrentPriority
        {
            get
            {
                if (Drafted) return HarvesterPriority.Drafted;
                if (ShouldReturnToIdle) return HarvesterPriority.Idle;
                if (ShouldHarvest && CanHarvest)
                {
                    return HarvesterPriority.Harvest;
                }
                if (ShouldUnload && CanUnload)
                {
                    return HarvesterPriority.Unload;
                }
                return HarvesterPriority.Idle;
            }
        }
        //FX Settings
        public override Color[] ColorOverrides => new Color[] { ContainerComp.DominantColor };
        public override float[] OpacityFloats => new float[] { ContainerComp.StoredPercent };
        public override bool[] DrawBools => new bool[] { true };

        public void Notify_ContainerFull() { }
        public void Notify_RefineryDestroyed(Comp_TiberiumRefinery notifier)
        {
            ResolveNewRefinery(notifier);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Settings
            Scribe_Values.Look(ref forceReturn, "ForceReturn");
            Scribe_Values.Look(ref harvestMode, "HarvestMode");
            Scribe_References.Look(ref preferredProducer, "prefProducer");
            Scribe_Defs.Look(ref preferredType, "prefType");
            //Data
            Scribe_Deep.Look(ref container, "tibContainer");
            Scribe_Values.Look(ref lastKnownPos, "lastPos");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if(Refinery != null)
                {
                    compRefinery = Refinery.GetComp<Comp_TiberiumRefinery>();
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                if (ParentBuilding == null)
                { 
                    ResolveNewRefinery(); 
                }
            }

            //TiberiumManager.HarvesterInfo.RegisterHarvester(this);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
        }

        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            base.Kill(dinfo, exactCulprit);

            if (!ContainerComp.IsEmpty)
            {
                foreach (var kv in ContainerComp.StoredValues)
                {
                    var valDef = kv.Key;
                    var amount = kv.Value;

                    if (amount <= 0 || valDef.ThingDroppedFromContainer == null)
                        continue;

                    // Determine explosion scale from % of value stored
                    float percent = (float)(amount / ContainerComp.Capacity);
                    float radius = kindDef.explosionRadius * percent;
                    int damage = Mathf.RoundToInt(10 * percent);
                    int count = Mathf.Max(1, Mathf.FloorToInt((float)(amount / valDef.ValueToThingRatio)));

                    GenExplosion.DoExplosion(
                        Position,
                        Map,
                        radius,
                        DamageDefOf.Bomb,
                        this,
                        damage,
                        armorPenetration: 0,
                        explosionSound: null,
                        weapon: null,
                        projectile: null,
                        intendedTarget: null,
                        postExplosionSpawnThingDef: valDef.ThingDroppedFromContainer,
                        postExplosionSpawnChance: 1f,
                        postExplosionSpawnThingCount: count,
                        applyDamageToExplosionCellsNeighbors: false,
                        preExplosionSpawnThingDef: null,
                        preExplosionSpawnChance: 0f,
                        preExplosionSpawnThingCount: 0,
                        chanceToStartFire: 0f,
                        damageFalloff: true,
                        direction: null,
                        ignoredThings: null,
                        affectedAngle: null,
                        doVisualEffects: true,
                        propagationSpeed: 1f,
                        excludeRadius: 0f,
                        doSoundEffects: true,
                        postExplosionSpawnThingDefWater: null,
                        screenShakeFactor: 1f,
                        flammabilityChanceCurve: null,
                        overrideCells: null
                    );
                }
            }

            GenSpawn.Spawn(kindDef.wreckDef, Position, Map);
            this.DeSpawn(DestroyMode.KillFinalize);
        }


        public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
        {
            base.PreApplyDamage(ref dinfo, out absorbed);
            //TODO: EVA Warning
            //GameComponent_EVA.EVAComp().ReceiveSignal();
        }

        public bool CanHarvestTiberium(TiberiumCrystalDef crystal)
        {
            return crystal.IsMoss ? HarvestMode == HarvestMode.Moss : HarvestMode != HarvestMode.Moss;
        }

        public void SetMainRefinery(Building building, Comp_TiberiumRefinery refinery, Comp_TiberiumRefinery lastParent)
        {
            if(lastParent != null)
            {
                Refinery = null;
                lastParent.RemoveHarvester(this);
                Messages.Message("TR_HarvesterNewRefinery".Translate(), new LookTargets(building, this), MessageTypeDefOf.NeutralEvent);
            }
            lastKnownPos = building.InteractionCell;
            Refinery = building;
            compRefinery = refinery;
            compRefinery.AddHarvester(this);
        }

        private void ResolveNewRefinery(Comp_TiberiumRefinery lastParent = null)
        {
            if (Map == null || !Map.listerBuildings.allBuildingsColonist.Any()) return;

            var refineries = Map.listerBuildings.allBuildingsColonist
                .Where(b => b.TryGetComp<Comp_TiberiumRefinery>() is { } comp && comp != lastParent)
                .Select(b => b.TryGetComp<Comp_TiberiumRefinery>())
                .ToList();

            if (refineries.NullOrEmpty()) return;

            var newRefinery = refineries.First();
            SetMainRefinery((Building)newRefinery.parent, newRefinery, lastParent);
        }


        public new void DynamicDrawPhase(DrawPhase drawPhase)
        {
            base.DynamicDrawPhase(drawPhase);
            if (Find.Selector.IsSelected(this) && Find.CameraDriver.CurrentZoom <= CameraZoomRange.Middle)
            {
                GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
                r.center = DrawPos;
                r.center.z += 1.5f;
                r.size = new Vector2(3, 0.15f);
                r.fillPercent = ContainerComp.StoredPercent;
                r.filledMat = TiberiumContent.Harvester_FilledBar;
                r.unfilledMat = TiberiumContent.Harvester_EmptyBar;
                r.margin = 0.12f;
                GenDraw.DrawFillableBar(r);
            }
        }

        private Texture2D HarvestModeTexture
        {
            get
            {
                return HarvestMode switch
                {
                    HarvestMode.Nearest => TiberiumContent.HarvesterNearest,
                    HarvestMode.Value => TiberiumContent.HarvesterValue,
                    HarvestMode.Moss => TiberiumContent.HarvesterMoss,
                    _ => BaseContent.BadTex
                };
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction != Faction.OfPlayer) yield break;

            if (DebugSettings.godMode)
            {
            }

            foreach (Gizmo g in ContainerComp.GetGizmos())
            {
                yield return g;
            }

            //Preferences
            yield return new Command_Action
            {
                defaultLabel = "TR_HarvesterMode".Translate(),
                defaultDesc = "TR_HarvesterModeDesc".Translate(),
                icon = HarvestModeTexture,
                action = delegate
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    list.Add(new FloatMenuOption("TRHMode_Nearest".Translate(),
                        delegate () { harvestMode = HarvestMode.Nearest; }));
                    list.Add(new FloatMenuOption("TRHMode_Valuable".Translate(),
                        delegate () { harvestMode = HarvestMode.Value; }));
                    list.Add(new FloatMenuOption("TRHMode_Moss".Translate(),
                        delegate () { harvestMode = HarvestMode.Moss; }));
                    FloatMenu menu = new FloatMenu(list);
                    menu.vanishIfMouseDistant = true;
                    Find.WindowStack.Add(menu);
                },
            };

            /*
            yield return new Command_Target
            {
                defaultLabel = "TR_ProducerPrefLabel".Translate(),
                defaultDesc = "TR_ProducerPrefDesc".Translate(),
                icon = BaseContent.BadTex,
                action = delegate (Thing thing)
                {

                }
            };


            yield return new Command_Action
            {
                defaultLabel = "TR_TypePrefLabel".Translate(),
                defaultDesc = "TR_TypePrefDesc".Translate(),
                icon = BaseContent.BadTex,
                action =
                {

                }
            };
            */

            yield return new Command_Action
            {
                defaultLabel = ForceReturn ? "TR_HarvesterHarvest".Translate() : "TR_HarvesterReturn".Translate(),
                defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
                icon = ForceReturn ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate
                {
                    ForceReturn = !ForceReturn;
                    this.jobs.EndCurrentJob(JobCondition.InterruptForced);
                },
            };

            yield return new Command_Target
            {
                defaultLabel = "TR_HarvesterRefinery".Translate(),
                defaultDesc = "TR_HarvesterRefineryDesc".Translate(),
                icon = TiberiumContent.HarvesterRefinery,
                targetingParams = RefineryTargetInfo.ForHarvester(),
                action = delegate (LocalTargetInfo targetInfo)
                {
                    if (targetInfo == null) return;
                    if (targetInfo.Thing is Building building)
                    {
                        var refinery = targetInfo.Thing.TryGetComp<Comp_TiberiumRefinery>();
                        if(refinery != null)
                            SetMainRefinery(building, refinery, RefineryComp);
                        //UpdateRefineries(b);
                    }
                },
            };
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine();
            if (DebugSettings.godMode)
            {
                sb.AppendLine("##Debug##");
                sb.AppendLine("CurJob: " + this.CurJob);
                sb.AppendLine("Priority: " + CurrentPriority);
                sb.AppendLine("Has Tiberium: " + HasAvailableTiberium);
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Can Harvest: " + CanHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Can Unload: " + CanUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Player Interrupted: " + PlayerInterrupt);
                sb.AppendLine("Is Unlading: " + IsUnloading);
                sb.AppendLine("Is Harvesting: " + IsHarvesting);

                /*
                if (IsWaiting)
                {
                    sb.AppendLine("Waiting: " + waitingTicks.TicksToSeconds());
                }

                sb.AppendLine("Drafted: " + Drafted);
                sb.AppendLine("Is Waiting: " + IsWaiting);
                sb.AppendLine("Forced Return: " + ForcedReturn);
                sb.AppendLine("Unloading: " + Unloading);
                sb.AppendLine("Capacity Full: " + Container.CapacityFull);
                sb.AppendLine("Tiberium f/mode Available: " + HasAvailableTiberium);
                sb.AppendLine("Can Unload: " + CanUnload);

                sb.AppendLine("Exact Storage: " + Container.TotalStorage);
                sb.AppendLine("Should Harvest: " + ShouldHarvest);
                sb.AppendLine("Should Unload: " + ShouldUnload);
                sb.AppendLine("Should Idle: " + ShouldIdle);
                sb.AppendLine("Mode: " + harvestMode);
                sb.AppendLine("HarvesterQueue: " + HarvestQueue.Count);
                sb.AppendLine("Contained In Queue:" + HarvestQueue.ToStringSafeEnumerable());
                */
                //sb.AppendLine("Current Harvest Target: " + CurrentHarvestTarget);               
                //sb.AppendLine("Valid: " + TNWManager.ReservationManager.TargetValidFor(this) + " CanBeHarvested: " + CurrentHarvestTarget?.CanBeHarvestedBy(this) + " Spawned: " + CurrentHarvestTarget?.Spawned + " Destroyed: " + CurrentHarvestTarget?.Destroyed);
            }
            return sb.ToString().TrimEndNewlines();
        }
    }
}
