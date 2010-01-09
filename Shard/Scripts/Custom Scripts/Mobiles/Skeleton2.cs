using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a skeletal corpse" )]  //Corspe name visable on death of creature
	public class Skeleton2 : BaseCreature  //Public call too ingame same as [add skeleton2
	{
		[Constructable]  // This just means its able to be created in the game
		public Skeleton2() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )  // Public class followed by combat AI, mode, range (tiles), attackspeed, speed, other dont change
		{
			Name = "a skeleton"; //In game name as it shows if you put "a dog" it would be a dog
			Body = Utility.RandomList( 50, 56 ); // in game body value if it were a llama it would be 292
			BaseSoundID = 0x48D; //sound id that it makes 

			SetStr( 56, 80 ); // creature strenght on a random utility can vary from 56-80
			SetDex( 56, 75 );// same thing different stat
			SetInt( 16, 40 );// same

			SetHits( 34, 48 ); // hit points varys from 34-48

			SetDamage( 3, 7 );// damage also adjust with its str 3-7

			SetDamageType( ResistanceType.Physical, 100 ); // damage is 100% physical

			SetResistance( ResistanceType.Physical, 15, 20 );//Resist
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 25, 40 );
			SetResistance( ResistanceType.Poison, 25, 35 );
			SetResistance( ResistanceType.Energy, 5, 15 ); // resist

			SetSkill( SkillName.MagicResist, 45.1, 60.0 ); // skills
			SetSkill( SkillName.Tactics, 45.1, 60.0 ); //skills
			SetSkill( SkillName.Wrestling, 45.1, 55.0 ); //skills

			Fame = 450; //fame awarded on death
			Karma = -450; // karma awarded on death

			VirtualArmor = 16; // virtual armor basically reduces damage

			switch ( Utility.Random( 5 )) //random utility to drop items into pack on death it stats that you will get one out of those five items randomly
			{
				case 0: PackItem( new BoneArms() ); break;
				case 1: PackItem( new BoneChest() ); break;
				case 2: PackItem( new BoneGloves() ); break;
				case 3: PackItem( new BoneLegs() ); break;
				case 4: PackItem( new BoneHelm() ); break;
			}
		}

		public override void GenerateLoot()  //generates loot packs
		{
			AddLoot( LootPack.Poor );
		}
        /// <summary>
        /// /////////////////////////////////////////////////////Copy this shit/////////////////////
        /// ///////////////////////Remember to change the Mobiles name and add a "2" at the end there should be 3
        /// </summary>
        /// <param name="c"></param>


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        public override bool BleedImmune{ get{ return true; } } //bools this is more advanced but is calling its immunities
		public override Poison PoisonImmune{ get{ return Poison.Lesser; } } // same just an override due to the posion
        /// <summary>
        ///         /// </summary>
        /// <param name="c"></param>
        public override void OnDeath(Container c) // overrides the OnDeath command in the Scripts>Engines>AI>Creature>BaseCreature.cs
        {

            base.OnDeath(c); //calls to the container
            switch (Utility.Random(60)) // random % rate being in this case you have a 10% chance to get one of these on drop
            {
                case 0: c.DropItem(new NewbieDungeonExchangeTicket());
                    break;

            }


        }

        /// <summary>
        /// </summary>
        /// <param name="serial"></param>
		public Skeleton2( Serial serial ) : base( serial ) //specific code given when opeaning a server would look like 0x000230010
		{
		}

		public override OppositionGroup OppositionGroup
		{
			get{ return OppositionGroup.FeyAndUndead; }
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
		}
	}
}
