using Sandbox;
using System;
using System.Linq;

namespace Instagib
{
	partial class Player
	{
		private ModelEntity trousers, jacket, shoes, hat;
		private bool dressed = false;
		
		public void Dress()
		{
			if ( dressed ) 
				return;

			var lowerModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/trousers/trousers.jeans.vmdl",
				"models/citizen_clothes/dress/dress.kneelength.vmdl",
				"models/citizen/clothes/trousers_tracksuit.vmdl",
				"models/citizen_clothes/shoes/shorts.cargo.vmdl",
				"models/citizen_clothes/trousers/trousers.lab.vmdl"
			} );

			trousers = CreateClothesEntity( lowerModel );
				
			var upperModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/jacket/labcoat.vmdl", 
				"models/citizen_clothes/jacket/jacket.red.vmdl",
				"models/citizen_clothes/gloves/gloves_workgloves.vmdl"
			} );

			jacket = CreateClothesEntity( upperModel );
			
			shoes = CreateClothesEntity( "models/citizen_clothes/shoes/shoes.workboots.vmdl" );
			
			var hatModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/hat/hat_hardhat.vmdl", 
				"models/citizen_clothes/hat/hat_woolly.vmdl",
				"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
				"models/citizen_clothes/hair/hair_malestyle02.vmdl",
				"models/citizen_clothes/hair/hair_femalebun.black.vmdl"
			} );

			hat = CreateClothesEntity( hatModel );
			
			dressed = true;
		}

		private ModelEntity CreateClothesEntity( string clothesModel )
		{
			var clothesEntity = new ModelEntity();
			clothesEntity.SetModel( clothesModel );
			clothesEntity.SetParent( this, true );
			clothesEntity.EnableShadowInFirstPerson = true;
			clothesEntity.EnableHideInFirstPerson = true;

			return clothesEntity;
		}
	}
}
