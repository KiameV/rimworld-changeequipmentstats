using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using System;
using System.Text;

namespace ChangeEquipmentStats
{
    public class Controller : Mod
    {
        public static Settings Settings;

        public Controller(ModContentPack content) : base(content)
        {
            Settings = base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "ChangeEquipmentStats".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        private readonly SortedDictionary<string, ThingDef> ApparelDefs = new SortedDictionary<string, ThingDef>();
        private readonly SortedDictionary<string, ThingDef> WeaponDefs = new SortedDictionary<string, ThingDef>();
        private ThingDef selected = null;
        private List<string> comBuffer = new List<string>();
        private Vector2 scroll = Vector2.zero;

        private List<Stats> exposedStats = null;

        public void DoWindowContents(Rect rect)
        {
            this.Init();

            int x = 10;
            int y = 40;

            string label = (this.selected == null || this.selected.IsWeapon) ? "Apparel".Translate() : this.selected.label.ToString();
            if (Widgets.ButtonText(new Rect(x, y, 150, 30), label))
            {
                this.DrawFloatingOptions(this.ApparelDefs.Values);
            }

            label = (this.selected == null || this.selected.IsApparel) ? "ShootReportWeapon".Translate() : this.selected.label.ToString();
            if (Widgets.ButtonText(new Rect(x + 175, y, 150, 30), label))
            {
                this.DrawFloatingOptions(this.WeaponDefs.Values);
            }
            y += 60;

            x = 20;

            if (this.selected != null)
            {
                int inputFieldCount = 1;
                if (this.selected.statBases != null)
                    inputFieldCount += this.selected.statBases.Count;
                if (this.selected.Verbs != null)
                    inputFieldCount += this.selected.Verbs.Count * 6;
                if (this.selected.tools != null)
                    inputFieldCount += this.selected.tools.Count * 4;

                Widgets.BeginScrollView(
                    new Rect(x, y, rect.width - 40, rect.height - y - 120),
                    ref this.scroll,
                    new Rect(0, 0, rect.width - 56, 20 + (40 * inputFieldCount)));

                y = 0;

                int comBufferIndex = 0;
                if (this.selected.statBases != null)
                {
                    foreach (StatModifier m in this.selected.statBases)
                    {
                        this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, m.stat.ToString(), this.comBuffer[comBufferIndex]);
                        ++comBufferIndex;
                    }
                }

                if (this.selected.Verbs != null)
                {
                    foreach (VerbProperties v in this.selected.Verbs)
                    {
                        this.DrawVerbInput(x, ref y, ref comBufferIndex, v);
                    }
                }

                if (this.selected.tools != null)
                {
                    foreach (Tool t in this.selected.tools)
                    {
                        this.DrawToolInput(x, ref y, ref comBufferIndex, t);
                    }
                }

                Widgets.EndScrollView();

                if (Widgets.ButtonText(new Rect(x, rect.yMax - 100, 100, 32), "ChangeEquipmentStats.Apply".Translate()))
                {
                    this.Apply();
                }

                if (Widgets.ButtonText(new Rect(x + 120, rect.yMax - 100, 100, 32), "Reset".Translate()))
                {
                    Backup.ApplyDefaultStats(this.selected);
                    this.SetTextBuffer(this.selected);
                }
            }


            if (Widgets.ButtonText(new Rect(10, rect.yMax - 32, 100, 32), "ChangeEquipmentStats.ResetAll".Translate()))
            {
                foreach (ThingDef d in this.ApparelDefs.Values)
                {
                    Backup.ApplyDefaultStats(d);
                }

                foreach (ThingDef d in this.WeaponDefs.Values)
                {
                    Backup.ApplyDefaultStats(d);
                }

                if (this.selected != null)
                    this.SetTextBuffer(this.selected);
            }
        }

        private void DrawToolInput(int x, ref int y, ref int comBufferIndex, Tool t)
        {
            StringBuilder sb = new StringBuilder(t.label);
            if (t.capacities != null)
            {
                foreach (ToolCapacityDef c in t.capacities)
                {
                    sb.Append(" (");
                    sb.Append(c.defName);
                    sb.Append(")");
                }
            }
            Widgets.Label(new Rect(x, y, 300, 32), sb.ToString());
            y += 40;

            this.comBuffer[comBufferIndex] = this.DrawInput(x + 20, ref y, "Power", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x + 20, ref y, "Armor Penetration", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x + 20, ref y, "Cooldown Time", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
        }

        private string DrawInput(int x, ref int y, string label, string valueBuffer)
        {
            Widgets.Label(new Rect(x, y, 225, 32), label);
            string s = Widgets.TextField(new Rect(x + 235, y, 75, 32), valueBuffer);
            y += 40;
            return s;
        }

        private void Apply()
        {
            ThingDef d = this.selected;
            Stats s = new Stats
            {
                label = d.defName,
                IsApparel = d.IsApparel,
                IsWeapon = d.IsWeapon
            };

            int index = 0;
            if (d.statBases != null)
            {
                s.StatModifiers = new List<Stat>(d.statBases.Count);
                foreach (StatModifier sm in d.statBases)
                {
                    float f = this.Parse(index, sm.value, sm.stat.ToString());
                    s.StatModifiers.Add(
                        new Stat
                        {
                            stat = sm.stat.ToString(),
                            value = f
                        });
                    ++index;
                }
            }

            if (d.Verbs != null)
            {
                s.VerbStats = new List<VerbStats>(d.Verbs.Count);
                foreach (VerbProperties vp in d.Verbs)
                {
                    s.VerbStats.Add(
                        new VerbStats
                        {
                            name = vp.verbClass.Name,
                            warmupTime = this.Parse(index, vp.warmupTime, "warm up"),
                            range = this.Parse(index + 1, vp.range, "range"),
                            timeBetweenShots = (int)this.Parse(index + 2, vp.ticksBetweenBurstShots, "time between shots"),
                            burstShotCount = (int)this.Parse(index + 3, vp.burstShotCount, "burst shot count"),
                            muzzleFlashScale = this.Parse(index + 4, vp.muzzleFlashScale, "muzzle flash"),
                            aiAvoidFriendlyRadius = this.Parse(index + 5, vp.ai_AvoidFriendlyFireRadius, "ai avoid radius"),
                        });
                    index += 6;
                }
            }

            if (d.tools != null)
            {
                s.Tools = new List<ToolStats>(d.tools.Count);
                foreach (Tool t in d.tools)
                {
                    s.Tools.Add(
                        new ToolStats
                        {
                            label = t.label,
                            power = this.Parse(index, t.power, "power"),
                            armorPenetration = this.Parse(index, t.armorPenetration, "armorPenetration"),
                            cooldownTime = this.Parse(index, t.cooldownTime, "cooldownTime"),
                        });
                }
            }

            s.ApplyStats(d);

            Messages.Message("Settings applied to " + d.label, MessageTypeDefOf.PositiveEvent);
        }

        private float Parse(int i, float original, string label)
        {
            if (i >= this.comBuffer.Count)
            {
                const string s = "Index gte to comBuffer count";
                Log.ErrorOnce(s, s.GetHashCode());
                return original;
            }

            if (float.TryParse(this.comBuffer[i], out float f))
                return f;
            Log.Error(label + ": Unable to convert [" + this.comBuffer[i] + "] to a number.");
            return original;
        }

        private void SetTextBuffer(ThingDef d)
        {
            for (int i = 0; i < comBuffer.Count; ++i)
                comBuffer[i] = "";

            if (d == null)
                return;

            int neededSize = 1;
            if (d.statBases != null)
                neededSize += d.statBases.Count;
            if (d.Verbs != null)
                neededSize += d.Verbs.Count * 6;
            if (this.selected.tools != null)
                neededSize += this.selected.tools.Count * 3;

            while (comBuffer.Count <= neededSize)
            {
                comBuffer.Add("");
            }

            int index = 0;
            if (d.statBases != null)
            {
                foreach (StatModifier m in d.statBases)
                {
                    comBuffer[index] = m.value.ToString();
                    ++index;
                }
            }

            if (d.Verbs != null)
            {
                this.PopulateVerbBuffers(ref index, d.Verbs);
            }

            if (d.tools != null)
            {
                foreach (Tool t in d.tools)
                {
                    comBuffer[index] = t.power.ToString();
                    ++index;
                    comBuffer[index] = t.armorPenetration.ToString();
                    ++index;
                    comBuffer[index] = t.cooldownTime.ToString();
                    ++index;
                }
            }
        }

        public override void ExposeData()
        {
            this.Init();

            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                this.exposedStats = new List<Stats>(this.ApparelDefs.Count + this.WeaponDefs.Count);
                foreach (ThingDef d in this.ApparelDefs.Values)
                {
                    exposedStats.Add(new Stats(d));
                }
                
                foreach (ThingDef d in this.WeaponDefs.Values)
                {
                    exposedStats.Add(new Stats(d));
                }
            }

            Scribe_Collections.Look(ref this.exposedStats, "statsOverrides");
        }

        private void DrawFloatingOptions(IEnumerable<ThingDef> defs)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (ThingDef d in defs)
            {
                if (d != this.selected)
                {
                    options.Add(new FloatMenuOption(d.label, delegate
                    {
                        this.selected = d;
                        this.SetTextBuffer(this.selected);
                        this.scroll = Vector2.zero;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }

        public void Init()
        {
            if (this.ApparelDefs.Count == 0 ||
                this.WeaponDefs.Count == 0)
            {
                this.ApparelDefs.Clear();
                this.WeaponDefs.Clear();

                foreach (ThingDef d in DefDatabase<ThingDef>.AllDefs)
                {
                    if (d.IsApparel)
                    {
                        this.ApparelDefs.Add(d.label, d);
                    }
                    if (d.IsWeapon)
                    {
                        this.WeaponDefs.Add(d.label, d);
                    }
                }

                Backup.Init(this.ApparelDefs.Values, this.WeaponDefs.Values);
            }

            if (this.ApparelDefs.Count != 0 ||
                this.WeaponDefs.Count != 0)
            {
                if (this.exposedStats != null)
                {
#if DEBUG
                    StringBuilder sb = new StringBuilder("Exposed:");
                    sb.AppendLine();
                    foreach (Stats s in exposedStats)
                    {
                        sb.Append(s.label + ", ");
                    }
                    sb.AppendLine();
                    sb.AppendLine("Apparel:");
                    foreach (string s in ApparelDefs.Keys)
                    {
                        sb.Append(s + ", ");
                    }
                    sb.AppendLine();
                    sb.AppendLine("Weapons:");
                    foreach (string s in WeaponDefs.Keys)
                    {
                        sb.Append(s + ", ");
                    }
                    sb.AppendLine();
                    Log.Warning(sb.ToString());
#endif

                    foreach (Stats s in this.exposedStats)
                    {
                        if (s.IsApparel)
                        {
                            if (this.ApparelDefs.TryGetValue(s.label, out ThingDef d))
                            {
                                s.ApplyStats(d);
                            }
                        }
                        if (s.IsWeapon)
                        {
                            if (this.WeaponDefs.TryGetValue(s.label, out ThingDef d))
                            {
                                s.ApplyStats(d);
                            }
                        }
                    }

                    this.exposedStats.Clear();
                    this.exposedStats = null;
                }
            }
        }

#region Verbs
        private void PopulateVerbBuffers(ref int comBufferIndex, List<VerbProperties> verbs)
        {
            foreach (VerbProperties v in verbs)
            {
                this.comBuffer[comBufferIndex] = v.warmupTime.ToString();
                ++comBufferIndex;
                this.comBuffer[comBufferIndex] = v.range.ToString();
                ++comBufferIndex;
                this.comBuffer[comBufferIndex] = v.ticksBetweenBurstShots.ToString();
                ++comBufferIndex;
                this.comBuffer[comBufferIndex] = v.burstShotCount.ToString();
                ++comBufferIndex;
                this.comBuffer[comBufferIndex] = v.muzzleFlashScale.ToString();
                ++comBufferIndex;
                this.comBuffer[comBufferIndex] = v.ai_AvoidFriendlyFireRadius.ToString();
                ++comBufferIndex;
            }
        }

        private void DrawVerbInput(int x, ref int y, ref int comBufferIndex, VerbProperties v)
        {
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "Warnup Time", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "Range", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "Time Between Shots", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "Burst Shot Count", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "Muzzle Flash Scale", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
            this.comBuffer[comBufferIndex] = this.DrawInput(x, ref y, "(AI) Avoid Friendly Fire Radius", this.comBuffer[comBufferIndex]);
            ++comBufferIndex;
        }
#endregion
    }
}