using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Instagib.UI
{
	public class WinnerScreen : Panel
	{
		List<AnimSceneObject> objects;

		public WinnerScreen()
		{
			StyleSheet.Load( "/Code/UI/WinnerScreen.scss" );

			Add.Label( "Alex Wins!", "title" );

			var world = new SceneWorld();
			var pos = Vector3.Forward * 90;
			var dir = 0 - pos;
			var scene = Add.ScenePanel( world, pos, Rotation.LookAt( dir, Vector3.Up ), 80f, "scene" );

			using ( SceneWorld.SetCurrent( world ) )
			{
				var baseOffset = Vector3.Down * 32;
				objects = new();
				objects.Add( new AnimSceneObject( Model.Load( "models/maya_testcube_100.vmdl" ), new Transform(
					baseOffset + new Vector3( -20, 0, -80 ),
					Rotation.Identity,
					0.6f ) ) );

				objects.Add( new AnimSceneObject( Model.Load( "models/maya_testcube_100.vmdl" ), new Transform(
					baseOffset + new Vector3( 0, 0, -20 ),
					Rotation.Identity,
					0.2f ) ) );

				objects.Add( new AnimSceneObject( Model.Load( "models/maya_testcube_100.vmdl" ), new Transform(
					baseOffset + new Vector3( 0, -20, -25 ),
					Rotation.Identity,
					0.2f ) ) );

				objects.Add( new AnimSceneObject( Model.Load( "models/maya_testcube_100.vmdl" ), new Transform(
					baseOffset + new Vector3( 0, 20, -30 ),
					Rotation.Identity,
					0.2f ) ) );


				objects.Add( new AnimSceneObject( Model.Load( "models/citizen/citizen.vmdl" ), new Transform( 
					baseOffset, Rotation.Identity, 0.75f ) ) );

				objects.Add( new AnimSceneObject( Model.Load( "models/citizen/citizen.vmdl" ), new Transform(
					baseOffset + new Vector3( 0, -20, -5 ), Rotation.Identity, 0.75f ) ) );

				objects.Add( new AnimSceneObject( Model.Load( "models/citizen/citizen.vmdl" ), new Transform(
					baseOffset + new Vector3( 0, 20, -10 ), Rotation.Identity, 0.75f ) ) );

				Light.Point( new Vector3( 32, 0, 64 ), 256, Color.White * 10.0f );
			}

			scene.AmbientColor = Color.White * 0.25f;
		}

		public override void Tick()
		{
			foreach ( var obj in objects )
			{
				obj.Update( Time.Delta );
				obj.SetAnimVector( "aim_eyes", Vector3.Forward * 90 - (obj.Position + Vector3.Up * 48f) );
			}
		}
	}
}
