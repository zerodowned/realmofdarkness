using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Mobiles;

namespace Server.Gumps
{
	public class GameTeamSelector : Gump
	{
		private CTFGame m_Game;
		private int m_TeamSize;

		public GameTeamSelector( CTFGame game ) : this( game, game.TeamSize )
		{
		}


		public GameTeamSelector( CTFGame game, int teamSize ) : base( 50, 50 )
		{
                            
			m_Game = game;
			m_TeamSize = teamSize;



			Closable = true;
			Dragable = false;

			AddPage( 0 );
			AddBackground( 0, 0, 250, 220, 5054 );
			AddBackground( 10, 10, 230, 200, 3000 );

			AddPage( 1 );
			AddLabel( 20, 20, 0, "Select a team:" );

					AddButton( 20, 60, 4005, 4006, 1, GumpButtonType.Reply, 0 );
					AddLabel( 55, 60, 0, "Join To CTF");	

		}
      public enum Buttons
          {
            OK,
          }

		public override void OnResponse( NetState state, RelayInfo info )
		{
                       
			Mobile from = state.Mobile;

		if (from.Mount != null)
			{
				from.SendMessage("You must be unmounted to join the Game!");
			}
		else
		{

            		switch (info.ButtonID)
           		{
                    		case 0:
                    		{
                      			break;
                    		}
					if ( m_Game.Deleted )
						return;

                    		case 1:
                    		{
					if (from.Mount == null)
					{
					CTFTeam teamo = (CTFTeam)m_Game.Teams[0];
					CTFTeam teamt = (CTFTeam)m_Game.Teams[1];
					if ( teamo.ActiveMemberCount == m_TeamSize )
                                	{
                                        	CTFTeam teama = m_Game.GetTeam( 1 );	

						if ( teama != null && teama.ActiveMemberCount < m_TeamSize )
						{
							from.MoveToWorld( new Point3D( 5704, 1042, 0 ), Map.Felucca );
			        			m_Game.SwitchTeams( from, teama );
							from.SendMessage( "You have joined to CTF." );
						}
						else
						{
                         				from.CloseGump( typeof( GameTeamSelector ) );
						}

                        			if( teamo.ActiveMemberCount == m_Game.TeamSize && teamt.ActiveMemberCount == m_Game.TeamSize)
                           				m_Game.ResetGame();
                      			}
                     			else if( teamt.ActiveMemberCount == m_TeamSize ) 
                      			{
                                        	CTFTeam teamb = m_Game.GetTeam( 0 );	

						if ( teamb != null && teamb.ActiveMemberCount < m_TeamSize )
						{
							from.MoveToWorld( new Point3D( 5704, 1117, 0 ), Map.Felucca );
			        			m_Game.SwitchTeams( from, teamb );
							from.SendMessage( "You have joined to CTF." );
						}
						else
						{
                         				from.CloseGump( typeof( GameTeamSelector ) );
						}	
                        			if( teamo.ActiveMemberCount == m_Game.TeamSize && teamt.ActiveMemberCount == m_Game.TeamSize)
                           				m_Game.ResetGame();
                      			}
                      			else
                       			{
                        			CTFTeam teamc = m_Game.GetTeam( Utility.Random( 2 ) );
						if ( teamc != null && teamc.ActiveMemberCount < m_TeamSize )
						{
							from.MoveToWorld( new Point3D( 1571, 1743, 15 ), Map.Felucca );
			       			 	m_Game.SwitchTeams( from, teamc );
							from.SendMessage( "You have joined to CTF." );
						}
						else
						{
                         				from.CloseGump( typeof( GameTeamSelector ) );
						}
                          			if( teamo.ActiveMemberCount == m_Game.TeamSize && teamt.ActiveMemberCount == m_Game.TeamSize)
                           				m_Game.ResetGame();
                       			}
					}
					else
						from.SendMessage("I say unmount first!");
                         		break;
                    		}  
			}	
		}
	}   
}

	public class GameJoinGump : Gump
	{
		private CTFGame m_Game;
		public GameJoinGump( CTFGame game, string gameName ) : base( 20, 30 )
		{
			m_Game = game;

			AddPage( 0 );
			AddBackground( 0, 0, 550, 220, 5054 );
			AddBackground( 10, 10, 530, 200, 3000 );
			
			AddPage( 1 );
			AddLabel( 20, 20, 0, String.Format( "Welcome to {0}!", gameName ) );
			AddLabel( 20, 60, 0, "You may join to this game and finally will get your prize!" );
			AddLabel( 20, 80, 0, "For any cheater actions you will go to jail." );
			AddLabel( 20, 100, 0, "Will be HONEST.  Enjoy!" );

			AddLabel( 55, 180, 0, "Cancel" );
			AddButton( 20, 180, 4005, 4006, 0, GumpButtonType.Reply, 0 );
			AddLabel( 165, 180, 0, "Okay, Join!" );
			AddButton( 130, 180, 4005, 4006, 1, GumpButtonType.Reply, 0 );
		}

                public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;
			from.CloseGump( typeof( GameJoinGump ) );

			if ( info.ButtonID == 1 )
				from.SendGump( new GameTeamSelector(m_Game) );
		}
	}
}
