using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Players
{
	public class DoctorArchiveGachaPlayer : ModPlayer
	{
		public int PullsSinceLastSixStar;
		public int PullsInCurrentTenPull;
		public bool TenPullHadAtLeastFiveStar;

		public override void Initialize()
		{
			PullsSinceLastSixStar = 0;
			PullsInCurrentTenPull = 0;
			TenPullHadAtLeastFiveStar = false;
		}

		public void BeginTenPull()
		{
			PullsInCurrentTenPull = 0;
			TenPullHadAtLeastFiveStar = false;
		}

		public void RegisterPull(int stars)
		{
			PullsInCurrentTenPull++;

			if (stars >= 6)
				PullsSinceLastSixStar = 0;
			else
				PullsSinceLastSixStar++;

			if (stars >= 5)
				TenPullHadAtLeastFiveStar = true;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["DoctorArchiveGacha.PullsSinceLastSixStar"] = PullsSinceLastSixStar;
		}

		public override void LoadData(TagCompound tag)
		{
			PullsSinceLastSixStar = tag.GetInt("DoctorArchiveGacha.PullsSinceLastSixStar");
			PullsInCurrentTenPull = 0;
			TenPullHadAtLeastFiveStar = false;
		}
	}
}
