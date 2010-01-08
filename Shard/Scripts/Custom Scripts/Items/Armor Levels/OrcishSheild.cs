using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{


	
	public class OrcishShield : WoodenKiteShield
    {
       
        

      public override int BasePhysicalResistance{ get{ return 5; } }
	

        [Constructable]
        public OrcishShield()
        {
            Name = "Orcish Shield";
            Weight = 4.0;
		Hue = 55;
		Attributes.BonusHits = 1;
		Attributes.DefendChance = 5;
            
            
        }

        public OrcishShield( Serial serial ) : base(serial)
        {
        }

       //////Sets Level//////
        //int itmlevel = (Utility.RandomMinMax(2, 10)); // sets randomlevel
        int ItmLevel = 7;  //sets one levelL        ////////////////////

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