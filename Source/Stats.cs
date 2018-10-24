using RimWorld;
using System.Collections.Generic;
using Verse;

namespace ChangeEquipmentStats
{
    class Stats : IExposable
    {
        public string DefName = null;
        public bool IsApparel = false;
        public bool IsWeapon = false;
        public List<Stat> StatModifiers = null;
        public List<VerbStats> VerbStats = null;
        public List<ToolStats> Tools = null;

        public Stats() { }
        public Stats(ThingDef d)
        {
            this.DefName = d.defName;
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
                            Log.Message(to.stat + " = " + to.value);
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
            Scribe_Values.Look(ref this.DefName, "defName");
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
}
