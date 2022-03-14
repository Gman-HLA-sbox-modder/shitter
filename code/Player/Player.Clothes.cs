﻿using Sandbox;
using System;
using System.Linq;

public class ClothingEntity : ModelEntity { }

partial class DeathmatchPlayer
{
	ModelEntity pants;
	ModelEntity jacket;
	ModelEntity shoes;
	ModelEntity hat;

	bool dressed = false;

	/// <summary>
	/// Bit of a hack to putr random clothes on the player
	/// </summary>
	public void Dress()
	{
		if ( dressed ) return;
		dressed = true;

		if ( Rand.Int( 0, 3 ) != 1 )
		{
			var model = Rand.FromArray( new[]
			{
				"models/citizen_clothes/trousers/trousers.jeans.vmdl",
				"models/citizen_clothes/dress/dress.kneelength.vmdl",
				"models/citizen_clothes/trousers/trousers_tracksuit.vmdl",
				"models/citizen_clothes/shoes/shorts.cargo.vmdl",
				"models/citizen_clothes/trousers/trousers.lab.vmdl"
			} );

			pants = new ClothingEntity();
			pants.SetModel( model );
			pants.SetParent( this, true );
			pants.EnableShadowInFirstPerson = true;
			pants.EnableHideInFirstPerson = true;
			pants.Tags.Add("clothes");

			if ( model.Contains( "dress" ) )
				jacket = pants;
		}

		if ( Rand.Int( 0, 3 ) != 1 && jacket == null )
		{
			var model = Rand.FromArray( new[]
			{
				"models/citizen_clothes/jacket/labcoat.vmdl",
				"models/citizen_clothes/jacket/jacket.red.vmdl",
				"models/citizen_clothes/gloves/gloves_workgloves.vmdl"
			} );

			jacket = new ClothingEntity();
			jacket.SetModel( model );
			jacket.SetParent( this, true );
			jacket.EnableShadowInFirstPerson = true;
			jacket.EnableHideInFirstPerson = true;
			jacket.Tags.Add("clothes");
		}

		if ( Rand.Int( 0, 3 ) != 1 )
		{
			shoes = new ClothingEntity();
			shoes.SetModel( "models/citizen_clothes/shoes/shoes.workboots.vmdl" );
			shoes.SetParent( this, true );
			shoes.EnableShadowInFirstPerson = true;
			shoes.EnableHideInFirstPerson = true;
			shoes.Tags.Add("clothes");
		}

		if ( !dressed )
			return;

		hat = new ClothingEntity();
		hat.SetModel( "models/poopemoji/poopemoji_hat.vmdl" );
		hat.SetParent( this, true );
		hat.EnableShadowInFirstPerson = true;
		hat.EnableHideInFirstPerson = true;
		hat.Tags.Add("clothes");
	}
}
