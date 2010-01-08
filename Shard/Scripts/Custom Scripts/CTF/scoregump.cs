using System;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Mobiles;

namespace Server.Gumps
{
	public class scoregump : Gump
	{
		private CTFGame m_Game;
		private static Mobile m_Mob;

		public scoregump(CTFGame game, Mobile mob) : base( 200, 0 )
		{
			m_Game = game;
			m_Mob = mob;
			this.Closable=true;
			this.Disposable=false;
			this.Dragable=false;
			this.Resizable=false;
			this.AddPage(0);
			this.AddBackground(177, -3, 103, 26, 9550);


					CTFTeam team = (CTFTeam)m_Game.Teams[0];
					CTFTeam team2 = (CTFTeam)m_Game.Teams[1];
				if ( team != null)
					this.AddLabel(224, 1, team.Hue,""+team.Points+"" );
				else
					this.AddLabel(224, 1, team2.Hue,""+team2.Points+"" );

		}
		

	}
}