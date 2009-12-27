using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{



	public class HideOfCronos : StuddedChest
    {
       
        
	public override int BaseFireResistance{ get{ return 8; } }
      public override int BasePhysicalResistance{ get{ return 18; } }
	public override int BaseColdResistance{ get{ return 9; } }
	public override int BaseEnergyResistance{ get{ return 7; } }
	public override int BasePoisonResistance{ get{ return 7; } }

        [Constructable]
        public HideOfCronos()
        {
            Name = "Hide of Cronos";
            Weight = 12.0;
		Hue = 0;
            ArmorAttributes.MageArmor = 1;
		Attributes.BonusHits = 3;
		Attributes.LowerManaCost = 5;
            
        }

        public HideOfCronos( Serial serial ) : base(serial)
        {
        }

       //////Sets Level//////
        //int itmlevel = (Utility.RandomMinMax(2, 10)); // sets randomlevel
        int itmlevel = 10;  //sets one level
        ////////////////////

        public override bool OnEquip(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            
                if (!(pm.Level >= itmlevel))  //Player lvl vs item lvl
                
                {
                    pm.SendMessage("The required level is {0} to use this, you are only {1}", itmlevel, pm.Level);
                    // Tell player they dont have proper level to equip.
                    return false;
                    //and dont let them equip it.
                }
                
            
            else return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            
                list.Add("Required Level: 10", itmlevel.ToString()); // value: ~1_val~
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