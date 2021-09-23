using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	[Library( "ent_van", Title = "Van" )]
	public class Van : CarEntity
	{
		protected override float wheelForwardOffset => 10;
		public override int boxCount => 4;
		public override float maxBoxSize => 1;
		public override void Spawn()
		{

			base.Spawn();

			SetupPhysicsFromOBB( PhysicsMotionType.Dynamic,
				new Vector3( -100, -40, -5 ) + Vector3.Up * 25,
				new Vector3( 100, 40, 5 ) + Vector3.Up * 25 );

			var bodyName = "models/car/van_a01_utility_body.vmdl";

			var body = new ModelEntity
			{
				Parent = this,
				LocalPosition = new Vector3( 0, 0, 8 ),
				LocalRotation = Rotation.From( 0, 180, 0 ),
			};
			body.SetModel( bodyName );
			body.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
			body.SetInteractsExclude( CollisionLayer.Player );

			var st = new Seat()
			{
				Parent = this,
				LocalPosition = Vector3.Up * 38 + Vector3.Forward * 53 + Vector3.Left * 12.5f,
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
				defLocalPos = Vector3.Forward * 79 + Vector3.Up * 43 + Vector3.Left * 37.5f,
				shouldCloseDoorAfterSec = true,
				rotateAngle = new Angles( 0, -90, 0 ),
				defLocalRot = new Angles( 0, 180, 0 )
			};

			leftDoor.stModel( "models/car/van_a01_left_front_door.vmdl" );

			var rightDoor = new CarDoor()
			{
				Parent = this,
				isMoving = false,
				defLocalPos = Vector3.Forward * 79 + Vector3.Up * 43 + Vector3.Right * 37.5f,
				shouldCloseDoorAfterSec = true,
				rotateAngle = new Angles( 0, 90, 0 ),
				defLocalRot = new Angles( 0, 180, 0 )
			};

			rightDoor.stModel( "models/car/van_a01_right_front_door.vmdl" );

			var tailgate = new CarDoor()
			{
				Parent = this,
				isMoving = false,
				defLocalPos = Vector3.Backward * 84 + Vector3.Up * 79,
				shouldCloseDoorAfterSec = false,
				rotateAngle = new Angles( -90, 0, 0 ),
				defLocalRot = new Angles( 0, 180, 0 )
			};

			tailgate.stModel( "models/car/van_a01_tailgate.vmdl" );

			var rightRearDoor = new CarDoor()
			{
				Parent = this,
				defLocalPos = Vector3.Forward * 26 + Vector3.Up * 18 + Vector3.Right * 37.5f,
				defLocalRot = new Angles( 0, 180, 0 ),
				shouldCloseDoorAfterSec = false,
				movePos = Vector3.Backward * 40,
				isMoving = true
			};

			rightRearDoor.stModel( "models/car/van_a01_right_rear_door.vmdl" );

			doorsToSeat = new[] { leftDoor, rightDoor };
		}

		public override void ClientSpawn()
		{

			base.ClientSpawn();

			{
				var hood = new ModelEntity();
				hood.SetModel( "models/car/van_a01_hood.vmdl" );
				hood.Transform = Transform;
				hood.Parent = this;
				hood.LocalPosition = Vector3.Forward * 88 + Vector3.Up * 53;
				hood.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var grille = new ModelEntity();
				grille.SetModel( "models/car/van_a01_front_grille.vmdl" );
				grille.Transform = Transform;
				grille.Parent = this;
				grille.LocalPosition = Vector3.Forward * 93 + Vector3.Up * 40;
				grille.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var l_light = new ModelEntity();
				l_light.SetModel( "models/car/van_a01_headlight.vmdl" );
				l_light.Transform = Transform;
				l_light.Parent = this;
				l_light.LocalPosition = Vector3.Forward * 93 + Vector3.Up * 39.5f + Vector3.Left * 28f;
				l_light.LocalRotation = Rotation.From( 0, 180, 0 );

				var r_light = new ModelEntity();
				r_light.SetModel( "models/car/van_a01_headlight.vmdl" );
				r_light.Transform = Transform;
				r_light.Parent = this;
				r_light.LocalPosition = Vector3.Forward * 93 + Vector3.Up * 39.5f + Vector3.Right * 28f;
				r_light.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var undercarriage = new ModelEntity();
				undercarriage.SetModel( "models/car/van_a01_undercarriage_complex.vmdl" );
				undercarriage.Transform = Transform;
				undercarriage.Parent = this;
				undercarriage.LocalPosition = Vector3.Up * 8;
				undercarriage.LocalRotation = Rotation.From( 0, 180, 0 );
			}

			{
				var bumper = new ModelEntity();
				bumper.SetModel( "models/car/van_a01_front_bumper.vmdl" );
				bumper.Transform = Transform;
				bumper.Parent = this;
				bumper.LocalPosition = Vector3.Forward * 0 + Vector3.Up * 8;
				bumper.LocalRotation = Rotation.From( 0, 180, 0 );
			}
		}

		

		public override CarPreviewModelInfo[] GetModelInfos()
		{
			List<CarPreviewModelInfo> infos = new();

			#region ServerModels

			infos.Add(new CarPreviewModelInfo() 
			{ 
				path = "models/car/van_a01_utility_body.vmdl",
				position = new Vector3(0, 0, 8),
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_left_front_door.vmdl",
				position = Vector3.Forward * 79 + Vector3.Up * 43 + Vector3.Left * 37.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_right_front_door.vmdl",
				position = Vector3.Forward * 79 + Vector3.Up * 43 + Vector3.Right * 37.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_tailgate.vmdl",
				position = Vector3.Backward * 84 + Vector3.Up * 79,
				angles = new Angles( 0, 180, 0 )
			} );

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_right_rear_door.vmdl",
				position = Vector3.Forward * 26 + Vector3.Up * 18 + Vector3.Right * 37.5f,
				angles = new Angles( 0, 180, 0 )
			} );

			#endregion

			#region ClientModels

			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_front_bumper.vmdl",
				position = Vector3.Forward * 0 + Vector3.Up * 8,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_undercarriage_complex.vmdl",
				position = Vector3.Up * 8,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_headlight.vmdl",
				position = Vector3.Forward * 93 + Vector3.Up * 39.5f + Vector3.Right * 28f,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_headlight.vmdl",
				position = Vector3.Forward * 93 + Vector3.Up * 39.5f + Vector3.Left * 28f,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_front_grille.vmdl",
				position = Vector3.Forward * 93 + Vector3.Up * 40,
				angles = new Angles( 0, 180, 0 )
			} );
			infos.Add( new CarPreviewModelInfo()
			{
				path = "models/car/van_a01_hood.vmdl",
				position = Vector3.Forward * 88 + Vector3.Up * 53,
				angles = new Angles( 0, 180, 0 )
			} );

			#endregion


			//	infos.AddRange(CarEntity.GetModelInfos());

			infos.AddRange(base.GetModelInfos());

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
				player.LocalPosition = Vector3.Up * 24 + Vector3.Forward * 58 + Vector3.Left * 20;
				player.LocalRotation = Rotation.Identity;
				player.LocalScale = 1;
				player.PhysicsBody.Enabled = false;

				driver = player;
			}

			return true;
		}
	}
}
