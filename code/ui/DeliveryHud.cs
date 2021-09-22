using Sandbox.UI;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace DeliveryGamemode
{
	/// <summary>
	/// This is the HUD entity. It creates a RootPanel clientside, which can be accessed
	/// via RootPanel on this entity, or Local.Hud.
	/// </summary>
	public partial class DeliveryHudEntity : Sandbox.HudEntity<RootPanel>
	{
		public DeliveryHudEntity()
		{
			if ( IsClient )
			{
				RootPanel.SetTemplate( "/ui/deliveryhud.html" );

				RootPanel.StyleSheet.Load( "/ui/DeliveryHud.scss" );

				RootPanel.AddChild<DeliveryPosHelper>();
				RootPanel.AddChild<CarSelectorUI>();
				RootPanel.AddChild<EditingHints>();
				//RootPanel.AddChild<DeliveryOrdersMenu>();
			}
		}
	}

}
