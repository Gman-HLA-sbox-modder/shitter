using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{
	public Label Weapon;
	public Label Inventory;

	public Ammo()
	{
		Weapon = Add.Label( "100", "weapon" );
		Inventory = Add.Label( "100", "inventory" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		var weapon = player.ActiveChild as BaseDmWeapon;
		SetClass( "active", weapon != null );

		if ( weapon == null )
		{
			Style.Opacity = 0f;
			return;
		}

		Style.Opacity = 1f;
		Weapon.Text = $"{weapon.AmmoClip} / ";
		Weapon.Style.Opacity = weapon.UseClip ? 1f : 0f;

		var inv = weapon.AvailableAmmo();
		Inventory.Text = inv.ToString();
		Inventory.SetClass( "active", inv >= 0 );
	}
}
