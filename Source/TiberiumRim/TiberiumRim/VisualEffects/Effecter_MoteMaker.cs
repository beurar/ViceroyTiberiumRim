﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Effecter_MoteMaker : Effecter
    {
        public EffecterDefTR def => (EffecterDefTR)base.def;

        public Effecter_MoteMaker(EffecterDef def) : base(def)
        {
        }

        public void Tick(TargetInfo A, TargetInfo B)
        {
            foreach (var t in this.children)
            {
                (t as SubEffecter_MoteMaker).Tick(def.tickInterval, A, B);
            }
        }
    }

    public class SubEffecter_MoteMaker : SubEffecter_Sprayer
    {
        private int ticksUntilMote = 0;

        public SubEffecter_MoteMaker(SubEffecterDef def, Effecter parent) : base(def, parent)
        {
        }

        public override void SubEffectTick(TargetInfo A, TargetInfo B)
        {
            base.MakeMote(A, B);
        }

        public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1, bool force = false)
        {
            if (Rand.Value < def.chancePerTick)
            {
                base.MakeMote(A, B);
            }
        }

        public void Tick(int ticks, TargetInfo A, TargetInfo B)
        {
            if (ticksUntilMote <= 0)
            {
                if (Rand.Value < def.chancePerTick)
                {
                    SubEffectTick(A, B);
                }
                ticksUntilMote = def.ticksBetweenMotes;
            }
            ticksUntilMote -= ticks;
        }

        /*
        public void Tick(int interval, TargetInfo A, TargetInfo B)
        {
            if (ticksUntilMote <= 0)
            {
                if (def.chancePerTick >= 1f)
                    SubEffectTick(A, B);
                else
                    SubTrigger(A, B);
                ticksUntilMote = def.ticksBetweenMotes;
            }
            ticksUntilMote -= interval;
        }
        */

    }
}
