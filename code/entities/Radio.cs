using Sandbox;
using System;

[Library( "ent_radio", Title = "Radio", Spawnable = true )]
public partial class RadioEntity : Prop, IUse
{

	public bool Enable { get; set; } = true;
	private Sound RadioSnd;

	public override void Spawn()
	{
		base.Spawn();
		CheckSnd();
		SetModel( "models/heavybox/radio.vmdl" );
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
			CheckSnd();
		}

		return false;
	}

	public void CheckSnd()
	{
		if(Enable==true){
			Enable=false;
			RadioSnd = base.PlaySound( "radio.radio_music" );
		}else{
			Enable=true;
			RadioSnd.Stop();
		}
	}

	protected override void OnDestroy()
	{
		RadioSnd.Stop();
		base.OnDestroy();
	}
}
