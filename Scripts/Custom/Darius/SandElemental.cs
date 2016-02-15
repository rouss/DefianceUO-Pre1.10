using System;
using System.Collections;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "a sand elemental corpse" )]
	public class SandElemental : BaseCreature
	{
		public override double DispelDifficulty{ get{ return 117.5; } }
		public override double DispelFocus{ get{ return 45.0; } }

		[Constructable]
		public SandElemental() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a sand elemental";
			Body = 14;
			BaseSoundID = 268;
			Hue = 1834;

			SetStr( 126, 155 );
			SetDex( 66, 85 );
			SetInt( 71, 92 );

			SetHits( 70, 80 );

			SetDamage( 8, 15 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 30, 35 );
			SetResistance( ResistanceType.Fire, 10, 20 );
			SetResistance( ResistanceType.Cold, 10, 20 );
			SetResistance( ResistanceType.Poison, 15, 25 );
			SetResistance( ResistanceType.Energy, 15, 25 );

			SetSkill( SkillName.MagicResist, 60.1, 90.0 );
			SetSkill( SkillName.Tactics, 60.1, 100.0 );
			SetSkill( SkillName.Wrestling, 50.1, 100.0 );

			Fame = 3000;
			Karma = -3000;

			VirtualArmor = 30;


		}

		public override void GenerateLoot( bool spawning )
		{
			AddLoot( LootPack.Meager, 2 );
			AddLoot( LootPack.Gems );
			if ( !spawning )
				if ( Utility.Random( 100 ) < 10 ) PackItem( new Sand() );
		}

		public override int TreasureMapLevel{ get{ return 1; } }

		public SandElemental( Serial serial ) : base( serial )
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