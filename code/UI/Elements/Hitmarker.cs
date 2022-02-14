using Sandbox.UI;
using System.Threading.Tasks;

namespace Instagib.UI
{
	public partial class Hitmarker : Panel
	{
		public static Hitmarker CurrentHitmarker;

		public Hitmarker()
		{
			CurrentHitmarker = this;
			StyleSheet.Load( "/Code/UI/Elements/Hitmarker.scss" );
		}

		public void OnHit() => new HitmarkerInstance( this );

		public class HitmarkerInstance : Panel
		{
			public HitmarkerInstance( Panel parent )
			{
				Parent = parent;
				_ = KillAfterTime();
			}

			async Task KillAfterTime()
			{
				await Task.Delay( 50 );
				Delete();
			}
		}
	}
}
