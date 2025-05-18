using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using TeleCore.FlowCore;
using TeleCore;

namespace TiberiumRim
{
    /// <summary>
    /// Defines a cost in terms of Tiberium volumes from the TeleCore fluid system.
    /// </summary>
    public class TiberiumCost
    {
        public bool useDirectStorage = false;

        // General cost in total volume
        public float generalCost;

        // Specific fluid types and exact required amounts
        public List<NetworkValueCost> specificTypes;

        public bool HasSpecifics => specificTypes?.Any(c => c != null && c.amount > 0 && c.valueDef != null) == true;
        public float SpecificCost => specificTypes?.Where(c => c.HasValue).Sum(c => c.amount) ?? 0f;
        public float TotalCost => generalCost + SpecificCost;

        public IEnumerable<NetworkValueCost> SpecificCosts => specificTypes?.Where(s => s.HasValue) ?? Enumerable.Empty<NetworkValueCost>();
        public IEnumerable<NetworkValueDef> AllowedTypes => SpecificCosts.Select(c => c.valueDef);

        private float ValueForTypesWithoutSpecifics(Comp_TiberiumContainer container)
        {
            float totalValue = 0f;
            foreach (var type in AllowedTypes)
            {
                float stored = (float)container.Get(type);
                float specific = SpecificCosts.FirstOrDefault(c => c.valueDef == type)?.amount ?? 0f;
                totalValue += Mathf.Max(0f, stored - specific);
            }
            return totalValue;
        }

        public bool CanPayWith(Comp_TiberiumContainer container)
        {
            if (useDirectStorage)
            {
                float totalStored = (float)container.TotalStored;
                if (totalStored < TotalCost) return false;

                float needed = TotalCost;
                foreach (var typeCost in SpecificCosts)
                {
                    if (container.Get(typeCost.valueDef) >= typeCost.amount)
                        needed -= typeCost.amount;
                }

                if (generalCost > 0f)
                {
                    if (ValueForTypesWithoutSpecifics(container) >= generalCost)
                        needed -= generalCost;
                }

                return needed <= 0f;
            }

            return false; // external networks not handled in this context
        }

        public void PayWithContainerComp(Comp_TiberiumContainer container)
        {
            float totalCost = TotalCost;
            if (totalCost <= 0) return;

            foreach (var typeCost in SpecificCosts)
            {
                if (container.TryTake(typeCost.valueDef, typeCost.amount))
                    totalCost -= typeCost.amount;
            }

            foreach (var def in AllowedTypes)
            {
                double remaining = totalCost;
                if (container.TryTake(def, remaining))
                    totalCost -= (float)remaining;
            }

            if (totalCost > 0f)
                Log.Warning($"[TiberiumCost] Payment from {container.parent} incomplete. Remaining: {totalCost}");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("TiberiumCost(");
            sb.Append("General: ").Append(generalCost);

            foreach (var c in SpecificCosts)
            {
                sb.Append(" | ").Append(c.valueDef?.defName ?? "null").Append(": ").Append(c.amount);
            }
            sb.Append(")");
            return sb.ToString();
        }
    }

    public class NetworkValueCost
    {
        public NetworkValueDef valueDef;
        public float amount;

        public bool HasValue => valueDef != null && amount > 0f;
    }
}
