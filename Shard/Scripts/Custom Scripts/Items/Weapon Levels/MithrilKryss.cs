using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{
////////////////////////////////////////////////////////// 
//  		Do not edit above this line.
//////////////////////////////////////////////////////////
	 [FlipableAttribute( 0x1401, 0x1400 )]  /// Change to proper item
	public class MithrilKryss : BaseSword  /// set item name
    {
       /////////////////Sets Weapon Speical Attacks/////////////
        public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.ArmorIgnore; } }
        public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.InfectiousStrike; } }

       //////////////Set Strenght Req and Min / Max Damage//////
        public override int AosStrengthReq { get { return 10; } }
        public override int AosMinDamage { get { return 10; } }
        public override int AosMaxDamage { get { return 12; } }
        public override int AosSpeed { get { return 53; } }

      ///////////////Not really needed///////////////////
        public override int OldStrengthReq { get { return 10; } }
        public override int OldMinDamage { get { return 3; } }
        public override int OldMaxDamage { get { return 28; } }
        public override int OldSpeed { get { return 53; } }
      ////////////Sets Armor Ignore Min / Max damage /////
        public override int InitMinHits { get { return 31; } }
        public override int InitMaxHits { get { return 90; } }

        [Constructable]
        public MithrilKryss()  
            : base(0x1401)  /// Sets item name and ItemID
        {
            Name = "Mithril Kryss";  /// Sets ingame name
            Weight = 4.0;
            Layer = Layer.OneHanded;  /// Sets weapon layers 1/2 handed
	Hue = 1151;
             Attributes.AttackChance = 10;  /// hit chance increase
             Attributes.WeaponDamage = 20;  ///damage increase
	Attributes.WeaponSpeed = 15;   ///swing speed
            
        }

        public MithrilKryss(Serial serial)  /// sets item name
            : base(serial)
        {
        }

       //////Sets Level//////
        //int itmlevel = (Utility.RandomMinMax(2, 10)); // sets randomlevel
        int ItmLevel = 5;  //sets one level
        ////////////////////

        public override bool OnEquip(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            
                if (!(pm.Level >= ItmLevel))  //Player lvl vs item lvl
                
                {
                    pm.SendMessage("The required level is {0} to use this, you are only {1}", ItmLevel, pm.Level);
                    // Tell player they dont have proper level to equip.
                    return false;
                    //and dont let them equip it.
                }
                
            
            else return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            
                list.Add("Required Level: 5", ItmLevel.ToString()); // value: ~1_val~
                //add required level to equip item to the properties list
        }
       

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}