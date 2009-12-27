using System;
using Server.Items;

namespace Server.Items
{
	public class NecroRegStone : Item
	{
		public override string DefaultName
		{
			get { return "a reagent stone: 50 of each necro reagent for 1200 gold"; }
		}

		[Constructable]
		public NecroRegStone() : base( 0xED4 )
		{
			Movable = false;
			Hue = 190;
		}

		public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;
			if ( pack.ConsumeTotal( typeof( Gold ), 1200 ) )
			{
			BagOfNecroReagents regBag = new BagOfNecroReagents( 50 );

			if ( !from.AddToBackpack( regBag ) )
				regBag.Delete();
			}
			else
			{
			from.SendMessage( "You do not have enough money.");
			}



		}

		public NecroRegStone( Serial serial ) : base( serial )
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