﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumStructureMapInfo : MapInformation
    {
        public HashSet<TiberiumProducer> AllProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumProducer> ResearchProducers = new HashSet<TiberiumProducer>();
        public HashSet<TiberiumBlossom> Blossoms = new HashSet<TiberiumBlossom>();
        public HashSet<TiberiumGeyser> Geysers = new HashSet<TiberiumGeyser>();

        public TiberiumStructureMapInfo(Map map) : base(map) { }

        public TiberiumProducer ClosestProducer(Pawn seeker)
        {
            return AllProducers.MinBy(x => x.Position.DistanceTo(seeker.Position));
        }

        public void TryRegister(TRBuilding tibobj)
        {
            switch (tibobj)
            {
                case TiberiumProducer p:
                {
                    AllProducers.Add(p);
                    if (p.def.forResearch)
                        ResearchProducers.Add(p);
                    if (p is TiberiumBlossom b)
                        Blossoms.Add(b);
                    break;
                }
                case TiberiumGeyser g:
                    Geysers.Add(g);
                    break;
            }
        }

        public void Deregister(TRBuilding tibobj)
        {
            switch (tibobj)
            {
                case TiberiumProducer p:
                {
                    AllProducers.Remove(p);
                    if (p.def.forResearch)
                        ResearchProducers.Remove(p);
                    if (p is TiberiumBlossom b)
                        Blossoms.Remove(b);
                    break;
                }
                case TiberiumGeyser g:
                    Geysers.Remove(g);
                    break;
            }
        }
    }
}
