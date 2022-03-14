using Sandbox;
using System;

class Poojectile : Prop
{
	public BaseDmWeapon Weapon { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		PlaySound( "projectile" );
	}

	protected override void OnPhysicsCollision(CollisionEventData eventData)
	{
		var propData = GetModelPropData();

		var minImpactSpeed = propData.MinImpactDamageSpeed;
		if (minImpactSpeed <= 0.0f) minImpactSpeed = 500;

		var impactDmg = propData.ImpactDamage;
		if (impactDmg <= 0.0f) impactDmg = 10;

		var speed = eventData.Speed;

		if ( !(speed > minImpactSpeed) )
			return;

		// I take damage from high speed impacts
		if (Health > 0)
		{
			var damage = 69420f;//speed / minImpactSpeed * impactDmg;
			TakeDamage(DamageInfo.Generic(damage).WithFlag(DamageFlags.PhysicsImpact)); //poojectile takes damage
		}

		// Whatever I hit takes more damage
		if ( !eventData.Entity.IsValid() || eventData.Entity == this )
			return;

		if(IsServer) {
			//var damage = speed / minImpactSpeed * DamageMultiplier; // * impactDmg;// * 1.2f;
			eventData.Entity.TakeDamage(DamageInfo.Generic(Weapon.Damage/2)
				.WithFlag(DamageFlags.PhysicsImpact)
				.WithWeapon(Weapon)
				.WithAttacker(Weapon.Owner)
				.WithPosition(eventData.Position)
				.WithForce(eventData.PreVelocity));
		}
	}
}

