using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class NewbieDungeonExchangeTicket : Item
	{
	

		[Constructable]
		public NewbieDungeonExchangeTicket() : base( 8012 )
		{
		Name = "Exchange Ticket";
		Hue = 1150;
		Weight = 0.1;
		Stackable = true;
		
		}

		public NewbieDungeonExchangeTicket( Serial serial ) : base( serial )
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
		}

		
	}
}