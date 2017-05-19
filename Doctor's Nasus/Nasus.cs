using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Nasus
{
    internal class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static Spell.Active Q;
        public static Spell.Targeted W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Nasus")) return;
            Chat.Print("Doctor's Nasus Loaded!", Color.Orange);
            Chat.Print("Mercedes7", Color.Red);
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Targeted(SpellSlot.W, 600);
            E = new Spell.Skillshot(SpellSlot.E, 650, SkillShotType.Circular, 500, 20, 380);
            R = new Spell.Active(SpellSlot.R);
            Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("Doctor's Nasus", "Nasus");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R] Low Hp"));
            ComboMenu.Add("Rhp", new Slider("Low Hp Use [R]", 50));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            HarassMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("MHR", new Slider("Min Mana Harass", 50));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("Lane Clear Settings");
            LaneClearMenu.Add("QLC", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("ELC", new CheckBox("Use [E] LaneClear", false));
            LaneClearMenu.Add("mine", new Slider("Min hit minions use [E]", 3, 1, 6));
            LaneClearMenu.Add("MLC", new Slider("Min Mana LaneClear", 10));
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("QLH", new CheckBox("Use [Q] LastHit"));
            LaneClearMenu.Add("MLH", new Slider("Min Mana LastHit", 10));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("MJC", new Slider("Min Mana JungleClear", 10));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Drawing Settings", "Misc");
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawW", new CheckBox("[W] Range", false));
            Misc.Add("DrawE", new CheckBox("[E] Range", false));
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue)
                return;

            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = E.Range }.Draw(_Player.Position);
            }
			
            if (Misc["DrawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2, Radius = W.Range }.Draw(_Player.Position);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
			
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
			
            KillSteal();
            RLogic();
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 30, 50, 70, 90, 110 }[Q.Level] + 1.0f * _Player.FlatPhysicalDamageMod) + Player.Instance.GetBuffCount("NasusQStacks"));
        }

        public static float EDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 55, 95, 135, 175, 215 }[E.Level] + 0.6f * _Player.FlatMagicDamageMod));
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(250))
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && _Player.Position.Distance(target) > 175)
                {
                    W.Cast(target);
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target.Position);
                }
            }
        }

        private static void RLogic()
        {
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var hp = ComboMenu["Rhp"].Cast<Slider>().CurrentValue;
            if (useR && R.IsReady() && !Player.Instance.IsInShopRange() && _Player.Position.CountEnemyChampionsInRange(600) >= 1)
            {
                if (Player.Instance.HealthPercent <= hp)
                {
                    R.Cast();
                }
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLC"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELC"].Cast<CheckBox>().CurrentValue;
            var minE = LaneClearMenu["mine"].Cast<Slider>().CurrentValue;
            var mana = LaneClearMenu["MLC"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var minions in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(E.Range)))
            {
                if (useQ && Q.IsReady() && minions.IsValidTarget(250) && Player.Instance.Distance(minions.ServerPosition) <= 225f
                && QDamage(minions) + Player.Instance.GetAutoAttackDamage(minions) >= minions.TotalShieldHealth())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minions);
                }

                if (useE && E.IsReady() && minions.IsValidTarget(E.Range) && minions.CountEnemyMinionsInRange(450) >= minE && !Orbwalker.IsAutoAttacking)
                {
                    E.Cast(minions);
                }
            }
        }

        private static void LastHit()
        {
            var useQ = LaneClearMenu["QLH"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["MLH"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var minions in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(250)))
            {
                if (useQ && Q.IsReady() && minions.IsValidTarget(250) && Player.Instance.Distance(minions.ServerPosition) <= 225f
                && QDamage(minions) + Player.Instance.GetAutoAttackDamage(minions) >= minions.TotalShieldHealth())
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minions);
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["MHR"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (Player.Instance.ManaPercent < mana) 
            {
                return;
            }

            if (target != null)
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(250))
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range) && _Player.Position.Distance(target) > 175)
                {
                    W.Cast(target);
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target.Position);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MJC"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(E.Range));
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsInAutoAttackRange(Player.Instance) && Player.Instance.Distance(monster.ServerPosition) <= 225f)
                {
                    Q.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, monster);
                }

                if (useE && E.IsReady() && monster.IsValidTarget(E.Range))
                {
                    E.Cast(monster);
                }
            }
        }

        public static void Flee()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target != null)
            {
                if (W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast(target);
                }
            }
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(E.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(250))
                {
                    if (target.Health + target.AttackShield <= QDamage(target))
                    {
                        Q.Cast();
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                }

                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health + target.AttackShield <= EDamage(target))
                    {
                        E.Cast(target.Position);
                    }
                }

                if (Ignite != null && KillStealMenu["ign"].Cast<CheckBox>().CurrentValue && Ignite.IsReady())
                {
                    if (target.Health < _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                    {
                        Ignite.Cast(target);
                    }
                }
            }
        }
    }
}
