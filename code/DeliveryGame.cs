
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace DeliveryGamemode
{

	/// <summary>
	/// This is your game class. This is an entity that is created serverside when
	/// the game starts, and is replicated to the client. 
	/// 
	/// You can use this to create things like HUDs and declare which player class
	/// to use for spawned players.
	/// </summary>
	/// 

	[Library("deliverygamemode", Title = "Delivery GameMode")]
	public partial class DeliveryGame : Sandbox.Game
	{
		public class DeliveryPointWithModel
		{
			public Vector3 position;
			public Vector3 normal;
			public ModelEntity model;
		}

		public class DeliveryPoint
		{
			public Vector3 position { get; set; }
			public Vector3 normal { get; set; }
		}

		List<CarSelector> selectors = new List<CarSelector>();

		public class CarSelectorInfo
		{
			public Vector3 position { get; set; }
			public Angles rotation { get; set; }
		}

		public class OrderInfo
		{
			public DeliveryPoint start;
			public DeliveryPoint end;
			public Cargo[] cargos;
		}

		public List<DeliveryPointWithModel> points = new ();
		public List<DeliveryPoint> lastPoints = new ();

		class DeliveryMapInfo
		{
			public DeliveryPoint[] points { get; set; }
			public CarSelectorInfo[] selectors { get; set; }
		} 

		public void CreateDeliveryPoint(Vector3 position, Vector3 normal)
		{
			var closest = points.Where( t => Vector3.DistanceBetween( t.position + t.normal * 50, position + normal * 50 ) / 1000 < 0.1f)
				.OrderBy(t => Vector3.DistanceBetween(t.position + t.normal * 50, position + normal * 50)).FirstOrDefault();

			if ( closest != null )
			{
				if ( closest.model != null )
				{
					closest.model.Delete();
				}
				points.Remove( closest );
			}
			else
			{
				var point = new DeliveryPointWithModel()
				{

					position = position,
					normal = normal,
					model = null
				};
				if ( IsClient )
				{
					/*	foreach ( var p in points )
						{
							p.model.Delete();
						} */

					var model = new ModelEntity( "models/dev/sphere.vmdl" );
					model.Position = position + normal * 50;
					model.Scale = 0.35f;
					model.EnableShadowCasting = false;
					model.EnableShadowReceive = false;
					model.RenderColor = Color.Red;
					point.model = model;
				}
				points.Add( point );
			}
			lastPoints = points.Select( t => new DeliveryPoint() { position = t.position, normal = t.normal } ).ToList();

			if ( IsServer )
			{



				BaseFileSystem system = FileSystem.Data;

				var path = "map-configs";

				system.CreateDirectory( path );

				var data = system.CreateSubSystem( path );

				data.WriteJson( Global.MapName + ".txt", new DeliveryMapInfo
				{
					points = points.Select(
						t => new DeliveryPoint() { position = t.position, normal = t.normal } 
						).ToArray(),
					selectors = selectors.Select(
						t => new CarSelectorInfo() { position = t.Position, rotation = t.Rotation.Angles() } 
						).ToArray()
				} ); 

			}
		}

		public void CreateCarSelector(Vector3 position, Angles rotation)
		{
			if(IsServer && PlayerScore.All.Length <= 1 )
			{

				var selector = new CarSelector();

				selector.Position = position;

				selector.Rotation = Rotation.From( rotation );

				selectors.Add( selector );

				BaseFileSystem system = FileSystem.Data;

				var path = "map-configs";

				system.CreateDirectory( path );

				var data = system.CreateSubSystem( path );

				data.WriteJson( Global.MapName + ".txt", new DeliveryMapInfo
				{
					points = points.Select(
						t => new DeliveryPoint() { position = t.position, normal = t.normal }
						).ToArray(),
					selectors = selectors.Select(
						t => new CarSelectorInfo() { position = t.Position, rotation = t.Rotation.Angles() }
						).ToArray()
				} );
			}
		}

		public void DeleteCarSelector(CarSelector selector)
		{
			if ( IsServer && PlayerScore.All.Length <= 1)
			{
				selectors.Remove( selector );
				selector.Delete();

				BaseFileSystem system = FileSystem.Data;

				var path = "map-configs";

				system.CreateDirectory( path );

				var data = system.CreateSubSystem( path );

				data.WriteJson( Global.MapName + ".txt", new DeliveryMapInfo
				{
					points = points.Select(
						t => new DeliveryPoint() { position = t.position, normal = t.normal }
						).ToArray(),
					selectors = selectors.Select(
						t => new CarSelectorInfo() { position = t.Position, rotation = t.Rotation.Angles() }
						).ToArray()
				} );
			}
		}

		public void ChangePointsDrawState(bool value)
		{
			if ( IsClient )
			{
				foreach(var point in points)
				{
					if ( !value )
					{
						if ( point.model != null )
						{
							point.model.Delete();
						}
					}
					else
					{
						var model = new ModelEntity( "models/dev/sphere.vmdl" );
						model.Position = point.position + point.normal * 50;
						model.Scale = 0.35f;
						model.RenderColor = Color.Red;
						point.model = model; 
					}
				}
			}

		}

	/*	public DeliveryPoint GetNearPoint(Vector3 position)
		{

		} */

		public Cargo SpawnCargo(Vector3 position, DeliveryPlayer owner, float size)
		{
			Cargo cargo = new Cargo();
			cargo.Scale = size;
			cargo.Position = position;
			cargo.cargoOwner = owner;

			return cargo;
		}

		public OrderInfo GetNewOrder(int boxCounts, DeliveryPlayer player, float size)
		{
			var info = new OrderInfo();
			var start = lastPoints.OrderByDescending(t => Vector3.DistanceBetween(player.Position, t.position)).First();
			lastPoints.Remove( start );
			var end = lastPoints.OrderByDescending( t => Vector3.DistanceBetween(start.position, t.position)).First();
			lastPoints.Remove( end );
			info.start = start;
			info.end = end;

			List<Cargo> cargos = new List<Cargo>();

			for(int i = 0;i < boxCounts;i++ )
			{

				var position = new Vector3();
				position.x = Rand.Float( -25, 25 );
				position.y = Rand.Float( -25, 25 );
				position.z = Rand.Float( -25, 25 );

				position += start.position + start.normal * 50;

				cargos.Add( SpawnCargo( position, player, size ) );
			}

			info.cargos = cargos.ToArray();

			if(lastPoints.Count <= 1)
			{
				lastPoints = points.Select(t => new DeliveryPoint() { position = t.position, normal = t.normal } ).ToList();
			}

			return info;
		}

		public DeliveryGame()
		{
			if ( IsServer )
			{
				new DeliveryHudEntity();

				BaseFileSystem system = FileSystem.Data;
				var path = "map-configs";
				system.CreateDirectory( path );
				var data = system.CreateSubSystem( path );


				BaseFileSystem mounted = FileSystem.Mounted;
				var pathToDefaults = "/data/default-map-configs/";
				var defaultconfigs = mounted.FindFile( pathToDefaults ).ToArray();

				foreach(var conf in defaultconfigs)
				{
					if(!data.FileExists(conf))
					{
						data.WriteAllText(conf, mounted.ReadAllText(pathToDefaults + conf));
					}
				}

				if(data.FileExists(Global.MapName + ".txt"))
				{
					var mapInfo = data.ReadJson<DeliveryMapInfo>(Global.MapName + ".txt");
					points = mapInfo.points.Select(t => new DeliveryPointWithModel() { model = null, position = t.position, normal = t.normal } ).ToList();
					lastPoints = mapInfo.points.ToList();
					selectors = mapInfo.selectors.Select( t => new CarSelector() { Position = t.position, Rotation = Rotation.From(t.rotation) } ).ToList();
				}
			}
			
		}

		public struct DeliveryPointNetwork
		{
		    public Vector3 position;
			public Vector3 normal;
		}


		[ClientRpc]
		public void FromMapInfo( DeliveryPointNetwork[] points, DeliveryPointNetwork[] lastPoints )
		{
			this.lastPoints = lastPoints.Select( t => new DeliveryPoint() { position = t.position, normal = t.normal } ).ToList();
			this.points = points.Select( t => new DeliveryPointWithModel() {
				position = t.position,
				normal = t.normal,
				model = null
			} ).ToList();
		}

		/// <summary>
		/// A client has joined the server. Make them a pawn to play with
		/// </summary>
		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new DeliveryPlayer();
			client.Pawn = player;

			player.Respawn();

			//Van car = new Van();
			//car.Position = player.Position;
			//player.ownedCar = car;

			FromMapInfo( To.Single( client ), 
				points.Select(t => new DeliveryPointNetwork() { position = t.position, normal = t.normal } ).ToArray(),
				lastPoints.Select(t => new DeliveryPointNetwork() { position = t.position, normal = t.normal } ).ToArray());
			
		//	Cargo cargo = new Cargo();
		//	cargo.Position = player.Position + player.Rotation.Forward * 100;
		}
	}

}
