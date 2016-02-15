using System;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.Multis;
using Server.Mobiles;

namespace Server.SkillHandlers
{
	public class Hiding
	{
		public static void Initialize()
		{
			SkillInfo.Table[21].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			if ( m.Target != null || m.Spell != null )
			{
				m.SendLocalizedMessage( 501238 ); // You are busy doing something else and cannot hide.
				return TimeSpan.FromSeconds( 1.0 );
			}

			double bonus = 0.0;

			BaseHouse house = BaseHouse.FindHouseAt( m );

			if ( house != null && house.IsFriend( m ) )
			{
				bonus = 100.0;
			}
			else if ( !Core.AOS )
			{
				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X - 1, m.Y, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X + 1, m.Y, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X, m.Y - 1, 127 ), m.Map, 16 );

				if ( house == null )
					house = BaseHouse.FindHouseAt( new Point3D( m.X, m.Y + 1, 127 ), m.Map, 16 );

				if ( house != null )
					bonus = 50.0;
			}

			int range = 18 - (int)(m.Skills[SkillName.Hiding].Value / 10);

         // Added by Luthor
         bool afterTele = ((m is PlayerMobile) && (DateTime.Now <= (((PlayerMobile)m).LastTeleTime).AddMilliseconds(500)));
			bool badCombat = ( !afterTele && m.Combatant != null && m.InRange( m.Combatant.Location, range ) && m.Combatant.InLOS( m ) );
         bool ok = ( !badCombat /*&& m.CheckSkill( SkillName.Hiding, 0.0 - bonus, 100.0 - bonus )*/ );

			if ( ok )
			{
            // Only search for additional combatants if teleport hasn't "broken LOS"
            if(!afterTele)
            {
               foreach ( Mobile check in m.GetMobilesInRange( range ) )
               {
                  if ( check.InLOS( m ) && check.Combatant == m )
                  {
                     badCombat = true;
                     ok = false;
                     break;
                  }
               }
            }

				ok = ( !badCombat && m.CheckSkill( SkillName.Hiding, 0.0 - bonus, 100.0 - bonus ) );
			}

			if ( badCombat && !((m is PlayerMobile) && afterTele) )
			{
				m.RevealingAction();

				m.LocalOverheadMessage( MessageType.Regular, 0x22, 501237 ); // You can't seem to hide right now.

				return TimeSpan.FromSeconds( 1.0 );
			}
			else
			{
				if ( ok )
				{
					m.Hidden = true;

					m.LocalOverheadMessage( MessageType.Regular, 0x1F4, 501240 ); // You have hidden yourself well.
				}
				else
				{
					m.RevealingAction();

					m.LocalOverheadMessage( MessageType.Regular, 0x22, 501241 ); // You can't seem to hide here.
				}

				return TimeSpan.FromSeconds( 10.0 );
			}
		}
	}
}