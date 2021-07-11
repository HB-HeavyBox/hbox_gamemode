﻿using Sandbox;

[Library( "weapon_haxgun", Title = "HaxGun", Spawnable = true )]
partial class Haxgun : Weapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 1.0f;
	public override float SecondaryRate => 15.0f;
	public override float ReloadTime => 5.0f;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( "models/heavybox/monitormodelfinal.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		ShootEffects();
		PlaySound( "haxgun.shoot" );

		if ( Host.IsServer )
		{
			var ent = new Prop
			{
				Position = Owner.EyePos + Owner.EyeRot.Forward * 50,
				Rotation = Owner.EyeRot
			};

			ent.SetModel("models/heavybox/monitormodelfinal.vmdl");
			ent.Velocity = Owner.EyeRot.Forward * 10000;
		}
		// ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
		
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

		ShootEffects();
		// PlaySound( "haxgun.shoot" );

		if ( Host.IsServer )
		{
			var ent = new Prop
			{
				Position = Owner.EyePos + Owner.EyeRot.Forward * 50,
				Rotation = Owner.EyeRot
			};

			ent.SetModel("models/heavybox/monitormodelfinal.vmdl");
			ent.Velocity = Owner.EyeRot.Forward * 10000;
		}
		// ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		// Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		// Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}
	
	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 2 );
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
