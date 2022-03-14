using Sandbox;

[Library( "dm_shitsbow", Title = "Shitbow" )]
[Hammer.EditorModel( "weapons/rust_crossbow/rust_crossbow.vmdl" )]
partial class Crossbow : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_crossbow/v_rust_crossbow.vmdl";

	public override float PrimaryRate => 1;
	public override int Bucket => 3;
	public override AmmoType AmmoType => AmmoType.Crossbow;

	[Net]
	public bool Zoomed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = 3;
		SetModel( "weapons/rust_crossbow/rust_crossbow.vmdl" );
	}

	public override void AttackPrimary()
	{
		if ( !TakeAmmo( 0 ) )
		{
			DryFire();
			return;
		}

		ShootEffects();

		PlaySound( "shoot" );

		if ( IsServer )
		using ( Prediction.Off() )
		{
			ShootShit( true );
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		Zoomed = Input.Down( InputButton.Attack2 );
	}

	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		if ( Zoomed )
		{
			camSetup.FieldOfView = 20;
		}
	}

	public override void BuildInput( InputBuilder owner ) 
	{
		if ( Zoomed )
		{
			owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 4.0f, 1.0f, 0.5f );
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	void ShootShit( bool isBig = false )
	{
		var ent = new Poojectile
		{
			Position = Owner.EyePosition + (Owner.EyeRotation.Forward * (isBig ? 70 : 40)),
			Rotation = Owner.EyeRotation,
			Weapon = this
		};

		ent.SetModel( "models/poopemoji/poopemoji.vmdl" );
		ent.Velocity = Owner.EyeRotation.Forward * 1000000;
		ent.Scale = 1f;
	}
}
