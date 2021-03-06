using System;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
	public class NobleHide : BaseGMJewel
	{
		public override bool CastHide{ get{ return false; } }

		public override void HideEffects(Mobile from)
		{
			from.Hidden = !from.Hidden;
			Entity entity = new Entity( from.Serial, from.Location, from.Map );
			if (from.Hidden)
			{
				Effects.SendLocationParticles( entity, 0x3709, 1, 30, 5, 7, 9965, 0 );
				Effects.SendLocationParticles( entity, 0x376A, 1, 30, 5, 3, 9502, 0 );
			}
			else
			{
				from.FixedParticles( 0x3709, 1, 30, 9965, 5, 7, EffectLayer.Waist );
				from.FixedParticles( 0x376A, 1, 30, 9502, 5, 3, (EffectLayer)255 );
			}
			Effects.PlaySound( entity.Location, entity.Map, 0x244 );
		}

		[Constructable]
		public NobleHide() : base(AccessLevel.GameMaster, 0xCB, 0x1ECD )
		{
			Hue = 2119;
			Name = "GM Noble Ball";
		}
		public NobleHide( Serial serial ) : base( serial )
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
	}
}