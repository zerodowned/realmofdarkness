using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;

namespace Server.Items
{
	public class CTFsash : BaseMiddleTorso
	{
		[Constructable]
		public CTFsash() : this( 0 )
		{
		}

		[Constructable]
		public CTFsash( int hue ) : base( 0x1541, hue )
		{
                        Name = "CTF Sash";
			Hue = 1157;

		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );


				list.Add("Work only in CTF region");
		}

                public override bool OnEquip( Mobile from ) 
		{

                Region region = from.Region;
                PlayerMobile pm = (PlayerMobile)from;

                if (region.Name == "CTF")
                      {
                        Attributes.LowerRegCost = 100;
                        return true;
                      }
                 else
                      { 
                        Attributes.LowerRegCost = 0;
                        return true;
                      }
		}

		public virtual bool Dye( Mobile from, DyeTub sender )
		{
				return false;
		}

		public CTFsash( Serial serial ) : base( serial )
		{
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}