using System;
using System.Collections.Generic;
using Verse;

namespace ChangeEquipmentStats
{
    class Backup
    {
        private readonly static Dictionary<ThingDef, Stats> backup = new Dictionary<ThingDef, Stats>();
        private readonly static Dictionary<ThingDef, ProjectileStats> pbackup = new Dictionary<ThingDef, ProjectileStats>();

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

            if (pbackup == null || pbackup.Count == 0)
            {
                foreach (ThingDef d in weaponDefs)
                {
                    if (d.Verbs != null)
                    {
                        foreach (VerbProperties v in d.Verbs)
                        {
                            if (v.defaultProjectile != null)
                            {
                                if (!pbackup.ContainsKey(v.defaultProjectile))
                                {
                                    pbackup.Add(v.defaultProjectile, new ProjectileStats(v.defaultProjectile));
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ApplyDefaultStats(ThingDef def)
        {
            if (backup.TryGetValue(def, out Stats s))
            {
                s.ApplyStats(def);
                if (def.Verbs != null)
                {
                    foreach (VerbProperties v in def.Verbs)
                    {
                        if (v.defaultProjectile != null)
                        {
                            if (pbackup.TryGetValue(def, out ProjectileStats p))
                            {
                                p.ApplyStats(v.defaultProjectile);
                            }
                        }
                    }
                }
            }
        }
    }
}
