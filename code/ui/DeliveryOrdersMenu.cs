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
	public class DeliveryOrdersMenu : Panel
	{
		private bool isOpened = false;
		private bool isPressed = false;

		public DeliveryOrdersMenu()
		{
			StyleSheet.Load( "/ui/DeliveryOrdersMenu.scss" );

			Panel phonePanel = Add.Panel( "phone" );
			Panel menuPanel = phonePanel.Add.Panel( "menu" );
		}

		public override void Tick()
		{
			base.Tick();

			if(Input.Down(InputButton.Menu) && !isPressed)
			{
				isOpened = !isOpened;
				isPressed = true;
			}

			if(Input.Released(InputButton.Menu))
			{
				isPressed = false;
			}

			SetClass( "open", isOpened );
		}
	}
}
