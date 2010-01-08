using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	
	public class DragonEgg1 : Item
	{
		

		[Constructable]
		public DragonEgg1() : base( 3164 )
		{
			Weight = 2.0;
			Hue = 1150;
			Name = "a dragon egg";
			
		}

		public DragonEgg1( Serial serial ) : base( serial )
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