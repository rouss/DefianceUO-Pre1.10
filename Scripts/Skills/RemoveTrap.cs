using System;
using Server.Targeting;
using Server.Items;
using Server.Network;
using Server.Factions;

namespace Server.SkillHandlers
{
	public class RemoveTrap
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.RemoveTrap].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			bool nokit = m.Backpack == null || m.Backpack.FindItemByType( typeof( FactionTrapRemovalKit ) ) == null;
			Faction faction = Faction.Find( m );
			bool checkskill = nokit && ( faction == null || !faction.IsCommander( m ) );

			if ( checkskill && m.Skills[SkillName.Lockpicking].Value < 50 )
				m.SendLocalizedMessage( 502366 ); // You do not know enough about locks.  Become better at picking locks.
			else if ( checkskill && m.Skills[SkillName.DetectHidden].Value < 50 )
				m.SendLocalizedMessage( 502367 ); // You are not perceptive enough.  Become better at detect hidden.
			else
			{
				m.Target = new InternalTarget();

				m.SendLocalizedMessage( 502368 ); // Wich trap will you attempt to disarm?
			}

			return TimeSpan.FromSeconds( 10.0 ); // 10 second delay before beign able to re-use a skill
		}

		private class InternalTarget : Target
		{
			public InternalTarget() :  base ( 2, false, TargetFlags.None )
			{
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( targeted is Mobile )
					from.SendLocalizedMessage( 502816 ); // You feel that such an action would be inappropriate
				else if ( targeted is TrapableContainer )
				{

					if ( from.Skills[SkillName.Lockpicking].Value < 50 )
					{
						from.SendLocalizedMessage( 502366 ); // You do not know enough about locks.  Become better at picking locks.
						return;
					}
					else if ( from.Skills[SkillName.DetectHidden].Value < 50 )
					{
						from.SendLocalizedMessage( 502367 ); // You are not perceptive enough.  Become better at detect hidden.
						return;
					}

					TrapableContainer targ = (TrapableContainer)targeted;

					from.Direction = from.GetDirectionTo( targ );

					if ( targ.TrapType == TrapType.None )
					{
						from.SendLocalizedMessage( 502373 ); // That doesn't appear to be trapped
						return;
					}

					from.PlaySound( 0x241 );

					int minskill;
					int maxskill;
					LockableContainer lc = targ as LockableContainer;
					if (lc != null && (targ.TrapType == TrapType.ExplosionTrap || targ.TrapType == TrapType.PoisonTrap))
					{
						// values used for tinkertraps
						maxskill = targ.TrapPower;
						minskill = (int)(targ.TrapPower * 0.8);
					}
					else // original values
					{
						maxskill = targ.TrapPower + 30;
						minskill = targ.TrapPower;
					}

					if ( from.CheckTargetSkill( SkillName.RemoveTrap, targ, minskill, maxskill) )
					{
						targ.TrapPower = 0;
						targ.TrapType = TrapType.None;

//						if (lc != null)
//							lc.TinkerTrapCreator = null;

						from.SendLocalizedMessage( 502377 ); // You successfully render the trap harmless
					}
					else
					{
						from.SendLocalizedMessage( 502372 ); // You fail to disarm the trap... but you don't set it off
					}
				}
				else if ( targeted is BaseFactionTrap )
				{
					BaseFactionTrap trap = (BaseFactionTrap) targeted;
					Faction faction = Faction.Find( from );

					FactionTrapRemovalKit kit = ( from.Backpack == null ? null : from.Backpack.FindItemByType( typeof( FactionTrapRemovalKit ) ) as FactionTrapRemovalKit );

					bool commander = trap.Faction != null && trap.Faction.IsCommander( from );

					bool isOwner = ( trap.Placer == from || commander );


					if ( !commander && from.Skills[SkillName.Lockpicking].Value < 50 )
						from.SendLocalizedMessage( 502366 ); // You do not know enough about locks.  Become better at picking locks.
					else if ( !commander && from.Skills[SkillName.DetectHidden].Value < 50 )
						from.SendLocalizedMessage( 502367 ); // You are not perceptive enough.  Become better at detect hidden.
					else if ( faction == null )
						from.SendLocalizedMessage( 1010538 ); // You may not disarm faction traps unless you are in an opposing faction
					else if ( faction == trap.Faction /*&& trap.Faction != null*/ && !isOwner )
						from.SendLocalizedMessage( 1010537 ); // You may not disarm traps set by your own faction!
					else if ( trap.Faction != null && kit == null )
						from.SendLocalizedMessage( 1042530 ); // You must have a trap removal kit at the base level of your pack to disarm a faction trap.
					else
					{
						if ( from.CheckTargetSkill( SkillName.RemoveTrap, trap, 80.0, 100.0 ) || commander )
						{
							from.PrivateOverheadMessage( MessageType.Regular, trap.MessageHue, trap.DisarmMessage, from.NetState );

							if ( !isOwner )
							{
								int silver = faction.AwardSilver( from, trap.SilverFromDisarm );

								if ( silver > 0 )
									from.SendLocalizedMessage( 1008113, true, silver.ToString( "N0" ) ); // You have been granted faction silver for removing the enemy trap :
							}

							trap.Delete();
						}
						else
							from.SendLocalizedMessage( 502372 ); // You fail to disarm the trap... but you don't set it off

						if ( !isOwner && kit != null )
							kit.ConsumeCharge( from );
					}
				}
				else
					from.SendLocalizedMessage( 502373 ); // That doesn't appear to be trapped
			}
		}
	}
}