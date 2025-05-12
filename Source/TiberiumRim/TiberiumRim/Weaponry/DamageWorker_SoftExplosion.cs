using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class DamageWorker_SoftExplosion : DamageWorker
    {
        private static readonly ThingDef Mote_ExplosionFlash =
            DefDatabase<ThingDef>.GetNamed("Mote_ExplosionFlash", errorOnFail: false);

        public override void ExplosionStart(Explosion explosion, List<IntVec3> cellsToAffect)
        {
            if (this.def.explosionHeatEnergyPerCell > 1.401298E-45f)
            {
                GenTemperature.PushHeat(
                    explosion.Position,
                    explosion.Map,
                    this.def.explosionHeatEnergyPerCell * cellsToAffect.Count
                );
            }

            if (Mote_ExplosionFlash != null)
            {
                MoteMaker.MakeStaticMote(
                    explosion.Position,
                    explosion.Map,
                    Mote_ExplosionFlash,
                    explosion.radius * 6f
                );
            }

            if (explosion.Map == Find.CurrentMap)
            {
                float magnitude = (explosion.Position.ToVector3Shifted() - Find.Camera.transform.position).magnitude;
                Find.CameraDriver.shaker.DoShake(4f * explosion.radius / magnitude);
            }

            ExplosionVisualEffectCenter(explosion);
        }
    }
}
