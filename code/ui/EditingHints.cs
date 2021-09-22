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
	class EditingHints : Panel
	{
		public EditingHints()
		{
			StyleSheet.Load( "/ui/EditingHints.scss" );

			Add.Label( "Editing Mode (Press Attack To Set Delivery Point, Attack2 To Set Vehicle Selector, Hold R To Quit)");

			

			var img = Add.Image( "/ui/phone.png", "img" );
		}

		public override void Tick()
		{
			base.Tick();

		 	SetClass( "active", Local.Client.Pawn is DeliveryPlayer ply && ply.isEditing);
		}
	}
}
