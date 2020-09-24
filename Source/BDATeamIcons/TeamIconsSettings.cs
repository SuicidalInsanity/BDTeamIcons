namespace BDTeamIcons
{
	public class TeamIconSettings
	{
		public static string settingsConfigURL = "GameData/BDATeamIcons/settings.cfg";

		[SettingsDataField] public static bool TEAMICONS = true;
		[SettingsDataField] public static bool TEAMNAMES = false;
		[SettingsDataField] public static bool VESSELNAMES = true;
		[SettingsDataField] public static bool SCORE = false;
		[SettingsDataField] public static bool HEALTHBAR = false;
		[SettingsDataField] public static bool MISSILES = false;
		[SettingsDataField] public static bool DEBRIS = true;
		[SettingsDataField] public static bool PERSISTANT = true;
		[SettingsDataField] public static bool POINTERS = true;
		[SettingsDataField] public static float ICONSCALE = 1.0f;
	}
}
