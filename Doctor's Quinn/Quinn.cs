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

namespace Quinn
{
    static class Program
    {
        public static Menu Menu, ComboMenu, HarassMenu, JungleClearMenu, LaneClearMenu, KillStealMenu, Auto, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
		public static Spell.Skillshot Q;
        public static Spell.Skillshot W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;
        public static Item Botrk;
        public static Item Bil;
        public static Item Youmuu;

        public static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Quinn")) return;
            Chat.Print("Doctor's Quinn Loaded!", Color.Orange);
            Q = new Spell.Skillshot(SpellSlot.Q, 1025, SkillShotType.Linear, 250, 1550, 60);
            W = new Spell.Skillshot(SpellSlot.W, 2100, SkillShotType.Circular, 0, 5000, 300);
            E = new Spell.Targeted(SpellSlot.E, 750);
            R = new Spell.Active(SpellSlot.R);
            Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Bil = new Item(3144, 475f);
            Youmuu = new Item(3142, 10);
            Menu = MainMenu.AddMenu("Doctor's Quinn", "Quinn");
            Menu.AddGroupLabel("Mercedes7");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use [Q] Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use [W] Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboR", new CheckBox("Use [R]"));

            Auto = Menu.AddSubMenu("Interrupt/AntiGap] Settings", "Auto");
            Auto.AddGroupLabel("Interrupt/AntiGap Settings");
            Auto.Add("interQ", new CheckBox("Use [E] Interrupt"));
            Auto.Add("antigap", new CheckBox("Use [E] AntiGap"));

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Use [Q] Harass", false));
            HarassMenu.Add("HarassE", new CheckBox("Use [E] Harass"));
            HarassMenu.Add("ManaHarass", new Slider("Mana Harass", 50));

            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            JungleClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            JungleClearMenu.Add("ManaJungle", new Slider("Mana JungleClear", 30));

            LaneClearMenu = Menu.AddSubMenu("Farm Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LaneClear Settings");
            LaneClearMenu.Add("QLaneClear", new CheckBox("Use [Q] LaneClear"));
            LaneClearMenu.Add("minQLaneClear", new Slider("Min hit [Q] LaneClear", 3, 1, 6));
            LaneClearMenu.Add("ELaneClear", new CheckBox("Use [E] LaneClear"));
            LaneClearMenu.Add("ManaLaneClear", new Slider("Mana LaneClear", 60));
            LaneClearMenu.AddGroupLabel("LastHit Settings");
            LaneClearMenu.Add("ELastHit", new CheckBox("Use [E] LastHit"));
            LaneClearMenu.Add("ManaLastHit", new Slider("Mana LastHit", 70));

            KillStealMenu = Menu.AddSubMenu("KillSteal Settings", "KillSteal");
            KillStealMenu.AddGroupLabel("KillSteal Settings");
            KillStealMenu.Add("KsQ", new CheckBox("Use [Q] KillSteal"));
            KillStealMenu.Add("KsE", new CheckBox("Use [E] KillSteal"));
            KillStealMenu.Add("ign", new CheckBox("Use [Ignite] KillSteal"));

            Misc = Menu.AddSubMenu("Misc Settings", "Misc");
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawQ", new CheckBox("[Q] Range"));
            Misc.Add("DrawE", new CheckBox("[E] Range"));
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));
            Misc.AddGroupLabel("Items Settings");
            Misc.Add("you", new CheckBox("Use [Youmuu]"));
            Misc.Add("riu", new CheckBox("Use [Hydra]"));
            Misc.Add("BOTRK", new CheckBox("Use [BOTRK]"));
            Misc.Add("ihp", new Slider("My HP Use BOTRK <=", 50));
            Misc.Add("ihpp", new Slider("Enemy HP Use BOTRK <=", 50));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterruptableSpell += Interupt;
            Orbwalker.OnPostAttack += ResetAttack;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 3, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 3, Radius = E.Range }.Draw(_Player.Position);
            }
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
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                ComboW();
            }
			
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }
			
            KillSteal();
            Item();
            AutoQuinn();
        }

        public static bool RActive()
        {
            return Player.Instance.HasBuff("QuinnR");
        }

        private static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !Orbwalker.IsAutoAttacking)
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange(target) < Player.Instance.Position.Distance(target))
                {
                    E.Cast(target);
                }					
            }
        }

        private static void LaneClear()
        {
            var useQ = LaneClearMenu["QLaneClear"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["ELaneClear"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLaneClear"].Cast<Slider>().CurrentValue;
            var minQ = LaneClearMenu["minQLaneClear"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var minion in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range)))
            {
                if (useQ && Q.IsReady() && minion.CountEnemyMinionsInRange(180) >= minQ)
                {
                    Q.Cast(minion);
                }

                if (useE && E.IsReady() && !minion.IsValidTarget(Player.Instance.GetAutoAttackRange()) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }

        public static void LastHit()
        {
            var useE = LaneClearMenu["ELastHit"].Cast<CheckBox>().CurrentValue;
            var mana = LaneClearMenu["ManaLastHit"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var minion in EntityManager.MinionsAndMonsters.GetLaneMinions().Where(e => e.IsValidTarget(Q.Range)))
            {
                if (useE && E.IsReady() && !minion.IsValidTarget(Player.Instance.GetAutoAttackRange()) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = JungleClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["ManaJungle"].Cast<Slider>().CurrentValue;
            var monster = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, E.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            if (monster != null)
            {
                if (useQ && Q.IsReady() && monster.IsValidTarget(E.Range))
                {
                    Q.Cast(monster.Position);
                }

                if (useE && E.IsReady() && monster.IsValidTarget(E.Range))
                {
                    E.Cast(monster);
                }
            }
        }

        private static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaHarass"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < mana)
            {
                return;
            }

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && !Orbwalker.IsAutoAttacking)
                {
                    if (Q.GetPrediction(target).HitChance >= HitChance.High)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
				
                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }					
            }
        }

        public static void Item()
        {
            var item = Misc["BOTRK"].Cast<CheckBox>().CurrentValue;
            var yous = Misc["you"].Cast<CheckBox>().CurrentValue;
            var Minhp = Misc["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Misc["ihpp"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(520, DamageType.Physical);
            if (target != null)
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && target.IsValidTarget(475))
                {
                    Bil.Cast(target);
                }
				
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
				
                if (yous && Youmuu.IsReady() && Youmuu.IsOwned() && target.IsValidTarget(500) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Youmuu.Cast();
                }
            }
        }

        private static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var champ = (AIHeroClient)e;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useE && E.IsReady() && target.IsValidTarget(E.Range) && _Player.Position.Distance(target) < Player.Instance.GetAutoAttackRange(target) && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    E.Cast(target);
                }
            }
        }
		
        private static void ComboW()
        {
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(2100, DamageType.Physical);
            if (target == null)
            {
                return;
            }
             
            if (useW && W.IsReady())
            {
                if (NavMesh.GetCollisionFlags(W.GetPrediction(target).CastPosition.To2D()).HasFlag(CollisionFlags.Grass) && !target.VisibleOnScreen)
                {
                    W.Cast(W.GetPrediction(target).CastPosition.To2D().To3D());
                }
            }
        }

        private static void AutoQuinn()
        {
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            if (useR && R.IsReady() && Player.Instance.IsInShopRange() && !RActive())
            {
                R.Cast();
            }
        }


        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var inter = Auto["interQ"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }

            if (inter && E.IsReady() && i.DangerLevel == DangerLevel.Medium && E.IsInRange(sender))
            {
                E.Cast(sender);
            }
        }
		
        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {	
            if (Auto["antigap"].Cast<CheckBox>().CurrentValue && sender.IsEnemy && e.Sender.Distance(_Player) < 325 && !e.Sender.ChampionName.ToLower().Contains("MasterYi"))
            {
                E.Cast(e.Sender);
            }
        }

        private static void KillSteal()
        {
            var KsQ = KillStealMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = KillStealMenu["KsE"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(Q.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (Player.Instance.GetSpellDamage(target, SpellSlot.Q) >= target.Health)
                    {
                        if (Q.GetPrediction(target).HitChance >= HitChance.High)
                        {
                            Q.Cast(Q.GetPrediction(target).CastPosition);
                        }
                    }
                }

                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (Player.Instance.GetSpellDamage(target, SpellSlot.E) >= target.Health)
                    {
                        E.Cast(target);
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
