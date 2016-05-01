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
    class Jayce : Plugin
    {
        private Spell.SpellBase QE;
        private Vector3 gatePos;
        private float LastCast;

        public Jayce()
        {
            QE = new Spell.Skillshot(SpellSlot.Q, 1650, SkillShotType.Linear, 25, 1900, 70);
            Q = new Spell.Targeted(SpellSlot.Q, 600);
            W = new Spell.Active(SpellSlot.W, 285);
            E = new Spell.Targeted(SpellSlot.E, 240);
            Q2 = new Spell.Skillshot(SpellSlot.Q, 1030, SkillShotType.Linear, 25, 1450, 70)
            {
                MinimumHitChance = HitChance.High
            };
            W2 = new Spell.Active(SpellSlot.W);
            E2 = new Spell.Skillshot(SpellSlot.E, 650, SkillShotType.Circular, 1, int.MaxValue, 120);
            R = new Spell.Active(SpellSlot.R);

            ModeMenu = PluginMenu.AddSubMenu("Modes", "Modes");
            ModeMenu.AddGroupLabel("Combo");
            ModeMenu.Add("jayce.combo.q", new CheckBox("Use Q Hammer"));
            ModeMenu.Add("jayce.combo.q2", new CheckBox("Use Q Cannon"));
            ModeMenu.Add("jayce.combo.w", new CheckBox("Use W Hammer"));
            ModeMenu.Add("jayce.combo.w2", new CheckBox("Use W Cannon"));
            ModeMenu.Add("jayce.combo.e", new CheckBox("Use E Hammer"));
            ModeMenu.Add("jayce.combo.qe", new CheckBox("Use QE Cannon"));
            ModeMenu.Add("jayce.combo.r", new CheckBox("Switch Form"));

            ModeMenu.AddGroupLabel("Harass");
            ModeMenu.Add("jayce.harass.q2", new CheckBox("Use Q Cannon"));
            ModeMenu.Add("jayce.harass.w2", new CheckBox("Use W Cannon"));
            ModeMenu.Add("jayce.harass.qe", new CheckBox("Use QE Cannon"));

            MiscMenu = PluginMenu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddGroupLabel("Key Binds");
            MiscMenu.Add("jayce.quickscope", new KeyBind("Quickscope", false, KeyBind.BindTypes.HoldActive, 'A'));
            MiscMenu.Add("jayce.insec", new KeyBind("Insec", false, KeyBind.BindTypes.HoldActive, 'G'));
            MiscMenu.Add("jayce.flashinsec", new CheckBox("->Use Flash Insec"));

            MiscMenu.Add("jayce.test", new KeyBind("test", false, KeyBind.BindTypes.HoldActive, 'N'));

            MiscMenu.AddGroupLabel("Settings");
            MiscMenu.Add("jayce.gapcloser", new CheckBox("Use E on Gapcloser"));
            MiscMenu.Add("jayce.interrupt", new CheckBox("Use E to Interrupt"));
            MiscMenu.Add("jayce.parallel", new CheckBox("Place gate parallel", false));
            MiscMenu.Add("jayce.gatedistance", new Slider("Gate Distance", 60, 60, 400));

            MiscMenu.AddGroupLabel("Kill Steal");
            MiscMenu.Add("jayce.killsteal.qe", new CheckBox("Use QE Cannon"));

            DrawMenu = PluginMenu.AddSubMenu("Drawing", "Drawing");
            DrawMenu.AddGroupLabel("Spell Ranges");
            DrawMenu.Add("jayce.draw.q", new CheckBox("Draw Q Hammer"));
            DrawMenu.Add("jayce.draw.q2", new CheckBox("Draw Q Cannon"));
            DrawMenu.Add("jayce.draw.qe", new CheckBox("Draw QE Cannon"));

            DrawMenu.AddGroupLabel("Other");
            DrawMenu.Add("jayce.draw.cds", new CheckBox("Draw Cooldowns"));

            
            //Orbwalker.OnPreAttack += OnPreAttack;
            //Orbwalker.OnPostAttack += OnPostAttack;
            Gapcloser.OnGapcloser += OnGapCloser;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
        }

        private void OnGapCloser(AIHeroClient target, Gapcloser.GapcloserEventArgs spell)
        {
            if (MiscMenu["jayce.gapcloser"].Cast<CheckBox>().CurrentValue && target.IsEnemy && CD[2] == 0 && E.IsInRange(target) && (isMelee || ((!isMelee && R.IsReady() && R.Cast()))))
            {
                E.Cast(target);
            }
        }

        private void OnInterruptableSpell(Obj_AI_Base target, Interrupter.InterruptableSpellEventArgs spell)
        {
            if (MiscMenu["jayce.interrupt"].Cast<CheckBox>().CurrentValue && target.IsEnemy && CD[2] == 0 && E.IsInRange(target) && (isMelee || ((!isMelee && R.IsReady() && R.Cast()))))
            {
                E.Cast(target);
            }
        }

        private void OnPreAttack(AttackableUnit target, EventArgs args)
        {
            if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) &&
                !isMelee && W.IsReady())
            {
                Core.DelayAction(delegate { W.Cast(); }, 300);
            }
        }

        private void OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear)) &&
                !isMelee && W.IsReady())
            {
                W.Cast();
            }
        }

        public override void Update()
        {
            if (!myHero.IsDead && !Shop.IsOpen)
            {
                ShouldE();
                UpdateCooldowns();
                KillSteal();
                if (MiscMenu["jayce.quickscope"].Cast<KeyBind>().CurrentValue)
                {
                    EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    if (!isMelee && Q.IsReady() && E.IsReady())
                    {
                        CastQE(Game.CursorPos);
                    }
                }
                if (MiscMenu["jayce.insec"].Cast<KeyBind>().CurrentValue)
                {
                    Insec();
                }
            }
        }

        public override void Draw()
        {
            if (!myHero.IsDead)
            {
                if (DrawMenu["jayce.draw.q"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(CD[0] == 0 ? SharpDX.Color.Green : SharpDX.Color.Red, Q.Range, myHero.Position);
                if (DrawMenu["jayce.draw.q2"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw(CD[3] == 0 ? SharpDX.Color.Green : SharpDX.Color.Red, Q2.Range, myHero.Position);
                if (DrawMenu["jayce.draw.qe"].Cast<CheckBox>().CurrentValue)
                    Circle.Draw((CD[3] == 0 && CD[5] == 0) ? SharpDX.Color.Green : SharpDX.Color.Red, QE.Range, myHero.Position);

                if (DrawMenu["jayce.draw.cds"].Cast<CheckBox>().CurrentValue)
                {
                    DrawCooldowns();
                }

                if (MiscMenu["jayce.insec"].Cast<KeyBind>().CurrentValue)
                {
                    DrawInsec(TargetSelector.GetTarget(QE.Range, DamageType.Physical));
                }
            }
        }

        public override void Combo()
        {
            AIHeroClient target = TargetSelector.GetTarget(QE.Range, DamageType.Physical);
            if (Helper.IsValidTarget(target))
            {
                if (!isMelee)
                {
                    ShouldE();
                    if (ModeMenu["jayce.combo.qe"].Cast<CheckBox>().CurrentValue && Q.IsReady() && E.IsReady() && E.IsLearned)
                    {
                        CastQE(target);
                    }
                    if (ModeMenu["jayce.combo.q2"].Cast<CheckBox>().CurrentValue && Q2.IsReady() && !E.IsReady())
                    {
                        Q2.Cast(target);
                    }
                    if (ModeMenu["jayce.combo.w2"].Cast<CheckBox>().CurrentValue && W2.IsReady() && myHero.Distance(target.Position) <= myHero.AttackRange + 40)
                    {
                        W2.Cast();
                    }
                    if (ModeMenu["jayce.combo.r"].Cast<CheckBox>().CurrentValue && !Q2.IsReady() && !W2.IsReady() && R.IsReady() && myHero.Distance(target.Position) < Q.Range + 20 && CD[0] == 0)
                    {
                        R.Cast();
                    }
                }
                else
                {
                    /*
                    var heroes = EntityManager.Heroes.Enemies.Where(x=> x.Distance(myHero.Position) < QE.Range);
                    foreach (var hero in heroes)
                    {
                    	if (Prediction.Position.Collision.LinearMissileCollision(hero, myHero.Position.To2D(), Helper.extend(myHero.Position, hero.Position, QE.Range, 1).To2D(), ((Spell.Skillshot)QE).Speed, ((Spell.Skillshot)QE).Width, ((Spell.Skillshot)QE).CastDelay))
                    	{
                    	}
                    }*/

                    if (ModeMenu["jayce.combo.q"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(target))
                    {
                        Q.Cast(target);
                    }
                    
                    if (ModeMenu["jayce.combo.w"].Cast<CheckBox>().CurrentValue && W.IsReady() && W.IsInRange(target))
                    {
                        W.Cast();
                    }
                    if (ModeMenu["jayce.combo.r"].Cast<CheckBox>().CurrentValue && !Q.IsReady() && !W.IsReady() && R.IsReady())
                    {
                        R.Cast();
                    }
                }
            }
        }

        public override void Harass()
        {
            AIHeroClient target = TargetSelector.GetTarget(QE.Range, DamageType.Physical);
            if (Helper.IsValidTarget(target))
            {
                if (!isMelee || ((isMelee && R.IsReady() && R.Cast())))
                {
                    ShouldE();
                    if (ModeMenu["jayce.harass.qe"].Cast<CheckBox>().CurrentValue && Q.IsReady() && E.IsReady() && E.IsLearned)
                    {
                        CastQE(target);
                    }
                    if (ModeMenu["jayce.harass.q2"].Cast<CheckBox>().CurrentValue && Q2.IsReady() && !E.IsReady())
                    {
                        Q2.Cast(target);
                    }
                    if (ModeMenu["jayce.harass.w2"].Cast<CheckBox>().CurrentValue && W2.IsReady() && myHero.Distance(target.Position) <= myHero.AttackRange + 25)
                    {
                        W2.Cast();
                    }
                }
            }
        }

        public override void Flee()
        {
            if (isMelee)
            {
                if (Q.IsReady())
                {
                    AIHeroClient bestChampion = EntityManager.Heroes.Enemies.OrderBy(x => x.Distance(Game.CursorPos))
                    .Where(x => Q.IsInRange(x) && x.Distance(Game.CursorPos) + 200 < myHero.Distance(Game.CursorPos)).FirstOrDefault();
                    if (bestChampion != null)
                    {
                        Q.Cast(bestChampion);
                    }
                    else
                    {
                        Obj_AI_Minion bestMinion = EntityManager.MinionsAndMonsters.CombinedAttackable.OrderBy(x => x.Distance(Game.CursorPos))
                        .Where(x => Q.IsInRange(x) && x.Distance(Game.CursorPos) + 200 < myHero.Distance(Game.CursorPos)).FirstOrDefault();
                        if (bestMinion != null)
                        {
                            Q.Cast(bestMinion);
                        }
                    }
                }
            }
            else
            {
                if (E.IsReady())
                {
                    E2.Cast(Helper.extend(myHero.Position, Game.CursorPos, 80, 1));
                }
            }
            if (R.IsReady()) R.Cast();
        }

        public void Insec()
        {
            AIHeroClient target = TargetSelector.GetTarget(QE.Range, DamageType.Physical);
            if (Helper.IsValidTarget(target))
            {
                Vector3 insecPos = Helper.extend(target.Position, Game.CursorPos, 150, -1);
                EloBuddy.Player.IssueOrder(GameObjectOrder.MoveTo, insecPos);

                if ((isMelee || ((!isMelee && R.IsReady() && R.Cast()))) && E.IsReady()) // melee and e is ready
                {
                    if (ShouldInsec(target.Position, insecPos, E.Range))
                    {
                        E.Cast(target);
                        return;
                    }
                    if (myHero.Distance(target.Position) + 30 < myHero.Distance(insecPos))
                    {
                        // jumping/flashing to target will be faster than walking there
                        if (Q.IsReady() && Q.IsInRange(target))
                        {
                            Q.Cast(target);
                        }
                        if (MiscMenu["jayce.flashinsec"].Cast<CheckBox>().CurrentValue && myHero.Distance(insecPos) < 410 && myHero.Distance(insecPos) > 160)
                        {
                            SpellDataInst spell = myHero.Spellbook.Spells.FirstOrDefault(a => a.Name.ToLower().Contains("summonerflash"));
                            if (spell != null && spell.IsReady)
                            {
                                myHero.Spellbook.CastSpell(spell.Slot, insecPos);
                            }
                        }
                    }
                }
            }
        }

        private void CastQE(AIHeroClient target)
        {
            PredictionResult QEPred = ((Spell.Skillshot)QE).GetPrediction(target);
            if (QEPred.HitChance >= HitChance.High)
            {
                CastQE(QEPred.CastPosition);
            }
            else if (QEPred.HitChance == HitChance.Collision)
            {
                Obj_AI_Base collision = QEPred.CollisionObjects.OrderBy(x => x.Distance(myHero.Position)).First();
                if (Helper.extend(collision.Position, QEPred.UnitPosition, collision.BoundingRadius, -1).Distance(QEPred.UnitPosition) < 160)
                {
                    CastQE(QEPred.CastPosition);
                }
            }
        }

        private void CastQE(Vector3 position)
        {
            LastCast = Game.Time;
            gatePos = Helper.extend(myHero.Position, position, MiscMenu["jayce.gatedistance"].Cast<Slider>().CurrentValue, 1); // in front, horizontal

            if (MiscMenu["jayce.parallel"].Cast<CheckBox>().CurrentValue)
            {
                gatePos = new Vector3(myHero.Position.Y + myHero.Position.X - gatePos.Y, myHero.Position.Y - myHero.Position.X + gatePos.X, myHero.Position.Z);
            }
            QE.Cast(position);
        }

        private void KillSteal()
        {
            foreach (AIHeroClient target in EntityManager.Heroes.Enemies.OrderBy(x => x.Health).Where(x => x.IsValidTarget(QE.Range) && x.Health < myHero.Level * 150 && Helper.IsValidTarget(x)))
            {
                if ((Q2dmg(target) * 1.4f) > target.Health && CD[3] == 0 && CD[5] == 0 &&
                    ((Spell.Skillshot)QE).GetPrediction(target).HitChance >= HitChance.High &&
                    (!isMelee || ((isMelee && R.IsReady() && R.Cast()))))
                {
                    CastQE(target);
                    return;
                }

                if ((Q2dmg(target)) > target.Health && CD[3] == 0 && CD[5] != 0 &&
                    ((Spell.Skillshot)Q2).GetPrediction(target).HitChance >= HitChance.High &&
                    (!isMelee || ((isMelee && R.IsReady() && R.Cast()))))
                {
                    Q2.Cast(target);
                    return;
                }

                if ((Edmg(target)) > target.Health && CD[2] == 0 && E.IsInRange(target) &&
                    (isMelee || ((!isMelee && R.IsReady() && R.Cast()))))
                {
                    E.Cast(target);
                    return;
                }
            }
        }

        private void ShouldE()
        {
            if (CD[5] == 0 && Game.Time - LastCast < 0.20)
            {
                E2.Cast(gatePos);
            }
        }

        public override float Qdmg(Obj_AI_Base target)
        {
            return myHero.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 0, 40, 80, 120, 160, 200, 240 }[Q.Level] + 1.2f * myHero.FlatPhysicalDamageMod);
        }

        public override float Edmg(Obj_AI_Base target)
        {
            return myHero.CalculateDamageOnUnit(target, DamageType.Magical, new float[] { 0, 8f, 10.4f, 12.8f, 15.2f, 17.6f, 20f }[E.Level] * (target.MaxHealth / 100) + myHero.FlatPhysicalDamageMod);
        }

        public override float Q2dmg(Obj_AI_Base target)
        {
            return myHero.CalculateDamageOnUnit(target, DamageType.Physical, new float[] { 0, 70, 120, 170, 220, 270, 320 }[Q.Level] + 1.2f * myHero.FlatPhysicalDamageMod);
        }
    }
}