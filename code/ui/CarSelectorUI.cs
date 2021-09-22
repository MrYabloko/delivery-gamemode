using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace DeliveryGamemode
{
	public class CarSelectorUI : Panel
	{
		Panel menuPanel;
		Panel navigationPanel;

		ScenePanel ViewPanel;

		Angles CamAngles = new( 25.0f, 0.0f, 0.0f );
		float CamDistance = 300;
		Vector3 CamPos => Vector3.Up * 10 + CamAngles.Direction * -CamDistance;

		Type currentCarType;

		public CarSelectorUI()
		{
			StyleSheet.Load( "/ui/CarSelectorUI.scss" );

			menuPanel = Add.Panel("menu");

			navigationPanel = menuPanel.Add.Panel("navigation");

		//	ViewPanel = menuPanel.Add.ScenePanel(SceneWorld.Current, Vector3.Zero, Rotation.Identity, 90, "viewer");
			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				//	SceneObject.CreateModel( "models/citizen_props/roadcone01.vmdl", Transform.Zero );
				
			/*	foreach(var mdl in Van.GetModelInfos())
				{
					SceneObject.CreateModel( mdl.path, new Transform(mdl.position, Rotation.From(mdl.angles)) );
				} */

				SceneObject.CreateModel( "models/room.vmdl", Transform.Zero );

				Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Forward * 150.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Backward * 150.0f, 200, Color.White * 15f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Left * 100.0f, 200, Color.White * 15.0f );

				ViewPanel = menuPanel.Add.ScenePanel( SceneWorld.Current, CamPos, Rotation.From( CamAngles ), 45, "viewer" );
			//	ViewPanel.Style.Width = 512;
			//	ViewPanel.Style.Height = 512;
			}

			ViewPanel.Add.Button( "Back", "button", () =>
			{
				if ( ViewPanel.World != null )
				{
					ViewPanel.World.Delete();
				}
				ViewPanel.SetClass( "active", false );
			} );

			ViewPanel.Add.Button( "Spawn", "button", () =>
			{
				DeliveryPlayer.SpawnCar( Local.Pawn.NetworkIdent, Library.GetAttribute(currentCarType).Name );
			} );

			var cars = Library.GetAll<CarEntity>().ToList();
			cars.Remove( typeof( CarEntity ) );

			foreach (var car in cars)
			{
				var attr = Library.GetAttribute( car );

				navigationPanel.Add.Button( attr.Title, "navbutton", () => 
				{
					SetSceneWorld(car);
				} );

			//	SetSceneWorld( car );
			}
		}

		public void SetSceneWorld(Type car)
		{
			var carInstnace = Library.Create<CarEntity>( car );
			carInstnace.Position = Vector3.Down * 1000;
			var infos = carInstnace.GetModelInfos();
			carInstnace.Delete();

			if (ViewPanel.World != null)
			{
				ViewPanel.World.Delete();
			}

			var scene = new SceneWorld();
			using ( SceneWorld.SetCurrent( scene ) )
			{
				SceneObject.CreateModel( "models/room.vmdl", Transform.Zero );

				Light.Point( Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Forward * 150.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Backward * 150.0f, 200, Color.White * 15f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 50.0f + Vector3.Left * 100.0f, 200, Color.White * 15.0f );
				Light.Point( Vector3.Up * 50.0f, 200.0f, Color.White * 5.0f );

				foreach ( var mdl in infos )
				{
					SceneObject.CreateModel( mdl.path, new Transform( mdl.position, Rotation.From( mdl.angles ) ) );
				}

			}

			currentCarType = car;

			ViewPanel.World = scene;
			ViewPanel.SetClass( "active", true );
		}

		public override void Tick()
		{
			base.Tick();

		//	Log.Info( Local.Pawn is DeliveryPlayer player && player.currentSelector != null );
			SetClass("open", Local.Pawn is DeliveryPlayer player && player.currentSelector != null );

			if ( HasMouseCapture )
			{
				CamAngles.pitch += Mouse.Delta.y;
				CamAngles.yaw -= Mouse.Delta.x;
				CamAngles.pitch = CamAngles.pitch.Clamp( 0, 90 );
			}

			ViewPanel.CameraPosition = CamPos;
			ViewPanel.CameraRotation = Rotation.From( CamAngles );
		}

		public override void OnMouseWheel( float value )
		{
			CamDistance += value * 50;
			CamDistance = CamDistance.Clamp( 100, 1500 );

			base.OnMouseWheel( value );
		}

		public override void OnButtonEvent( ButtonEvent e )
		{
			if ( e.Button == "mouseleft" )
			{
				SetMouseCapture( e.Pressed );
			}

			base.OnButtonEvent( e );
		}
	}
}
