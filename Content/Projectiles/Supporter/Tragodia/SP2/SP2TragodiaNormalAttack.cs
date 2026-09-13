using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class SP2TragodiaNormalAttack : TragodiaNormalAttack
	{
		protected override float DamageRadius => 80f;
		protected override float DamageMultiplier => 1.5f;
		protected override float ImpairmentInnerRadius => 80f;
		protected override float ImpairmentOuterRadius => 150f;
	}
}