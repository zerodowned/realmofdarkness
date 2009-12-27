using System;
using Server.Items;

namespace Server.Items
{
	public class BoltSupplyStone : Item
	{
		public override string DefaultName
		{
			get { return "a bolt supply stone: 500 bolts for 5000 gold"; }
		}

		[Constructable]
		public BoltSupplyStone() : base( 0xED4 )
		{
			Movable = false;
			Hue = 240;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;
			if ( pack.ConsumeTotal( typeof( Gold ), 5000 ) )
			{
			BagOfBolts regBag = new BagOfBolts( 500 );

			if ( !from.AddToBackpack( regBag ) )
				regBag.Delete();
			}
			else
			{
			from.SendMessage( "You do not have enough money.");
			}



		}

		public BoltSupplyStone( Serial serial ) : base( serial )
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