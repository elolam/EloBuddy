﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace Hecarim7
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, Auto, JungleClearMenu, LaneClearMenu, KillStealMenu, Skin, Drawings;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Font Thm;
        public static Font thn;
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Hecarim")) return;
            Chat.Print("Doctor's Hecarim Loaded!", Color.Orange);
			Q = new Spell.Active(SpellSlot.Q, 350);
            W = new Spell.Active(SpellSlot.W, 525);
            E = new Spell.Active(SpellSlot.E);
            R= new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Linear, 250, 800, 200);
            R.AllowedCollisionCount = int.MaxValue;
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 20, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
            thn = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 22, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });
			Menu = MainMenu.AddMenu("Doctor's Hecarim", "Hecarim");
			ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Aoe Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Aoe"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 3, 0, 5));
            ComboMenu.AddGroupLabel("Ultimate Selected Target Settings");
            ComboMenu.Add("ComboSL", new KeyBind("Use [R] On Selected Target", false, KeyBind.BindTypes.HoldActive, 'T'));
            ComboMenu.AddGroupLabel("Interrupt Settings");
            ComboMenu.Add("inter", new CheckBox("Use [R] Interrupt"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("ManaQ", new Slider("Min Mana Harass [Q]", 40));
            HarassMenu.AddSeparator();
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass", false) );
            HarassMenu.Add("ManaW", new Slider("Min Mana Harass [W]", 40));
                
			Auto = Menu.AddSubMenu("Auto Harass Settings", "Auto Harass");
			Auto.AddGroupLabel("Auto Harass Settings");
            Auto.Add("AutoQ", new CheckBox("Auto [Q]"));
            Auto.Add("ManaQ", new Slider("Min Mana Auto [Q]", 60));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JungleMana", new Slider("Min Mana JungleClear", 20));		

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("LastQ", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("LhMana", new Slider("Min Mana Lasthit [Q]", 60));
            LaneClearMenu.AddSeparator();
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("LastQLC", new CheckBox("Always [Q] LaneClear (Keep Passive Q)"));
            LaneClearMenu.Add("CantLC", new CheckBox("Only [Q] Killable Minion", false));
            LaneClearMenu.Add("ManaLC", new Slider("Min Mana [Q] LaneClear", 50));
            LaneClearMenu.Add("LastWLC", new CheckBox("Use [W] LaneClear"));
            LaneClearMenu.Add("ManaLCW", new Slider("Min Mana [W] LaneClear", 70));			

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsW", new CheckBox("Use [W] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));
            KillStealMenu.AddSeparator();
            KillStealMenu.AddGroupLabel("Ultimate Settings");
            KillStealMenu.Add("KsR", new CheckBox("Use [R] KillSteal"));
            KillStealMenu.Add("minKsR", new Slider("Use [R] KillSteal If Enemy Distance >", 100, 1, 1000));
            KillStealMenu.AddGroupLabel("Distance < 125 = Always ,Recommended Distance 500");

            Skin = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            Skin.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Skin.Add("skin.Id", new ComboBox("Skin Mode", 4, "Default", "1", "2", "3", "4", "5"));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("[Q] Range"));
            Drawings.Add("DrawW", new CheckBox("[W] Range", false));
            Drawings.Add("DrawR", new CheckBox("[R] Range"));
            Drawings.Add("DrawT", new CheckBox("Draw [E] Time"));
            Drawings.Add("DrawRhit", new CheckBox("[R] Draw Hit"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterruptableSpell += Interupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }
			
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = W.Range }.Draw(_Player.Position);
            }
			
            if (Drawings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }

            if (Drawings["DrawT"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (Player.Instance.HasBuff("HecarimRamp"))
                {
                    DrawFont(thn, "E Time : " + ETime(Player.Instance), (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.GreenYellow);
                }
            }
			
            var target = TargetSelector.GetTarget(R.Range, DamageType.Mixed);
            if (Drawings["DrawRhit"].Cast<CheckBox>().CurrentValue && target != null && R.IsReady() && target.IsValidTarget(R.Range))
            {
                var RPred = R.GetPrediction(target);
                var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
				
                if (RPred.CastPosition.CountEnemyChampionsInRange(250) >= MinR)
                {
                    Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                    DrawFont(Thm, "Ultimate Can Hit : " + RPred.CastPosition.CountEnemyChampionsInRange(250), (float)(ft[0] - 70), (float)(ft[1] + 20), SharpDX.Color.Orange);
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
			{
                Harass();
			}
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
			{
                LastHit();
			}
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
			{
				Combo();
			}

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
			{
				Flee();
			}

            if (ComboMenu["ComboSL"].Cast<KeyBind>().CurrentValue)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
			
            KillSteal();
            AutoQ();
            RSelected();
			
            if (_Player.SkinId != Skin["skin.Id"].Cast<ComboBox>().CurrentValue)
            {
                if (checkSkin())
                {
                    Player.SetSkinId(SkinId());
                }
            }
        }

        public static int SkinId()
        {
            return Skin["skin.Id"].Cast<ComboBox>().CurrentValue;
        }
		
        public static bool checkSkin()
        {
            return Skin["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        public static float ETime(Obj_AI_Base target)
        {
            if (target.HasBuff("HecarimRamp"))
            {
                return Math.Max(0, target.GetBuff("HecarimRamp").EndTime) - Game.Time;
            }
            return 0;
        }
		
        private static void Combo()
        {
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead))
            {
                if (useE && E.IsReady() && target.IsValidTarget(700))
                {
                    E.Cast();
                }
				
                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
				
                if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(target);
					
                    if (RPred.CastPosition.CountEnemyChampionsInRange(250) >= MinR && RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
		    	}
				
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast();
                }
            }
        }

        private static void RSelected()
        {
            var targetS = TargetSelector.SelectedTarget;
            var useSL = ComboMenu["ComboSL"].Cast<KeyBind>().CurrentValue;
            if (targetS == null) return;

            if (useSL && R.IsReady() && targetS.IsValidTarget(R.Range))
            {
                var RPred = R.GetPrediction(targetS);
                if (RPred.HitChance >= HitChance.High)
                {
                    R.Cast(RPred.CastPosition);
                }
            }
        }

        private static void LaneClear()
        {
            var laneQMN = LaneClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var laneWMN = LaneClearMenu["ManaLCW"].Cast<Slider>().CurrentValue;
            var useQLH = LaneClearMenu["LastQLC"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["CantLC"].Cast<CheckBox>().CurrentValue;
            var useWLH = LaneClearMenu["LastWLC"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= W.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion != null)
            {
                if (useQLH && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.ManaPercent >= laneQMN)
                {
                    Q.Cast();
                }
				
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < QDamage(minion) && Player.Instance.ManaPercent >= laneQMN)
                {
                    Q.Cast();
                }
				
                if (useWLH && W.IsReady() && minion.IsValidTarget(W.Range) && Player.Instance.ManaPercent >= laneWMN && _Player.Position.CountEnemyMinionsInRange(W.Range) >= 3)
                {
                    W.Cast();
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(R.Range));
            if (Player.Instance.ManaPercent <= mana) return;
            if (jungleMonsters != null)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters))
                {
                    Q.Cast();
                }
				
                if (useE && E.IsReady() && jungleMonsters.IsValidTarget(800))
                {
                    E.Cast();
                }
				
                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var ManaQ = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var ManaW = HarassMenu["ManaW"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(W.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && Player.Instance.ManaPercent >= ManaQ)
                {
                    Q.Cast();
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && Player.Instance.ManaPercent >= ManaW)
                {
                    W.Cast();
                }
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var Inter = ComboMenu["inter"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }

            if (Inter && R.IsReady() && i.DangerLevel == DangerLevel.High && R.IsInRange(sender))
            {
                R.Cast(sender.Position);
            }
        }

        private static void AutoQ()
        {
            var useQ = Auto["AutoQ"].Cast<CheckBox>().CurrentValue;
            var mana = Auto["ManaQ"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && !Tru(_Player.Position) && Player.Instance.ManaPercent >= mana && target.IsValidTarget(Q.Range) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Q.Cast();
                }
            }
        }

        private static void Flee()
        {
            if (E.IsReady())
            {
                E.Cast();
            }
        }

        public static bool Tru(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["LastQ"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LhMana"].Cast<Slider>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion != null)
            {
                if (useQ && Q.IsReady() && Player.Instance.ManaPercent >= mana && minion.IsValidTarget(Q.Range) && minion.Health < QDamage(minion))
                {
                    Q.Cast();
                }
            }
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 150, 250, 350 }[R.Level] + 1.0f * _Player.FlatMagicDamageMod));
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 35, 60, 85, 110, 130 }[Q.Level] + 0.4f * _Player.FlatPhysicalDamageMod));
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsW = KillStealMenu["KsW"].Cast<CheckBox>().CurrentValue;
            var KsR = KillStealMenu["KsR"].Cast<CheckBox>().CurrentValue;
            var minKsR = KillStealMenu["minKsR"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast();
                    }
                }
				
                if (KsW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    if (target.Health + target.AttackShield <= Player.Instance.GetSpellDamage(target, SpellSlot.W))
                    {
                        W.Cast();
					}
                }
				
                if (KsR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    if (target.Health + target.AttackShield <= RDamage(target) && (!target.IsInRange(Player.Instance, minKsR) || Player.Instance.HealthPercent <= 25))
                    {
                        var RPred = R.GetPrediction(target);
                        if (RPred.HitChance >= HitChance.High)
                        {
                            R.Cast(RPred.CastPosition);
                        }
                    }
                }
				
                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health <= _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
		}
    }
}
