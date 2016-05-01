using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace LookSharp.Plugins
{
    class Other : Plugin
    {
        public Other()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 1075, SkillShotType.Linear, 0, 850, 110)
            {
                MinimumHitChance = HitChance.High,
                AllowedCollisionCount = int.MaxValue
            };

            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Circular, 0, int.MaxValue, 1);
            E = new Spell.Targeted(SpellSlot.E, 650);
            R = new Spell.Skillshot(SpellSlot.R, 625, SkillShotType.Circular, 0, int.MaxValue, 400);

            ModeMenu = PluginMenu.AddSubMenu("Modes", "Modes");
            ModeMenu.AddGroupLabel("Combo");
            ModeMenu.Add("other.combo.q", new CheckBox("Use Q"));
            ModeMenu.Add("other.combo.w", new CheckBox("Use W", false));
            
            
            ModeMenu.AddGroupLabel("Harass");
            ModeMenu.Add("other.harass.q", new CheckBox("Use Q"));
            ModeMenu.Add("other.harass.w", new CheckBox("Use W", false));

            ModeMenu.AddGroupLabel("Clear");
            ModeMenu.Add("other.clear.q", new CheckBox("Use Q"));
            ModeMenu.Add("other.clear.w", new CheckBox("Use W", false));

            MiscMenu = PluginMenu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Key Binds");
            MiscMenu.Add("other.special", new KeyBind("Special", false, KeyBind.BindTypes.HoldActive, 'A'));
            MiscMenu.Add("other.insec", new KeyBind("Insec", false, KeyBind.BindTypes.HoldActive, 'G'));
            MiscMenu.Add("other.flash_insec", new CheckBox("->Use Flash Insec"));

            MiscMenu.AddGroupLabel("Settings");
            MiscMenu.Add("other.gapcloser", new CheckBox("Gapcloser"));
            MiscMenu.Add("other.interrupt", new CheckBox("Interrupt"));

            MiscMenu.AddGroupLabel("Kill Steal");
            MiscMenu.Add("other.killsteal.q", new CheckBox("Q Killsteal"));

            DrawMenu = PluginMenu.AddSubMenu("Drawing", "Drawing");
            DrawMenu.AddGroupLabel("Ability Ranges");
            DrawMenu.Add("other.draw.q", new CheckBox("Draw Q"));
            DrawMenu.Add("other.draw.w", new CheckBox("Draw W"));
            DrawMenu.Add("other.draw.e", new CheckBox("Draw E"));
            DrawMenu.Add("other.draw.r", new CheckBox("Draw R"));
            
            Gapcloser.OnGapcloser += OnGapCloser;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
        }

        private void OnGapCloser(AIHeroClient target, Gapcloser.GapcloserEventArgs args)
        {
            if (MiscMenu["other.gapcloser"].Cast<CheckBox>().CurrentValue && target.IsEnemy)
            {
                // code
            }
        }

        private void OnInterruptableSpell(Obj_AI_Base target, Interrupter.InterruptableSpellEventArgs args)
        {
            if (MiscMenu["other.interrupt"].Cast<CheckBox>().CurrentValue && target.IsEnemy)
            {
                // code
            }
        }

        public override void Update()
        {
            // code
        }

        public override void Draw()
        {
            // code
        }

        public override void Combo()
        {
            AIHeroClient target = TargetSelector.GetTarget(1500, DamageType.Physical);
            if (Helper.IsValidTarget(target))
            {
                if (ModeMenu["other.combo.q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    // code
                }
            }
        }
    }
}
