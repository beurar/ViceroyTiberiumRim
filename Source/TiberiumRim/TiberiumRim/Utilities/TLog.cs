﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public static class TLog
    {
        public static void Error(string msg)
        {
            Log.Error("[TiberiumRim] " + msg);
        }

        public static void Warning(string msg)
        {
            Log.Warning("[TiberiumRim] " + msg);
        }
    }
}
