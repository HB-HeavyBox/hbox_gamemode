﻿namespace Sandbox.Tools
{
	[Library( "tool_balloon", Title = "Balloons", Description = "Create Balloons!", Group = "construction" )]
	public partial class BalloonTool : BaseTool
	{
		[Net]
		public string Mdl { get; set; }
		public Color32 Tint { get; set; }

		PreviewEntity previewModel;

		public override void Activate()
		{
			base.Activate();

			if ( Host.IsServer )
			{
				Tint = Color.Random.ToColor32();
				Mdl = Rand.FromArray( new[]
					{
						"models/citizen_props/balloontall01.vmdl",
						"models/citizen_props/balloonregular01.vmdl",
						"models/citizen_props/balloonheart01.vmdl",
						"models/citizen_props/balloonears01.vmdl"
					}
				);
			}
		}

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is BalloonEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Mdl ) )
			{
				previewModel.RelativeToNormal = false;
			}
		}

		public override void Simulate()
		{
			if ( previewModel.IsValid() )
			{
				previewModel.RenderColor = Tint;
				previewModel.SetModel(Mdl);
			}

			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				bool useRope = Input.Pressed( InputButton.Attack1 );
				if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity is BalloonEntity )
					return;

				var ent = new BalloonEntity
				{
					Position = tr.EndPos,
				};

				ent.SetModel( Mdl );
				ent.PhysicsBody.GravityScale = -0.2f;
				ent.RenderColor = Tint;

				Tint = Color.Random.ToColor32();
				Mdl = Rand.FromArray( new[]
					{
						"models/citizen_props/balloontall01.vmdl",
						"models/citizen_props/balloonregular01.vmdl",
						"models/citizen_props/balloonheart01.vmdl",
						"models/citizen_props/balloonears01.vmdl"
					}
				);

				if ( !useRope )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetEntity( 0, ent );

				var attachEnt = tr.Body.IsValid() ? tr.Body.Entity : tr.Entity;
				var attachLocalPos = tr.Body.Transform.PointToLocal( tr.EndPos ) * (1.0f / tr.Entity.Scale);

				if ( attachEnt.IsWorld )
				{
					rope.SetPosition( 1, attachLocalPos );
				}
				else
				{
					rope.SetEntityBone( 1, attachEnt, tr.Bone, new Transform( attachLocalPos ) );
				}
				var spring = PhysicsJoint.Spring
			
					.From( ent.PhysicsBody )
					.To( tr.Body, tr.Body.Transform.PointToLocal( tr.EndPos ) )
					.WithFrequency( 5.0f )
					.WithDampingRatio( 0.7f )
					.WithReferenceMass( 0 )
					.WithMinRestLength( 0 )
					.WithMaxRestLength( 100 )
					.WithCollisionsEnabled()
					.Create();
					
				spring.EnableAngularConstraint = false;
				spring.OnBreak( () =>
				{
					rope?.Destroy( true );
					spring.Remove();
				} );
			}
		}
	}
}