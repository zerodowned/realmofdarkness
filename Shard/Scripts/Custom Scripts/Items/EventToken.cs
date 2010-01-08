using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
public class EventToken : Item
	{
		[Constructable]
		public EventToken()
			: this( 1 )
		{
		}

		[Constructable]
		public EventToken( int amountFrom, int amountTo )
			: this( Utility.RandomMinMax( amountFrom, amountTo ) )
		{
		}

		[Constructable]
		public EventToken( int amount )
			: base( 0x3198 )
		{
			Name = "Event Token";
			Stackable = true;
			Amount = amount;
			Hue = 1152;
		}

		public EventToken( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}