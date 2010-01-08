using System;
using Server;
using Server.Mobiles;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	[Flipable( 0x1F03, 0x1F04 )]
	public class RobeOfTheRat : BaseOuterTorso
	{
		

		[Constructable]
		public RobeOfTheRat() : base( 0x1F04, 0xD6 )
		{
			Name = "Robe of the Rat";
			Weight = 3.0;
			SkillBonuses.SetValues( 0, SkillName.Hiding, 5 );
			Attributes.Luck = 5;
			Attributes.BonusDex = 1;
			if( Utility.RandomBool() )
			SkillBonuses.SetValues( 0, SkillName.Hiding, 5 );
			else
			SkillBonuses.SetValues( 0, SkillName.Stealth, 5 );
			
			// TODO: Supports arcane?
			// TODO: Elves Only
		}

		public RobeOfTheRat( Serial serial ) : base( serial )
		{
		}
        		int ItmLevel = 5;  //sets one level
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
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}