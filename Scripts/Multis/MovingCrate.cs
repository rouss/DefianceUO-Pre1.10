using System;
using System.Collections;
using Server;
using Server.Items;

using Server.Logging; //Al: Logging
using Server.Scripts.Commands; //Al: Logging, formatting

namespace Server.Multis
{
	public class MovingCrate : Container
	{
		public static readonly int MaxItemsPerSubcontainer = 20;
		public static readonly int Rows = 3;
		public static readonly int Columns = 5;
		public static readonly int HorizontalSpacing = 25;
		public static readonly int VerticalSpacing = 25;

		public override int LabelNumber{ get{ return 1061690; } } // Packing Crate

		private BaseHouse m_House;

		private Timer m_InternalizeTimer;

		public BaseHouse House
		{
			get{ return m_House; }
			set
            {
                //Al: Added to track down the m_House==null issue
                /*if (value == null)
                {
                    if (m_House == null)
                        GeneralLogging.WriteLine("MovingCrate", String.Format("House.set: value==null, m_House==null MovingCrate: {0}", CommandLogging.Format(this)));
                    else
                        GeneralLogging.WriteLine("MovingCrate", String.Format("House.set: value==null, m_House!=null, MovingCrate: {0}, m_House: {1}", CommandLogging.Format(this), CommandLogging.Format(m_House)));
                }*/
                m_House = value;
            }
		}

		public override int DefaultGumpID{ get{ return 0x44; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 20, 10, 150, 90 ); }
		}

		public override int DefaultMaxItems{ get{ return 0; } }
		public override int DefaultMaxWeight{ get{ return 0; } }

		public MovingCrate( BaseHouse house ) : base( 0xE3D )
		{
			Hue = 0x8A5;
			Movable = false;

			m_House = house;
		}

		public MovingCrate( Serial serial ) : base( serial )
		{
		}

		/*
		public override void AddNameProperties( ObjectPropertyList list )
		{
			base.AddNameProperties( list );

			if ( House != null && House.InternalizedVendors.Count > 0 )
				list.Add( 1061833, House.InternalizedVendors.Count.ToString() ); // This packing crate contains ~1_COUNT~ vendors/barkeepers.
		}
		*/

		public override void DropItem( Item dropped )
		{
			// 1. Try to stack the item
			foreach ( Item item in this.Items )
			{
				if ( item is PackingBox )
				{
					ArrayList subItems = item.Items;

					for ( int i = 0; i < subItems.Count; i++ )
					{
						Item subItem = (Item) subItems[i];

						if ( !(subItem is Container) && subItem.StackWith( null, dropped, false ) )
							return;
					}
				}
			}

			// 2. Try to drop the item into an existing container
			foreach ( Item item in this.Items )
			{
				if ( item is PackingBox )
				{
					Container box = (Container) item;
					ArrayList subItems = box.Items;

					if ( subItems.Count < MaxItemsPerSubcontainer )
					{
						box.DropItem( dropped );
						return;
					}
				}
			}

			// 3. Drop the item into a new container
			Container subContainer = new PackingBox();
			subContainer.DropItem( dropped );

			Point3D location = GetFreeLocation();
			if ( location != Point3D.Zero )
			{
				this.AddItem( subContainer );
				subContainer.Location = location;
			}
			else
			{
				base.DropItem( subContainer );
			}
		}

		private Point3D GetFreeLocation()
		{
			bool[,] positions = new bool[Rows, Columns];

			foreach ( Item item in this.Items )
			{
				if ( item is PackingBox )
				{
					int i = (item.Y - this.Bounds.Y) / VerticalSpacing;
					if ( i < 0 )
						i = 0;
					else if ( i >= Rows )
						i = Rows - 1;

					int j = (item.X - this.Bounds.X) / HorizontalSpacing;
					if ( j < 0 )
						j = 0;
					else if ( j >= Columns )
						j = Columns - 1;

					positions[i, j] = true;
				}
			}

			for ( int i = 0; i < Rows; i++ )
			{
				for ( int j = 0; j < Columns; j++ )
				{
					if ( !positions[i, j] )
					{
						int x = this.Bounds.X + j * HorizontalSpacing;
						int y = this.Bounds.Y + i * VerticalSpacing;

						return new Point3D( x, y, 0 );
					}
				}
			}

			return Point3D.Zero;
		}

		public override void SendCantStoreMessage( Mobile to, Item item )
		{
			to.SendLocalizedMessage( 1061145 ); // You cannot place items into a house moving crate.
		}

		public override bool CheckLift( Mobile from, Item item )
		{
			return base.CheckLift( from, item ) && House != null && !House.Deleted && House.IsOwner( from );
		}

		public override bool CheckItemUse( Mobile from, Item item )
		{
			return base.CheckItemUse( from, item ) && House != null && !House.Deleted && House.IsOwner( from );
		}

		public override void OnItemRemoved( Item item )
		{
			base.OnItemRemoved( item );

			if ( this.TotalItems == 0 )
				Delete();
		}

		public void RestartTimer()
		{
			if ( m_InternalizeTimer == null )
			{
				m_InternalizeTimer = new InternalizeTimer( this );
				m_InternalizeTimer.Start();
			}
			else
			{
				m_InternalizeTimer.Stop();
				m_InternalizeTimer.Start();
			}
		}

		public void Hide()
		{
			if ( m_InternalizeTimer != null )
			{
				m_InternalizeTimer.Stop();
				m_InternalizeTimer = null;
			}

			ArrayList toRemove = new ArrayList();
			foreach ( Item item in this.Items )
			{
				if ( item is PackingBox && item.Items.Count == 0 )
					toRemove.Add( item );
			}

			foreach ( Item item in toRemove )
			{
				item.Delete();
			}

			if ( this.TotalItems == 0 )
				Delete();
			else
				Internalize();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if ( House != null && House.MovingCrate == this )
				House.MovingCrate = null;

			if ( m_InternalizeTimer != null )
				m_InternalizeTimer.Stop();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 1 );

			writer.Write( (Item) m_House );

            //Al: Added to track down the m_House==null issue
            if (m_House == null)
                GeneralLogging.WriteLine("MovingCrate", String.Format("Serialize(): m_house==null, MovingCrate: {0}", CommandLogging.Format(this)));
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();

			m_House = (BaseHouse) reader.ReadItem();

            //Al: Added to track down the m_House==null issue
            if (m_House == null)
                GeneralLogging.WriteLine("MovingCrate", String.Format("Deserialize(): m_house==null, MovingCrate: {0}", CommandLogging.Format(this)));
            else
			    m_House.MovingCrate = this;

			Timer.DelayCall( TimeSpan.Zero, new TimerCallback( Hide ) );

			if ( version == 0 )
				MaxItems = -1; // reset to default
		}

		public class InternalizeTimer : Timer
		{
			private MovingCrate m_Crate;

			public InternalizeTimer( MovingCrate crate ) : base( TimeSpan.FromMinutes( 5.0 ) )
			{
				m_Crate = crate;

				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				m_Crate.Hide();
			}
		}
	}

	public class PackingBox : BaseContainer
	{
		public override int LabelNumber{ get{ return 1061690; } } // Packing Crate

		public override int DefaultGumpID{ get{ return 0x4B; } }
		public override int DefaultDropSound{ get{ return 0x42; } }

		public override Rectangle2D Bounds
		{
			get{ return new Rectangle2D( 16, 51, 168, 73 ); }
		}

		public override int DefaultMaxItems{ get{ return 0; } }
		public override int DefaultMaxWeight{ get{ return 0; } }

		public PackingBox() : base( 0x9A8 )
		{
			Movable = false;
		}

		public PackingBox( Serial serial ) : base( serial )
		{
		}

		public override void SendCantStoreMessage( Mobile to, Item item )
		{
			to.SendLocalizedMessage( 1061145 ); // You cannot place items into a house moving crate.
		}

		public override void OnItemRemoved( Item item )
		{
			base.OnItemRemoved( item );

			if ( item.GetBounce() == null && this.TotalItems == 0 )
				Delete();
		}

		public override void OnItemBounceCleared( Item item )
		{
			base.OnItemBounceCleared( item );

			if ( this.TotalItems == 0 )
				Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 1 ); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();

			if ( version == 0 )
				MaxItems = -1; // reset to default
		}
	}
}