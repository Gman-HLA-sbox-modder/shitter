using Sandbox;
using System;
using System.Linq;

partial class DeathmatchGame : Game
{
	public DeathmatchHud DeathmatchHud;

	public DeathmatchGame()
	{
		if ( !IsClient )
			return;

		DeathmatchHud = new();
		//InitPostProcess();
	}
	/*
	private Material _postProcessMaterial;

	public void InitPostProcess()
	{
		_postProcessMaterial = Material.Load("materials/cum_postprocess.vmat");
	}

	[Event("render.postprocess")]
	public void PostProcess()
	{
		if(_postProcessMaterial != null)
		{
			//Render.CopyFrameBuffer(false);
			//Render.Material = _postProcessMaterial;
			//Render.DrawScreenQuad();
		}
	}*/

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
