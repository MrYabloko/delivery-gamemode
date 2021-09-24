using Sandbox;
using System;
using DeliveryGamemode;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CarPreviewModelInfo
{
	public string path;
	public Vector3 position;
	public Angles angles;
}

[Library( "ent_car", Title = "Car", Spawnable = true )]
public partial class CarEntity : Prop
{
	[ConVar.Replicated( "debug_car" )]
	public static bool debug_car { get; set; } = false;

	[ConVar.Replicated( "car_accelspeed" )]
	public static float car_accelspeed { get; set; } = 500.0f;

	private CarWheel frontLeft;
	private CarWheel frontRight;
	private CarWheel backLeft;
	private CarWheel backRight;

	private float frontLeftDistance;
	private float frontRightDistance;
	private float backLeftDistance;
	private float backRightDistance;

	private bool frontWheelsOnGround;
	private bool backWheelsOnGround;
	private float accelerateDirection;
	private float airRoll;
	private float airTilt;
	private float grip;
	protected TimeSince timeSinceDriverLeft;
	protected TimeSince timeSincePassengerLeft;

	protected virtual float wheelForwardOffset => 0;
	public virtual int boxCount => 1;
	public virtual float maxBoxSize => 1;

	protected CarDoor[] doorsToSeat;

	[Net] private float WheelSpeed { get; set; }
	[Net] private float TurnDirection { get; set; }
	[Net] private float AccelerationTilt { get; set; }
	[Net] private float TurnLean { get; set; }

	[Net] public float MovementSpeed { get; private set; }
	[Net] public bool Grounded { get; private set; }

	private struct InputState
	{
		public float throttle;
		public float turning;
		public float breaking;
		public float tilt;
		public float roll;

		public void Reset()
		{
			throttle = 0;
			turning = 0;
			breaking = 0;
			tilt = 0;
			roll = 0;
		}
	}

	private InputState currentInput;

	public CarEntity()
	{
		frontLeft = new CarWheel( this );
		frontRight = new CarWheel( this );
		backLeft = new CarWheel( this );
		backRight = new CarWheel( this );
	}

	[Net] public Player driver { get; protected set; }
	[Net] public Player passenger { get; protected set; }

	private ModelEntity chassis_axle_rear;
	private ModelEntity chassis_axle_front;
	private ModelEntity wheel0;
	private ModelEntity wheel1;
	private ModelEntity wheel2;
	private ModelEntity wheel3;


	public override void Spawn()
	{
		base.Spawn();

		var modelName = "models/car/car.vmdl";

		SetModel( modelName );
		RenderColor = new Color(1, 1, 1, 0);
		//	SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		SetInteractsExclude( CollisionLayer.Player );
		EnableSelfCollisions = false; 
	}
	
	public virtual CarPreviewModelInfo[] GetModelInfos()
	{
		List<CarPreviewModelInfo> infos = new();

		#region frontWheels

		var frontPos = new Vector3( 1.05f, 0, 0.35f ) * (40.0f + wheelForwardOffset);

		infos.Add( new CarPreviewModelInfo()
		{
			path = "entities/modular_vehicle/wheel_a.vmdl",
			position = frontPos + Vector3.Right * (0.7f * 40),
			angles = new Angles(0, -90,0)
		} );

		infos.Add( new CarPreviewModelInfo()
		{
			path = "entities/modular_vehicle/wheel_a.vmdl",
			position = frontPos + Vector3.Left * (0.7f * 40),
			angles = new Angles( 0, 90, 0 )
		} );

		#endregion

		#region rearWheel

		var rearPos = new Vector3( -1.05f, 0, 0.35f ) * (40.0f + wheelForwardOffset);

		infos.Add( new CarPreviewModelInfo()
		{
			path = "entities/modular_vehicle/wheel_a.vmdl",
			position = rearPos + Vector3.Right * (0.7f * 40),
			angles = new Angles( 0, -90, 0 )
		} );

		infos.Add( new CarPreviewModelInfo()
		{
			path = "entities/modular_vehicle/wheel_a.vmdl",
			position = rearPos + Vector3.Left * (0.7f * 40),
			angles = new Angles( 0, 90, 0 )
		} );

		#endregion

		return infos.ToArray();
	} 

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		{
			chassis_axle_front = new ModelEntity();
			chassis_axle_front.SetModel( "entities/modular_vehicle/chassis_axle_front.vmdl" );
			chassis_axle_front.Transform = Transform;
			chassis_axle_front.Parent = this;
			chassis_axle_front.LocalPosition = new Vector3( 1.05f, 0, 0.35f ) * (40.0f + wheelForwardOffset);

			{
				wheel0 = new ModelEntity();
				wheel0.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				wheel0.SetParent( chassis_axle_front, "Wheel_Steer_R", new Transform( Vector3.Zero, Rotation.From( 0, 180, 0 ) ) );
			}

			{
				wheel1 = new ModelEntity();
				wheel1.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				wheel1.SetParent( chassis_axle_front, "Wheel_Steer_L", new Transform( Vector3.Zero, Rotation.From( 0, 0, 0 ) ) );
			}

			{
				var chassis_steering = new ModelEntity();
				chassis_steering.SetModel( "entities/modular_vehicle/chassis_steering.vmdl" );
				chassis_steering.SetParent( chassis_axle_front, "Axle_front_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
			}
		}

		{
			chassis_axle_rear = new ModelEntity();
			chassis_axle_rear.SetModel( "entities/modular_vehicle/chassis_axle_rear.vmdl" );
			chassis_axle_rear.Transform = Transform;
			chassis_axle_rear.Parent = this;
			chassis_axle_rear.LocalPosition = new Vector3( -1.05f, 0, 0.35f ) * (40.0f + wheelForwardOffset);

			{
				var chassis_transmission = new ModelEntity();
				chassis_transmission.SetModel( "entities/modular_vehicle/chassis_transmission.vmdl" );
				chassis_transmission.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
			}

			{
				wheel2 = new ModelEntity();
				wheel2.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				wheel2.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Left * (0.7f * 40), Rotation.From( 0, 90, 0 ) ) );
			}

			{
				wheel3 = new ModelEntity();
				wheel3.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				wheel3.SetParent( chassis_axle_rear, "Axle_Rear_Center", new Transform( Vector3.Right * (0.7f * 40), Rotation.From( 0, -90, 0 ) ) );
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( driver is DeliveryPlayer player1 )
		{
			RemoveDriver( player1 );
		}
		if ( passenger is DeliveryPlayer player2 )
		{
			RemovePassenger( player2 );
		}
	}

	public void ResetInput()
	{
		currentInput.Reset();
	}

	[Event.Tick.Server]
	protected void Tick()
	{
		if ( driver is DeliveryPlayer player1 )
		{
			if ( player1.LifeState != LifeState.Alive || player1.Vehicle != this )
			{
				RemoveDriver( player1 );
			}
		}
		if ( passenger is DeliveryPlayer player2 )
		{
			if ( player2.LifeState != LifeState.Alive || player2.Vehicle != this )
			{
				RemovePassenger( player2 );
			}
		}
	}

	public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;

		using ( Prediction.Off() )
		{
			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( owner.Pawn is DeliveryPlayer player && !player.IsUseDisabled() )
				{
					if ( player == driver )
					{
						RemoveDriver( player );
					}
					else if ( player == passenger )
					{
						RemovePassenger( player );
					}

					return;
				}
			}

			if ( owner.Pawn is DeliveryPlayer dr_player && dr_player == driver )
			{
				currentInput.Reset();

				currentInput.throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
				currentInput.turning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
				currentInput.breaking = (Input.Down( InputButton.Jump ) ? 1 : 0);
				currentInput.tilt = (Input.Down( InputButton.Run ) ? 1 : 0) + (Input.Down( InputButton.Duck ) ? -1 : 0);
				currentInput.roll = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
			}
		}
	}

	[Event.Physics.PreStep]
	public void OnPrePhysicsStep()
	{
		if ( !IsServer )
			return;

		var selfBody = PhysicsBody;
		if ( !selfBody.IsValid() )
			return;

		var body = selfBody.SelfOrParent;
		if ( !body.IsValid() )
			return;

		var dt = Time.Delta;

		body.DragEnabled = false;

		var rotation = selfBody.Rotation;

		accelerateDirection = currentInput.throttle.Clamp( -1, 1 ) * (1.0f - currentInput.breaking);
		TurnDirection = TurnDirection.LerpTo( currentInput.turning.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.001f, dt ) );

		airRoll = airRoll.LerpTo( currentInput.roll.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.0001f, dt ) );
		airTilt = airTilt.LerpTo( currentInput.tilt.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.0001f, dt ) );

		float targetTilt = 0;
		float targetLean = 0;

		var localVelocity = rotation.Inverse * body.Velocity;

		if ( backWheelsOnGround || frontWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( localVelocity.x );
			var speedFraction = MathF.Min( forwardSpeed / 500.0f, 1 );

			targetTilt = accelerateDirection.Clamp( -1.0f, 1.0f );
			targetLean = speedFraction * TurnDirection;
		}

		AccelerationTilt = AccelerationTilt.LerpTo( targetTilt, 1.0f - MathF.Pow( 0.01f, dt ) );
		TurnLean = TurnLean.LerpTo( targetLean, 1.0f - MathF.Pow( 0.01f, dt ) );

		if ( backWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( localVelocity.x );
			var speedFactor = 1.0f - (forwardSpeed / 5000.0f).Clamp( 0.0f, 1.0f );
			var acceleration = speedFactor * (accelerateDirection < 0.0f ? car_accelspeed * 0.5f : car_accelspeed) * accelerateDirection * dt;
			var impulse = rotation * new Vector3( acceleration, 0, 0 );
			body.Velocity += impulse;
		}

		RaycastWheels( rotation, true, out frontWheelsOnGround, out backWheelsOnGround, dt );
		var onGround = frontWheelsOnGround || backWheelsOnGround;
		var fullyGrounded = (frontWheelsOnGround && backWheelsOnGround);
		Grounded = onGround;

		if ( fullyGrounded )
		{
			body.Velocity += PhysicsWorld.Gravity * dt;
		}

		body.GravityScale = fullyGrounded ? 0 : 1;

		bool canAirControl = false;

		var v = rotation * localVelocity.WithZ( 0 );
		var vDelta = MathF.Pow( (v.Length / 1000.0f).Clamp( 0, 1 ), 5.0f ).Clamp( 0, 1 );
		if ( vDelta < 0.01f ) vDelta = 0;

		if ( debug_car )
		{
			DebugOverlay.Line( body.MassCenter, body.MassCenter + rotation.Forward.Normal * 100, Color.White, 0, false );
			DebugOverlay.Line( body.MassCenter, body.MassCenter + v.Normal * 100, Color.Green, 0, false );
		}

		var angle = (rotation.Forward.Normal * MathF.Sign( localVelocity.x )).Normal.Dot( v.Normal ).Clamp( 0.0f, 1.0f );
		angle = angle.LerpTo( 1.0f, 1.0f - vDelta );
		grip = grip.LerpTo( angle, 1.0f - MathF.Pow( 0.001f, dt ) );

		if ( debug_car )
		{
			DebugOverlay.ScreenText( new Vector2( 200, 200 ), $"{grip}" );
		}

		var angularDamping = 0.0f;
		angularDamping = angularDamping.LerpTo( 5.0f, grip );

		body.LinearDamping = 0.0f;
		body.AngularDamping = fullyGrounded ? angularDamping : 0.5f;

		if ( onGround )
		{
			localVelocity = rotation.Inverse * body.Velocity;
			WheelSpeed = localVelocity.x;
			var turnAmount = frontWheelsOnGround ? (MathF.Sign( localVelocity.x ) * 25.0f * CalculateTurnFactor( TurnDirection, MathF.Abs( localVelocity.x ) ) * dt) : 0.0f;
			body.AngularVelocity += rotation * new Vector3( 0, 0, turnAmount );

			airRoll = 0;
			airTilt = 0;

			var forwardGrip = 0.1f;
			forwardGrip = forwardGrip.LerpTo( 0.9f, currentInput.breaking );
			body.Velocity = VelocityDamping( Velocity, rotation, new Vector3( forwardGrip, grip, 0 ), dt );
		}
		else
		{
			var s = selfBody.Position + (rotation * selfBody.LocalMassCenter);
			var tr = Trace.Ray( s, s + rotation.Down * 50 )
				.Ignore( this )
				.Run();

			if ( debug_car )
				DebugOverlay.Line( tr.StartPos, tr.EndPos, tr.Hit ? Color.Red : Color.Green );

			canAirControl = !tr.Hit;
		}

		if ( canAirControl && (airRoll != 0 || airTilt != 0) )
		{
			var offset = 50 * Scale;
			var s = selfBody.Position + (rotation * selfBody.LocalMassCenter) + (rotation.Right * airRoll * offset) + (rotation.Down * (10 * Scale));
			var tr = Trace.Ray( s, s + rotation.Up * (25 * Scale) )
				.Ignore( this )
				.Run();

			if ( debug_car )
				DebugOverlay.Line( tr.StartPos, tr.EndPos );

			bool dampen = false;

			if ( currentInput.roll.Clamp( -1, 1 ) != 0 )
			{
				var force = tr.Hit ? 400.0f : 100.0f;
				var roll = tr.Hit ? currentInput.roll.Clamp( -1, 1 ) : airRoll;
				body.ApplyForceAt( selfBody.MassCenter + rotation.Left * (offset * roll), (rotation.Down * roll) * (roll * (body.Mass * force)) );

				if ( debug_car )
					DebugOverlay.Sphere( selfBody.MassCenter + rotation.Left * (offset * roll), 8, Color.Red );

				dampen = true;
			}

			if ( !tr.Hit && currentInput.tilt.Clamp( -1, 1 ) != 0 )
			{
				var force = 200.0f;
				body.ApplyForceAt( selfBody.MassCenter + rotation.Forward * (offset * airTilt), (rotation.Down * airTilt) * (airTilt * (body.Mass * force)) );

				if ( debug_car )
					DebugOverlay.Sphere( selfBody.MassCenter + rotation.Forward * (offset * airTilt), 8, Color.Green );

				dampen = true;
			}

			if ( dampen )
				body.AngularVelocity = VelocityDamping( body.AngularVelocity, rotation, 0.95f, dt );
		}

		localVelocity = rotation.Inverse * body.Velocity;
		MovementSpeed = localVelocity.x;
	}

	private static float CalculateTurnFactor( float direction, float speed )
	{
		var turnFactor = MathF.Min( speed / 500.0f, 1 );
		var yawSpeedFactor = 1.0f - (speed / 1000.0f).Clamp( 0, 0.6f );

		return direction * turnFactor * yawSpeedFactor;
	}

	private static Vector3 VelocityDamping( Vector3 velocity, Rotation rotation, Vector3 damping, float dt )
	{
		var localVelocity = rotation.Inverse * velocity;
		var dampingPow = new Vector3( MathF.Pow( 1.0f - damping.x, dt ), MathF.Pow( 1.0f - damping.y, dt ), MathF.Pow( 1.0f - damping.z, dt ) );
		return rotation * (localVelocity * dampingPow);
	}

	private void RaycastWheels( Rotation rotation, bool doPhysics, out bool frontWheels, out bool backWheels, float dt )
	{
		float forward = 42 + wheelForwardOffset;
		float right = 32;

		var frontLeftPos = rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
		var frontRightPos = rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;
		var backLeftPos = -rotation.Forward * forward + rotation.Right * right + rotation.Up * 20;
		var backRightPos = -rotation.Forward * forward - rotation.Right * right + rotation.Up * 20;

		var tiltAmount = AccelerationTilt * 2.5f;
		var leanAmount = TurnLean * 2.5f;

		float length = 20.0f;

		frontWheels =
			frontLeft.Raycast( length + tiltAmount - leanAmount, doPhysics, frontLeftPos * Scale, ref frontLeftDistance, dt ) |
			frontRight.Raycast( length + tiltAmount + leanAmount, doPhysics, frontRightPos * Scale, ref frontRightDistance, dt );

		backWheels =
			backLeft.Raycast( length - tiltAmount - leanAmount, doPhysics, backLeftPos * Scale, ref backLeftDistance, dt ) |
			backRight.Raycast( length - tiltAmount + leanAmount, doPhysics, backRightPos * Scale, ref backRightDistance, dt );
	}

	float wheelAngle = 0.0f;
	float wheelRevolute = 0.0f;


	[Event.Tick.Client]
	public void ClientTick()
	{
		wheelAngle = wheelAngle.LerpTo( TurnDirection * 25, 1.0f - MathF.Pow( 0.001f, Time.Delta ) );
		wheelRevolute += (WheelSpeed / (14.0f * Scale)).RadianToDegree() * Time.Delta;
		var wheelRotRight = Rotation.From( -wheelAngle, 180, -wheelRevolute );
		var wheelRotLeft = Rotation.From( wheelAngle, 0, wheelRevolute );
		var wheelRotBackRight = Rotation.From( 0, 90, -wheelRevolute );
		var wheelRotBackLeft = Rotation.From( 0, -90, wheelRevolute );

		RaycastWheels( Rotation, false, out _, out _, Time.Delta );

		float frontOffset = 20.0f - Math.Min( frontLeftDistance, frontRightDistance );
		float backOffset = 20.0f - Math.Min( backLeftDistance, backRightDistance );

		if ( chassis_axle_front != null )
		{
			chassis_axle_front.SetBoneTransform( "Axle_front_Center", new Transform( Vector3.Up * frontOffset ), false );
		}
		if ( chassis_axle_rear != null )
		{
			chassis_axle_rear.SetBoneTransform( "Axle_Rear_Center", new Transform( Vector3.Up * backOffset ), false );
		}

		if ( wheel0 != null && wheel1 != null && wheel2 != null && wheel3 != null )
		{
			wheel0.LocalRotation = wheelRotRight;
			wheel1.LocalRotation = wheelRotLeft;
			wheel2.LocalRotation = wheelRotBackRight;
			wheel3.LocalRotation = wheelRotBackLeft;
		}
	}

	private void RemoveDriver( DeliveryPlayer player )
	{
		driver = null;
		timeSinceDriverLeft = 0;

		ResetInput();

		if ( !player.IsValid() )
			return;

		player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleAnimator = null;
		player.VehicleCamera = null;
		player.SetParent( null );

		if ( player.PhysicsBody.IsValid() )
		{
			player.PhysicsBody.Enabled = true;
			player.PhysicsBody.Position = player.Position;
		}
	}

	private void RemovePassenger( DeliveryPlayer player )
	{
		passenger = null;
		timeSincePassengerLeft = 0;

		ResetInput();

		if ( !player.IsValid() )
			return;

		player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleAnimator = null;
		player.VehicleCamera = null;
		player.SetParent( null );

		if ( player.PhysicsBody.IsValid() )
		{
			player.PhysicsBody.Enabled = true;
			player.PhysicsBody.Position = player.Position;
		}
	}
	public virtual bool Sit( Entity user )
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

	public virtual bool SitAsPassenger(Entity user)
	{
		if ( user is DeliveryPlayer player && player.Vehicle == null && timeSincePassengerLeft > 1.0f )
		{
			player.Vehicle = this;
			player.VehicleController = new CarController();
			player.VehicleAnimator = new CarAnimator();
			player.VehicleCamera = new CarCamera();
			player.Parent = this;
			player.LocalPosition = Vector3.Up * 24 + Vector3.Forward * 58 + Vector3.Right * 20;
			player.LocalRotation = Rotation.Identity;
			player.LocalScale = 1;
			player.PhysicsBody.Enabled = false;

			passenger = player;
		}

		return true;
	}

	public bool IsUsableAsPassenger( Entity user )
	{
		bool doorsOpened = true;
		if ( doorsToSeat != null )
		{
			doorsOpened = !doorsToSeat.All( t => t.openState < 0.5f );
		}
		return user is DeliveryPlayer player && passenger == null && doorsOpened;
	}

	public bool IsUsable( Entity user )
	{
		bool doorsOpened = true;
		if(doorsToSeat != null)
		{
			doorsOpened = !doorsToSeat.All( t => t.openState < 0.5f );
		}
		return user is DeliveryPlayer player && player.ownedCar == this && driver == null && doorsOpened;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsServer )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		body = body.SelfOrParent;
		if ( !body.IsValid() )
			return;

		if ( other is DeliveryPlayer player && player.Vehicle == null )
		{
			var speed = body.Velocity.Length;
			var forceOrigin = Position + Rotation.Down * Rand.Float( 20, 30 );
			var velocity = (player.Position - forceOrigin).Normal * speed;
			var angularVelocity = body.AngularVelocity;

			OnPhysicsCollision( new CollisionEventData
			{
				Entity = player,
				Pos = player.Position + Vector3.Up * 50,
				Velocity = velocity,
				PreVelocity = velocity * 20.0f, // I don't know why the ragdolls now need more force
				PostVelocity = velocity,
				PreAngularVelocity = angularVelocity,
				Speed = speed,
			} );
		}
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( !IsServer )
			return;

		if ( eventData.Entity is DeliveryPlayer player && player.Vehicle != null )
		{
			return;
		}

		var propData = GetModelPropData();

		var minImpactSpeed = propData.MinImpactDamageSpeed;
		if ( minImpactSpeed <= 0.0f ) minImpactSpeed = 500;

		var impactDmg = propData.ImpactDamage;
		if ( impactDmg <= 0.0f ) impactDmg = 10;

		var speed = eventData.Speed;

		if ( speed > minImpactSpeed )
		{
			if ( eventData.Entity.IsValid() && eventData.Entity != this )
			{
				var damage = speed / minImpactSpeed * impactDmg * 1.2f;
				eventData.Entity.TakeDamage( DamageInfo.Generic( damage )
					.WithFlag( DamageFlags.PhysicsImpact )
					.WithFlag( DamageFlags.Vehicle )
					.WithAttacker( driver != null ? driver : this, driver != null ? this : null )
					.WithPosition( eventData.Pos )
					.WithForce( eventData.PreVelocity ) );

				if ( eventData.Entity.LifeState == LifeState.Dead && eventData.Entity is not DeliveryPlayer )
				{
					PhysicsBody.Velocity = eventData.PreVelocity;
					PhysicsBody.AngularVelocity = eventData.PreAngularVelocity;
				}
			}
		}
	}
}
