using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;

namespace TiberiumRim.Static
{
    public static class TiberiumCrystalResolver
    {
        public static TiberiumCrystalDef GetCrystalDefFor(NetworkValueDef netDef)
        {
            if (netDef == null || netDef.tags == null)
                return null;

            if (netDef.tags.Contains("GREEN"))
                return TiberiumDefOf.TiberiumGreen;
            if (netDef.tags.Contains("BLUE"))
                return TiberiumDefOf.TiberiumBlue;
            if (netDef.tags.Contains("RED"))
                return TiberiumDefOf.TiberiumRed;

            return null;
        }

        public static TiberiumProducerDef GetBlossomTreeFor(NetworkValueDef netDef)
        {
            if (netDef == null || netDef.tags == null)
                return null;

            if (netDef.tags.Contains("GREEN"))
                return TiberiumDefOf.BlossomTree;
            if (netDef.tags.Contains("BLUE"))
                return TiberiumDefOf.BlueBlossomTree;

            return null;
        }
    }

}
