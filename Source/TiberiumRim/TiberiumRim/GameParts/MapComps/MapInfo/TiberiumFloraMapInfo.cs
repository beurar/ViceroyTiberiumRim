// ─────────────────────────────────────────────────────────────────────────────
//  TIBERIUM FLORA MAP INFO
//  • Maintains two BoolGrids inside TiberiumFloraGrid:
//        - floraBools  –   cells that currently host a TiberiumPlant
//        - growBools   –   cells suitable for spontaneous flora growth
//  • Provides helper counts & cached queries.
//  • Handles long-event flood-fill on first map initialisation.
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumFloraMapInfo : MapInformation
    {
        private readonly TiberiumFloraGrid floraGrid;

        // Optional – if you later resurrect garden logic
        private readonly List<TiberiumGarden> gardens = new List<TiberiumGarden>();

        /* ── Helper properties ─────────────────────────────────────────── */
        public int FloraCount => floraGrid.floraBools.TrueCount;
        public int GrowableCells => floraGrid.growBools.TrueCount;
        public bool MossPresent => FloraCount > 0;      // used by MapComponent helper
        public IEnumerable<IntVec3> CellsWithFlora => floraGrid.floraBools.ActiveCells;

        /* ── ctor ──────────────────────────────────────────────────────── */
        public TiberiumFloraMapInfo(Map map) : base(map)
        {
            floraGrid = new TiberiumFloraGrid(map);
        }

        /* ── Save / Load (none beyond base) ────────────────────────────── */
        public override void ExposeData() { }

        /* ── First init: pre-compute growable cells ────────────────────── */
        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
            if (initAfterReload) return;                            // already done

            LongEventHandler.QueueLongEvent(() =>
            {
                var filler = map.floodFiller;
                foreach (IntVec3 cell in map.AllCells)
                {
                    if (ShouldGrowFloraAt(cell)) continue;          // already marked
                    TerrainDef terrain = cell.GetTerrain(map);

                    if (IsGarden(terrain))
                    {
                        filler.FloodFill(
                            cell,
                            c => c.GetTerrain(map) == terrain,
                            c => floraGrid.SetGrow(c, true)
                        );
                    }
                }
            }, "SettingFloraBools", false, null);
        }

        /* ── Tick – add logic later if needed ──────────────────────────── */
        public override void Tick() { }

        /* ── Draw overlay (Dev mode / debug) ───────────────────────────── */
        public override void Draw()
        {
            floraGrid.drawer.RegenerateMesh();
            floraGrid.drawer.MarkForDraw();
            floraGrid.drawer.CellBoolDrawerUpdate();
        }

        /* ── Registration helpers (called by TiberiumPlant) ───────────── */
        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            floraGrid.SetFlora(plant.Position, true);
            floraGrid.Notify_PlantSpawned(plant);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            floraGrid.SetFlora(plant.Position, false);
        }

        /* ── Query API used by other systems ───────────────────────────── */
        public bool HasFloraAt(IntVec3 c) => floraGrid.floraBools[c];
        public bool ShouldGrowFloraAt(IntVec3 c) => floraGrid.growBools[c];

        /* ── Internals ─────────────────────────────────────────────────── */
        private static bool IsGarden(TerrainDef def)
        {
            return def.IsMoss() || (def.IsSoil() && def.fertility >= 1.2f);
        }
    }
}
