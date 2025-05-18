using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_TiberiumRefinery : ThingComp
    {
        private bool recallHarvesters = false;

        public Comp_MechStation MechComp => parent.GetComp<Comp_MechStation>();
        public CompNetwork Net => parent.GetComp<CompNetwork>();
        public CompPowerTrader Power => parent.GetComp<CompPowerTrader>();
        public CompProperties_TiberiumRefinery Props => (CompProperties_TiberiumRefinery)props;
        public Comp_TiberiumContainer Container => parent.GetComp<Comp_TiberiumContainer>();

        public bool RecallHarvesters
        {
            get => recallHarvesters;
            private set => recallHarvesters = value;
        }

        public int HarvesterCount => MechComp.ConnectedMechs.Count;

        public bool CanBeRefinedAt => (Power?.PowerOn ?? true) && !parent.IsBrokenDown() && HasRoomInNetwork();

        private bool HasRoomInNetwork()
        {
            if (Net == null) return false;

            foreach (var part in Net.NetworkParts)
            {
                if (part.Volume == null) continue;
                if (part.Volume.Full)
                    return true;
            }

            return false;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref recallHarvesters, "recallHarvesters");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                AddHarvester(SpawnNewHarvester());
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            for (var i = MechComp.ConnectedMechs.Count - 1; i >= 0; i--)
            {
                if (MechComp.ConnectedMechs[i] is Harvester harvester)
                {
                    harvester.Notify_RefineryDestroyed(this);
                    if (mode != DestroyMode.Deconstruct)
                        Messages.Message("TR_RefineryLost".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                }
            }
        }

        private Harvester SpawnNewHarvester()
        {
            Harvester harvester = (Harvester)MechComp.MakeMech(Props.harvester);
            harvester.Rotation = parent.Rotation;
            harvester.SetMainRefinery((Building)parent, this, null);
            IntVec3 spawnLoc = parent.InteractionCell;
            return (Harvester)GenSpawn.Spawn(harvester, spawnLoc, parent.Map, parent.Rotation.Opposite);
        }

        public void AddHarvester(Harvester harvester)
        {
            if (MechComp.MainMechLink.Contains(harvester)) return;
            MechComp.TryAddMech(harvester);
        }

        public void RemoveHarvester(Harvester harvester)
        {
            MechComp.RemoveMech(harvester);
        }

        public void Notify_HarvesterReturned(NetworkValueDef type, double amount)
        {
            InjectTiberium(type, amount);
        }

        public void InjectTiberium(NetworkValueDef phaseDef, double amount)
        {
            if (Net == null || !Net.IsWorking) return;

            var val = DefDatabase<NetworkValueDef>.AllDefs.FirstOrDefault(x => x.defName == phaseDef.defName);
            if (val == null)
            {
                Log.Warning($"[Refinery] Unknown NetworkValueDef for phase: {phaseDef.defName}");
                return;
            }

            Net.AddVolume(parent, val, amount);
            Net.Notify_ReceivedValue();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            yield return new Command_Action
            {
                defaultLabel = RecallHarvesters ? "TR_RefineryAllow".Translate() : "TR_RefineryReturn".Translate(),
                defaultDesc = "TR_RefineryReturnDesc".Translate(),
                icon = RecallHarvesters ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
                action = delegate { RecallHarvesters = !RecallHarvesters; }
            };
        }

        internal IntVec3 PositionFor(Harvester harvester)
        {
            throw new NotImplementedException();
        }
    }

    public class CompProperties_TiberiumRefinery : CompProperties
    {
        public MechanicalPawnKindDef harvester;

        public CompProperties_TiberiumRefinery()
        {
            compClass = typeof(Comp_TiberiumRefinery);
        }
    }
}