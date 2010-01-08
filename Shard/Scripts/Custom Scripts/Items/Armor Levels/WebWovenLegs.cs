using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{


	[FlipableAttribute( 0x13cb, 0x13d2 )]
	public class WebWovenLegs : LeatherLegs
    {
       
        
	public override int BaseFireResistance{ get{ return 2; } }
      public override int BasePhysicalResistance{ get{ return 2; } }
	public override int BaseColdResistance{ get{ return 16; } }
	public override int BaseEnergyResistance{ get{ return 16; } }
	public override int BasePoisonResistance{ get{ return 4; } }

        [Constructable]
        public WebWovenLegs()
        {
           	Name = "Web Woven Legs"; 
            Weight = 2.0;
		Hue = 1150;
            


		if( Utility.RandomBool() )
				{
				Attributes.BonusHits = 5;
				Attributes.AttackChance = 5;
				
				}
		else
				{
				Attributes.LowerManaCost = 4;
				Attributes.LowerRegCost = 8;
            		
				}
        }

        public WebWovenLegs( Serial serial ) : base(serial)
        {
        }

       //////Sets Level//////
        int ItmLevel = (Utility.RandomMinMax(5, 10)); // sets randomlevel
        //int itmlevel = 0;  //sets one level
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