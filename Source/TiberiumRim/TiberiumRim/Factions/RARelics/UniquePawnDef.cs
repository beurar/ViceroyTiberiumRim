﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class UniquePawnDef : Def
    {
        public PawnKindDef kindDef;
        public FactionDef faction;

        //Basic Body
        public BodyTypeDef bodyType;
        public HeadTypeDef crownType;
        public Gender gender;
        public HairDef hairDef;
        public float melanin;
        public Color hairColor;

        public List<TraitDef> traits;

        //Identity
        public NameTriple uniqueName;
        public int birthDate;
        public int biologicalAge;

        public UniqueBackstoryDef childhood;
        public UniqueBackstoryDef adulthood;

        //Equipment
        public List<ThingDef> weapons;
        public List<ThingDef> inventory;
        public List<UniqueApparel> clothes;

        public List<HediffDef> hediffs;
    }

    public class UniqueApparel
    {
        public ThingDef thing;
        public ThingDef stuff;
    }

    public class UniqueBackstoryDef : Def
    {
        public string identifier;
        public BackstorySlot slot;
        public string title;
        public string titleFemale;
        public string titleShort;
        public string titleShortFemale;
        public string baseDesc;
        public WorkTags workDisables;
        public WorkTags requiredWorkTags;
        public List<string> spawnCategories = new List<string>();
        public List<TraitDef> forcedTraits;
        public List<TraitDef> disallowedTraits;
        public List<string> hairTags;
        private string nameMaker;
        private RulePackDef nameMakerResolved;

        public Dictionary<string, int> skillGains;

        public bool shuffleable = true;

        public BackstoryDef BackstoryFromThis()
        {
            BackstoryDef backstory = new BackstoryDef()
            {
                identifier = identifier,
                slot = slot,
                
            };
            backstory.PostLoad();
            backstory.ResolveReferences();
            return backstory;
        }
    }
}
