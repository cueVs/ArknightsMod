
public enum ElementalType: byte{
	Nervous = 1 << 0,
	Corrosion = 1 << 1,
	Burn = 1 << 2,
	Necrosis = 1 << 3,
}


public enum EntityType : byte{
	Player = 0,
	NPC = 1,
	Enemy = 2,
	Boss = 3,

}

public enum EntityMaxElemental : ushort {
	Player = 2000,
	NPC = 2000,
	Enemy = 2000,
	Boss = 2000
}