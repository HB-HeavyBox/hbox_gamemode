using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label Label;

	public Health()
	{
		Label = Add.Label( "❤️ 100", "value" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		var Health_text = "❤️"+$" {player.Health.CeilToInt()}";
		if(player.Health.CeilToInt() == 0){
			Health_text="☠️";
			Label.SetClass( "dead", true );
		}else if(player.Health.CeilToInt() <= 10){
			Health_text="🖤"+$" {player.Health.CeilToInt()}";
			Label.SetClass( "low_life", true );
		}else if(player.Health.CeilToInt() > 100){
			Health_text="💖"+$" {player.Health.CeilToInt()}";
		}else{
			Label.SetClass( "low_life", false );
			Label.SetClass( "dead", false );
		}

		Label.Text = Health_text;
	}
}
