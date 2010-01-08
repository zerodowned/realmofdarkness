using System; 
using Server; 

namespace Server.Items 
{ 
   	public class FullMagerySpellbook : Spellbook
   	{ 
      		[Constructable] 
      		public FullMagerySpellbook()
      		{          
            			this.Content = ulong.MaxValue; 
     		} 

      		public FullMagerySpellbook( Serial serial ) : base( serial ) 
     		{ 
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

	public class FullNecroSpellbook : Spellbook
	{
		public override SpellbookType SpellbookType{ get{ return SpellbookType.Necromancer; } }
		public override int BookOffset{ get{ return 100; } }
		public override int BookCount{ get{ return 17; } }
      
        		[Constructable]
        		public FullNecroSpellbook() : this((ulong)0x1FFFF)
        		{
            			this.Content = (ulong)0x1FFFF;        		
		}
       		[Constructable]
		public FullNecroSpellbook( ulong content ) : base( content, 0x2253 )
		{
			Hue = 0x0;
           			Layer = Layer.OneHanded;
		}

		public FullNecroSpellbook( Serial serial ) : base( serial )
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
			Layer = Layer.Invalid;
		}
	}

	public class FullSpellweavingSpellbook : Spellbook
    	{
        		public override SpellbookType SpellbookType { get { return SpellbookType.Arcanist; } }
        		public override int BookOffset { get { return 600; } }
        		public override int BookCount { get { return 16; } }

        		[Constructable]
        		public FullSpellweavingSpellbook() : this((ulong)0)
       		{
            			this.Content = (ulong)0xFFFF;
        		}
       		[Constructable]
        		public FullSpellweavingSpellbook(ulong content) : base(content, 0x2D50)
        		{
            			Hue = 0x8A2;
            			Layer = Layer.OneHanded;
        		}

        		public FullSpellweavingSpellbook(Serial serial) : base(serial)
        		{
        		}

        		public override void Serialize(GenericWriter writer)
       		{
            			base.Serialize(writer);
            			writer.Write((int)0); // version
        		}

        		public override void Deserialize(GenericReader reader)
        		{
            			base.Deserialize(reader);
            			int version = reader.ReadInt();
            			Layer = Layer.OneHanded;
        		}
    	}
}