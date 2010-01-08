using System;
using Server.Items;
using Server.Network;
using Server.Mobiles;


namespace Server.Items
{
    [FlipableAttribute(0xF47, 0xF48)]
    public class LevelOnlyExample : BaseAxe
    {
        public override WeaponAbility PrimaryAbility { get { return WeaponAbility.BleedAttack; } }
        public override WeaponAbility SecondaryAbility { get { return WeaponAbility.ConcussionBlow; } }

        public override int AosStrengthReq { get { return 35; } }
        public override int AosMinDamage { get { return 15; } }
        public override int AosMaxDamage { get { return 17; } }
        public override int AosSpeed { get { return 31; } }

        public override int OldStrengthReq { get { return 40; } }
        public override int OldMinDamage { get { return 6; } }
        public override int OldMaxDamage { get { return 38; } }
        public override int OldSpeed { get { return 30; } }

        public override int InitMinHits { get { return 31; } }
        public override int InitMaxHits { get { return 70; } }

        [Constructable]
        public LevelOnlyExample()
            : base(0xF47)
        {
            Name = "Example Level Item";
            Weight = 4.0;
            Layer = Layer.TwoHanded;

                WeaponAttributes.HitPoisonArea = 100;
                WeaponAttributes.ResistPoisonBonus = 20;
                Attributes.AttackChance = 15;
                Attributes.WeaponDamage = 50;
            
        }

        public LevelOnlyExample(Serial serial)
            : base(serial)
        {
        }

        #region LSB1
        int itmlevel = (Utility.RandomMinMax(2, 10));
        //sets item level - for now we'll make it a random level from 2 to 10.

        public override bool OnEquip(Mobile from)
        {
            PlayerMobile pm = from as PlayerMobile;

            
                if (!(pm.Level >= itmlevel))
                //is there level lower than the itemlevel?
                {
                    pm.SendMessage("The required level is {0} to use this, you are only {1}", itmlevel, pm.Level);
                    // Tell player they dont have proper level to equip.
                    pm.Hits -= 5;
                    //do some damage
                    return false;
                    //and dont let them equip it.
                }
                
            
            else return true;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            
                list.Add("Required Level: {0}", itmlevel.ToString()); // value: ~1_val~
                //add required level to equip item to the properties list
        }
        #endregion

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}