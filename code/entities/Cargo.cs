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
	[Library( "ent_cargo" )]
	public class Cargo : Prop
	{
		public DeliveryPlayer cargoOwner;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/citizen_props/crate01.vmdl_c" );
			//	Scale = Rand.Float( 0.35f, 0.85f );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic);
		//	CollisionGroup = CollisionGroup.Prop;
		//	ClearCollisionLayers();
		//	AddCollisionLayer( CollisionLayer.PhysicsProp );
			
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

		//	ui.Position = Position + Vector3.Up * 100;
		}

		public override void TakeDamage( DamageInfo info )
		{

		}
	}
}
