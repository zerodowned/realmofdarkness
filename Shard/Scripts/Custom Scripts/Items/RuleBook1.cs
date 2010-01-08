using System;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Gumps;
using Server.Mobiles;

namespace Server.Items
{
	
	public class RuleBook1 : Item
	{
		
		private RuleBookGump m_Rgump;

		[Constructable]
		public RuleBook1() : base( 8787 )
		{
		Weight = 1.0;
		Name = "Realm of Darkness Rule Book";
		Hue = 1174;	
		Movable = false;
		base.LootType = LootType.Blessed;
		}

		public RuleBook1( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
				from.SendGump( new RuleBookGump( from, this ) );
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


	
public class RuleBookGump : Gump
	{
private Mobile m_Mobile;
private RuleBook1 m_Book;

		public RuleBookGump( Mobile m, RuleBook1 item)
			: base( 0, 0 )
		{
m_Mobile = m;
m_Book = item;

		
		{
			this.Closable=false;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;

			this.AddPage(1);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard Rules");
			this.AddButton(230, 350, 242, 241, (int)Buttons.btnCancel0, GumpButtonType.Reply, 0);
			this.AddLabel(175, 100, 1069, @"Harrassment");
			this.AddLabel(195, 140, 1069, @"Staff");
			this.AddLabel(185, 180, 1069, @"Ventrilo");
			this.AddLabel(185, 220, 1069, @"Website");
			this.AddLabel(155, 270, 25, @"I Accept To All The Rules");
			this.AddLabel(155, 310, 25, @"I Dont Accept To All The Rules");
			this.AddButton(285, 100, 247, 248, (int)Buttons.btnHarrassment, GumpButtonType.Page, 2);
			this.AddButton(285, 220, 247, 248, (int)Buttons.btnStaff, GumpButtonType.Page, 5);
			this.AddButton(285, 140, 247, 248, (int)Buttons.btnWebsite, GumpButtonType.Page, 3);
			this.AddButton(285, 180, 247, 248, (int)Buttons.btnVent, GumpButtonType.Page, 4);
			this.AddItem(90, 50, 2);
            			this.AddItem(110, 50, 3);
            			this.AddItem(372, 49, 4);
            			this.AddItem(394, 50, 5);
			this.AddItem(110, 340, 8378);
			this.AddItem(375, 340, 8378);
			this.AddButton(125, 310, 210, 211, (int)Buttons.btnDontAccept, GumpButtonType.Reply, 1);
			//this.AddCheck(125, 270, 210, 211, false, 2);
			this.AddButton(125, 270, 210, 211, (int)Buttons.btnAccept, GumpButtonType.Reply, 2);

			this.AddPage(2);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddItem(110, 50, 3);
			this.AddItem(90, 50, 2);
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard Rules");
			this.AddItem(372, 49, 4);
			this.AddItem(394, 50, 5);
			this.AddItem(110, 340, 8378);
			this.AddButton(230, 350, 249, 247, (int)Buttons.btnOkay1, GumpButtonType.Page, 1);
			this.AddItem(375, 340, 8378);
			this.AddLabel(210, 90, 1069, @"Harrassment");
			this.AddHtml( 170, 130, 200, 130, @"Harrassment is not accepted.  These include: Racial Slurs, Verbal, Sexual, or any other form. If you are being harassed contact a GM.  The player violating this rule may suffer jailing, banning, or any other punishment seen fit.", (bool)true, (bool)true);
			
            			this.AddPage(3);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddItem(110, 340, 8378);
			this.AddItem(375, 340, 8378);
			this.AddButton(230, 350, 249, 247, (int)Buttons.btnOkay2, GumpButtonType.Page, 1);
			this.AddHtml( 170, 130, 200, 130, @"Staff
1) Staff will not make items.
2) Staff will not edit items.
3) Staff will not hue items.
4) Staff will not adjust player status.
5) Staff will not remove you from jail.
6) Staff will not unban you", (bool)true, (bool)true);
			this.AddItem(90, 50, 2);
			this.AddItem(110, 50, 3);
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard Rules");
			this.AddItem(372, 49, 4);
			this.AddItem(394, 50, 5);
			this.AddLabel(240, 90, 1069, @"Staff");
			
            this.AddPage(4);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddHtml( 170, 130, 200, 130, @"Vent
1) Hostname or IP sanjose.ventriloservers.biz
2) Port 3791", (bool)true, (bool)true);
			this.AddItem(372, 49, 4);
			this.AddItem(394, 50, 5);
			this.AddLabel(215, 90, 1069, @"Vent");
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard Rules");
			this.AddItem(110, 50, 3);
			this.AddItem(90, 50, 2);
			this.AddItem(110, 340, 8378);
			this.AddButton(230, 350, 249, 247, (int)Buttons.btnOkay3, GumpButtonType.Page, 1);
			this.AddItem(375, 340, 8378);
			
            this.AddPage(5);
			this.AddBackground(100, 41, 330, 354, 9200);
			this.AddHtml( 170, 130, 200, 130, @"Weblink", (bool)true, (bool)true);
			this.AddItem(372, 49, 4);
			this.AddItem(394, 50, 5);
			this.AddLabel(230, 90, 1069, @"Website");
			this.AddLabel(175, 60, 232, @"Realm of Darkness Shard Rules");
			this.AddItem(110, 50, 3);
			this.AddItem(90, 50, 2);
			this.AddItem(110, 340, 8378);
			this.AddButton(230, 350, 249, 247, (int)Buttons.btnOkay4, GumpButtonType.Page, 1);
			this.AddItem(375, 340, 8378);

		}

		}
		public enum Buttons
		{
			btnCancel0,
			RadDontAccept,
			btnHarrassment,
			btnVent,
			btnStaff,
			btnWebsite,
			btnDontAccept,
			btnAccept,
			btnOkay1,
			btnOkay2,
			btnOkay3,
			btnOkay4,
		}
        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;
            switch ( info.ButtonID ) 
            {
                case 1: //Close gump
                    {
                        from.CloseGump( typeof( RuleBookGump ) );
                        break;
                    }
                case 7:
                     {  
	         		
                       
		from.SendMessage( "Thank you for accepting to play by the rules" );
                        from.MoveToWorld(new Point3D(1436,1696,0), Map.Felucca);
		m_Book.Delete();
		break;
                    }



	}
   }
}
}
