using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	public class CarSelector : Prop, IUse
	{
		public bool IsUsable( Entity user )
		{
			return true;
		}

		public bool OnUse( Entity user )
		{
			if(user is DeliveryPlayer player)
			{
				player.setSelector( this );
				player.currentSelector = this;
			}

			return true;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/ticket_machine/ticket_machine.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Static );
		}
	}
}
