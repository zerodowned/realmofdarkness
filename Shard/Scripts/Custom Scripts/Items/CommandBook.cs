using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
	
	public class CommandBook : Item
	{
		
		private CommandBookGump m_CommandBookGump;

		[Constructable]
		public CommandBook() : base( 3834 )
		{
		Weight = 1.0;
		Name = "Book of Commands";
		Hue = 1161;	
		Movable = true;
		base.LootType = LootType.Blessed;
		}

		public CommandBook( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
				from.SendGump( new CommandBookGump( from, this ) );
			else
				from.SendLocalizedMessage( 1042001 ); // must be in your pack for you to use it.
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int)1 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			
		}
		
	}


	
public class CommandBookGump : Gump
	{


		public CommandBookGump( Mobile m, CommandBook item)
			: base( 0, 0 )
		{


		
		{
			this.Closable=true;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;

			this.AddPage(1);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard CommandBook");
			
			this.AddLabel(135, 100, 1069, @"[a : Shows Mailing System");
			this.AddLabel(135, 120, 1069, @"[afk Displays an AFK Message");
			this.AddLabel(135, 140, 1069, @"[c : Sends message to the world");
			this.AddLabel(135, 160, 1069, @"[expbar : Shows your experience bar");
			this.AddLabel(135, 180, 1096, @"[help : Lists commands");
			this.AddLabel(135, 200, 1096, @"[Lottery : Gives lottery drawing time and amount");
			this.AddLabel(135, 220, 1096, @"[Mail : Opens mail system");
			this.AddLabel(135, 240, 1096, @"[MOTD : Opens the message of the day");
			this.AddLabel(135, 260, 1096, @"[pm : Sends a private message");
			this.AddLabel(135, 280, 1096, @"[QuestLog : Shows a log of active quests");
			this.AddLabel(135, 300, 1096, @"[QuestPoints : Sows quest points");

			

		}

		}
		



	}
}
   

