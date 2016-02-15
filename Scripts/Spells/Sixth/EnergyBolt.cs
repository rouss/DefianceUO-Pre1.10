using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Sixth
{
	public class EnergyBoltSpell : Spell
	{
		private static SpellInfo m_Info = new SpellInfo(
				"Energy Bolt", "Corp Por",
				SpellCircle.Sixth,
				230,
				9022,
				Reagent.BlackPearl,
				Reagent.Nightshade
			);

		public EnergyBoltSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public override bool DelayedDamage{ get{ return true; } }

		public void Target( Mobile m )
		{
			if ( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if ( CheckHSequence( m ) )
			{
				Mobile source = Caster;

				SpellHelper.Turn( Caster, m );

				SpellHelper.CheckReflect( (int)this.Circle, ref source, ref m );

				double damage;

				/*if ( Core.AOS )
					damage = Utility.Random( 19, 18 );
				else
				{*/
					damage = Utility.Random( 26, 9 );

					if ( CheckResisted( m ) )
					{
						damage *= 0.75;

						m.SendLocalizedMessage( 501783 ); // You feel yourself resisting magical energy.
					}

					// Scale damage based on evalint and resist
					damage *= GetDamageScalar( m );
				//}

				// Do the effects
				source.MovingParticles( m, 0x379F, 7, 0, false, true, 3043, 4043, 0x211 );
				source.PlaySound( 0x20A );

				InternalTimer t = new InternalTimer( this, damage, m );
				t.Start();
			}

			FinishSequence();
		}

		private class InternalTimer : Timer
		{
			private Spell m_Spell;
			private Double m_Damage;
			private Mobile m_Defender;

			public InternalTimer( Spell spell, Double damage, Mobile defender ) : base( TimeSpan.FromSeconds( 0.7 ) )
			{
				m_Spell = spell;
				m_Damage = damage;
				m_Defender = defender;

				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				// Deal the damage
				SpellHelper.Damage( m_Spell, m_Defender, m_Damage, 0, 0, 0, 0, 100 );
			}
		}

		private class InternalTarget : Target
		{
			private EnergyBoltSpell m_Owner;

			public InternalTarget( EnergyBoltSpell owner ) : base( 12, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile from, object o )
			{
				if ( o is Mobile )
					m_Owner.Target( (Mobile)o );
			}

			protected override void OnTargetFinish( Mobile from )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}