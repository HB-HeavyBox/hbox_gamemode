using Sandbox;
using System;

[Library( "ent_lyres", Title = "Lyres", Spawnable = true )]
public partial class LyresEntity : Prop, IUse
{
	protected virtual Vector3 LightOffset => new Vector3( 0f, 0f, -50f );
	private SpotLightEntity worldLight;

	[Net]
	public bool Enabled { get; set; } = true;

	public override void Spawn()
	{
		
		base.Spawn();

		SetModel( "models/heavybox/spot/lyres.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		worldLight = CreateLight();
		worldLight.Enabled = true;
		worldLight.SetParent( this, "head", new Transform( LightOffset, Rotation.From( -90, 0, 0 ) ) );
		worldLight.EnableViewmodelRendering = true;
	}

		private SpotLightEntity CreateLight()
	{
		var muzzle = GetAttachment( "head" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		var light = new SpotLightEntity
		{
			Enabled = true,
			DynamicShadows = true,
			Range = 2048,
			Falloff = 0.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 0.0f,
			Brightness = 300,
			Color = Color.Random,
			InnerConeAngle = 10,
			OuterConeAngle = 10,
			FogStength = 1.0f
		};

		return light;
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		Enabled = !Enabled;

		return false;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		KillEffects();
		Delete();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient )
		{
			KillEffects();
		}
	}
}
