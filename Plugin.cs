using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Rendering;
using SharpDX;
using LookSharp.Plugins;

namespace LookSharp
{
    abstract class Plugin
    {
        protected static AIHeroClient myHero { get { return Player.Instance; } }
        protected static Plugin myPlugin = null;
        protected static Menu PluginMenu, ModeMenu, MiscMenu, DrawMenu;

        protected Spell.SpellBase Q, W, E, Q2, W2, E2, R;
        protected float[] CD = new float[6], CDtemp = new float[6]; //Q melee to E ranged
        protected bool isMelee { get { return !myHero.HasBuff("jaycestancegun"); } }

        

        public static void Init()
        {
            PluginMenu = MainMenu.AddMenu(myHero.ChampionName, myHero.ChampionName);
            PluginMenu.AddGroupLabel("Information");
            PluginMenu.AddLabel("Made by Lookaside");
            PluginMenu.AddLabel("Please upvote in forums!");

            switch (myHero.Hero)
            {
                case Champion.Jayce:
                    myPlugin = new Jayce();
                    break;
                default:
                    myPlugin = null;
                    break;
            }
            if (myPlugin == null)
            {
                Chat.Print("LookSharp => No implementation for Champion " + myHero.ChampionName);
            }
            else
            {
                Chat.Print("LookSharp => " + myHero.ChampionName + " Loaded!");
            }

            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private static void OnUpdate(EventArgs args)
        {
            if (!myHero.IsDead && !Shop.IsOpen)
            {
                myPlugin.Update();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)) myPlugin.Combo();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass)) myPlugin.Harass();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) myPlugin.LaneClear();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)) myPlugin.JungleClear();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)) myPlugin.LastHit();
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee)) myPlugin.Flee();
            }
        }

        private static void OnDraw(EventArgs args)
        {
            if (!myHero.IsDead)
            {
                myPlugin.Draw();
            }
        }

        // do not implement
        public virtual void Update() { }
        public virtual void Draw() { }
        public virtual void Combo() { }
        public virtual void Harass() { }
        public virtual void LaneClear() { }
        public virtual void JungleClear() { }
        public virtual void LastHit() { }
        public virtual void Flee() { }

        // champion methods

        public virtual float Qdmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.Q);
        }
        public virtual float Wdmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.W);
        }
        public virtual float Edmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.E);
        }
        public virtual float Q2dmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.Q);
        }
        public virtual float W2dmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.W);
        }
        public virtual float E2dmg(Obj_AI_Base target)
        {
            return myHero.GetSpellDamage(target, SpellSlot.E);
        }

        public void UpdateCooldowns()
        {
            if (isMelee)
            {
                CDtemp[0] = myHero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires;
                CDtemp[1] = myHero.Spellbook.GetSpell(SpellSlot.W).CooldownExpires;
                CDtemp[2] = myHero.Spellbook.GetSpell(SpellSlot.E).CooldownExpires;
            }
            else
            {
                CDtemp[3] = myHero.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires;
                CDtemp[4] = myHero.Spellbook.GetSpell(SpellSlot.W).CooldownExpires;
                CDtemp[5] = myHero.Spellbook.GetSpell(SpellSlot.E).CooldownExpires;
            }
            for (int i = 0; i < 6; ++i)
            {
                CD[i] = CDtemp[i] - Game.Time < 0 ? 0 : CDtemp[i] - Game.Time;
            }
        }

        public void DrawCooldowns()
        {
            Vector2 wts = Drawing.WorldToScreen(myHero.Position);
            wts[0] -= 40;
            wts[1] += 20;
            if (!isMelee)
                for (int i = 0; i < 3; ++i)
                    if (CD[i] == 0)
                        Drawing.DrawText(wts[0] + (i * 30), wts[1], System.Drawing.Color.Lime, "UP");
                    else
                        Drawing.DrawText(wts[0] + (i * 30), wts[1], System.Drawing.Color.Orange, CD[i].ToString("0.0"));
            else
                for (int i = 3; i < 6; ++i)
                    if (CD[i] == 0)
                        Drawing.DrawText(wts[0] + ((i - 3) * 30), wts[1], System.Drawing.Color.Lime, "UP");
                    else
                        Drawing.DrawText(wts[0] + ((i - 3) * 30), wts[1], System.Drawing.Color.Orange, CD[i].ToString("0.0"));
        }

        public void DrawInsec(AIHeroClient target)
        {
            if (Helper.IsValidTarget(target))
            {
                Vector3 insecPos = Helper.extend(target.Position, Game.CursorPos, 150, -1);
                Vector2 wtsx = Drawing.WorldToScreen(Game.CursorPos);
                Vector2 wts = Drawing.WorldToScreen(target.Position);
                Drawing.DrawLine(wts[0], wts[1], wtsx[0], wtsx[1], 2, System.Drawing.Color.Red);
                Circle.Draw(SharpDX.Color.Red, 110, insecPos);
            }
        }

        public bool ShouldInsec(Vector3 target, Vector3 insecPos, float range)
        {
            float tolerance = 0.3f;

            // ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) above/below point
            // a = target.position
            // b = line
            // c = myHero.position

            Vector3 lineA = Helper.RotateAroundPoint(insecPos, target, tolerance);
            Vector3 lineB = Helper.RotateAroundPoint(insecPos, target, -tolerance);
            float checkA = (lineA.X - target.X) * (myHero.Position.Y - target.Y) - (lineA.Y - target.Y) * (myHero.Position.X - target.X);
            float checkB = (lineB.X - target.X) * (myHero.Position.Y - target.Y) - (lineB.Y - target.Y) * (myHero.Position.X - target.X);

            return (checkA < 0 && checkB > 0 && myHero.Distance(target) < range && myHero.Distance(target) > 30);
        }
    }
}
