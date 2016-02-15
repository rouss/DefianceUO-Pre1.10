using System;
using Server.Network;
using Server.Targeting;
using Server.Items;
using Server.Spells;
using Server.Mobiles;

namespace Server.Items
{
	[FlipableAttribute( 0xF52, 0xF51 )]
	public class BlessingDagger : BaseKnife
	{
		public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.InfectiousStrike; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.ShadowStrike; } }

		public override int AosStrengthReq{ get{ return 10; } }
		public override int AosMinDamage{ get{ return 10; } }
		public override int AosMaxDamage{ get{ return 11; } }
		public override int AosSpeed{ get{ return 56; } }

		public override int OldStrengthReq{ get{ return 1; } }
		public override int OldMinDamage{ get{ return 3; } }
		public override int OldMaxDamage{ get{ return 15; } }
		public override int OldSpeed{ get{ return 55; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 40; } }

		public override SkillName DefSkill{ get{ return SkillName.Fencing; } }
		public override WeaponType DefType{ get{ return WeaponType.Piercing; } }
		public override WeaponAnimation DefAnimation{ get{ return WeaponAnimation.Pierce1H; } }

		[Constructable]
		public BlessingDagger() : base( 0xF52 )
		{
			Weight = 1.0;
			Name = "Dagger of Blessing";
		}

		public BlessingDagger( Serial serial ) : base( serial )
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
		}

		public override void OnHit( Mobile attacker, Mobile defender )
		{
			if ( defender is PlayerMobile )
			{
				AOS.Damage( attacker, 50, 0, 100, 0, 0, 0 );
				Delete();
				attacker.FixedParticles( 0x36BD, 20, 10, 5044, EffectLayer.Head );
				attacker.PlaySound( 0x307 );
			}
			if ( attacker.Alive )
			{
				if ( 0.07 > Utility.RandomDouble() )
				{
					SpellHelper.AddStatBonus( attacker, attacker, StatType.Str, 11, TimeSpan.FromMinutes( 2.0 ) );
					SpellHelper.AddStatBonus( attacker, attacker, StatType.Dex, 11, TimeSpan.FromMinutes( 2.0 ) );
					SpellHelper.AddStatBonus( attacker, attacker, StatType.Int, 11, TimeSpan.FromMinutes( 2.0 ) );

					attacker.FixedParticles( 0x373A, 10, 15, 5018, EffectLayer.Waist );
					attacker.PlaySound( 0x1EA );
					attacker.SendMessage("The strength of the dagger empowers you!");
				}
				base.OnHit( attacker, defender );
			}
		}
	}
}