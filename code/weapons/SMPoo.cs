using Sandbox;
using System;

[Library( "shitter_smpoo", Title = "SMPoo" )]
[Hammer.EditorModel( "weapons/rust_smg/rust_smg.vmdl" )]
partial class SMPoo : BaseDmWeapon
{ 
	public override string ViewModelPath => "weapons/rust_smg/v_rust_smg.vmdl";

	public override float PrimaryRate => 5.0f;
	public override float SecondaryRate => 1.0f;
	public override AmmoType AmmoType => AmmoType.Shit;
	public override int ClipSize => 100;
	public override float ReloadTime => 4.0f;
	public override int Bucket => 2;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_smg/rust_smg.vmdl" );
		AmmoClip = 100;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity)?.SetAnimParameter( "b_attack", true );
		
		// Tell the clients to play the shoot effects
		ShootEffects();
		PlaySound( "shoot" );
		
		// Shoot the bullets
		if (IsClient) return;
		ShootShit(true);
	}

	public override void AttackSecondary()
	{
		// Grenade lob
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		if ( Owner == Local.Pawn )
		{
			new Sandbox.ScreenShake.Perlin(0.5f, 4.0f, 1.0f, 0.5f);
		}

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 2 ); // TODO this is shit
	}

	void ShootShit(bool isBig = false)
	{
		var ent = new Poojectile
		{
			Position = Owner.EyePosition + (Owner.EyeRotation.Forward * (isBig ? 70 : 40)),
			Rotation = Owner.EyeRotation,
			Weapon = this
		};

		ent.SetModel("models/poopemoji/poopemoji_small.vmdl");
		ent.Velocity = Owner.EyeRotation.Forward * 10000;
		ent.Scale = .2f;
	}
}
