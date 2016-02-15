using System;

namespace Server.Factions
{
	public class FactionState
	{
		private Faction m_Faction;
		private Mobile m_Commander;
                // jakob, added this
		private Mobile m_DeputyCommander;
		// end
                private int m_Tithe;
		private int m_Silver;
		private PlayerStateCollection m_Members;
		private Election m_Election;
		private FactionItemCollection m_FactionItems;
		private FactionTrapCollection m_FactionTraps;

                // jakob, added this
		private const int BonusBroadcastsPerOwnedTown = 3;

		private int BroadcastsPerPeriod
		{
			get
			{
				int broadcasts = 2 + BonusBroadcastsPerOwnedTown * m_Faction.OwnedTowns;;

				return broadcasts;
			}
		}
		// end

		// jakob, changed this from 1 to 24
		private static readonly TimeSpan BroadcastPeriod = TimeSpan.FromHours( 1.0 );
		// end

		// jakob, make sure this has enough room
		private DateTime[] m_LastBroadcasts = new DateTime[2 + BonusBroadcastsPerOwnedTown * 8];
		// end

		public bool FactionMessageReady
		{
			get
			{
				// jakob, make this BroadcastsPerPeriod instead of m_LastBroadcasts.Length
				for ( int i = 0; i < BroadcastsPerPeriod; ++i )
				// end
				{
					if ( DateTime.Now >= (m_LastBroadcasts[i] + BroadcastPeriod) )
						return true;
				}

				return false;
			}
		}

		public void RegisterBroadcast()
		{
			// jakob, make this BroadcastsPerPeriod instead of m_LastBroadcasts.Length
			for ( int i = 0; i < BroadcastsPerPeriod; ++i )
			// end
			{
				if ( DateTime.Now >= (m_LastBroadcasts[i] + BroadcastPeriod) )
				{
					m_LastBroadcasts[i] = DateTime.Now;
					break;
				}
			}
		}

		public FactionItemCollection FactionItems
		{
			get{ return m_FactionItems; }
			set{ m_FactionItems = value; }
		}

		public FactionTrapCollection Traps
		{
			get{ return m_FactionTraps; }
			set{ m_FactionTraps = value; }
		}

		public Election Election
		{
			get{ return m_Election; }
			set{ m_Election = value; }
		}

                // jakob, added this
		public Mobile DeputyCommander
		{
			get{ return m_DeputyCommander; }
			set
			{
				if ( m_DeputyCommander != null )
				{
					m_DeputyCommander.InvalidateProperties();
					m_DeputyCommander.SendMessage( "You have been fired as Deputy Commander." );
				}

				m_DeputyCommander = value;

				if ( m_DeputyCommander != null )
				{
					m_DeputyCommander.SendMessage( "You have been chosen Deputy Commander of your faction" );

					m_DeputyCommander.InvalidateProperties();

					PlayerState pl = PlayerState.Find( m_DeputyCommander );

					if ( pl != null && pl.Finance != null )
						pl.Finance.Finance = null;

					if ( pl != null && pl.Sheriff != null )
						pl.Sheriff.Sheriff = null;
				}
			}
		}
		// end

		public Mobile Commander
		{
			get{ return m_Commander; }
			set
			{
				if ( m_Commander != null )
					m_Commander.InvalidateProperties();

				m_Commander = value;

				if ( m_Commander != null )
				{
					m_Commander.SendLocalizedMessage( 1042227 ); // You have been elected Commander of your faction

					m_Commander.InvalidateProperties();

					PlayerState pl = PlayerState.Find( m_Commander );

					if ( pl != null && pl.Finance != null )
						pl.Finance.Finance = null;

					if ( pl != null && pl.Sheriff != null )
						pl.Sheriff.Sheriff = null;

                                        // jakob, remove deputy commander when a new commander is elected
					m_DeputyCommander = null;
					// end
                                }
			}
		}

		public int Tithe
		{
			get{ return m_Tithe; }
			set{ m_Tithe = value; }
		}

		public int Silver
		{
			get{ return m_Silver; }
			set{ m_Silver = value; }
		}

		public PlayerStateCollection Members
		{
			get{ return m_Members; }
			set{ m_Members = value; }
		}

		public FactionState( Faction faction )
		{
			m_Faction = faction;
			m_Tithe = 50;
			m_Members = new PlayerStateCollection();
			m_Election = new Election( faction );
			m_FactionItems = new FactionItemCollection();
			m_FactionTraps = new FactionTrapCollection();
		}

		public FactionState( GenericReader reader )
		{
			int version = reader.ReadEncodedInt();

			switch ( version )
			{

                                // jakob, read deputycommander
				case 5:
				{
					if ( reader.ReadBool() )
						m_DeputyCommander = reader.ReadMobile();
					goto case 4;
				}
				// end
                                case 4:
				{
					int count = reader.ReadEncodedInt();

					for ( int i = 0; i < count; ++i )
					{
						DateTime time = reader.ReadDateTime();

						if ( i < m_LastBroadcasts.Length )
							m_LastBroadcasts[i] = time;
					}

					goto case 3;
				}
				case 3:
				case 2:
				case 1:
				{
					m_Election = new Election( reader );

					goto case 0;
				}
				case 0:
				{
					m_Faction = Faction.ReadReference( reader );

					m_Commander = reader.ReadMobile();

					if ( version < 4 )
					{
						DateTime time = reader.ReadDateTime();

						if ( m_LastBroadcasts.Length > 0 )
							m_LastBroadcasts[0] = time;
					}

					m_Tithe = reader.ReadEncodedInt();
					m_Silver = reader.ReadEncodedInt();

					int memberCount = reader.ReadEncodedInt();

					m_Members = new PlayerStateCollection();

					for ( int i = 0; i < memberCount; ++i )
					{
						PlayerState pl = new PlayerState( reader, m_Faction, m_Members );

						if ( pl.Mobile != null )
							m_Members.Add( pl );
					}

					m_Faction.State = this;
					m_Faction.UpdateRanks();

					m_FactionItems = new FactionItemCollection();

					if ( version >= 2 )
					{
						int factionItemCount = reader.ReadEncodedInt();

						for ( int i = 0; i < factionItemCount; ++i )
						{
							FactionItem factionItem = new FactionItem( reader, m_Faction );

							if ( !factionItem.HasExpired )
								factionItem.Attach();
							else
								Timer.DelayCall( TimeSpan.Zero, new TimerCallback( factionItem.Detach ) ); // sandbox detachment
						}
					}

					m_FactionTraps = new FactionTrapCollection();

					if ( version >= 3 )
					{
						int factionTrapCount = reader.ReadEncodedInt();

						for ( int i = 0; i < factionTrapCount; ++i )
						{
							BaseFactionTrap trap = reader.ReadItem() as BaseFactionTrap;

							if ( trap != null && !trap.CheckDecay() )
								m_FactionTraps.Add( trap );
						}
					}

					break;
				}
			}

			if ( version < 1 )
				m_Election = new Election( m_Faction );
		}

		public void Serialize( GenericWriter writer )
		{
			// jakob, 5 instead of 4
			writer.WriteEncodedInt( (int) 5 ); // version
			// end

                        // jakob, write deputy commander
			writer.Write( m_DeputyCommander != null );
			if ( m_DeputyCommander != null )
				writer.Write( (Mobile) m_DeputyCommander );
			// end

			writer.WriteEncodedInt( (int) m_LastBroadcasts.Length );

			for ( int i = 0; i < m_LastBroadcasts.Length; ++i )
				writer.Write( (DateTime) m_LastBroadcasts[i] );

			m_Election.Serialize( writer );

			Faction.WriteReference( writer, m_Faction );

			writer.Write( (Mobile) m_Commander );

			writer.WriteEncodedInt( (int) m_Tithe );
			writer.WriteEncodedInt( (int) m_Silver );

			writer.WriteEncodedInt( (int) m_Members.Count );

			for ( int i = 0; i < m_Members.Count; ++i )
			{
				PlayerState pl = (PlayerState) m_Members[i];

				pl.Serialize( writer );
			}

			writer.WriteEncodedInt( (int) m_FactionItems.Count );

			for ( int i = 0; i < m_FactionItems.Count; ++i )
				m_FactionItems[i].Serialize( writer );

			writer.WriteEncodedInt( (int) m_FactionTraps.Count );

			for ( int i = 0; i < m_FactionTraps.Count; ++i )
				writer.Write( (Item) m_FactionTraps[i] );
		}
	}
}