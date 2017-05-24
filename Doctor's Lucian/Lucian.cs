using System;
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

namespace Lucian
{
    static class Program
    {
        public static Spell.Targeted Q;
        public static Spell.Skillshot Q1;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Font Thm;
        private static bool DoubleAttack;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Menu Menu, ComboMenu, JungleClearMenu, HarassMenu, LaneClearMenu, Misc, KillSteals;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

// Menu

        private static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Lucian")) return;
            Chat.Print("Doctor's Lucian Loaded!", Color.Orange);
            Chat.Print("Mercedes7", Color.Red);
            Q = new Spell.Targeted(SpellSlot.Q, 675);
            Q1 = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, int.MaxValue, 65);
            Q1.AllowedCollisionCount = int.MaxValue;
            W = new Spell.Skillshot(SpellSlot.W, 1150, SkillShotType.Linear, 250, 1600, 80);
            E = new Spell.Skillshot(SpellSlot.E, 475, SkillShotType.Linear);
            R = new Spell.Skillshot(SpellSlot.R, 1400, SkillShotType.Linear, 500, 2800, 110);
            Thm = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Tahoma", Height = 16, Weight = FontWeight.Bold, OutputPrecision = FontPrecision.Default, Quality = FontQuality.ClearType });

            Menu = MainMenu.AddMenu("Doctor's Lucian", "Lucian");
            Menu.AddGroupLabel("Mercedes7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("CQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("CW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("CE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("EMode", new ComboBox("Combo [E] Mode:", 1, "E To Target", "E To Mouse"));
            ComboMenu.Add("CTurret", new KeyBind("Don't Use [E] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'T'));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("EHarass", new ComboBox("Harass [E] Mode:", 0, "E To Target", "E To Mouse"));
            HarassMenu.Add("HTurret", new CheckBox("Don't [E] Under Turret"));
            HarassMenu.Add("MinE", new Slider("Limit Enemies Around Target Use [E] Harass", 5, 1, 5));
            HarassMenu.Add("HM", new Slider("Mana Harass >=", 50, 0, 100));

            LaneClearMenu = Menu.AddSubMenu("Laneclear Settings", "Clear");
            LaneClearMenu.AddGroupLabel("Laneclear Settings");
            LaneClearMenu.Add("LQ", new CheckBox("Use [Q] Laneclear"));
            LaneClearMenu.Add("MinQ", new Slider("Min hit minions use [Q] LaneClear", 3, 1, 6));
            LaneClearMenu.Add("LW", new CheckBox("Use [W] Laneclear", false));
            LaneClearMenu.Add("LE", new CheckBox("Use [E] Laneclear", false));
            LaneClearMenu.Add("QMode", new ComboBox("LaneClear [Q] Mode:", 0, "Q Normal", "Q Harass Enemies"));
            LaneClearMenu.Add("LM", new Slider("Mana LaneClear >=", 50, 0, 100));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("JQ", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("JW", new CheckBox("Use [W] JungleClear"));
            JungleClearMenu.Add("JE", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("JM", new Slider("Mana JungleClear", 10, 0, 100));

            KillSteals = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillSteals.Add("QKs", new CheckBox("Use [Q] Ks"));
            KillSteals.Add("RKs", new CheckBox("Use [R] Ks"));

            Misc = Menu.AddSubMenu("Misc Settings", "Draw");
            Misc.AddGroupLabel("Anti Gapcloser");
            Misc.Add("AntiGap", new CheckBox("Use [E] Anti-Gapcloser", false));
            Misc.AddGroupLabel("Drawings Settings");
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings", false));
            Misc.Add("DrawE", new CheckBox("Draw [E]", false));
            Misc.Add("DrawQ", new CheckBox("Draw [Q]"));
            Misc.Add("DrawQ1", new CheckBox("Draw [Q1]", false));
            Misc.Add("DrawW", new CheckBox("Draw [W]", false));
            Misc.Add("DrawR", new CheckBox("Draw [R]", false));
            Misc.Add("DrawTR", new CheckBox("Status UnderTuret"));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            KillSteal();
        }

// Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;
			
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;
			
            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = W.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawQ1"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 1, Radius = Q1.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Yellow, BorderWidth = 1, Radius = R.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(Thm, "Use E Under Turret : Disable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(Thm, "Use E Under Turret : Enable", (float)(ft[0] - 70), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
            }
        }

// Flee Mode

        private static void Flee()
        {
            if (E.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= E.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, E.Range).To3D();
                Player.CastSpell(SpellSlot.E, Game.CursorPos);
            }
        }


//Damage

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 80, 110, 140, 170, 200 }[Q.Level] + new[] { 0, 0.6f, 0.75f, 0.9f, 1.05f, 1.2f }[Q.Level] * _Player.FlatPhysicalDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 40, 50, 60 }[R.Level] + 0.25f * _Player.FlatPhysicalDamageMod + 0.1f * _Player.FlatMagicDamageMod));
        }
		
//Underturret

        private static bool UnderTuret(Obj_AI_Base target)
        {
            var tower = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);
            return tower != null;
        }
		
//QExtend Credits for iRaxe

        private static void QExtend(AIHeroClient enemies)
        {
            if (enemies == null)
            {
                return;
            }

            if (enemies.Distance(Player.Instance) < Q.Range || !enemies.IsValidTarget(Q1.Range))
            {
                return;
            }

            var finishenemy = EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.Distance(Player.Instance) < Q1.Range).Cast<Obj_AI_Base>().ToList();
            finishenemy.AddRange(EntityManager.MinionsAndMonsters.Monsters.Where(m => m.Distance(Player.Instance) < Q1.Range));
            finishenemy.AddRange(EntityManager.Heroes.Enemies.Where(m => m.Distance(Player.Instance) < Q1.Range));
            var pred = Q1.GetPrediction(enemies);
            foreach (var minion in finishenemy.Select(minion => new
            {
                minion, polygon = new Geometry.Polygon.Rectangle(Player.Instance.ServerPosition.To2D(), Player.Instance.ServerPosition.Extend(minion.ServerPosition, Q1.Range), 65f)}).Where(@t => @t.polygon.IsInside(pred.CastPosition)).Select(@t => @t.minion))
            {
                Q.Cast(minion);
            }
        }
		
        private static Obj_AI_Minion LaneClearQ()
        {
            var minq = LaneClearMenu["MinQ"].Cast<Slider>().CurrentValue;
            var Minions = EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.Distance(Player.Instance) < Q.Range).ToList();
            var hitmonster = EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => m.Distance(Player.Instance) < Q1.Range).ToList();
            foreach (var minion in from minion in Minions
                let qHit = new Geometry.Polygon.Rectangle(Player.Instance.Position, Player.Instance.Position.Extend(minion.Position, Q1.Range).To3D(), Q1.Width)
                where hitmonster.Count(x => qHit.IsInside(x.Position.To2D())) >= minq
                select minion)
            {
                return minion;
            }
            if (EntityManager.MinionsAndMonsters.Monsters.Any(m => m.Distance(Player.Instance) < Q.Range))
            {
                var targetMonsters = EntityManager.MinionsAndMonsters.Monsters.Where(m => m.Distance(Player.Instance) < Q.Range).OrderByDescending(m => m.MinionLevel).ToList();
                return targetMonsters[0];
            }
            return null;
        }

//Harass Mode

        private static void Harass()
        {
            var useQ = HarassMenu["HQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["HM"].Cast<Slider>().CurrentValue;
            var minE = HarassMenu["MinE"].Cast<Slider>().CurrentValue;
            var turret = HarassMenu["HTurret"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }
			
            if (DoubleAttack == true)
            {
                return;
            }

            if (target != null)
            {
                if (useQ && !DoubleAttack && Q.IsReady())
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target);
                    }
                    else if (target.IsValidTarget(Q1.Range))
                    {
                        QExtend(target);
                    }

                }

                if (useW && !DoubleAttack && W.CanCast(target))
                {
                    W.Cast(target.Position);
                }

                if (useE && !DoubleAttack && E.IsReady() && target.IsValidTarget(E.Range + 450))
                {
                    if (HarassMenu["EHarass"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        if (turret)
                        {
                            if (!UnderTuret(target))
                            {
                                E.Cast(Player.Instance.Position.Extend(target.Position, E.Range).To3D());			
                            }
                        }
                        else
                        {
                            E.Cast(Player.Instance.Position.Extend(target.Position, E.Range).To3D());	
                        }
                        
                        if (HarassMenu["EHarass"].Cast<ComboBox>().CurrentValue == 1)
                        {
                            if (turret)
                            {
                                if (!UnderTuret(target))
                                {
                                    E.Cast(Player.Instance.Position.Extend(Game.CursorPos, E.Range).To3D());
                                }
                            }
                        }
                        else
                        {
                            E.Cast(Player.Instance.Position.Extend(Game.CursorPos, E.Range).To3D());
                        }
                    }
                }
            }
        }

//Combo Mode

        private static void Combo()
        {
            var useQ = ComboMenu["CQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["CW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["CE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var target = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);

            if (DoubleAttack == true)
            {
                return;
            }

            if (target != null)
            {
                if (useQ && !DoubleAttack && Q.IsReady())
                {
                    if (target.IsValidTarget(Q.Range))
                    {
                        Q.Cast(target);
                    }
                    else if (target.IsValidTarget(Q1.Range))
                    {
                        QExtend(target);
                    }
                }

                if (useW && !DoubleAttack && W.CanCast(target))
                {
                    W.Cast(target.Position);
                }

                if (useE && !DoubleAttack && E.IsReady() && target.IsValidTarget(E.Range + 450))
                {
                    if (ComboMenu["EMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        if (turret)
                        {
                            if (!UnderTuret(target))
                            {
                                E.Cast(Player.Instance.Position.Extend(target.Position, E.Range).To3D());			
                            }
                        }
                        else
                        {
                            E.Cast(Player.Instance.Position.Extend(target.Position, E.Range).To3D());	
                        }
                        
                        if (ComboMenu["EMode"].Cast<ComboBox>().CurrentValue == 1)
                        {
                            if (turret)
                            {
                                if (!UnderTuret(target))
                                {
                                    E.Cast(Player.Instance.Position.Extend(Game.CursorPos, E.Range).To3D());
                                }
                            }
                        }
                        else
                        {
                            E.Cast(Player.Instance.Position.Extend(Game.CursorPos, E.Range).To3D());
                        }
                    }
                }
            }
        }

//LaneClear Mode

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["LQ"].Cast<CheckBox>().CurrentValue;
            var useW = LaneClearMenu["LW"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["LE"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["LM"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q1.Range, DamageType.Physical);
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }
			
            if (DoubleAttack == true)
            {
                return;
            }

            if (minion != null)
            {
                if (useQ && !DoubleAttack && Q.IsReady())
                {
                    if (LaneClearMenu["QMode"].Cast<ComboBox>().CurrentValue == 0)
                    {
                        Q.Cast(LaneClearQ());
                    }
					
                    if (LaneClearMenu["QMode"].Cast<ComboBox>().CurrentValue == 1)
                    {
                        QExtend(target);
                    }
                }
				
                if (useW && !DoubleAttack && W.IsReady() && minion.IsValidTarget(Player.Instance.GetAutoAttackRange()))
                {
                    W.Cast(minion.Position);
                }

                if (useE && E.IsReady() && !DoubleAttack && minion.IsValidTarget(Player.Instance.GetAutoAttackRange()) && Player.Instance.GetAutoAttackDamage(minion) * 1.5f >= minion.Health)
                {
                    E.Cast(Player.Instance.Position.Extend(minion.Position, E.Range).To3D());	
                }
            }
        }

// JungleClear Mode

        private static void JungleClear()
        {
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            var useQ = JungleClearMenu["JQ"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["JW"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["JE"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["JM"].Cast<Slider>().CurrentValue;

            if (Player.Instance.ManaPercent <= mana)
            {
                return;
            }
			
            if (DoubleAttack == true)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && !DoubleAttack && Q.CanCast(monster))
                {
                    Q.Cast(monster);
                }

                if (useW && !DoubleAttack && W.IsReady() && monster.IsValidTarget(Player.Instance.GetAutoAttackRange()))
                {
                    W.Cast(monster.Position);
                }

                if (useE && !DoubleAttack && E.IsReady() && monster.IsValidTarget(Player.Instance.GetAutoAttackRange()))
                {
                    E.Cast(Player.Instance.Position.Extend(monster.Position, E.Range).To3D());	
                }
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

// KillSteal

        private static void KillSteal()
        {
            var Enemies = EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q1.Range) && !e.HasBuff("JudicatorIntervention") && !e.HasBuff("kindredrnodeathbuff") && !e.HasBuff("Undying Rage") && !e.IsDead && !e.IsZombie);
            var useQ = KillSteals["QKs"].Cast<CheckBox>().CurrentValue;
            var useR = KillSteals["RKs"].Cast<CheckBox>().CurrentValue;
            foreach (var target in Enemies)
            {
                if (useQ && Q.IsReady())
                {
                    if (QDamage(target) >= target.Health)
                    {
                        if (target.IsValidTarget(Q.Range))
                        {
                            Q.Cast(target);
                        }
					
                        else if (target.IsValidTarget(Q1.Range))
                        {
                            QExtend(target);
                        }
                    }
                }

                if (useR && R.IsReady())
                {
                    if (target.Health <= RDamage(target) * 5)
                    {
                        R.Cast(target.Position);
                    }
                }
            }
        }

// OnProcessSpellCast
		
        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.NetworkId != Player.Instance.NetworkId) return;
			
            if (args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E)
            {
                DoubleAttack = true;
            }

            if (args.Slot == SpellSlot.E)
            {
                Orbwalker.ResetAutoAttack();
            }
        }

// AntiGap

        private static void Gapcloser_OnGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {
            if (Misc["AntiGap"].Cast<CheckBox>().CurrentValue && E.IsReady() && sender.IsEnemy && sender.IsVisible && sender.IsInRange(Player.Instance, E.Range))
            {
                E.Cast(Player.Instance.Position.Shorten(sender.Position, E.Range));
            }
        }
    }
}
