﻿using Sandbox;
using Sandbox.UI;

public class ChargeIndicator : Panel
{
	private float value;

	/// <summary>
	/// A float value in range (0, 1) representing the total charge progress
	/// </summary>
	public float Value
	{
		get => value;
		set
		{
			this.value = value;
			Style.Width = width / value;
		}
	}

	private int width = 250; // px

	public static ChargeIndicator Current { get; set; }

	public ChargeIndicator()
	{
		Current = this;
		StyleSheet.Load( "/ui/ChargeIndicator.scss" );
	}

	public override void Tick()
	{
		if (Local.Pawn.IsValid)
		{
			if ( Local.Pawn.Health <= 0 )
			{
				// Player is dead
				Value = 0;
				return;
			}

			if ( (Local.Pawn as Player).ActiveChild is shitthrower pistol )
			{
				Value = pistol.ChargeTime / pistol.TimeSinceChargeStart;
				if ( pistol.TimeSinceChargeStart.Relative.AlmostEqual( 0, 0.1f ) )
				{
					Value = 0;
				}
			}
			else
			{
				// Weapon isn't valid
				Value = 0;
			}
		}

		base.Tick();
	}
}
