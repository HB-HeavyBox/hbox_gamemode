using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using System;

[Library]
public partial class SpawnList : Panel
{
	VirtualScrollPanel Canvas;

	public SpawnList()
	{
		AddClass( "spawnpage" );
		AddChild( out Canvas, "canvas" );
		int i = 0;

		string[] name = new string[]{
						"Arrow",
						"Ball",
						"Citizen",
						"Balloon Ears",
						"Balloon Heart",
						"Balloon Regular",
						"Balloon Tall",
						"Bathroom Sink",
						"Beach Ball",
						"Beer Tankard",
						"Broom",
						"Cardboard Box",
						"Chair 1",
						"Chair 2",
						"Chair 3",
						"Chair 4",
						"Chair 5",
						"Chips Packet",
						"Coffee Mug",
						"Coin",
						"Road Diviser",
						"Crate",
						"Crowbar",
						"Foam Hand",
						"Grit Bin",
						"Hotdog",
						"Icecream Cone",
						"Newspaper",
						"Old oven",
						"Recycling Bin",
						"Road Cone",
						"Can of Soda",
						"Trash Bag 1",
						"Trash Can 1",
						"Trash Can 2",
						"Wheel 1",
						"Wheel 2",
						"Wine Glass 1",
						"Wine Glass 2",
						"Gaming Tower",
						"Container",
						"Desk",
						"Gaming Keyboard",
						"LCD Monitor",
						"Old Monitor",
						"Gaming Mouse",
						"Radio",
						"Graphic Card",
						"Seat",
						"Light",
						"Light Arrow",
						"Cube",
						"Room",
						"Fuel Barrel",
						"Plastic Bench",
						"Old Bench",
						"Can",
						"Cup",
						"Food Box",
						"Street Bin",
						"Gas Cylinder Fat",
						"Gas Cylinder Tall",
						"Bollard",
						"Post Box",
						"Railling",
						"Star Bin",
						"Ticket Dispenser",
						"Ticket Machine",
						"WaterMelon",
						"Thruster Projector",
						"Torch"
					};

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemSize = new Vector2( 100, 100 );
		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var file = (string)data;
			var fileTitle = file;
			var btn = cell.Add.Button( name[i] );
			btn.AddClass( "icon" );
			btn.AddEventListener( "onclick", () => ConsoleSystem.Run( "spawn", "models/" + file ) );
			btn.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"/models/{file}_c.png", false )
			};
			i++;
		};
		

		var props = FileSystem.Mounted.FindFile( "models", "*.vmdl_c.png", true );

		foreach ( var file in props )
		{
			Log.Info(file);
			Canvas.AddItem( file.Remove( file.Length - 6 ) );
		}
	}
}
