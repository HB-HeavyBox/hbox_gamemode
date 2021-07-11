using Sandbox;

public partial class Tool
{
	[ClientRpc]
	public void CreateHitEffects( Vector3 hitPos )
	{
		var particle = Particles.Create( "particles/tool_hit.vpcf", hitPos );
		particle.SetPosition( 0, hitPos );
		particle.Destroy( false );

		PlaySound( "tool.fire" ).SetRandomPitch(0.9f,1.2f);
	}
}
