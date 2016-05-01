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
    class Structure : Utility
    {
        private Menu StructureMenu;
        private static readonly int TurretRange = 875 /*+ Program.myHero.BoundingRadius*/;
        private GameObject target;
        private Obj_AI_Base turret;

        public Structure()
        {
            StructureMenu = UtilityMenu.AddSubMenu("Structure", "Structure");
            StructureMenu.AddGroupLabel("Options");
            StructureMenu.Add("structure.enable", new CheckBox("Enable"));
            StructureMenu.Add("structure.target", new CheckBox("Draw Turret Target", false));
            StructureMenu.Add("structure.health", new CheckBox("Health"));

            Drawing.OnEndScene += OnEndScene;
            Obj_AI_Base.OnBasicAttack += OnBasicAttack;
        }

        private void OnEndScene(EventArgs args)
        {
            if (StructureMenu["structure.enable"].Cast<CheckBox>().CurrentValue)
            {
                if (target != null && !target.IsDead && target.Distance(turret.Position) < TurretRange)
                {
                    Circle.Draw(SharpDX.Color.Chartreuse, target.BoundingRadius, target.Position);
                }
                else
                {
                    target = null;
                }


                foreach (Obj_Turret unit in ObjectManager.Get<Obj_Turret>().Where(x => x.HealthPercent > 0))
                {
                    
                    Vector2 pos = Drawing.WorldToMinimap(unit.Position);
                    float health = unit.HealthPercent;

                    if (health < 100 && health >= 75)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.LimeGreen, health.ToString("0"));
                    else if (health < 75 && health >= 50)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.YellowGreen, health.ToString("0"));
                    else if (health < 50 && health >= 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Orange, health.ToString("0"));
                    else if (health < 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Red, health.ToString("0"));
                    
                    float distance = myHero.Position.Distance(unit.Position);
                    if (distance < TurretRange + 512)
                    {
                        int color = SharpDX.Color.Yellow.ToAbgr() - ((int)(distance > TurretRange ? (TurretRange + 512 - distance) / 2 : 255) << 16);
                        Circle.Draw(SharpDX.Color.FromAbgr(color), TurretRange, unit.Position);
                    }

                }

                foreach (Obj_HQ unit in ObjectManager.Get<Obj_HQ>().Where(x => x.HealthPercent > 0))
                {
                    Vector2 pos = Drawing.WorldToMinimap(unit.Position);
                    int health = (int)unit.HealthPercent;

                    if (health < 100 && health >= 75)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.LimeGreen, health.ToString("0"));
                    else if (health < 75 && health >= 50)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.YellowGreen, health.ToString("0"));
                    else if (health < 50 && health >= 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Orange, health.ToString("0"));
                    else if (health < 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Red, health.ToString("0"));
                }

                foreach (Obj_BarracksDampener unit in ObjectManager.Get<Obj_BarracksDampener>().Where(x => x.HealthPercent > 0))
                {
                    Vector2 pos = Drawing.WorldToMinimap(unit.Position);
                    int health = (int)unit.HealthPercent;

                    if (health < 100 && health >= 75)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.LimeGreen, health.ToString("0"));
                    else if (health < 75 && health >= 50)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.YellowGreen, health.ToString("0"));
                    else if (health < 50 && health >= 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Orange, health.ToString("0"));
                    else if (health < 25)
                        Drawing.DrawText(pos[0], pos[1], System.Drawing.Color.Red, health.ToString("0"));
                }
            }
        }

        private void OnBasicAttack(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (StructureMenu["structure.target"].Cast<CheckBox>().CurrentValue && sender.IsStructure() && sender.Distance(myHero.Position) < 3000)
            {
                target = args.Target;
                turret = sender;
            }
        }
    }
}
