using System;
using Server;
using Server.Network;
using Server.Prompts;
using Server.Items;
using Server.Targeting;
using Server.Mobiles;

namespace Xanthos.Evo
{
	public class CheapEvoPointsTarget : Target
	{
		private CheapEvoPointsDeed m_Deed;

		public CheapEvoPointsTarget( CheapEvoPointsDeed deed ) : base( 18, false, TargetFlags.None )
		{
			m_Deed = deed;
		}

		protected override void OnTarget( Mobile from, object target )
		{
			if ( target is BaseCreature && target is IEvoCreature )
			{
				IEvoCreature mob = (IEvoCreature)target;
				BaseCreature creature = (BaseCreature)target;

				if ( creature.Controlled && creature.ControlMaster == from )
				{
					mob.Ep += m_Deed.Points;
					m_Deed.Consume(); // Delete the evo deed
					from.SendMessage( "Your pet consumes the food and gain 1 million EP!" );
					if ( mob.Stage < mob.FinalStage && mob.Ep >= mob.NextEpThreshold )
						mob.Evolve( false );
				}
			}
			else
				from.SendMessage( "You must feed an evolution creature." );
		}
	}

	public class CheapEvoPointsDeed : Item
	{
		private int m_Points;

		[CommandProperty( AccessLevel.GameMaster )]
		public int Points{ get{ return m_Points; } set{ m_Points = value; } }

		[Constructable]
		public CheapEvoPointsDeed() : this( 1000000 )
		{
		}

		[Constructable]
		public CheapEvoPointsDeed( int points ) : base( 0x14F0 )
		{
			Weight = 1.0;
			Hue = 1169;
			Name = "a 1 million EP deed";
			LootType = LootType.Blessed;
			m_Points = points;
		}

		public CheapEvoPointsDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( (int) 0 );
			writer.Write( m_Points );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			LootType = LootType.Blessed;

			int version = reader.ReadEncodedInt();
			m_Points = reader.ReadInt();
		}

		public override bool DisplayLootType{ get{ return true; } }

		public override void OnDoubleClick( Mobile from )
		{
			if ( IsChildOf( from.Backpack ) )
			{
				from.SendMessage( "Select your evolution pet." );
				from.Target = new CheapEvoPointsTarget( this );
			}
			else
				from.SendLocalizedMessage( 1042001 );
		}
	}
}