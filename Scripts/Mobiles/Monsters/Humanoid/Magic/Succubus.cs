using System;
using System.Collections;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a succubus corpse" )]
	public class Succubus : BaseCreature
	{
		[Constructable]
		public Succubus () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.1, 0.2 )
		{
			Name = "a succubus";
			Body = 149;
			BaseSoundID = 0x4B0;

			SetStr( 488, 620 );
			SetDex( 121, 170 );
			SetInt( 498, 657 );

			SetHits( 312, 353 );

			SetDamage( 18, 28 );

			SetSkill( SkillName.EvalInt, 90.1, 100.0 );
			SetSkill( SkillName.Magery, 99.1, 100.0 );
			SetSkill( SkillName.Meditation, 90.1, 100.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );
			SetSkill( SkillName.Tactics, 80.1, 90.0 );
			SetSkill( SkillName.Wrestling, 80.1, 90.0 );

			Fame = 24000;
			Karma = -24000;

			VirtualArmor = 80;

			PackGold( 850, 1100 );
			PackScroll( 4, 8 );
			PackScroll( 4, 8 );
			PackArmor( 0, 5 );
			PackWeapon( 0, 5 );
			PackArmor( 0, 5 );
			PackWeapon( 0, 5 );
			PackSlayer();

                           switch ( Utility.Random( 200 ))
        		 {
           			case 0: PackItem( new SexChangeDeed() ); break;
        		 }
		}

		public void DrainLife()
		{
			ArrayList list = new ArrayList();

			foreach ( Mobile m in this.GetMobilesInRange( 2 ) )
			{
				if ( m == this || !CanBeHarmful( m ) )
					continue;

				if ( m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned || ((BaseCreature)m).Team != this.Team) )
					list.Add( m );
				else if ( m.Player )
					list.Add( m );
			}

			foreach ( Mobile m in list )
			{
				DoHarmful( m );

				m.FixedParticles( 0x374A, 10, 15, 5013, 0x496, 0, EffectLayer.Waist );
				m.PlaySound( 0x231 );

				m.SendMessage( "You feel the life drain out of you!" );

				int toDrain = Utility.RandomMinMax( 10, 40 );

				Hits += toDrain;
				m.Damage( toDrain, this );
			}
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			base.OnGaveMeleeAttack( defender );

			if ( 0.1 >= Utility.RandomDouble() )
				DrainLife();
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );

			if ( 0.1 >= Utility.RandomDouble() )
				DrainLife();
		}

		public Succubus( Serial serial ) : base( serial )
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
}