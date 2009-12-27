using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Engines.Craft;
using Server.Engines.PartySystem;

namespace Server
{
    public class Actions
    {
        public static void StartGain(Mobile killer, Mobile killed)
        {
            Setup set = new Setup();
            PlayerMobile pm = null;

            if (killer is PlayerMobile) //Find & Set killer
                pm = killer as PlayerMobile;
            else
            {
                BaseCreature bc = killer as BaseCreature;

                if (bc.Controlled && set.ExpFromPetKills)
                    pm = bc.GetMaster() as PlayerMobile;
            }

            if (pm == null) //if no killer Exit system
                return;

            double Gain = Figures.GetKillExp(pm, killed, new Setup()); //Get the exp for the kill

            FinalizeExp(pm, killed, Gain, new Setup());
        }

        public static void ExpPlugin(Mobile m, double Gain)
        {
            Setup set = new Setup();
            FinalizeExp(m, null, Gain, false, new Setup());
        }

        public static void FinalizeExp(Mobile m, Mobile killed, double Gain, Setup set)//for kills (no Craft bool)
        {
            FinalizeExp(m, killed, Gain, false, new Setup());
        }

        public static void FinalizeExp(Mobile m, double Gain, bool Craft, Setup set)//for craft (no killed Mobile)
        {
            FinalizeExp(m, null, Gain, Craft, new Setup());
        }

        public static void FinalizeExp(Mobile m, Mobile killed, double Gain, bool Craft, Setup set)
        {
            double BeforeFilter;//used only for party exp system
            double AfterLevelDiffFilter;
            double AfterCapFilter;
            Party party = Party.Get(m);

            if (!Craft && killed != null && party != null && set.ExpPartyShare)
            {
                int PlayersInRange = 0;

                foreach (PartyMemberInfo info in party.Members)
                {
                    PlayerMobile pm = info.Mobile as PlayerMobile;

                    if (pm.Alive && pm.InRange(killed, set.PartyRange))
                        PlayersInRange++;
                }//Dumb that I have to count players this way....

                if (set.ExpEvenPartyShare)
                    BeforeFilter = Gain / PlayersInRange;
                else
                    BeforeFilter = Gain;

                foreach (PartyMemberInfo info in party.Members)
                {
                    PlayerMobile pm = info.Mobile as PlayerMobile;

                    if (pm.Alive && pm.InRange(killed, set.PartyRange))
                    {
                        AfterLevelDiffFilter = Figures.LevelDiffFilter(pm, killed, BeforeFilter, new Setup());
                        AfterCapFilter = Figures.CapFilter(pm, AfterLevelDiffFilter, new Setup());
                        AddExp(pm, AfterCapFilter, new Setup());
                    }
                }
            }
            else
            {
                PlayerMobile pm = m as PlayerMobile;
                AfterLevelDiffFilter = Figures.LevelDiffFilter(pm, killed, Gain, new Setup());
                AfterCapFilter = Figures.CapFilter(pm, AfterLevelDiffFilter, new Setup());

                AddExp(pm, AfterCapFilter, new Setup());
            }
        }

        public static void AddExp(Mobile m, double Gain, Setup set)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (Gain > 0)
                pm.SendMessage("You've gained {0} exp.", Math.Round(Gain, 2));

            pm.KillExp += Math.Round(Gain, 2);
            pm.AccKillExp += Math.Round(Gain, 2);

            if (set.RefreshExpBarOnGain && pm.HasGump(typeof(ExpBar)))
            {
                pm.CloseGump(typeof(ExpBar));
                pm.SendGump(new ExpBar(pm));
            }

            if (pm.Level < pm.LevelCap && pm.Exp >= pm.LevelAt)
                DoLevel(pm, new Setup());
        }

        public static void DoLevel(Mobile m, Setup set)
        {
            double TimesLeveled = 0;
            PlayerMobile pm = m as PlayerMobile;

            pm.PlaySound(0x20F);
            pm.FixedParticles(0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist);
            pm.FixedParticles(0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist);

            if (set.StatRefillOnLevel)
            {
                if (pm.Hits < pm.HitsMax)
                    pm.Hits = pm.HitsMax;

                if (pm.Mana < pm.ManaMax)
                    pm.Mana = pm.ManaMax;

                if (pm.Stam < pm.StamMax)
                    pm.Stam = pm.StamMax;
            }


            /*
            while (pm.Exp >= pm.LevelAt && pm.Level != pm.LevelCap)
            {
                if (set.AccumulativeExp)
			set.pm.Exp = 0;	
                    return;

                int newexp = 0;

                if (pm.Exp > pm.LevelAt)
                    newexp = pm.Exp - pm.LevelAt;

                pm.Exp = newexp;
                TimesLeveled++;
            }
            */

            for (int i = 1; pm.Exp >= pm.LevelAt; i++)
            {
		
///////testing
int curlv = pm.Level + 1;
if (pm.Level > 0 && pm.Level < 5)
pm.LevelAt += 250;
else if (pm.Level > 4 && pm.Level < 10)
pm.LevelAt += 500;
else if (pm.Level > 9 && pm.Level < 15)
pm.LevelAt += 1000;
else if (pm.Level > 14 && pm.Level < 20)
pm.LevelAt += 2500;
else if (pm.Level > 19 && pm.Level < 25)
pm.LevelAt += 5000;
else if (pm.Level > 24 && pm.Level < 30)
pm.LevelAt += 7500;
else if (pm.Level > 29 && pm.Level < 35)
pm.LevelAt += 10000;
else if (pm.Level > 34 && pm.Level < 40)
pm.LevelAt += 14000;
else if (pm.Level > 39 && pm.Level < 45)
pm.LevelAt += 19000;
else if (pm.Level > 44 && pm.Level < 50)
pm.LevelAt += 25000;
else if (pm.Level > 49 && pm.Level < 55)
pm.LevelAt += 32000;
else if (pm.Level > 54 && pm.Level < 60)
pm.LevelAt += 40000;
else if (pm.Level > 59 && pm.Level < 65)
pm.LevelAt += 55000;
else if (pm.Level > 64 && pm.Level < 70)
pm.LevelAt += 68000;
else if (pm.Level > 69 && pm.Level < 75)
pm.LevelAt += 80000;
else if (pm.Level > 74 && pm.Level < 80)
pm.LevelAt += 90000;
else if (pm.Level > 79 && pm.Level < 85)
pm.LevelAt += 100000;
else if (pm.Level > 84 && pm.Level < 90)
pm.LevelAt += 125000;
else if (pm.Level > 89 && pm.Level < 95)
pm.LevelAt += 150000;
else if (pm.Level > 94 && pm.Level < 100)
pm.LevelAt += 200000;
///testing
//pm.LevelAt += set.NextLevelAt;
                //pm.AccLevelAt += (int)(set.NextLevelAt + pm.AccKillExp);

                if (set.BonusStatOnLevel && pm.RawStatTotal != pm.StatCap && set.ChanceForBonusStat < Utility.Random(100))
                {
                    switch (Utility.Random(3))
                    {
                        case 0: pm.RawStr += 1; break;
                        case 1: pm.RawDex += 1; break;
                        case 2: pm.RawInt += 1; break;
                    }
                }

                TimesLeveled = i;
            }

            if (set.RefreshExpBarOnGain && pm.HasGump(typeof(ExpBar)))
            {
                pm.CloseGump(typeof(ExpBar));
                pm.SendGump(new ExpBar(pm));
            }

            pm.SendMessage("You're Level has increased by {0}", TimesLeveled);
            pm.Level += (int)TimesLeveled;
        }
    }
}