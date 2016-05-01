/*
using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace LookSharp.Utilities
{
    internal class Time
    {
        public bool CalledInvisible = false;
        public bool CalledVisible = false;
        public int InvisibleTime;
        public bool Pinged = false;
        public int StartInvisibleTime;
        public int StartVisibleTime;
        public int VisibleTime;
    }

    class GankAlert : Utility
    {
        private readonly Dictionary<int, Time> enemies = new Dictionary<int, Time>();
        private Menu GankeAlertMenu;
        private int percentHealth;
        private Color color;

        public GankAlert(Menu config)
        {
            GankeAlertMenu = UtilityMenu.AddSubMenu("gankalert", "Gank Alert");
            GankeAlertMenu.Add("gankalert.enable", new CheckBox("Enable"));
            GankeAlertMenu.Add("gankalert.invisibletime", new Slider("Invisible Time", 5, 0, 10));
            GankeAlertMenu.Add("gankalert.visibletime", new Slider("Visible Time", 5, 0, 10));
            GankeAlertMenu.Add("gankalert.radius", new Slider("Radius", 3000, 500, 5000));

            foreach (AIHeroClient hero in EntityManager.Heroes.AllHeroes.Where(x => x.IsEnemy || Plugin.HasSmite(x)))
            {
                enemies.Add(hero.NetworkId, new Time());
            }
            Drawing.OnDraw += OnDraw;
        }

        private void OnDraw(EventArgs args)
        {
            if (!myHero.IsDead && GankeAlertMenu["gankalert.enable"].Cast<CheckBox>().CurrentValue)
            {
                foreach (var enemy in enemies)
                {
                    UpdateTime(enemy);
                }
            }
        }

        private void UpdateTime(KeyValuePair<int, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!menu.Item("Enable").GetValue<bool>()) return;

            if (hero.IsVisible)
            {
                if (!enemies[hero].CalledVisible)
                {
                    enemies[hero].CalledVisible = true;
                    enemies[hero].StartVisibleTime = Environment.TickCount;
                }
                enemies[hero].CalledInvisible = false;
                enemies[hero].VisibleTime = (Environment.TickCount - enemies[hero].StartVisibleTime) / 1000;
            }
            else
            {
                if (!enemies[hero].CalledInvisible)
                {
                    enemies[hero].CalledInvisible = true;
                    enemies[hero].StartInvisibleTime = Environment.TickCount;
                }
                enemies[hero].CalledVisible = false;
                enemies[hero].InvisibleTime = (Environment.TickCount - enemies[hero].StartInvisibleTime) / 1000;
            }
        }



        /*
        private void OnDraw(EventArgs args)
        {
            if (!menu.Item("Enable").GetValue<bool>()) return;

            foreach (var enemy in enemies)
            {
                UpdateTime(enemy);
            }

            //Conditions for gank
            foreach (Obj_AI_Hero hero in enemies.Select(enemy => enemy.Key).Where(hero => !hero.IsDead && hero.IsVisible && enemies[hero].InvisibleTime >= InvisibleTime &&
                            enemies[hero].VisibleTime <= VisibleTime && hero.Distance(ObjectManager.Player.Position) <= TriggerGank))
            {
                percentHealth = (int)((hero.Health / hero.MaxHealth) * 100);
                if (percentHealth >= 75)
                    color = Color.Red;
                else if (percentHealth < 75 && percentHealth >= 50)
                    color = Color.Orange;
                else if (percentHealth < 50 && percentHealth >= 25)
                    color = Color.YellowGreen;
                else
                    color = Color.LimeGreen;

                Drawing.DrawLine(Drawing.WorldToScreen(ObjectManager.Player.Position), Drawing.WorldToScreen(hero.Position), 5, color);

                if (menu.Item("ChatAlert").GetValue<bool>())
                {
                    if (!enemies[hero].Pinged)
                    {
                        enemies[hero].Pinged = true;
                        Notifications.AddNotification("<font color = \"#FF0000\">Gank: </font>" + hero.ChampionName, 2000);
                        Utility.DelayAction.Add((VisibleTime + 1) * 1000, () => { enemies[hero].Pinged = false; });
                    }
                }
            }
        }

        private void UpdateTime(KeyValuePair<Obj_AI_Hero, Time> enemy)
        {
            Obj_AI_Hero hero = enemy.Key;
            if (!menu.Item("Enable").GetValue<bool>()) return;

            if (hero.IsVisible)
            {
                if (!enemies[hero].CalledVisible)
                {
                    enemies[hero].CalledVisible = true;
                    enemies[hero].StartVisibleTime = Environment.TickCount;
                }
                enemies[hero].CalledInvisible = false;
                enemies[hero].VisibleTime = (Environment.TickCount - enemies[hero].StartVisibleTime) / 1000;
            }
            else
            {
                if (!enemies[hero].CalledInvisible)
                {
                    enemies[hero].CalledInvisible = true;
                    enemies[hero].StartInvisibleTime = Environment.TickCount;
                }
                enemies[hero].CalledVisible = false;
                enemies[hero].InvisibleTime = (Environment.TickCount - enemies[hero].StartInvisibleTime) / 1000;
            }
        }
        public int TriggerGank { get { return menu.Item("TriggerRange").GetValue<Slider>().Value; } }
        public int InvisibleTime { get { return menu.Item("InvisibleTime").GetValue<Slider>().Value; } }
        public int VisibleTime { get { return menu.Item("VisibleTime").GetValue<Slider>().Value; } }
    }
}
*/