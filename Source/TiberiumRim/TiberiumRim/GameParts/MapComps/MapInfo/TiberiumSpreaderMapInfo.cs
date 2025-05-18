// ─────────────────────────────────────────────────────────────────────────────
//  TIBERIUM SPREADER  (fast-growth bursts around producers)
//  • Iterates all crystals in fields flagged MarkedForFastGrowth.
//  • Calls TickLong() on each crystal once per “long tick” burst.
//  • Obeys the new suppression mask: crystals in suppressed cells are skipped.
// ─────────────────────────────────────────────────────────────────────────────

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Retains the classic “fast growth” mechanic tied to <see cref="TiberiumProducer"/>.
    /// Ambient, soil-based growth now lives in a separate manager; this class only
    /// handles the accelerated burst that occurs when refineries flag a field
    /// <c>MarkedForFastGrowth</c>.
    /// </summary>
    public class TiberiumSpreaderMapInfo : MapInformation
    {
        // ── dependencies ───────────────────────────────────────────────────
        private readonly TiberiumSuppressionMapInfo suppression;   // new mask

        // ── state ──────────────────────────────────────────────────────────
        private readonly List<TiberiumProducer> producers = new List<TiberiumProducer>();
        private IEnumerator<TiberiumCrystal>? crystalIterator;

        // ── constructor ────────────────────────────────────────────────────
        public TiberiumSpreaderMapInfo(Map map, TiberiumSuppressionMapInfo suppression)
            : base(map)
        {
            this.suppression = suppression;
        }

        // ── producer registration helpers (unchanged API) ─────────────────
        public void RegisterField(TiberiumProducer p) => producers.Add(p);
        public void DeregisterField(TiberiumProducer p) => producers.Remove(p);

        // ── LINQ shortcuts identical to legacy ----------------------------
        public IEnumerable<TiberiumField> TiberiumFields => producers.Select(p => p.TiberiumField);
        public IEnumerable<TiberiumField> MarkedFields => TiberiumFields.Where(f => f.MarkedForFastGrowth);
        public IEnumerable<TiberiumCrystal> TiberiumCrystals
            => MarkedFields.SelectMany(f => f.GrowingCrystals);

        private bool ShouldSpread => MarkedFields.Any();
        private bool CanReset => TiberiumCrystals.Any();

        // ── main tick — fires each game tick from MapComponent_Tiberium ----
        public override void Tick()
        {
            if (!ShouldSpread) return;

            // Initialise enumerator the moment we have crystals and none yet iterating
            if (crystalIterator == null && CanReset)
            {
                ResetIterator();
            }
            else if (crystalIterator != null)
            {
                // Walk the enumerator in one tick – preserves legacy behaviour.
                do
                {
                    var crystal = crystalIterator.Current;
                    if (crystal?.Spawned ?? false)
                    {
                        // NEW: Skip crystals growing inside a suppressed cell.
                        if (!suppression.IsSuppressed(crystal.Position))
                            crystal.TickLong();
                    }
                }
                while (crystalIterator.MoveNext());

                // Finished the list – discard until next burst.
                crystalIterator = null;
            }
        }

        // ── internal: rebuild the enumerator list once per burst -----------
        private void ResetIterator()
        {
            var list = TiberiumCrystals.ToList();
            if (list.Any())
                crystalIterator = list.GetEnumerator();
        }
    }
}
