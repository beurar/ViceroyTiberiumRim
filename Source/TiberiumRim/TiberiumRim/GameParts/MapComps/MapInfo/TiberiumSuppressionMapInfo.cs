// ─────────────────────────────────────────────────────────────────────────────
//  TIBERIUM SUPPRESSION  (anti-crystal pylons & masks)
//  • Maintains two grids:
//        - coveredBools     – every cell that falls inside ANY pylon radius
//        - suppressionBools – cells currently blocked (device powered / on)
//  • Public helpers mirror the legacy API: IsCovered, IsSuppressed, etc.
//  • A simple “rebuild on dirty” strategy replaces the old SuppressionGrid.
// ─────────────────────────────────────────────────────────────────────────────

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class TiberiumSuppressionMapInfo : MapInformation
    {
        // ── internal state ─────────────────────────────────────────────────
        private readonly HashSet<Comp_Suppression> sources = new HashSet<Comp_Suppression>(); // active pylons
        private readonly HashSet<Comp_Suppression> dirty = new HashSet<Comp_Suppression>(); // changed this tick

        private BoolGrid coveredBools;      // any-radius mask
        private BoolGrid suppressionBools;  // active suppression mask

        // ── public enumerable mirrors (for overlays) ───────────────────────
        public IEnumerable<IntVec3> SuppressedCells => suppressionBools.ActiveCells;
        public IEnumerable<IntVec3> CoveredCells => coveredBools.ActiveCells;

        // ── ctor ───────────────────────────────────────────────────────────
        public TiberiumSuppressionMapInfo(Map map) : base(map)
        {
            coveredBools = new BoolGrid(map);
            suppressionBools = new BoolGrid(map);
        }

        // ── tick: rebuild grids only when something changed ────────────────
        public override void Tick()
        {
            if (dirty.Count == 0) return;

            RebuildGrids();
            dirty.Clear();
        }

        // ── lookup helpers (same signatures as before) ─────────────────────
        public bool IsSuppressed(IntVec3 cell) => suppressionBools[cell];
        public bool IsCovered(IntVec3 cell) => coveredBools[cell];

        public bool IsInSuppressionCoverage(IntVec3 cell,
                                            out List<Comp_Suppression> list)
        {
            list = null!;
            if (!IsCovered(cell)) return false;
            list = sources.Where(s => s.CoversCell(cell)).ToList();
            return list.Count > 0;
        }

        public bool IsSuppressed(IntVec3 cell,
                                 out List<Comp_Suppression> list)
        {
            list = null!;
            if (!IsSuppressed(cell)) return false;
            list = sources.Where(s => s.AffectsCell(cell)).ToList();
            return list.Count > 0;
        }

        // ── registration API (unchanged) ───────────────────────────────────
        public void RegisterSuppressor(Comp_Suppression comp)
        {
            if (sources.Add(comp))
                dirty.Add(comp);
        }

        public void DeregisterSuppressor(Comp_Suppression comp)
        {
            if (sources.Remove(comp))
                dirty.Add(comp); // rebuild to remove its footprint
        }

        // ── toggle / mark-dirty helpers (unchanged names) ──────────────────
        public void Toggle(Comp_Suppression comp, bool toggleOn)
        {
            // caller already flipped the comp’s enabled state; just flag rebuild
            if (sources.Contains(comp))
                dirty.Add(comp);
        }

        public void MarkDirty(List<Comp_Suppression> comps)
        {
            foreach (var c in comps)
                dirty.Add(c);
        }
        public void MarkDirty(Comp_Suppression comp) => dirty.Add(comp);

        // ── core rebuild routine ────────────────────────────────────────────
        private void RebuildGrids()
        {
            coveredBools.Clear();
            suppressionBools.Clear();

            foreach (var comp in sources)
            {
                // 1) radius coverage (regardless of power state)
                foreach (var cell in comp.SuppressionCells)
                    if (cell.InBounds(map))
                        coveredBools[cell] = true;

                // 2) active suppression cells (device powered / enabled)
                if (!comp.Enabled) continue; // your comp’s own flag
                foreach (var cell in comp.SuppressionCells)
                    if (cell.InBounds(map))
                        suppressionBools[cell] = true;
            }
        }
    }
}
