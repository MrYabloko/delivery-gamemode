using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sandbox;

namespace DeliveryGamemode
{
	class Seat : ModelEntity, IUse
	{
		public delegate bool UseDelegate( Entity user );
		public UseDelegate usable;
		public UseDelegate use;

		public bool IsUsable( Entity user )
		{
			return usable( user );
		}

		public bool OnUse( Entity user )
		{
			return use( user );
		}
	}
}
