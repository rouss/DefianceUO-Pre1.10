using System;
using Server;
using Server.Gumps;
using System.Collections;
using Server.ContextMenus;

namespace Server.Items.Staff
{
	/// <summary>
	/// David O'Hara
	/// 08-13-2004
	/// Version 3.0
	/// This orb allows staff to switch between a Player access level and their current staff level.
	/// It also sets the mortality as appropriate for staff.
	/// A home location can be set/used thru the context menu.
	/// Will auto resurrect it's owner on death.
	/// </summary>
	public class StaffOrb : Item
	{
		private Mobile m_Owner;
		private AccessLevel m_StaffLevel;
		public Point3D m_HomeLocation;
		public Map m_HomeMap;
		private bool m_AutoRes = true;

		[Constructable]
		public StaffOrb( ) : base( 0x0E2F )
		{
			LootType = LootType.Blessed;
			Weight = 0;
			Name = "Unassigned Staff Orb";
		}

		public StaffOrb( Serial serial ) : base( serial )
		{
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public Point3D HomeLocation
		{
			get
			{
				return m_HomeLocation;
			}
			set
			{
				m_HomeLocation = value;
			}
		}

		[CommandProperty( AccessLevel.Counselor, AccessLevel.GameMaster )]
		public Map HomeMap
		{
			get
			{
				return m_HomeMap;
			}
			set
			{
				m_HomeMap = value;
			}
		}

		[CommandProperty( AccessLevel.Counselor)]
		public bool AutoRes
		{
			get
			{
				return m_AutoRes;
			}
			set
			{
				m_AutoRes = value;
			}
		}

		private class GoHomeEntry : ContextMenuEntry
		{
			private StaffOrb m_Item;
			private Mobile m_Mobile;

			public GoHomeEntry( Mobile from, Item item ) : base( 5134 ) // uses "Goto Loc" entry
			{
				m_Item = (StaffOrb)item;
				m_Mobile = from;
			}

			public override void OnClick()
			{
				// go to home location
				m_Mobile.Location = m_Item.HomeLocation;
				if ( m_Item.HomeMap != null )
					m_Mobile.Map = m_Item.HomeMap;
			}
		}

		private class SetHomeEntry : ContextMenuEntry
		{
			private StaffOrb m_Item;
			private Mobile m_Mobile;

			public SetHomeEntry( Mobile from, Item item ) : base( 2055 ) // uses "Mark" entry
			{
				m_Item = (StaffOrb)item;
				m_Mobile = from;
			}

			public override void OnClick()
			{
				// set home location
				m_Item.HomeLocation = m_Mobile.Location;
				m_Item.HomeMap = m_Mobile.Map;
				m_Mobile.SendMessage( "The home location on your orb has been set to your current position." );
			}
		}

		public static void GetContextMenuEntries( Mobile from, Item item, ArrayList list )
		{
			list.Add( new GoHomeEntry( from, item ) );
			list.Add( new SetHomeEntry( from, item ) );
		}

		public override void GetContextMenuEntries(Mobile from, ArrayList list)
		{
			if ( m_Owner == null )
			{
				return;
			}
			else
			{
				if ( m_Owner != from )
				{
					from.SendMessage( "This is not yours to use." );
					return;
				}
				else
				{
					base.GetContextMenuEntries( from, list );
					StaffOrb.GetContextMenuEntries( from, this, list );
				}
			}
		}

		public override DeathMoveResult OnInventoryDeath(Mobile parent)
		{
			if ( m_AutoRes && parent == m_Owner )
			{
				SwitchAccessLevels( parent );
				new AutoResTimer( parent ).Start();
			}
			return base.OnInventoryDeath (parent);
		}

		public override void OnDoubleClick(Mobile from)
		{
			// set owner if not already set -- this is only done the first time.
			if ( m_Owner == null )
			{
				m_Owner = from;
				this.Name = m_Owner.Name.ToString() + "'s Staff Orb";
				this.HomeLocation = from.Location;
				this.HomeMap = from.Map;
				from.SendMessage( "This orb has been assigned to you." );
			}
			else
			{
				if ( m_Owner != from )
				{
					from.SendMessage( "This is not yours to use." );
					return;
				}
				else
				{
					SwitchAccessLevels( from );
				}

			}
		}

		private class AutoResTimer : Timer
		{
			private Mobile m_Mobile;
			public AutoResTimer( Mobile mob ) : base( TimeSpan.FromSeconds( 5.0 ) )
			{
				m_Mobile = mob;
			}

			protected override void OnTick()
			{
				m_Mobile.Resurrect();
				m_Mobile.SendMessage( "As a staff member, you should be more careful in the future." );
				Stop();
			}

		}

		private void SwitchAccessLevels( Mobile from )
		{
			// check current access level
			if ( from.AccessLevel == AccessLevel.Player )
			{
                if (m_StaffLevel != AccessLevel.Player)
                {
                    // return to staff status
                    from.AccessLevel = m_StaffLevel;
                    //Al: m_StaffLevel is reset when regaining staffstatus
                    //    to fix ability to regain an old (higher) accesslevel.
                    m_StaffLevel = AccessLevel.Player;
                    from.Blessed = true;
                }
                else
                    from.SendMessage("Please use the staff orb you used to become player.");
			}
			else
			{
				m_StaffLevel = from.AccessLevel;
				from.AccessLevel = AccessLevel.Player;
				from.Blessed = false;
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 3 ); // version

			// version 3
			writer.Write( m_AutoRes );

			// version 2
			writer.Write( m_HomeLocation );
			writer.Write( m_HomeMap );

			writer.Write( m_Owner );
			writer.WriteEncodedInt( (int)m_StaffLevel );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			switch ( version )
			{
				case 3:
				{
					m_AutoRes = reader.ReadBool();
					goto case 2;
				}
				case 2:
				{
					m_HomeLocation = reader.ReadPoint3D();
					m_HomeMap = reader.ReadMap();
					goto case 1;
				}
				case 1:
				{
					m_Owner = reader.ReadMobile();
					m_StaffLevel = (AccessLevel)reader.ReadEncodedInt();
					break;
				}
			}
		}

	}
}