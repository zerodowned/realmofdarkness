using System;
using Server.Items;
using Server.Gumps;
using Server.Mobiles;
using Server.Spells;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using Server;

namespace Server.Items
{
	[FlipableAttribute( 0xEDC, 0xEDB )]
	public class GameJoinStone : Item
	{
		private CTFGame m_Game;
		private string m_GameName;
                private int m_ppln = 0;
                private int m_pplm=12;
		private bool m_AllowSameIP;

                public Timer m_Timer;

		[CommandProperty( AccessLevel.Seer )]
		public CTFGame Game
                { 
                  get{ return m_Game; } 
                  set{ m_Game = value; } 
                }

		[CommandProperty( AccessLevel.Seer )]
		public string GameName
                { 
                  get{ return m_GameName; } 
                  set{ m_GameName = value; } 
                }

		[CommandProperty( AccessLevel.GameMaster )]
		public int PeopleNow
                { 
                  get{ return m_ppln; } 
                  set{ m_ppln = value; InvalidateProperties(); } 
                }

		[CommandProperty( AccessLevel.GameMaster )]
		public int PeopleMax
                { 
                  get{ return m_pplm; } 
                  set{ m_pplm = value; InvalidateProperties(); } 
                }

		[CommandProperty( AccessLevel.GameMaster )]
		public bool AllowSameIP{ get{ return m_AllowSameIP; } set{ m_AllowSameIP = value; } }

		[Constructable]
		public GameJoinStone() : base( 0xEDC )
		{
                        Name = "CTF Join Stone";
			Movable = false;

		}

                public override void GetProperties(ObjectPropertyList list)
                {
                   base.GetProperties(list);

		if (m_Game!=null)
		{

                    	CTFTeam teamo = (CTFTeam)m_Game.Teams[0];
			CTFTeam teamt = (CTFTeam)m_Game.Teams[1];
                        m_ppln = teamo.ActiveMemberCount + teamt.ActiveMemberCount;

                  
                       if ( m_ppln < m_pplm )
                              {
			        m_Timer.Start();
                                list.Add("Players: " + m_ppln.ToString() + " of " + m_pplm.ToString());
                              }
                        else
                              {
			        m_Timer.Start();
                                list.Add("Game in progress");
                              }
		}
                }     


		public override void OnDoubleClick( Mobile from )
		{
if (!m_AllowSameIP)
{
	if (from.SkillsTotal >= 5000)
	{
	List<IPAddress> adr = m_Game.GetAddresses();	

			for(int i = 0; i < adr.Count; ++i)
			{
				if(adr[i].ToString() == from.NetState.Address.ToString())
				{
					from.SendMessage(String.Format("There is already someone signed up with your IP address, {0}.", from.NetState.Address.ToString()));
					return;
				}
			}

	if ( from.InRange( this.GetWorldLocation(), 2 ) ) 
	{ 
			if ( m_Game != null )
			{
				if ( m_Game.OpenJoin )
				{
                                    if(!m_Game.Running)
                                       {
					   if ( m_Game.IsInGame( from ) )
					   {
						from.SendGump( new GameTeamSelector( m_Game) );
					   }
					   else
					   {
						  if ( from.AccessLevel == AccessLevel.Player )
							from.SendGump( new GameJoinGump( m_Game, m_GameName) );
						  else
							from.SendMessage( "It might not be wise for staff to be playing..." );
                                           }
                                       }
                                       else
                                          from.SendMessage( "The Game in Progress. Please return later and try again." );
				}
				else
				{
					from.SendMessage( "{0} join is closed.", m_GameName );
				}
			}
			else
			{
				from.SendMessage( "This stone must be linked to a game stone.  Please contact a game master." );
			}
	}
	else
	{
		from.SendMessage("You must be close to stone.");
	}
	}
	else
	from.SendMessage("You must have biggest that 500 skills total.");
}
else
{
	if (from.SkillsTotal >= 5000)
	{
	if ( from.InRange( this.GetWorldLocation(), 2 ) ) 
	{ 
			if ( m_Game != null )
			{
				if ( m_Game.OpenJoin )
				{
                                    if(!m_Game.Running)
                                       {
					   if ( m_Game.IsInGame( from ) )
					   {
						from.SendGump( new GameTeamSelector( m_Game) );
					   }
					   else
					   {
						  if ( from.AccessLevel == AccessLevel.Player )
							from.SendGump( new GameJoinGump( m_Game, m_GameName) );
						  else
							from.SendMessage( "It might not be wise for staff to be playing..." );
                                           }
                                       }
                                       else
                                          from.SendMessage( "The Game in Progress. Please return later and try again." );
				}
				else
				{
					from.SendMessage( "{0} join is closed.", m_GameName );
				}
			}
			else
			{
				from.SendMessage( "This stone must be linked to a game stone.  Please contact a game master." );
			}
	}
	else
	{
		from.SendMessage("You must be close to stone.");
	}
	}
	else
	from.SendMessage("You must have biggest that 500 skills total.");
}
		}

		public GameJoinStone( Serial serial ) : base( serial )
		{
			m_Timer = new RefreshTimer(this,this);
			m_Timer.Start();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 5); // version
			writer.Write( m_Game );
			writer.Write( m_GameName );
			writer.Write( (int)m_ppln );
			writer.Write( (int)m_pplm );
			writer.Write( (bool)m_AllowSameIP );


		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			switch ( version )
			{
				case 5:
				{
				  m_Game = reader.ReadItem() as CTFGame;
                                  goto case 4;
				}
                                case 4:
                                {
			          m_GameName = reader.ReadString();
                                  goto case 3;
                                }
				case 3:
				{
                                  m_ppln = reader.ReadInt();
                                  goto case 2;
				}

				case 2:
				{
                                  m_pplm = reader.ReadInt();
                                  goto case 1;
				}
				case 1:
				{
				  m_AllowSameIP = reader.ReadBool();
                                  goto case 0;
				}
                                case 0:
                                {
                                  break;
                                }
			}
		}

                private class RefreshTimer : Timer
		{
			private GameJoinStone m_Owner;
			private GameJoinStone m_gam;

			public RefreshTimer( GameJoinStone owner, GameJoinStone gam ) : base( TimeSpan.FromSeconds(0.1) )
			{
				m_Owner = owner;
				m_gam = gam;
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
			  if (m_gam.Game != null)
				m_Owner.PeopleNow++;
			}
		}

	}
}
