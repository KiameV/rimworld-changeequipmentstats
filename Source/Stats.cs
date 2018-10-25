using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ChangeEquipmentStats
{
    class Stats : IExposable
    {
        public string label = null;
        public bool IsApparel = false;
        public bool IsWeapon = false;
        public List<Stat> StatModifiers = null;
        public List<VerbStats> VerbStats = null;
        public List<ToolStats> Tools = null;

        public Stats() { }
        public Stats(ThingDef d)
        {
            this.label = d.label;
            this.IsApparel = d.IsApparel;
            this.IsWeapon = d.IsWeapon;
            this.SetStatModifiers(d.statBases);
            this.SetVerbs(d.Verbs);
            this.SetTools(d.tools);
        }

        public void SetTools(List<Tool> tools)
        {
            if (tools != null)
            {
                this.Tools = new List<ToolStats>(tools.Count);
                foreach (Tool t in tools)
                {
                    this.Tools.Add(new ToolStats(t));
                }
            }
        }

        public void SetVerbs(List<VerbProperties> verbs)
        {
            if (verbs != null)
            {
                this.VerbStats = new List<VerbStats>(verbs.Count);
                foreach (VerbProperties v in verbs)
                {
                    this.VerbStats.Add(new VerbStats(v));
                }
            }
        }

        public void SetStatModifiers(List<StatModifier> modifiers)
        {
            if (modifiers != null)
            {
                this.StatModifiers = new List<Stat>(modifiers.Count);
                foreach (StatModifier m in modifiers)
                {
                    this.StatModifiers.Add(
                        new Stat
                        {
                            stat = m.stat.ToString(),
                            value = m.value
                        });
                }
            }
        }

        public void ApplyStats(ThingDef d)
        {
#if DEBUG
            Log.Warning("ApplyStats for " + d.label);
#endif
            if (this.StatModifiers != null && d.statBases != null)
            {
                foreach (Stat from in this.StatModifiers)
                {
                    foreach (StatModifier to in d.statBases)
                    {
                        if (to.stat.ToString().EqualsIgnoreCase(from.stat))
                        {
#if DEBUG
                            Log.Message(to.stat + " = " + to.value);
#endif
                            to.value = from.value;
                            break;
                        }
                    }
                }
            }

            if (this.VerbStats != null && d.Verbs != null)
            {
                foreach (VerbStats from in this.VerbStats)
                {
                    foreach (VerbProperties to in d.Verbs)
                    {
                        if (to.verbClass.Name.Equals(from.name))
                        {
#if DEBUG
                            Log.Message("warmupTime = " + from.warmupTime);
                            Log.Message("range = " + from.range);
                            Log.Message("timeBetweenShots = " + from.timeBetweenShots);
                            Log.Message("burstShotCount = " + from.burstShotCount);
                            Log.Message("muzzleFlashScale = " + from.muzzleFlashScale);
                            Log.Message("aiAvoidFriendlyRadius = " + from.aiAvoidFriendlyRadius);
#endif
                            to.warmupTime = from.warmupTime;
                            to.range = from.range;
                            to.ticksBetweenBurstShots = (int)from.timeBetweenShots;
                            to.burstShotCount = (int)from.burstShotCount;
                            to.muzzleFlashScale = from.muzzleFlashScale;
                            to.ai_AvoidFriendlyFireRadius = from.aiAvoidFriendlyRadius;

                            break;
                        }
                    }
                }
            }

            if (this.Tools != null && d.tools != null)
            {
                foreach (ToolStats from in this.Tools)
                {
                    foreach (Tool to in d.tools)
                    {
                        if (from.label.Equals(to.label))
                        {
                            to.power = from.power;
                            to.armorPenetration = from.armorPenetration;
                            to.cooldownTime = from.cooldownTime;
                            break;
                        }
                    }
                }
            }
#if DEBUG
            Log.Warning("ApplyStats Done");
#endif
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.label, "label");
            Scribe_Values.Look(ref this.IsApparel, "isApparel");
            Scribe_Values.Look(ref this.IsWeapon, "isWeapon");
            Scribe_Collections.Look(ref this.StatModifiers, "statModifiers", LookMode.Deep);
            Scribe_Collections.Look(ref this.VerbStats, "verbStats", LookMode.Deep);
            Scribe_Collections.Look(ref this.Tools, "tools", LookMode.Deep);
        }
    }

    class Stat : IExposable
    {
        public string stat;
        public float value;

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.stat, "stat");
            Scribe_Values.Look(ref this.value, "value");
        }
    }

    class VerbStats : IExposable
    {
        public string name;
        public float warmupTime = 0;
        public float range = 0;
        public float timeBetweenShots = 0;
        public float burstShotCount = 0;
        public float muzzleFlashScale = 0;
        public float aiAvoidFriendlyRadius = 0;
        public string projectile;

        public VerbStats() { }
        public VerbStats(VerbProperties v)
        {
            this.name = v.verbClass.Name;
            this.warmupTime = v.warmupTime;
            this.range = v.range;
            this.timeBetweenShots = v.ticksBetweenBurstShots;
            this.burstShotCount = v.burstShotCount;
            this.muzzleFlashScale = v.muzzleFlashScale;
            this.aiAvoidFriendlyRadius = v.ai_AvoidFriendlyFireRadius;
            if (v.defaultProjectile != null)
            {
                this.projectile = v.defaultProjectile.defName;
                ProjectileStats.Add(v.defaultProjectile);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.name, "name");
            Scribe_Values.Look(ref this.warmupTime, "warmupTime");
            Scribe_Values.Look(ref this.range, "range");
            Scribe_Values.Look(ref this.timeBetweenShots, "timeBetweenShots");
            Scribe_Values.Look(ref this.burstShotCount, "burstShotCount");
            Scribe_Values.Look(ref this.muzzleFlashScale, "muzzleFlashScale");
            Scribe_Values.Look(ref this.aiAvoidFriendlyRadius, "aiAvoidFriendlyRadius");
            Scribe_Values.Look(ref this.projectile, "projectile");
        }
    }

    class ToolStats : IExposable
    {
        public string label;
        public float power;
        public float armorPenetration;
        public float cooldownTime;

        public ToolStats() { }
        public ToolStats(Tool t)
        {
            this.label = t.label;
            this.power = t.power;
            this.armorPenetration = t.armorPenetration;
            this.cooldownTime = t.cooldownTime;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.label, "label");
            Scribe_Values.Look(ref this.power, "power");
            Scribe_Values.Look(ref this.armorPenetration, "armorPenetration");
            Scribe_Values.Look(ref this.cooldownTime, "cooldownTime");
        }
    }

    class ProjectileStats : IExposable
    {
        public static List<ProjectileStats> Stats = new List<ProjectileStats>();

        public string defName;
        public int damage;
        public float stoppingPower;
        public float armorPenetration;
        public float speed;

        public ProjectileStats() { }
        public ProjectileStats(ThingDef d)
        {
            this.defName = d.defName;
            this.damage = GetDamage(d.projectile);
            this.stoppingPower = d.projectile.stoppingPower;
            this.armorPenetration = GetArmorPenetration(d.projectile);
            this.speed = d.projectile.speed;
        }

        public static void Add(ThingDef def)
        {
            foreach (ProjectileStats s in Stats)
            {
                if (s.defName.Equals(def.defName))
                {
                    return;
                }
            }
            Stats.Add(new ProjectileStats(def));
        }

        public void ApplyStats(ThingDef def)
        {
            foreach(ProjectileStats s in Stats)
            {
                if (s.defName.Equals(def.defName))
                {
                    SetDamage(def.projectile, this.damage);
                    def.projectile.stoppingPower = this.stoppingPower;
                    SetArmorPenetration(def.projectile, this.armorPenetration);
                    def.projectile.speed = this.speed;
                    break;
                }
            }
        }

        public static int GetDamage(ProjectileProperties p)
        {
            FieldInfo fi = typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance);
            return (int)fi.GetValue(p);
        }

        public static void SetDamage(ProjectileProperties p, int value)
        {
            FieldInfo fi = typeof(ProjectileProperties).GetField("damageAmountBase", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(p, value);
        }

        public static float GetArmorPenetration(ProjectileProperties p)
        {
            FieldInfo fi = typeof(ProjectileProperties).GetField("armorPenetrationBase", BindingFlags.NonPublic | BindingFlags.Instance);
            return (float)fi.GetValue(p);
        }

        public static void SetArmorPenetration(ProjectileProperties p, float value)
        {
            FieldInfo fi = typeof(ProjectileProperties).GetField("armorPenetrationBase", BindingFlags.NonPublic | BindingFlags.Instance);
            fi.SetValue(p, value);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.defName, "defName");
            Scribe_Values.Look(ref this.damage, "damage");
            Scribe_Values.Look(ref this.stoppingPower, "stoppingPower");
            Scribe_Values.Look(ref this.armorPenetration, "armorPenetration");
            Scribe_Values.Look(ref this.speed, "speed");
        }
    }
}
