using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim.Weaponry
{
    public class Projectile_Instant : Projectile
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
        }

        protected override void Impact(Thing hitThing, bool blockedByShield)
        {
            GenClamor.DoClamor(this, 2.1f, ClamorDefOf.Impact);
        }

        protected virtual void Finish()
        {
            this.Destroy();
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
        }
    }
}
