using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	[Library( "ent_hatchback", Title = "Hatchback" )]
	public class Hatchback : CarEntity
	{
		protected override float wheelForwardOffset => 6;
		public override int boxCount => 2;
		public override float maxBoxSize => 0.8f;
		public override void Spawn()
		{

			base.Spawn();
			{
				Vector3 size = new Vector3( 155, 75, 10 );
				Vector3 pos = Vector3.Up * 25 + Vector3.Backward * 6;

				SetupPhysicsFromOBB( PhysicsMotionType.Dynamic,
					-(size / 2) + pos,
					(size / 2) + pos );
			}

			var bodyName = "models/car/hatchback_a01/car_hatchback_a01_shell.vmdl";

			var body = new ModelEntity
			{
				Parent = this,
				LocalPosition = new Vector3( 0, 0, 8 ) + Vector3.Forward * 8,
				LocalRotation = Rotation.From( 0, 180, 0 ),
			};
			body.SetModel( bodyName );
			body.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			body.SetInteractsExclude( CollisionLayer.Player );

			var st = new Seat()
			{
				Parent = this,
				LocalPosition = Vector3.Up * 29 + Vector3.Left * 12.5f,
				LocalRotation = Rotation.From( 0, 0, 0 ),
				LocalScale = 1,
				CollisionGroup = CollisionGroup.Interactive,
				EnableSelfCollisions = false,

			};
			st.usable = delegate ( Entity ent ) { return this.IsUsable( ent ); };
			st.use = delegate ( Entity ent ) { return Sit( ent ); };
			{
				Vector3 size = Vector3.One * 30;
				Vector3 pos = Vector3.Up * 15;
				st.SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, -(size / 2) + pos, size / 2 + pos );
				st.SetInteractsExclude( CollisionLayer.PhysicsProp );
			}
			var leftDoor = new CarDoor()
			{
				Parent = this,
				defLocalPos = Vector3.Forward * 25 + Vector3.Up * 37 + Vector3.Left * 35.5f,
				shouldCloseDoorAfterSec = true,
				rotateAngle = new Angles( 0, -90, 0 ),
				defLocalRot = new Angles( -90, 0, -90 )
			};

			leftDoor.stModel( "models/car/hatchback_a01/car_hatchback_a01_left_door.vmdl" );

			var rightDoor = new CarDoor()
			{
				Parent = this,
				isMoving = false,
				defLocalPos = Vector3.Forward * 25 + Vector3.Up * 34 + Vector3.Right * 35.5f,
				shouldCloseDoorAfterSec = true,
				rotateAngle = new Angles( 0, 90, 0 ),
				defLocalRot = new Angles( -90, 0, 90 )
			};

			rightDoor.stModel( "models/car/hatchback_a01/car_hatchback_a01_right_door.vmdl" );

			var tailgate = new CarDoor()
			{
				Parent = this,
				isMoving = false,
				defLocalPos = Vector3.Backward * 73 + Vector3.Up * 68,
				shouldCloseDoorAfterSec = false,
				rotateAngle = new Angles( 90, 0, 0 ),
				defLocalRot = new Angles( 0, 0, 0 )
			};

			tailgate.stModel( "models/car/hatchback_a01/car_hatchback_a01_rear_door.vmdl" );

			doorsToSeat = new [] { leftDoor, rightDoor };
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			{
				var hood = new ModelEntity();
				hood.SetModel( "models/car/hatchback_a01/car_hatchback_a01_hood.vmdl" );
				hood.Transform = Transform;
				hood.Parent = this;
				hood.LocalPosition = Vector3.Forward * 33.25f + Vector3.Up * 53.5f;
				hood.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var grille = new ModelEntity();
				grille.SetModel( "models/car/hatchback_a01/car_hatchback_a01_grille.vmdl" );
				grille.Transform = Transform;
				grille.Parent = this;
				grille.LocalPosition = Vector3.Forward * 75 + Vector3.Up * 39;
				grille.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var l_light_rim = new ModelEntity();
				l_light_rim.SetModel( "models/car/hatchback_a01/car_hatchback_a01_headlight_rim" );
				l_light_rim.Transform = Transform;
				l_light_rim.Parent = this;
				l_light_rim.LocalPosition = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Left * 28.5f;
				l_light_rim.LocalRotation = Rotation.From( 0, 180, 0 );

				var r_light_rim = new ModelEntity();
				r_light_rim.SetModel( "models/car/hatchback_a01/car_hatchback_a01_headlight_rim" );
				r_light_rim.Transform = Transform;
				r_light_rim.Parent = this;
				r_light_rim.LocalPosition = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Right * 28.5f;
				r_light_rim.LocalRotation = Rotation.From( 0, 180, 0 );

				var l_light = new ModelEntity();
				l_light.SetModel( "models/car/hatchback_a01/car_hatchback_a01_headlight" );
				l_light.Transform = Transform;
				l_light.Parent = this;
				l_light.LocalPosition = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Left * 28.5f;
				l_light.LocalRotation = Rotation.From( 0, 180, 0 );

				var r_light = new ModelEntity();
				r_light.SetModel( "models/car/hatchback_a01/car_hatchback_a01_headlight" );
				r_light.Transform = Transform;
				r_light.Parent = this;
				r_light.LocalPosition = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Right * 28.5f;
				r_light.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var undercarriage = new ModelEntity();
				undercarriage.SetModel( "models/car/hatchback_a01/car_hatchback_a01_undercarriage_complex.vmdl" );
				undercarriage.Transform = Transform;
				undercarriage.Parent = this;
				undercarriage.LocalPosition = Vector3.Forward * 8 + Vector3.Up * 8;
				undercarriage.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var front_bumper = new ModelEntity();
				front_bumper.SetModel( "models/car/hatchback_a01/car_hatchback_a01_bumper_front.vmdl" );
				front_bumper.Transform = Transform;
				front_bumper.Parent = this;
				front_bumper.LocalPosition = Vector3.Forward * 71 + Vector3.Up * 23 + Vector3.Right * 18;
				front_bumper.LocalRotation = Rotation.From( 0, 180, 0 );

				var rear_bumper = new ModelEntity();
				rear_bumper.SetModel( "models/car/hatchback_a01/car_hatchback_a01_bumper_rear.vmdl" );
				rear_bumper.Transform = Transform;
				rear_bumper.Parent = this;
				rear_bumper.LocalPosition = Vector3.Backward * 82.5f + Vector3.Up * 23 + Vector3.Right * 18;
				rear_bumper.LocalRotation = Rotation.From( 0, 180, 0 );
			}
		}



		public override CarPreviewModelInfo[] GetModelInfos()
		{
			List<CarPreviewModelInfo> infos = new();

			#region ServerModels

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_shell.vmdl",
				position = new Vector3( 0, 0, 8 ) + Vector3.Forward * 8,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_left_door.vmdl",
				position = Vector3.Forward * 25 + Vector3.Up * 37 + Vector3.Left * 35.5f,
				angles = new Angles( -90, 0, -90 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_right_door.vmdl",
				position = Vector3.Forward * 25 + Vector3.Up * 34 + Vector3.Right * 35.5f,
				angles = new Angles( -90, 0, 90 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_rear_door.vmdl",
				position = Vector3.Backward * 73 + Vector3.Up * 68,
				angles = new Angles( 0, 0, 0 )
			} );

			#endregion

			#region ClientModels

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_bumper_front.vmdl",
				position = Vector3.Forward * 71 + Vector3.Up * 23 + Vector3.Right * 18,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_bumper_rear.vmdl",
				position = Vector3.Backward * 82.5f + Vector3.Up * 23 + Vector3.Right * 18,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_undercarriage_complex.vmdl",
				position = Vector3.Forward * 8 + Vector3.Up * 8,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_headlight_rim",
				position = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Left * 28.5f,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_headlight_rim",
				position = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Right * 28.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_headlight",
				position = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Left * 28.5f,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_headlight",
				position = Vector3.Forward * 69 + Vector3.Up * 40f + Vector3.Right * 28.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_grille.vmdl",
				position = Vector3.Forward * 75 + Vector3.Up * 39,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/hatchback_a01/car_hatchback_a01_hood.vmdl",
				position = Vector3.Forward * 33.25f + Vector3.Up * 53.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			#endregion


			//	infos.AddRange(CarEntity.GetModelInfos());

			infos.AddRange( base.GetModelInfos() );

			return infos.ToArray();
		}




		public override bool Sit( Entity user )
		{
			if ( user is DeliveryPlayer player && player.Vehicle == null && timeSinceDriverLeft > 1.0f )
			{
				player.Vehicle = this;
				player.VehicleController = new CarController();
				player.VehicleAnimator = new CarAnimator();
				player.VehicleCamera = new CarCamera();
				player.Parent = this;
				player.LocalPosition = Vector3.Up * 16 + Vector3.Forward * 12 + Vector3.Left * 15;
				player.LocalRotation = Rotation.From(-12,0,0);
				player.LocalScale = 1;
				player.PhysicsBody.Enabled = false;

				driver = player;
			}

			return true;
		}
	}
}
