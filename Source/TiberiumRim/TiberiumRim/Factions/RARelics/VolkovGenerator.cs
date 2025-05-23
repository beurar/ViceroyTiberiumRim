﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    /*
    public static class VolkovGenerator
    {
        private static Pawn Volkov;

        public static Pawn GenerateVolkovCyborg(Map map)
        {
            Pawn volkov = GenerateVolkov(map);
            var parts = volkov.def.race.body.AllParts;
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("RegenerativeNanites"));
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("AugmentedEye"), parts.FirstOrDefault(p => p.IsInGroup(BodyPartGroupDefOf.Eyes)));
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CannonImplant"), parts.FirstOrDefault(p => p.def == BodyPartDefOf.Arm));
            return volkov;
        }

        public static Pawn GenerateVolkov(Map map)
        {
            Pawn pawn = (Pawn) ThingMaker.MakeThing(RedAlertDefOf.Volkov.race);
            pawn.kindDef = RedAlertDefOf.Volkov;
            pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(RedAlertDefOf.SovjetFaction));
            PawnComponentsUtility.CreateInitialComponents(pawn);

            pawn.gender = Gender.Male;
            
            //Age
            pawn.ageTracker.AgeBiologicalTicks = 30 * GenDate.TicksPerYear;
            pawn.ageTracker.AgeChronologicalTicks = (GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile).x) - 1948) * GenDate.TicksPerYear;
            pawn.ageTracker.BirthAbsTicks = (long)GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;

            //Needs
            pawn.needs.SetInitialLevels();
            //pawn.needs.mood = new Need_Mood(pawn);

            NameTriple name = new NameTriple("Volkov", "Volkov", "Commando");
            pawn.Name = name;
            pawn.story.birthLastName = name.Last;

            pawn.story.Childhood = BackstoryFrom(BackstorySlot.Adulthood);
            pawn.story.Adulthood = BackstoryFrom(BackstorySlot.Childhood);

            pawn.story.skinColorOverride = new UnityEngine.Color(0.831f, 0.471f, 0.063f);
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.story.headType = DefDatabase<HeadTypeDef>.GetNamed("Narrow");
            pawn.story.HairColor = default;
            pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");

            pawn.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamed("Tough"), DefDatabase<TraitDef>.GetNamed("Tough").degreeDatas.First().degree, true));
            pawn.story.traits.GainTrait(new Trait(DefDatabase<TraitDef>.GetNamed("ShootingAccuracy"), DefDatabase<TraitDef>.GetNamed("ShootingAccuracy").degreeDatas.First().degree, true));
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Transhumanist, TraitDefOf.Transhumanist.degreeDatas.First().degree, true));

            //Skills
            var allSkills = DefDatabase<SkillDef>.AllDefsListForReading;
            foreach (var skill in allSkills)
            {
                //TODO: Test Range Only, adjust for finalized value LOOK AT: PawnGenerator.FinalLevelOfSkill
                int skillLevel = Rand.Range(0, 20);
                SkillRecord skillRec = pawn.skills.GetSkill(skill);
                skillRec.Level = skillLevel;
                if(skillRec.TotallyDisabled) continue;
                
            }

            if (pawn.workSettings != null && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                pawn.workSettings.EnableAndInitialize();
            }

            if (Find.Scenario != null)
            {
                Find.Scenario.Notify_NewPawnGenerating(pawn, PawnGenerationContext.NonPlayer);
            }

            //Gear
            Apparel pants = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_Pants"), ThingDef.Named("Leather_Panthera"));
            Apparel shirt = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Leather_Wolf"));
            Apparel jacket = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_Jacket"), ThingDef.Named("Leather_Bear"));
            pawn.apparel.Wear(pants);
            pawn.apparel.Wear(shirt);
            pawn.apparel.Wear(jacket);
            ThingWithComps teslaGun = (ThingWithComps) ThingMaker.MakeThing(ThingDef.Named("Sovjet_TeslaGun"));
            pawn.equipment.AddEquipment(teslaGun);

            var parts = pawn.def.race.body.AllParts;
            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("AugmentedEye"), parts.FirstOrDefault(p => p.IsInGroup(BodyPartGroupDefOf.Eyes)));
            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CannonImplant"), parts.FirstOrDefault(p => p.def == BodyPartDefOf.Arm));

            return pawn;
        }

        private static BackstoryDef BackstoryFrom(BackstorySlot slot)
        {
            return new BackstoryDef()
            {
                baseDesc = "Test description",
                slot = slot,
            };
        }

        public static Pawn GenerateChitzkoi(Map map)
        {
            PawnGenerationRequest request = new PawnGenerationRequest()
            {
                Context = PawnGenerationContext.NonPlayer,
                KindDef = RedAlertDefOf.Chitzkoi,
            };
            return PawnGenerator.GeneratePawn(request);
        }
    }
    */
}
