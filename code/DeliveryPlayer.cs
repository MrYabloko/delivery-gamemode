using Sandbox;
using System;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using Sandbox.UI;

namespace DeliveryGamemode
{
	public partial class DeliveryPlayer : Player
	{
		[Net] public bool isEditing { get; private set; }

		public void ChangeEditingMode( bool value )
		{
			isEditing = value;
			((DeliveryGame)Game.Current).ChangePointsDrawState( value );
		}

		public CarEntity ownedCar;

		[Net] public PawnController VehicleController { get; set; }
		[Net] public PawnController NoclipController { get; set; }
		[Net] public PawnAnimator VehicleAnimator { get; set; }
		[Net, Predicted] public ICamera VehicleCamera { get; set; }
		[Net, Predicted] public Entity Vehicle { get; set; }
		[Net, Predicted] public ICamera MainCamera { get; set; }

		Cargo grabbedCargo = null;

		float editingButtonPress;


		public ICamera LastCamera { get; set; }
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();

			NoclipController = new NoclipController();

			Animator = new StandardPlayerAnimator();

			MainCamera = LastCamera;
			Camera = MainCamera;

			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Vehicle = null;
			VehicleAnimator = null;
			VehicleCamera = null;
			VehicleController = null;

			base.Respawn();
		}

		public override void Spawn()
		{
			MainCamera = new FirstPersonCamera();
			LastCamera = MainCamera;
			Vehicle = null;
			VehicleAnimator = null;
			VehicleCamera = null;
			VehicleController = null;

			editingButtonPress = 0;
			isEditing = false;

			base.Spawn();
		}

		public DeliveryGame.OrderInfo info { get; private set; }
		public int orderState { get; private set; }

		public CarSelector currentSelector;

		[ClientRpc]
		public void setSelector(CarSelector selector)
		{
			currentSelector = selector;
		}


		ModelEntity arrow;

		[ClientRpc()]
		void SetOrderInfo(Vector3 startPosition, Vector3 startNormal, Vector3 endPosition, Vector3 endNormal, Cargo[] cargos)
		{
			info = new DeliveryGame.OrderInfo()
			{
				start = new DeliveryGame.DeliveryPoint() { position = startPosition, normal = startNormal },
				end = new DeliveryGame.DeliveryPoint() { position = endPosition, normal = endNormal },
				cargos = cargos
			};
			orderState = 0;

			if ( arrow != null )
			{
				arrow.EnableShadowReceive = true;
				arrow.EnableShadowCasting = false;
			//	arrow.EnableDrawOverWorld = true;
			}
		}

		[ClientRpc()]
		void ClearInfo()
		{
			info = null;
			timeSinceComplete = 0;
		}

		[ClientRpc()]
		void SetOrderState(int state)
		{
			orderState = state;
		}

		[ServerCmd]
		public static void SpawnCar( int networkPawnID, string typeName )
		{
			var type = Library.Get<CarEntity>(typeName);
			var playerEnt = FindByIndex(networkPawnID);
			if ( playerEnt is DeliveryPlayer player && player.currentSelector != null )
			{
				if(player.ownedCar != null)
				{
					player.ownedCar.Delete();
				}

				var spawnPosition = player.currentSelector.Position + player.currentSelector.Rotation.Backward * 100 + Vector3.Up * 100;
				var car = Library.Create<CarEntity>( type );
				car.Position = spawnPosition;
				car.Rotation = Rotation.Identity;
				player.ownedCar = car;

				if(player.info != null)
				{
					foreach(var cargo in player.info.cargos)
					{
						cargo.Delete();
					}
				}

				player.info = null;
				player.ClearInfo();
				player.orderState = 0;
				player.SetOrderState(0);
			}
		}

		TimeSince timeSinceComplete;

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if (arrow == null && IsClient)
			{
				arrow = new ModelEntity( "models/arrow.vmdl" );
				arrow.Position = Vector3.Down * 100;
				
			}

			if(info == null && ((DeliveryGame)Game.Current).points.Count >= 2 && IsServer
				&& timeSinceComplete > 10 && ownedCar != null)
			{
				info = ((DeliveryGame)Game.Current).GetNewOrder( ownedCar.boxCount, this, ownedCar.maxBoxSize );
				orderState = 0;
				SetOrderInfo(info.start.position, info.start.normal,
					info.end.position, info.end.normal, 
					info.cargos.ToArray());
			}

			if ( IsClient && info != null && orderState == 0 )
			{
				var pos = info.start.position + info.start.normal * 50;
				arrow.Position = pos;
				arrow.Scale = 2;

			}

			if(IsServer && info != null && orderState == 0)
			{
				var dist = Vector3.DistanceBetween( Position, info.start.position ) / 1000;
				if ( dist < 0.25f )
				{
					orderState = 1;
					SetOrderState( 1 );
				}
			}

			if ( IsClient && orderState == 1 )
			{
				var pos = info.end.position + info.end.normal * 50;
				Vector3 eye = EyePos;

				if ( GetActiveCamera() is CarCamera camera && !camera.firstPerson )
				{
					eye = camera.Pos;
				}

				var direction = (pos - eye).Normal;

				arrow.Position = info.end.position + info.end.normal * 50 ;
				//	var dist = Vector3.DistanceBetween( Position, arrow.Position ) / 500;
				//	dist = dist < 1 ? 1 : dist;
				arrow.Scale = 2;
			}

			if(IsServer && currentSelector != null)
			{
				var dist = Vector3.DistanceBetween( currentSelector.Position, Position ) / 51.49f;
				if(dist > 2)
				{
					currentSelector = null;
					setSelector( null );
				}
			}

			if(IsServer && orderState == 1)
			{
				if ( info.cargos.All( t => (Vector3.DistanceBetween( info.end.position + info.end.normal * 50, t.Position ) / 1000) < 0.25f )
					&& Vector3.DistanceBetween(GetActiveController().Pawn.Position, info.end.position + info.end.normal * 50 ) / 1000 > 0.45f)
				{
					orderState = 2;
					SetOrderState( 2 );
					foreach(var cargo in info.cargos)
					{
						cargo.cargoOwner = null;
						cargo.DeleteAsync(10f);
					}
					timeSinceComplete = 0;
					info = null;
				}
			}

			if(IsClient && orderState == 2)
			{
				arrow.Position = Vector3.Down * 500;
				arrow.EnableDrawOverWorld = false;
			}



			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if(editingButtonPress > 1 && PlayerScore.All.Length <= 1 )
			{
				ChangeEditingMode( !isEditing );
				editingButtonPress = 0;
			}

			if(Input.Down(InputButton.Reload))
			{
				editingButtonPress += Time.Delta;
			}
			else
			{
				editingButtonPress = 0;
			}

			if(isEditing && PlayerScore.All.Length > 1)
			{
				ChangeEditingMode( false );
			}

			var eyePos = EyePos;
			var eyeRot = EyeRot;
			var eyeDir = EyeRot.Forward;



			if (Input.Pressed(InputButton.Attack2))
			{
				if(isEditing)
				{
					var tr = Trace.Ray( eyePos, eyePos + eyeDir * 1000 )
						.UseHitboxes()
						.Ignore( this, false )
						.Radius( 2.0f )
						.HitLayer( CollisionLayer.All )
						.Run();
					if ( tr.Hit )
					{
						if ( tr.Entity is CarSelector selector )
						{
							((DeliveryGame)Game.Current).DeleteCarSelector( selector );
						}
						else
						{
							var rot = new Angles(0,EyeRot.Yaw() + 180,0);
							((DeliveryGame)Game.Current).CreateCarSelector( tr.EndPos, rot );
						}
					}
				}
				else
				{
					if ( grabbedCargo == null )
					{
						var tr = Trace.Ray( eyePos, eyePos + eyeDir * 100 )
							.UseHitboxes()
							.Ignore( this, false )
							.Radius( 2.0f )
							.HitLayer( CollisionLayer.All )
							.Run();

						if ( tr.Entity is Cargo cargo && cargo.cargoOwner == this)
						{
							grabbedCargo = cargo;
						}
					}
					else
					{
						grabbedCargo = null;
					}
				}
			}

			if(Input.Pressed(InputButton.Attack1))
			{
				if ( isEditing )
				{
					var tr = Trace.Ray( eyePos, eyePos + eyeDir * 1000 )
						.UseHitboxes()
						.Ignore( this, false )
						.Radius( 2.0f )
						.HitLayer( CollisionLayer.All )
						.Run();
					if(tr.Hit)
					{
						((DeliveryGame)Game.Current).CreateDeliveryPoint(tr.EndPos, tr.Normal);
					}
				}
				else
				{
					if ( grabbedCargo != null )
					{
						grabbedCargo.PhysicsBody.Velocity = eyeDir * 500;
						grabbedCargo = null;
					}
				}
			}

			if(grabbedCargo != null)
			{
				var finalPos = (eyePos + eyeDir * 100);
				grabbedCargo.PhysicsBody.Velocity = (finalPos - 
					(grabbedCargo.Position + grabbedCargo.Rotation.Up * 20)) * 10;
				if(Vector3.DistanceBetween(finalPos, grabbedCargo.Position) > 100)
				{
					grabbedCargo = null;
				}
			}

			TickPlayerUse();

			SimulateActiveChild( cl, ActiveChild );

			Camera = GetActiveCamera();

		}

		protected override void TickPlayerUse()
		{
			if ( !Host.IsServer ) return;

			// Turn prediction off
			using ( Prediction.Off() )
			{
				if ( Input.Pressed( InputButton.Use ) )
				{
					Using = FindUsable();

					if ( Using == null )
					{
						UseFail();
						return;
					}
				}

				if ( !Input.Down( InputButton.Use ) )
				{
					StopUsing();
					return;
				}

				if ( !Using.IsValid() )
					return;

				// If we move too far away or something we should probably ClearUse()?

				//
				// If use returns true then we can keep using it
				//
				if ( Using is IUse use && use.OnUse( this ) )
					return;

				StopUsing();
			}
		}



		public override void OnKilled()
		{
			base.OnKilled();

			VehicleController = null;
			VehicleAnimator = null;
			VehicleCamera = null;
			Vehicle = null;


			LastCamera = MainCamera;
			MainCamera = new SpectateRagdollCamera();
			Camera = MainCamera;
			Controller = null;

			EnableDrawing = false;
		}

		public override PawnController GetActiveController()
		{
			if ( VehicleController != null ) return VehicleController;
			if ( DevController != null ) return DevController;
			if ( isEditing ) return NoclipController;

			return base.GetActiveController();
		}

		public override PawnAnimator GetActiveAnimator()
		{
			if ( VehicleAnimator != null ) return VehicleAnimator;

			return base.GetActiveAnimator();
		}

		public bool IsUseDisabled()
		{
			return ActiveChild is IUse use && use.IsUsable( this );
		}

		protected override Entity FindUsable()
		{
			if ( IsUseDisabled() )
				return null;

			// First try a direct 0 width line
			var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
				.HitLayer( CollisionLayer.Debris | CollisionLayer.Trigger )
				.Ignore( this )
				.Run();

			// Nothing found, try a wider search
			if ( !IsValidUseEntity( tr.Entity ) )
			{
				tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * (85 * Scale) )
				.Radius( 2 )
				.HitLayer( CollisionLayer.Debris | CollisionLayer.Trigger )
				.Ignore( this )
				.Run();
			}

			// Still no good? Bail.
			if ( !IsValidUseEntity( tr.Entity ) ) return null;

			return tr.Entity;
		}

		protected override void UseFail()
		{
			if ( IsUseDisabled() )
				return;

			base.UseFail();
		}

		public ICamera GetActiveCamera()
		{
			if ( VehicleCamera != null ) return VehicleCamera;

			return MainCamera;
		}
	}
}
