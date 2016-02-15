using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "an ore elemental corpse" )]
	public class VeriteElemental : BaseCreature
	{
		[Constructable]
		public VeriteElemental() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a verite elemental";
			Body = 113;
			BaseSoundID = 268;

			SetStr( 226, 255 );
			SetDex( 126, 145 );
			SetInt( 71, 92 );

			SetHits( 136, 153 );

			SetDamage( 9, 16 );

			SetSkill( SkillName.MagicResist, 50.1, 95.0 );
			SetSkill( SkillName.Tactics, 60.1, 100.0 );
			SetSkill( SkillName.Wrestling, 60.1, 100.0 );

			Fame = 3500;
			Karma = -3500;

			VirtualArmor = 35;

			PackGold( 250, 400 );
			PackGem();
			PackItem( new VeriteOre( 25 ) );
			PackSlayer();
			PackArmor( 0, 4 );
			PackWeapon( 0, 4 );
		}

		public override bool AutoDispel{ get{ return true; } }

		public VeriteElemental( Serial serial ) : base( serial )
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