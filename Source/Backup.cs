using System;
using System.Collections.Generic;
using Verse;

namespace ChangeEquipmentStats
{
    class Backup
    {
        private readonly static Dictionary<ThingDef, Stats> backup = new Dictionary<ThingDef, Stats>();

        public static void Init(IEnumerable<ThingDef> apparelDefs, IEnumerable<ThingDef> weaponDefs)
        {
            if (backup == null || backup.Count == 0)
            {
                foreach (ThingDef d in apparelDefs)
                {
                    backup.Add(d, new Stats(d));
                }

                foreach (ThingDef d in weaponDefs)
                {
                    backup.Add(d, new Stats(d));
                }
            }
        }

        public static void ApplyDefaultStats(ThingDef def)
        {
            if (backup.TryGetValue(def, out Stats s))
            {
                s.ApplyStats(def);
            }
        }
    }
}
