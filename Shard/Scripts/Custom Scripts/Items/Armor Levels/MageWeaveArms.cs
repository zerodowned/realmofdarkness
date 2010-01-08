using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{


	[FlipableAttribute( 0x13cd, 0x13c5 )]
	public class MageWeaveArms : LeatherArms
    {
       
        
	public override int BaseFireResistance{ get{ return 4; } }
      public override int BasePhysicalResistance{ get{ return 6; } }
	public override int BaseColdResistance{ get{ return 12; } }
	public override int BaseEnergyResistance{ get{ return 10; } }
	public override int BasePoisonResistance{ get{ return 3; } }

        [Constructable]
        public MageWeaveArms()
        {
            Name = "Mage Weave Arms";
            Weight = 2.0;
		Hue = 0;
		Attributes.LowerRegCost = 10;
		Attributes.LowerManaCost = 4;
            
            
        }

        public MageWeaveArms( Serial serial ) : base(serial)
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

            
                list.Add("Required Level: {0}", ItmLevel.ToString()); // value: ~1_val~
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