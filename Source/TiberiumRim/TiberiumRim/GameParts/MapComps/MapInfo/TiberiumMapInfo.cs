// ─────────────────────────────────────────────────────────────────────────────
//  TIBERIUM MAP INFO
//  • Tracks every crystal on the map, categorised by harvest type & def.
//  • Maintains a BoolGrid + reference grids for quick spatial queries.
//  • Provides helper flags / counts requested by MapComponent_Tiberium.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public enum HarvestType { Valuable, Unvaluable, Unharvestable }

    public class TiberiumMapInfo : MapInformation
    {
        /* ── Libraries ────────────────────────────────────────────────── */
        public readonly HashSet<Thing> AllTiberiumCrystals = new HashSet<Thing>();

        public readonly Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals =
            new Dictionary<HarvestType, List<TiberiumCrystal>>();

        public readonly Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes =
            new Dictionary<HarvestType, List<TiberiumCrystalDef>>();

        public readonly Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef =
            new Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>>();

        /* ── Grid ─────────────────────────────────────────────────────── */
        private readonly TiberiumGrid tiberiumGrid;
        public TiberiumGrid TiberiumGrid => tiberiumGrid;

        /* ── Helper properties (used by outside code) ─────────────────── */
        public int TotalCount => AllTiberiumCrystals.Count;
        public float Coverage => TotalCount / (float)map.Area;
        public bool AnyCrystalsPresent => TotalCount > 0;       // queried by MapComponent
        public int ValuableCrystalCount => TiberiumCrystals[HarvestType.Valuable].Count;
        public bool AnyMossPresent => TiberiumCrystals[HarvestType.Unvaluable].Count > 0;

        public TiberiumCrystalDef MostValuableType
        {
            get
            {
                var list = TiberiumCrystalTypes[HarvestType.Valuable];
                return list.Count == 0
                    ? null
                    : list.OrderByDescending(t => t.tiberium.harvestValue).First();
            }
        }

        /* ── ctor ─────────────────────────────────────────────────────── */
        public TiberiumMapInfo(Map map) : base(map)
        {
            tiberiumGrid = new TiberiumGrid(map);

            // Initialise dictionaries
            foreach (HarvestType ht in (HarvestType[])System.Enum.GetValues(typeof(HarvestType)))
            {
                TiberiumCrystals[ht] = new List<TiberiumCrystal>();
                TiberiumCrystalTypes[ht] = new List<TiberiumCrystalDef>();
            }
        }

        public override void ExposeData() { }

        /* ── Tick (placeholder – add if needed) ───────────────────────── */
        public override void Tick() { }

        /* ── Draw debug overlay ───────────────────────────────────────── */
        public override void Draw()
        {
            tiberiumGrid.Drawer.RegenerateMesh();
            tiberiumGrid.Drawer.MarkForDraw();
            tiberiumGrid.Drawer.CellBoolDrawerUpdate();
        }

        /* ── Spatial queries ──────────────────────────────────────────── */
        public TiberiumCrystal TiberiumAt(IntVec3 cell)
        {
            return tiberiumGrid.TiberiumCrystals[map.cellIndices.CellToIndex(cell)];
        }

        public bool HasTiberiumAt(IntVec3 c) => tiberiumGrid.TiberiumBoolGrid[c];
        public bool CanGrowFrom(IntVec3 c) => tiberiumGrid.GrowFromGrid[c];
        public bool CanGrowTo(IntVec3 c) => tiberiumGrid.GrowToGrid[c];
        public bool IsAffectedCell(IntVec3 c) => tiberiumGrid.AffectedCells[c];

        /* ── Registration helpers ─────────────────────────────────────── */
        public void RegisterTiberium(TiberiumCrystal crystal)
        {
            var type = crystal.def.HarvestType;
            if (TiberiumCrystals[type].Contains(crystal)) return;

            AllTiberiumCrystals.Add(crystal);
            TiberiumCrystals[type].Add(crystal);
            tiberiumGrid.SetCrystal(crystal);

            if (!TiberiumCrystalTypes[type].Contains(crystal.def))
                TiberiumCrystalTypes[type].Add(crystal.def);

            List<TiberiumCrystal> list;
            if (TiberiumCrystalsByDef.TryGetValue(crystal.def, out list))
                list.Add(crystal);
            else
                TiberiumCrystalsByDef.Add(crystal.def, new List<TiberiumCrystal> { crystal });
        }

        public void DeregisterTiberium(TiberiumCrystal crystal)
        {
            var def = crystal.def;
            var type = def.HarvestType;

            AllTiberiumCrystals.Remove(crystal);
            TiberiumCrystals[type].Remove(crystal);
            tiberiumGrid.ResetCrystal(crystal.Position);
            TiberiumCrystalsByDef[def].Remove(crystal);

            if (!TiberiumCrystals[type].Any(c => c.def == def))
                TiberiumCrystalTypes[type].Remove(def);
        }
    }
}
