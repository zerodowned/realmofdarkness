using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a godly corpse" )]
	public class Ares : BaseCreature
	{
		private DateTime nextabil;
		private DateTime nextspawn;
		private int stage;
		private int kills;
		private int minibosskills;
		private ArrayList spawns;
		private Item chair;
		private int hits;
		
		[Constructable]
		public Ares() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			this.Female = false;
			stage = 0;
			kills = 0;
			minibosskills = 0;
			spawns = new ArrayList();
			Name = "Ares";
			Body = 400;

			SetStr( 400 );
			SetDex( 104, 260 );
			SetInt( 91, 100 );

			SetHits( 100000 );
			hits = this.Hits;

			SetDamage( 30, 50 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 100 );
			SetResistance( ResistanceType.Fire, -10 );
			SetResistance( ResistanceType.Cold, -10 );
			SetResistance( ResistanceType.Poison, -10 );
			SetResistance( ResistanceType.Energy, -10 );

			SetSkill( SkillName.Anatomy, 300.0 );
			SetSkill( SkillName.Swords, 300.0 );
			SetSkill( SkillName.MagicResist, 50.3, 80.0 );
			SetSkill( SkillName.Tactics, 300.0 );
			SetSkill( SkillName.ArmsLore, 300.0 );

			Fame = 10000;
			Karma = -10000;

			VirtualArmor = 50;
			HairItemID = 8252;
			HairHue = 1174;

			AddItem( new BladeOfAres() );
			AddItem( new AresChest() );
			AddItem( new AresArms() );
			AddItem( new AresGloves() );
			AddItem( new AresCloak() );
			AddItem( new AresLegs() );
			AddItem( new AresBoots() );
			this.CantWalk = true;
		}

		public override bool AlwaysMurderer{ get{ return true; }}

		public override void OnThink()
		{
			if ( this.stage == 0 )
				Stage1();
			if ( nextabil <= DateTime.Now )
			{
				nextabil = DateTime.Now + TimeSpan.FromSeconds( 30.0 );
				DoSpecialAttack();
			}

			if ( nextspawn <= DateTime.Now && spawns.Count < 30 )
			{
				nextspawn = DateTime.Now + TimeSpan.FromMinutes( 2.0 );
				DoSpawn();
			}

			if ( this.Hits > hits )
				this.Hits = hits;

			hits = this.Hits;

			base.OnThink();
		}

		public void DoSpecialAttack()
		{
			ArrayList mlist = new ArrayList();
			IPooledEnumerable eable = this.Map.GetMobilesInRange( this.Location, 10 );
			foreach( Mobile m in eable )
				mlist.Add( m );
			eable.Free();
			if ( mlist != null && mlist.Count > 0 )
			{
				for( int i = 0; i < mlist.Count; i++ )
				{
					Mobile m = (Mobile)mlist[i];
					if ( m is PlayerMobile )
					{
						AOS.Damage( m, this, Utility.Random( 40, 60 ), 0, 100, 0, 0, 0 );
						m.FixedParticles( 0x3709, 10, 30, 5052, EffectLayer.LeftFoot );
						this.Say( "Ha ha ha!" );
					}
				}
			}
		}

		public void AddKill()
		{
			if ( kills < 1000 )
			{
				kills += 1;
				if ( kills >= 1000 )
					Stage2();
			}
			else if ( this.stage == 2 )
			{
				minibosskills += 1;
				if ( minibosskills >= 4 )
					Stage3();
			}
		}

		public void DoSpawn()
		{
			if ( stage == 1 && spawns.Count < 30 )
			{
				int chance = Utility.Random( 1, 3 );
				if ( chance == 1 )
				{
					WarTroll troll = new WarTroll( this );
					troll.Home = new Point3D( this.X, this.Y, this.Z - 20 );
					troll.RangeHome = 30;
					troll.MoveToWorld( troll.Home, this.Map );
				}
				if ( chance == 2 )
				{
					WarSpirit troll = new WarSpirit( this );
					troll.Home = new Point3D( this.X, this.Y, this.Z - 20 );
					troll.RangeHome = 30;
					troll.MoveToWorld( troll.Home, this.Map );
				}
				if ( chance == 3 )
				{
					WarMonger troll = new WarMonger( this );
					troll.Home = new Point3D( this.X, this.Y, this.Z - 20 );
					troll.RangeHome = 30;
					troll.MoveToWorld( troll.Home, this.Map );
				}
			}
			else if ( stage == 2 && spawns.Count < 4 )
			{
				WarMonger troll = new WarMonger( this );
				troll.Name = "Spawn of Ares";
				troll.HitsMaxSeed += 5000;
				troll.Hits += 5000;
				troll.DamageMin += 20;
				troll.DamageMax += 20;
				troll.Home = new Point3D( this.X, this.Y, this.Z - 20 );
				troll.RangeHome = 30;
				troll.MoveToWorld( troll.Home, this.Map );
			}				
		}

		public void Stage1()
		{
			this.Frozen = true;
			stage = 1;
			StoneChair chair1 = new StoneChair();
			chair1.Movable = false;
			chair1.Hue = 2949;
			this.chair = chair1;
			chair1.MoveToWorld( this.Location, this.Map );
			this.Z += 20;
			chair1.Z += 20;
			this.Blessed = true;
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
			DoSpawn();
		}

		public void Stage2()
		{
			stage = 2;
			this.Say( "Come forth, my children!" );
			if ( spawns.Count > 0 )
			{
				foreach( Mobile m in spawns )
					m.Delete();
			}
			this.DoSpawn();
		}

		public void Stage3()
		{
			this.Frozen = false;
			this.Blessed = false;
			stage = 3;
			if ( this.chair != null )
				this.chair.Delete();
			this.CantWalk = false;
			this.Say( "FACE MY WRATH!" );
			new DescendTimer( this, 0 ).Start();
		}
			

		public override void GenerateLoot()
		{
			AddLoot( LootPack.SuperBoss );
			AddLoot( LootPack.Gems );
			AddLoot( LootPack.Gems );
			AddLoot( LootPack.Gems );
		}

		public Ares( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( (int) stage );
			writer.Write( (int) kills );
			writer.Write( (int) minibosskills );
			writer.Write( (Item) chair );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			stage = reader.ReadInt();
			kills = reader.ReadInt();
			minibosskills = reader.ReadInt();
			chair = reader.ReadItem();
		}

		private class DescendTimer : Timer
		{
			private Mobile m;
			private int count;
			public DescendTimer( Mobile from, int c ) : base( TimeSpan.FromSeconds( 0.1 ))
			{
				m = from;
				count = c;
			}
			protected override void OnTick()
			{
				if ( m != null && count < 20 )
				{
					count += 1;
					m.Z -= 1;
					new DescendTimer( m, count ).Start();
				}
			}
		}
	}
}