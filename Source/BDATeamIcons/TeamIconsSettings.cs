
namespace BDTeamIcons
{
	internal class TeamIconSettings
	{
		public static string settingsConfigURL = "GameData/BDATeamIcons/settings.cfg";

		[SettingsDataField] public static bool TEAMICONS = true;
		[SettingsDataField] public static bool TEAMNAMES = false;
		[SettingsDataField] public static bool VESSELNAMES = true;
		[SettingsDataField] public static bool MISSILES = false;
		[SettingsDataField] public static bool DEBRIS = true;
		[SettingsDataField] public static bool PERSISTANT = true;
		[SettingsDataField] public static float ICONSCALE = 1.0f;
		/*
		[SettingsDataField] public static Color TEAM_1_COLOR = XKCDColors.Red;
		[SettingsDataField] public static Color TEAM_2_COLOR = XKCDColors.Blue;
		[SettingsDataField] public static Color TEAM_3_COLOR = XKCDColors.Green;
		[SettingsDataField] public static Color TEAM_4_COLOR = XKCDColors.Yellow;
		[SettingsDataField] public static Color TEAM_5_COLOR = XKCDColors.Purple;
		[SettingsDataField] public static Color TEAM_6_COLOR = XKCDColors.BloodOrange;
		[SettingsDataField] public static Color TEAM_7_COLOR = XKCDColors.Cyan;
		[SettingsDataField] public static Color TEAM_8_COLOR = XKCDColors.DarkGreen;
		*/
	}
}
