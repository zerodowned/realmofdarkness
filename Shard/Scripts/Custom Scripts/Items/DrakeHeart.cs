using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	
	public class DrakeHeart : Item
	{
		

		[Constructable]
		public DrakeHeart() : base( 3186 )
		{
			Weight = 2.0;
			Hue = 38;
			Name = "a drake heart";
			
		}

		public DrakeHeart( Serial serial ) : base( serial )
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