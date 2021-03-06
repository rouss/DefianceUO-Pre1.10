using System;
using Server.Network;

namespace Server.Items
{

	public class StoneSculpture : Item
	{
		[Constructable]
		public StoneSculpture() : base( 0x2848 )
		{
			Movable = true;
			Name = "a rare stone sculpture by lightous";
		}

		public StoneSculpture( Serial serial ) : base( serial )
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