// ─────────────────────────────────────────────────────────────────────────────
//  MAP COMPONENT : Tiberium
//  Oversees every terrain-level manifestation of the crystal plague.
//
//  SUB-SYSTEM MATRIX
//  ────────────────
//  • Info            – immutable flags & cached cell lists
//  • Suppression     – anti-plague pylons (bool grids)
//  • Growth          – ambient + fast-burst spread (obeys Suppression)
//  • Hediffs         – pawn afflictions
//  • PollutionInfo   – miasma / filth overlays
//  • BlossomInfo     – blooming crystal flowers (static décor / resource)
//  • FloraInfo       – small tiberium flora sprites
//  • HarvesterInfo   – keeps track of mobile harvesters
//  • TerrainInfo     – tainted terrain conversions
//  • UndergroundInfo – subterranean resource pockets
//  • StructureInfo   – statically-placed structures (reactors, silos, etc.)
//  • WaterInfo       – contaminated water tracking
//  • PawnInfo        – pawns currently suffering “tib-sick”
// ─────────────────────────────────────────────────────────────────────────────

using RimWorld;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Single coordinator for every *terrain* Tiberium mechanic.
    /// NO dependency on TeleCore – pipes & liquids are handled elsewhere.
    /// </summary>
    public class MapComponent_Tiberium : MapComponent
    {
        // ── sub-systems (instantiated in FinalizeInit) ────────────────────
        public TiberiumMapInfo Info;
        public TiberiumSuppressionMapInfo Suppression;
        public TiberiumSpreaderMapInfo Growth;
        public TiberiumHediffMapInfo Hediffs;
        public TiberiumPollutionMapInfo PollutionInfo;
        public TiberiumBlossomMapInfo BlossomInfo;
        public TiberiumFloraMapInfo FloraInfo;
        public TiberiumHarvesterMapInfo HarvesterInfo;
        public TiberiumTerrainMapInfo TerrainInfo;
        public TiberiumUndergroundMapInfo UndergroundInfo;
        public TiberiumStructureMapInfo StructureInfo;
        public TiberiumWaterMapInfo WaterInfo;
        public TiberiumPawnMapInfo PawnInfo;
        public MapComponent_TeleCore Network;

        // ── ctor ───────────────────────────────────────────────────────────
        public MapComponent_Tiberium(Map map) : base(map) { }

        // ── initialisation ────────────────────────────────────────────────
        public override void FinalizeInit()
        {
            /* 1 ░ Core facts & environment */
            Info = new TiberiumMapInfo(map);
            TerrainInfo = new TiberiumTerrainMapInfo(map);
            WaterInfo = new TiberiumWaterMapInfo(map);
            UndergroundInfo = new TiberiumUndergroundMapInfo(map);

            /* 2 ░ Suppression grid (must exist before Growth) */
            Suppression = new TiberiumSuppressionMapInfo(map);

            /* 3 ░ Growth – needs Suppression reference */
            Growth = new TiberiumSpreaderMapInfo(map, Suppression);

            /* 4 ░ Living world / pawns */
            Hediffs = new TiberiumHediffMapInfo(map);
            PawnInfo = new TiberiumPawnMapInfo(map);
            HarvesterInfo = new TiberiumHarvesterMapInfo(map);

            /* 5 ░ Decorative / resource flora */
            BlossomInfo = new TiberiumBlossomMapInfo(map);
            FloraInfo = new TiberiumFloraMapInfo(map);

            /* 6 ░ Static structures & ambience */
            StructureInfo = new TiberiumStructureMapInfo(map);
            PollutionInfo = new TiberiumPollutionMapInfo(map);

            /* 7 ░ Telecore network */
            Network = map.GetComponent<MapComponent_TeleCore>();
        }

        // ── per-tick execution order ───────────────────────────────────────
        public override void MapComponentTick()
        {
            /* Core world updates */
            Info.Tick();
            TerrainInfo.Tick();
            WaterInfo.Tick();
            UndergroundInfo.Tick();

            /* Suppression before any growth occurs */
            Suppression.Tick();
            Growth.Tick();

            /* Living entities */
            Hediffs.Tick();
            PawnInfo.Tick();
            HarvesterInfo.Tick();

            /* Flora & blossoms */
            BlossomInfo.Tick();
            FloraInfo.Tick();

            /* Static structures & ambience */
            StructureInfo.Tick();
            PollutionInfo.Tick();
        }

        // ── save / load ─────────────────────────────────────────────────────
        public override void ExposeData()
        {
            // NOTE: Suppression must be loaded *before* Growth to satisfy ctor.
            Scribe_Deep.Look(ref Info, "Info", map);
            Scribe_Deep.Look(ref TerrainInfo, "TerrainInfo", map);
            Scribe_Deep.Look(ref WaterInfo, "WaterInfo", map);
            Scribe_Deep.Look(ref UndergroundInfo, "UndergroundInfo", map);

            Scribe_Deep.Look(ref Suppression, "Suppression", map);
            Scribe_Deep.Look(ref Growth, "Growth", map, Suppression);

            Scribe_Deep.Look(ref Hediffs, "Hediffs", map);
            Scribe_Deep.Look(ref PawnInfo, "PawnInfo", map);
            Scribe_Deep.Look(ref HarvesterInfo, "HarvesterInfo", map);

            Scribe_Deep.Look(ref BlossomInfo, "BlossomInfo", map);
            Scribe_Deep.Look(ref FloraInfo, "FloraInfo", map);

            Scribe_Deep.Look(ref StructureInfo, "StructureInfo", map);
            Scribe_Deep.Look(ref PollutionInfo, "PollutionInfo", map);
        }

        // Registrations functions
        public void RegisterNewThing(Thing thing)
        {
            if (thing is IPollutionSource source)
            {
                PollutionInfo.RegisterSource(source);
            }
        }

        public void DeregisterThing(Thing thing)
        {
            if (thing is IPollutionSource source)
            {
                PollutionInfo.DeregisterSource(source);
            }
        }

        public void RegisterTiberiumBuilding(TRBuilding building)
        {
            StructureInfo.TryRegister(building);
            if (building is TiberiumProducer p)
            {
                Growth.RegisterField(p);
            }
        }

        public void DeregisterTiberiumBuilding(TRBuilding building)
        {
            StructureInfo.Deregister(building);
            if (building is TiberiumProducer p)
            {
                Growth.DeregisterField(p);
            }
        }

        public void RegisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            Info.RegisterTiberium(crystal);
            TerrainInfo.Notify_TibSpawned(crystal);
            Hediffs.Notify_TibChanged();
        }

        public void DeregisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            Info.DeregisterTiberium(crystal);
            Hediffs.Notify_TibChanged();
        }

        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.RegisterTiberiumPlant(plant);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.DeregisterTiberiumPlant(plant);
        }
    }
}
