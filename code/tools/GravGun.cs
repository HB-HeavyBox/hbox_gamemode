﻿using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

[Library( "weapon_gravgun", Title = "Gravitygun", Spawnable = true  )]
public partial class GravGun : Carriable
{
	public override string ViewModelPath => "models/gravgun/v_gravgun.vmdl";

	private PhysicsBody holdBody;
	private WeldJoint holdJoint;
	private GenericJoint collisionJoint;

	public PhysicsBody HeldBody { get; private set; }
	public Rotation HeldRot { get; private set; }
	public ModelEntity HeldEntity { get; private set; }

	protected virtual float MaxPullDistance => 2000.0f;
	protected virtual float MaxPushDistance => 500.0f;
	protected virtual float LinearFrequency => 10.0f;
	protected virtual float LinearDampingRatio => 1.0f;
	protected virtual float AngularFrequency => 10.0f;
	protected virtual float AngularDampingRatio => 1.0f;
	protected virtual float PullForce => 20.0f;
	protected virtual float PushForce => 1000.0f;
	protected virtual float ThrowForce => 2000.0f;
	protected virtual float HoldDistance => 60.0f;
	protected virtual float AttachDistance => 150.0f;
	protected virtual float DropCooldown => 0.5f;
	protected virtual float BreakLinearForce => 2000.0f;

	private TimeSince timeSinceDrop;

	private Sound GrabSnd;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/gravgun/gravgun.vmdl" );

		CollisionGroup = CollisionGroup.Weapon;
		SetInteractsAs( CollisionLayer.Debris );
	}

	public override void Simulate( Client client )
	{
		if ( Owner is not Player owner ) return;

		if ( !IsServer )
			return;

		using ( Prediction.Off() )
		{
			var eyePos = owner.EyePos;
			var eyeRot = owner.EyeRot;
			var eyeDir = owner.EyeRot.Forward;

			if ( HeldBody.IsValid() && HeldBody.PhysicsGroup != null )
			{
				if ( holdJoint.IsValid && !holdJoint.IsActive )
				{
					GrabEnd();
				}
				else if ( Input.Pressed( InputButton.Attack1 ) )
				{
					if ( HeldBody.PhysicsGroup.BodyCount > 1 )
					{
						// Don't throw ragdolls as hard
						HeldBody.PhysicsGroup.ApplyImpulse( eyeDir * (ThrowForce * 0.5f), true );
						HeldBody.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * ThrowForce, true );
					}
					else
					{
						HeldBody.ApplyImpulse( eyeDir * (HeldBody.Mass * ThrowForce) );
						HeldBody.ApplyAngularImpulse( Vector3.Random * (HeldBody.Mass * ThrowForce) );
					}

					PlaySound( "gravgun.fire" ).SetRandomPitch(0.9f,1.2f);
					GrabSnd.Stop();
					GrabEnd();
				}
				else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					timeSinceDrop = 0;
					PlaySound( "gravgun.release" );
					GrabSnd.Stop();
					GrabEnd();
				}
				else
				{
					GrabMove( eyePos, eyeDir, eyeRot );
				}

				return;
			}

			if ( timeSinceDrop < DropCooldown )
				return;

			var tr = Trace.Ray( eyePos, eyePos + eyeDir * MaxPullDistance )
				.UseHitboxes()
				.Ignore( owner, false )
				.Radius( 2.0f )
				.HitLayer( CollisionLayer.Debris )
				.Run();

			if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld  )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					PlaySound( "gravgun.empty_fire" ).SetRandomPitch(0.9f,1.2f);
				}else if ( Input.Pressed( InputButton.Attack2 ) )
				{
					PlaySound( "gravgun.empty_grab_fire" ).SetRandomPitch(0.9f,1.2f);;
				}
			}

			if ( !tr.Hit || !tr.Body.IsValid() || !tr.Entity.IsValid() || tr.Entity.IsWorld )
				return;

			if ( tr.Entity.PhysicsGroup == null )
				return;

			var modelEnt = tr.Entity as ModelEntity;
			if ( !modelEnt.IsValid() )
				return;

			var body = tr.Body;

			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				if ( tr.Distance < MaxPushDistance && !IsBodyGrabbed( body ) )
				{
					var pushScale = 1.0f - Math.Clamp( tr.Distance / MaxPushDistance, 0.0f, 1.0f );
					body.ApplyImpulseAt( tr.EndPos, eyeDir * (body.Mass * (PushForce * pushScale)) );
					PlaySound( "gravgun.fire" ).SetRandomPitch(0.9f,1.2f);
				}
			}
			else if ( Input.Down( InputButton.Attack2 ) )
			{
				var physicsGroup = tr.Entity.PhysicsGroup;

				if ( physicsGroup.BodyCount > 1 )
				{
					body = modelEnt.PhysicsBody;
					if ( !body.IsValid() )
						return;
				}

				if ( eyePos.Distance( body.Position ) <= AttachDistance )
				{
					GrabSnd.Stop();
					GrabSnd = PlaySound( "gravgun.loop_grab" );
					GrabStart( modelEnt, body, eyePos + eyeDir * HoldDistance, eyeRot );
				}
				
				if ( !IsBodyGrabbed( body ) )
				{
					physicsGroup.ApplyImpulse( eyeDir * -PullForce, true );
					// PlaySound( "gravgun.empty_grab_fire" );
					// being grab
				}
			}
		}
	}

	private void Activate()
	{
		if ( !holdBody.IsValid() )
		{
			holdBody = new PhysicsBody
			{
				BodyType = PhysicsBodyType.Keyframed
			};
		}
	}

	private void Deactivate()
	{
		GrabEnd();

		holdBody?.Remove();
		holdBody = null;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if ( IsServer )
		{
			Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( IsServer )
		{
			Deactivate();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			Deactivate();
		}
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	private static bool IsBodyGrabbed( PhysicsBody body )
	{
		// There for sure is a better way to deal with this
		if ( All.OfType<PhysGun>().Any( x => x?.HeldBody?.PhysicsGroup == body?.PhysicsGroup ) ) return true;
		if ( All.OfType<GravGun>().Any( x => x?.HeldBody?.PhysicsGroup == body?.PhysicsGroup ) ) return true;

		return false;
	}

	private void GrabStart( ModelEntity entity, PhysicsBody body, Vector3 grabPos, Rotation grabRot )
	{
		if ( !body.IsValid() )
			return;

		if ( body.PhysicsGroup == null )
			return;

		if ( IsBodyGrabbed( body ) )
			return;

		PlaySound( "gravgun.grab_fire" ).SetRandomPitch(0.9f,1.2f);
		GrabEnd();

		HeldBody = body;
		HeldRot = grabRot.Inverse * HeldBody.Rotation;

		holdBody.Position = grabPos;
		holdBody.Rotation = HeldBody.Rotation;

		HeldBody.Wake();
		HeldBody.EnableAutoSleeping = false;

		collisionJoint = PhysicsJoint.Generic
			.From( (Owner as Player).PhysicsBody )
			.To( HeldBody )
			.Create();

		holdJoint = PhysicsJoint.Weld
			.From( holdBody )
			.To( HeldBody, HeldBody.LocalMassCenter )
			.WithLinearSpring( LinearFrequency, LinearDampingRatio, 0.0f )
			.WithAngularSpring( AngularFrequency, AngularDampingRatio, 0.0f )
			.Breakable( HeldBody.Mass * BreakLinearForce, 0 )
			.Create();

		HeldEntity = entity;

		var client = GetClientOwner();
		client?.Pvs.Add( HeldEntity );

		HeldEntity.GlowState = GlowStates.GlowStateOn;
		HeldEntity.GlowDistanceStart = 0;
		HeldEntity.GlowDistanceEnd = 1000;
		HeldEntity.GlowColor = new Color( 0.8f, 0.7f ,0);
		HeldEntity.GlowActive = true;
		
	}

	private void GrabEnd()
	{
		if ( holdJoint.IsValid )
		{
			holdJoint.Remove();
		}

		if ( collisionJoint.IsValid )
		{
			collisionJoint.Remove();
		}

		if ( HeldBody.IsValid() )
		{
			HeldBody.EnableAutoSleeping = true;
		}

		if ( HeldEntity.IsValid() )
		{
			var client = GetClientOwner();
			client?.Pvs.Remove( HeldEntity );

			HeldEntity.GlowActive = false;
			HeldEntity.GlowState = GlowStates.GlowStateOff;
		}

		HeldBody = null;
		HeldRot = Rotation.Identity;
		HeldEntity = null;
	}

	private void GrabMove( Vector3 startPos, Vector3 dir, Rotation rot )
	{
		if ( !HeldBody.IsValid() )
			return;

		holdBody.Position = startPos + dir * HoldDistance;
		holdBody.Rotation = rot * HeldRot;
	}

	public override bool IsUsable( Entity user )
	{
		return Owner == null || HeldBody.IsValid();
	}
}
