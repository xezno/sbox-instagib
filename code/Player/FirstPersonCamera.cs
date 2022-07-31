namespace Instagib;

public class FirstPersonCamera : Sandbox.FirstPersonCamera
{
	public override void Update()
	{
		base.Update();

		ZNear = 10;
		ZFar = 10000;
	}
}
