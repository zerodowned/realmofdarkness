using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class BagOfBolts : Bag
	{
		[Constructable]
		public BagOfBolts() : this( 500 )
		{
		}

		[Constructable]
		public BagOfBolts( int amount )
		{
			DropItem( new Bolt   ( amount ) );
                        			
		}

		public BagOfBolts( Serial serial ) : base( serial )
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