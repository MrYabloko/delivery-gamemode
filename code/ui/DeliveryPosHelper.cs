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
	public partial class DeliveryPosHelper : Panel
	{
		public Panel markerPanel;
		public Label distanceLabel;
		public DeliveryPosHelper()
		{
			StyleSheet.Load( "/ui/DeliveryPosHelper.scss" );

			markerPanel = Add.Panel( "marker" );
			distanceLabel = Add.Label();

		}

		public override void Tick()
		{
			base.Tick();

			if(Local.Pawn is DeliveryPlayer player)
			{
				distanceLabel.Style.Dirty();
				markerPanel.Style.Dirty();
				if (player.info != null && player.orderState != 2)
				{
					var point = player.info.end;

					if ( player.info.cargos.Length > 0 )
					{
						var cargo = player.info.cargos.OrderByDescending( t => Vector3.DistanceBetween( t.Position, player.Position ) ).FirstOrDefault();

						if ( (Vector3.DistanceBetween( cargo.Position, player.Position ) / 1000) > 1 )
						{
							point = new DeliveryGame.DeliveryPoint() { position = cargo.Position, normal = Vector3.Up };
						}
					}

					if ( Vector3.DistanceBetween( point.position + point.normal * 50, player.Position ) / 1000 > 1)
					{
						var transf = new PanelTransform();
						var screenPos = (point.position + point.normal * 50).ToScreen();

						transf.AddTranslateX( Length.Pixels(
							 screenPos.x * Screen.Width - (Screen.Width / 2) ) );

						transf.AddTranslateY( Length.Pixels(
							screenPos.y * Screen.Height - (Screen.Height / 2) ) );

						var distance = Vector3.DistanceBetween( point.position + point.normal * 50, player.Position ) / 52.49f;
						string distanceString = "";
						if(distance >= 1000)
						{
							distanceString = (distance / 1000).ToString();
							distanceString = distanceString.Substring( 0, distanceString.Length < 4 ? distanceString.Length : 4 ) + "km";
						}
						else
						{
							distanceString = MathX.CeilToInt(distance).ToString() + "m";
						}

						distanceLabel.Text = distanceString;

						markerPanel.Style.Transform = transf;

						distanceLabel.Style.Transform = transf;
					}
					else
					{
						var transf = new PanelTransform();
						transf.AddTranslateX( Screen.Height );
						transf.AddTranslateY( Screen.Width );
						markerPanel.Style.Transform = transf;
						distanceLabel.Style.Transform = transf;
					}
				}
				else
				{
					var transf = new PanelTransform();
					transf.AddTranslateX( Screen.Height );
					transf.AddTranslateY( Screen.Width );
					markerPanel.Style.Transform = transf;
					distanceLabel.Style.Transform = transf;
				} 
			} 
		}
	}
}
