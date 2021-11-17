using Sandbox;

[Library( "Coom", Title = "Coomer" )]
[Hammer.EditorModel( "weapons/rust_pistol/rust_pistol.vmdl" )]
partial class CoomLauncher : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 15.0f;
	public override AmmoType AmmoType => AmmoType.Cum;
	public override int ClipSize => 100;
	public override float ReloadTime => 3.0f;

	Sound? currentSound;
	Vector3 PooPos;

	public override int Bucket => 1;

	private AnimEntity player => Owner as AnimEntity;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
		AmmoClip = 100;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.Attack1 );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = -0.5f;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		// Tell the clients to play the shoot effects
		ShootEffects();
		PlaySound( "shoot" );
		player.SetAnimBool("b_attack", true);

		// Shoot the bullets
		if (IsClient) return;
		ShootShit();
	}

	public override void Simulate( Client owner )
	{
		base.Simulate( owner );

		if ( !IsReloading )
			return;

		// Reset timers so that people don't mess with them while reloading
		TimeSincePrimaryAttack = 0;
	}

	void ShootShit()
	{
		var ent = new Poojectile
		{
			Position = Owner.EyePos + (Owner.EyeRot.Forward * 40),
			Rotation = Owner.EyeRot,
			Weapon = this,
			DamageMultiplier = 1.3f
		};

		ent.SetModel("models/cum.vmdl");
		ent.Velocity = Owner.EyeRot.Forward * 10000;
	}
}
