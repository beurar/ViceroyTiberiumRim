using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_SonicInhibitor : ThingComp
    {
        public CompProperties_SonicInhibitor Props => (CompProperties_SonicInhibitor) base.props;

        private static readonly ThingDef Mote_PsycastAreaEffect =
    DefDatabase<ThingDef>.GetNamed("Mote_PsycastAreaEffect", errorOnFail: false);

        public override void CompTickRare()
        {
            if (Mote_PsycastAreaEffect == null)
            {
                Log.Warning("Mote_PsycastAreaEffect not found. Skipping visual effect.");
            }
            else
            {
                MoteMaker.MakeStaticMote(parent.Position, parent.Map, Mote_PsycastAreaEffect, Props.radius * 0.35f);
                MoteMaker.MakeStaticMote(parent.Position, parent.Map, Mote_PsycastAreaEffect, Props.radius);
            }

            foreach (var intVec3 in GenRadial.RadialCellsAround(parent.Position, Props.radius, true))
            {
                var tib = intVec3.GetTiberium(parent.Map);
                tib?.TakeDamage(new DamageInfo(TRDamageDefOf.TRSonic, TRUtils.Range(Props.damageRange)));
            }
        }


        private void StartEmission()
        {

        }

        
    }

    public class CompProperties_SonicInhibitor : CompProperties
    {
        public float radius = 10;
        public FloatRange damageRange = new FloatRange(2, 10);
        public CompProperties_SonicInhibitor()
        {
            compClass = typeof(Comp_SonicInhibitor);
        }
    }
}
