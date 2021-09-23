using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	[Library( "car_door" )]
	public class CarDoor : ModelEntity, IUse
	{
		public bool isOpening = false;
		public Angles defLocalRot;
		public Vector3 defLocalPos;
		public Vector3 movePos;
		public Angles rotateAngle;

		public bool isMoving = false;

		public float openState;
		public bool shouldCloseDoorAfterSec = false;

		public Vector3 offsetPos;
		public string modelName;

		TimeSince timeSinceOpen = 0;

		public void stModel(string modelName)
		{
			SetModel( modelName );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}

		public bool IsUsable( Entity user )
		{
			return true;
		}

		public override void Spawn()
		{
			base.Spawn();

			openState = 0;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

		}

		public bool OnUse( Entity user )
		{
			if(openState >= 1 || openState <= 0)
				isOpening = !isOpening;

			if(isOpening)
			{
				timeSinceOpen = 0;
			}

			return true;
		}

		[Event.Tick.Server]
		public void Tick()
		{
			if ( !IsServer )
				return;
			openState += (isOpening ? Time.Delta : -Time.Delta) * 3;
			openState = openState.Clamp(0, 1);
			var angles = (!isMoving ? rotateAngle * openState : Angles.Zero) + defLocalRot ;
			LocalRotation = angles.ToRotation();

			LocalPosition = defLocalPos + (isMoving ? movePos * openState : Vector3.Zero);

			if ( !isOpening )
				timeSinceOpen = 0;

			if ( timeSinceOpen > 2.5f && shouldCloseDoorAfterSec)
				isOpening = false;
		}
	}
}
