﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class WeatherEvent_IonStrike : WeatherEvent
    {
        //RGB (250,250,80)
        //private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new Color(0.980392156f‬, 0.980392156f‬, 0.012254901f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.95f, 1f), 1.15f);
        private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new ColorInt(250, 250, 80).ToColor, 
                                                                                   new Color(0.784313738f, 0.8235294f, 0.847058833f), 
                                                                                   new Color(0.9f, 0.95f, 1f), 0.8f);
        private static readonly SkyTarget Target = new SkyTarget(1, LightningFlashColors, 1, 1);

        private const int FlashFadeInTicks = 3;
        private const int MinFlashDuration = 15;
        private const int MaxFlashDuration = 60;
        private const float FlashShadowDistance = 5f;

        private int duration;
        private int ageTicks;

        private Mesh boltMesh;
        private IntVec3 strikeLoc = IntVec3.Invalid;
        private Vector2 shadowVector;

        public WeatherEvent_IonStrike(Map map) : base(map)
        {
            duration = TRUtils.Range(15, 60);
            this.shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
        }

        public override void FireEvent()
        {
            if (DoStrike)
            {
                if (!strikeLoc.IsValid)
                    strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
                boltMesh = LightningBoltMeshPool.RandomBoltMesh;
                if (strikeLoc.Fogged(map)) return;
                GenExplosion.DoExplosion(this.strikeLoc, this.map, 1.9f, DamageDefOf.Bomb, null, -1, -1f, null, null, null, null, null, 0f, 1, null, false, null, 0f, 1, 0f, false);
                Vector3 loc = strikeLoc.ToVector3Shifted();
                for (int i = 0; i < 4; i++)
                {
                    ThrowSmoke(loc, map, 1.5f);
                    ThrowMicroSparks(loc, map);
                    LightningGlow(loc, map, 1.5f);
                    //MoteMaker.ThrowLightningGlow(loc, this.map, 1.5f);
                }
                SoundDefOf.Thunder_OnMap.PlayOneShot(SoundInfo.InMap(new TargetInfo(strikeLoc, map, true)));
            }
            else
            {
                SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
            }
        }

        private static void ThrowSmoke(Vector3 loc, Map map, float scale)
        {
            var def = DefDatabase<ThingDef>.GetNamed("Mote_Smoke", errorOnFail: false);
            if (def == null || !loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;

            if (ThingMaker.MakeThing(def) is MoteThrown mote)
            {
                mote.Scale = scale;
                mote.rotationRate = Rand.Range(-30f, 30f);
                mote.exactPosition = loc;
                mote.SetVelocity(Rand.Range(0f, 360f), Rand.Range(0.6f, 0.75f));
                GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
            }
        }

        private static void ThrowMicroSparks(Vector3 loc, Map map)
        {
            var def = DefDatabase<ThingDef>.GetNamed("Mote_MicroSparks", errorOnFail: false);
            if (def == null || !loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;

            if (ThingMaker.MakeThing(def) is MoteThrown mote)
            {
                mote.Scale = Rand.Range(0.2f, 0.6f);
                mote.rotationRate = Rand.Range(-120f, 120f);
                mote.exactPosition = loc;
                mote.SetVelocity(Rand.Range(0f, 360f), Rand.Range(1f, 1.5f));
                GenSpawn.Spawn(mote, loc.ToIntVec3(), map);
            }
        }


        private static readonly ThingDef Mote_LightningGlow =
    DefDatabase<ThingDef>.GetNamed("Mote_LightningGlow", errorOnFail: false);

        private void LightningGlow(Vector3 loc, Map map, float size)
        {
            if (Mote_LightningGlow == null)
            {
                Log.Warning("Mote_LightningGlow not found. Skipping lightning glow effect.");
                return;
            }

            if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;

            if (ThingMaker.MakeThing(Mote_LightningGlow) is MoteThrown moteThrown)
            {
                moteThrown.instanceColor = LightningFlashColors.sky;
                moteThrown.Scale = Rand.Range(4f, 6f) * size;
                moteThrown.rotationRate = Rand.Range(-3f, 3f);
                moteThrown.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
                moteThrown.SetVelocity(Rand.Range(0f, 360f), 1.2f);
                GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map, WipeMode.Vanish);
            }
        }


        public override void WeatherEventTick()
        {
            ageTicks++;
        }

        public override void WeatherEventDraw()
        {
            Graphics.DrawMesh(this.boltMesh, this.strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity,  FadedMaterialPool.FadedVersionOf(TiberiumContent.IonLightningMat, LightningBrightness), 0);
        }

        public override bool Expired => ageTicks > duration;
        public override float SkyTargetLerpFactor => LightningBrightness;
        public override SkyTarget SkyTarget => Target;
        public override Vector2? OverrideShadowVector => this.shadowVector;

        private float LightningBrightness
        {
            get
            {
                if (this.ageTicks <= 3)
                {
                    return (float)this.ageTicks / 3f;
                }
                return 1f - (float)this.ageTicks / (float)this.duration;
            }
        }

        private bool DoStrike => TRUtils.Chance(0.4f);
    }
}
