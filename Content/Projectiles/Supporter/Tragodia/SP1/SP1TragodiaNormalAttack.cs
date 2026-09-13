using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1
{
	public class SP1TragodiaNormalAttack : TragodiaNormalAttack
	{
	
		protected override float DamageMultiplier => 1.5f;
		protected override float ImpairmentInnerRatio => 0.54f; 
		protected override float ImpairmentOuterRatio => 0.27f;   
	}
}