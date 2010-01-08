using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	
	public class DragonHeart : Item
	{
		

		[Constructable]
		public DragonHeart() : base( 2487 )
		{
			Weight = 2.0;
			Hue = 38;
			Name = "a dragon heart";
			
		}

		public DragonHeart( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			if ( Weight == 1.0 )
				Weight = 2.0;
		}
	}
}