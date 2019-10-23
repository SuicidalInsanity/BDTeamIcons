namespace BDTeamIcons
{
	public class TeamIconSettings
	{
		public static string settingsConfigURL = "GameData/BDATeamIcons/settings.cfg";

		[SettingsDataField] public static bool TEAMICONS = true;
		[SettingsDataField] public static bool TEAMNAMES = false;
		[SettingsDataField] public static bool VESSELNAMES = true;
		[SettingsDataField] public static bool MISSILES = false;
		[SettingsDataField] public static bool DEBRIS = true;
		[SettingsDataField] public static bool PERSISTANT = true;
		[SettingsDataField] public static bool POINTERS = true;
		[SettingsDataField] public static float ICONSCALE = 1.0f;

		[SettingsDataField] public static string TEAM_1_COLOR = "255,0,0,200";
		[SettingsDataField] public static string TEAM_2_COLOR = "0,0,255,200";
		[SettingsDataField] public static string TEAM_3_COLOR = "0,255,0,200";
		[SettingsDataField] public static string TEAM_4_COLOR = "255,255,0,200";
		[SettingsDataField] public static string TEAM_5_COLOR = "255,192,0,200";
		[SettingsDataField] public static string TEAM_6_COLOR = "255,0,255,200";
		[SettingsDataField] public static string TEAM_7_COLOR = "0,255,255,200";
		[SettingsDataField] public static string TEAM_8_COLOR = "0,128,0,200";

	}
}
