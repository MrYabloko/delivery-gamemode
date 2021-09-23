using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	public partial class InteractibleSeat : ModelEntity, IUse
	{
		TimeSince timeSinceDriverLeft;
		[Net] public Player driver { get; protected set; }

		public CarDoor[] doorsToSeat;
		private void RemoveDriver( DeliveryPlayer player )
		{
			driver = null;
			timeSinceDriverLeft = 0;

			if ( !player.IsValid() )
				return;

			player.Vehicle = null;
			player.VehicleController = null;
			player.VehicleAnimator = null;
			player.VehicleCamera = null;
			player.Parent = null;

			if ( player.PhysicsBody.IsValid() )
			{
				player.PhysicsBody.Enabled = true;
				player.PhysicsBody.Position = player.Position;
			}
		}

		[Event.Tick.Server]
		protected void Tick()
		{
			if ( driver is DeliveryPlayer player )
			{
				if ( player.LifeState != LifeState.Alive || player.Vehicle != this )
				{
					RemoveDriver( player );
				}
			}
		}

		public virtual bool OnUse( Entity user )
		{
			if ( user is DeliveryPlayer player && player.Vehicle == null && timeSinceDriverLeft > 1.0f )
			{
				player.Vehicle = this;
				player.VehicleController = new CarController();
				player.VehicleAnimator = new CarAnimator();
				player.VehicleCamera = new CarCamera();
				player.Parent = this;
				player.LocalPosition = Vector3.Zero;
				player.LocalRotation = Rotation.Identity;
				player.LocalScale = 1;
				player.PhysicsBody.Enabled = false;

				driver = player;
			}

			return true;
		}

		public bool IsUsable( Entity user )
		{
			bool doorsOpened = true;
			if ( doorsToSeat != null )
			{
				doorsOpened = !doorsToSeat.All( t => t.openState < 0.5f );
			}
			return user is DeliveryPlayer player && driver == null && doorsOpened;
		}
	}
}
