using Sandbox;
using System;
using System.Linq;

namespace Instagib
{
	partial class Player
	{
		[ServerCmd]
		public static void DressPlayer()
		{
			var caller = ConsoleSystem.Caller.Pawn as Player;
			Undress( caller );
			Dress( caller, false );
		}

		public static void Undress( ModelEntity entity )
		{
			foreach ( var ent in entity.Children.Where( e => e.Tags.Has( "clothes" ) ) )
			{
				ent.Delete();
			}
		}
		
		public static void Dress( ModelEntity entity, bool legsOnly = false )
		{
			var dressed = entity.Children.Any( e => e.Tags.Has( "clothes" ) );
			if ( dressed )
				Undress( entity );

			var lowerModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/trousers/trousers.jeans.vmdl",
				"models/citizen_clothes/trousers/trousers_tracksuitblue.vmdl",
				"models/citizen_clothes/shoes/shorts.cargo.vmdl",
				"models/citizen_clothes/trousers/trousers.lab.vmdl",
				"models/citizen_clothes/trousers/trousers.smart.vmdl",
				"models/citizen_clothes/trousers/trousers.smarttan.vmdl",
				"models/citizen_clothes/trousers/trousers.police.vmdl",
				"models/citizen_clothes/trousers/smarttrousers/smarttrousers.vmdl"
			} );

			var trousers = CreateClothesEntity( lowerModel, entity );
			
			if ( legsOnly )
				return;
				
			var upperModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/jacket/labcoat.vmdl", 
				"models/citizen_clothes/jacket/jacket.red.vmdl",
				"models/citizen_clothes/gloves/gloves_workgloves.vmdl",
				"models/citizen_clothes/shirt/shirt_longsleeve.plain.vmdl",
				"models/citizen_clothes/jacket/suitjacket/suitjacket.vmdl",
				"models/citizen_clothes/jacket/suitjacket/suitjacketunbuttonedshirt.vmdl"
			} );

			var jacket = CreateClothesEntity( upperModel, entity );
			var shoes = CreateClothesEntity( "models/citizen_clothes/shoes/shoes.workboots.vmdl", entity );
			
			var hatModel = Rand.FromArray( new[]
			{
				"models/citizen_clothes/hat/hat_hardhat.vmdl", 
				"models/citizen_clothes/hat/hat_woolly.vmdl",
				"models/citizen_clothes/hat/hat_securityhelmet.vmdl",
				"models/citizen_clothes/hair/hair_malestyle02.vmdl",
				"models/citizen_clothes/hair/hair_femalebun.black.vmdl",
				"models/citizen_clothes/hat/hat.tophat.vmdl",
				"models/citizen_clothes/hat/hat_beret.red.vmdl",
				"models/citizen_clothes/hat/hat_cap.vmdl",
				"models/citizen_clothes/hat/hat_leathercap.vmdl",
				"models/citizen_clothes/hat/hat_woollybobble.vmdl"
			} );

			var hat = CreateClothesEntity( hatModel, entity );
			
			// foreach ( var ent in entity.Children.Where( e => e.Tags.Has( "clothes" ) ) )
			// {
			// 	Log.Info( ( ent as ModelEntity ).GetModelName() );
			// }
		}

		private static ModelEntity CreateClothesEntity( string clothesModel, Entity entity )
		{
			var clothesEntity = new ModelEntity();
			clothesEntity.SetModel( clothesModel );
			clothesEntity.SetParent( entity, true );
			
			// Overrides - match parent entity
			clothesEntity.EnableShadowCasting = entity.EnableShadowCasting;
			clothesEntity.EnableShadowInFirstPerson = entity.EnableShadowInFirstPerson;
			clothesEntity.EnableHideInFirstPerson = entity.EnableHideInFirstPerson;
			clothesEntity.EnableViewmodelRendering = entity.EnableViewmodelRendering;
			clothesEntity.EnableShadowOnly = entity.EnableShadowOnly;
			
			clothesEntity.Tags.Add( "clothes" );

			return clothesEntity;
		}
	}
}
