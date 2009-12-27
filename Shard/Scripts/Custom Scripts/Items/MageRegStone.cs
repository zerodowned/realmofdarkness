using System;
using Server.Items;

namespace Server.Items
{
	public class MageRegStone : Item
	{
		public override string DefaultName
		{
			get { return "a reagent stone: 50 of each magery reagent for 1800 gold"; }
		}

		[Constructable]
		public MageRegStone() : base( 0xED4 )
		{
			Movable = false;
			Hue = 195;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;
			if ( pack.ConsumeTotal( typeof( Gold ), 1800 ) )
			{
			BagOfReagents regBag = new BagOfReagents( 50 );

			if ( !from.AddToBackpack( regBag ) )
				regBag.Delete();
			}
			else
			{
			from.SendMessage( "You do not have enough money.");
			}



		}

		public MageRegStone( Serial serial ) : base( serial )
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