using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "eldor corpse" )]
	public class Eldor : BaseCreature
	{
		[Constructable]
		public Eldor () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "Eldor the Dreaded";
			Body = 11;
			BaseSoundID = 1170;

			SetStr( 196, 220 );
			SetDex( 126, 145 );
			SetInt( 286, 310 );

			SetHits( 118, 132 );

			SetDamage( 5, 17 );

			SetDamageType( ResistanceType.Physical, 20 );
			SetDamageType( ResistanceType.Poison, 80 );

			SetResistance( ResistanceType.Physical, 40, 50 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 20, 30 );
			SetResistance( ResistanceType.Poison, 90, 100 );
			SetResistance( ResistanceType.Energy, 20, 30 );

			SetSkill( SkillName.EvalInt, 65.1, 70.0 );
			SetSkill( SkillName.Magery, 45.1, 60.0 );
			SetSkill( SkillName.Meditation, 65.1, 80.0 );
			SetSkill( SkillName.MagicResist, 45.1, 60.0 );
			SetSkill( SkillName.Tactics, 55.1, 60.0 );
			SetSkill( SkillName.Wrestling, 60.1, 61.0 );

			Fame = 1000;
			Karma = -1000;

			VirtualArmor = 36;

			PackItem( new SpidersSilk( 8 ) );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich );
		}

		public override Poison PoisonImmune{ get{ return Poison.Lethal; } }
		public override Poison HitPoison{ get{ return Poison.Lethal; } }
		public override int TreasureMapLevel{ get{ return 3; } }

		public Eldor( Serial serial ) : base( serial )
		{
		}

		public override void OnDeath( Container c )
		{
		
		base.OnDeath( c );
		switch ( Utility.Random ( 10 ) )
		{
			case 0: c.DropItem( new WebWovenLegs() ); 
			break;

		}


		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			if ( BaseSoundID == 263 )
				BaseSoundID = 1170;
		}
	}
}