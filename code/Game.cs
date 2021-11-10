using Sandbox;
using System;
using System.Linq;

[Library( "shitter", Title = "Shitter" )]
partial class DeathmatchGame : Game
{
	public DeathmatchHud DeathmatchHud;

	public DeathmatchGame()
	{
		if ( IsClient ) { DeathmatchHud = new(); }
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		ItemRespawn.Init();
	}

	public override void ClientJoined( Client cl )
	{
		var player = new DeathmatchPlayer();
		cl.Pawn = player;
		player.Respawn();

		base.ClientJoined( cl );
	}
}
