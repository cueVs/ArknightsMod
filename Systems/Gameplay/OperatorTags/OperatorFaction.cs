using System;

namespace ArknightsMod.Systems.Gameplay.OperatorTags
{
	[Flags]
	public enum OperatorFaction
	{
		None = 0,
		Sarkaz = 1 << 0,
		Kazimierz = 1 << 1,
		AveMujica = 1 << 2,
		AbyssalHunter = 1 << 3,
		Laterano = 1 << 4,
		RhodesIsland = 1 << 5,
		Reunion = 1 << 6,
		Ursus = 1 << 7,
		Seaborn = 1 << 8
	}
}
