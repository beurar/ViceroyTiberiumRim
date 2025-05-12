using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Designator_BuildFixed : Designator_Build
    {
        private ThingDef stuffDef;

        public Designator_BuildFixed(BuildableDef entdef) : base(entdef)
        {
            this.iconProportions = new Vector2(1f, 1f);
            stuffDef = (bool)entdef?.MadeFromStuff ? GenStuff.DefaultStuffFor(entdef) : null;
        }

        public TRThingDef TRThingDef => entDef as TRThingDef;

        public override void DesignateSingleCell(IntVec3 c)
        {
            if (TutorSystem.TutorialMode && !TutorSystem.AllowAction(new EventPack(base.TutorTagDesignate, c)))
                return;

            if (DebugSettings.godMode || entDef.GetStatValueAbstract(StatDefOf.WorkToBuild, stuffDef).Equals(0f))
            {
                if (entDef is TerrainDef terrainDef)
                {
                    base.Map.terrainGrid.SetTerrain(c, terrainDef);
                }
                else
                {
                    Thing thing = ThingMaker.MakeThing((ThingDef)entDef, stuffDef);
                    if (TRThingDef != null)
                        thing.SetFactionDirect(TRThingDef.devObject ? null : Faction.OfPlayer);

                    GenSpawn.Spawn(thing, c, base.Map, placingRot, WipeMode.Vanish, false);
                }
            }
            else
            {
                GenSpawn.WipeExistingThings(c, placingRot, entDef.blueprintDef, base.Map, DestroyMode.Deconstruct);
                GenConstruct.PlaceBlueprintForBuild(entDef, c, base.Map, placingRot, Faction.OfPlayer, stuffDef);
            }

            // I think ThrowMetaPuffs is now part of MoteMaker?
            var rect = GenAdj.OccupiedRect(c, placingRot, entDef.Size);
            foreach (var cell in rect)
            {
                if (!cell.ShouldSpawnMotesAt(base.Map) || base.Map.moteCounter.SaturatedLowPriority) continue;

                var puff = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("Mote_MetaPuff", false)) as MoteThrown;
                if (puff != null)
                {
                    puff.Scale = Rand.Range(1.5f, 2.2f);
                    puff.rotationRate = Rand.Range(-30f, 30f);
                    puff.exactPosition = cell.ToVector3Shifted();
                    puff.SetVelocity(Rand.Range(0f, 360f), Rand.Range(0.3f, 0.5f));
                    GenSpawn.Spawn(puff, cell, base.Map);
                }
            }

            if (entDef is ThingDef thingDef && thingDef.IsOrbitalTradeBeacon)
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BuildOrbitalTradeBeacon, KnowledgeAmount.Total);

            if (TutorSystem.TutorialMode)
                TutorSystem.Notify_Event(new EventPack(base.TutorTagDesignate, c));

            if (entDef.PlaceWorkers == null) return;

            foreach (var placeWorker in entDef.PlaceWorkers)
            {
                placeWorker.PostPlace(base.Map, entDef, c, placingRot);
            }
        }
    }
}
