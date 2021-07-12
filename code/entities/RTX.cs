using Sandbox;
using System;

[Library( "ent_rtx", Title = "RTX3080", Spawnable = true )]
public partial class RTXEntity : Prop, IUse
{

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/heavybox/rtx/rtx.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		if ( user is Player player )
		{
			
		}

		return false;
	}
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
