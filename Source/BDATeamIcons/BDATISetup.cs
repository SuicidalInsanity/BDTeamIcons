using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;
using BDArmory.Modules;
using BDArmory.Misc;
using BDArmory.UI;
/*
* Milestone 1: Toolbar icon + GUI for toggling Team Icons - Done
* Milestone 2: Unique icons for different vessel types - link into KSP VESSELTYPE - Done
* Milestone 3: User selected team colors - have the chooser, need to figure out how to get selected colors to save to settings - Done
* Milestone 4: getting colorized icons working - will likely have to make each icon a gameobject, then have it load the icon texture as a material and tint the material - Done
* MileStone 5: more than 2 team support. -Done
* *Milestone 6: Figure out how to have TI activation toggle the F4 SHOW_LABELS (or is it Flt_Show_labels?) method to sim a keypress?
*/
namespace BDTeamIcons
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	class BDATISetup : MonoBehaviour
	{
		private ApplicationLauncherButton toolbarButton = null;
		public static Rect WindowRectGUI;

		private string windowTitle = "BDArmory Team UI Icons";
		public static BDATISetup Instance = null;
		public static GUISkin BDGuiSkin = HighLogic.Skin;
		private bool showTeamIconGUI = false;
		float toolWindowWidth = 250;
		float toolWindowHeight = 150;
		float teamWindowHeight = 25;
		public int selectedTeam;
		public bool UpdateTeamColor = false;
		private float updateList = 0;
		private bool maySavethisInstance = false;

		public SortedList<string, List<MissileFire>> weaponManagers = new SortedList<string, List<MissileFire>>();

		public static string textureDir = "BDATeamIcons/Icons/";
	
		private Texture2D dit;
		public Texture2D TextureIconDebris
		{
			get { return dit ? dit : dit = GameDatabase.Instance.GetTexture(textureDir + "debrisIcon", false); }
		}
		private Texture2D mit;
		public Texture2D TextureIconMissile
		{
			get { return mit ? mit : mit = GameDatabase.Instance.GetTexture(textureDir + "missileIcon", false); }
		}

		private Texture2D ti7;
		public Texture2D TextureIconGeneric
		{
			get { return ti7 ? ti7 : ti7 = GameDatabase.Instance.GetTexture(textureDir + "Icon_Generic", false); }
		}
		private Texture2D ti1A;
		public Texture2D TextureIconShip
		{
			get { return ti1A ? ti1A : ti1A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Ship", false); }
		}
		private Texture2D ti2A;
		public Texture2D TextureIconPlane
		{
			get { return ti2A ? ti2A : ti2A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Plane", false); }
		}
		private Texture2D ti3A;
		public Texture2D TextureIconRover
		{
			get { return ti3A ? ti3A : ti3A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Rover", false); }
		}
		private Texture2D ti4A;
		public Texture2D TextureIconBase
		{
			get { return ti4A ? ti4A : ti4A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Base", false); }
		}
		private Texture2D ti5A;
		public Texture2D TextureIconProbe
		{
			get { return ti5A ? ti5A : ti5A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Probe", false); }
		}
		private Texture2D ti6A;
		public Texture2D TextureIconSub
		{
			get { return ti6A ? ti6A : ti6A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Sub", false); }
		}

		void Start()
		{
			Instance = this;
			if (HighLogic.LoadedSceneIsFlight)
				maySavethisInstance = true;
			if (ConfigNode.Load(TeamIconSettings.settingsConfigURL) == null)
			{
				var node = new ConfigNode();
				node.AddNode("IconSettings");
				node.Save(TeamIconSettings.settingsConfigURL);
			}
			AddToolbarButton();
			LoadConfig();
			UpdateList();
		}

		private void MissileFireOnToggleTeam(MissileFire wm, BDTeam team)
		{
			if (TeamIconSettings.TEAMICONS)
			{
				UpdateList();
			}
		}
		private void VesselEventUpdate(Vessel v)
		{
			if (TeamIconSettings.TEAMICONS)
			{
				UpdateList();
			}
		}
		private void Update()
		{
			if (TeamIconSettings.TEAMICONS)
			{
				updateList -= Time.fixedDeltaTime;
				if (updateList < 0)
				{
					UpdateList();
					updateList = 0.5f; // check team lists less often than every frame
				}
			}
		}
		private void UpdateList()
		{
			weaponManagers.Clear();

			using (List<Vessel>.Enumerator v = FlightGlobals.Vessels.GetEnumerator())
				while (v.MoveNext())
				{
					if (v.Current == null || !v.Current.loaded || v.Current.packed)
						continue;
					using (var wms = v.Current.FindPartModulesImplementing<MissileFire>().GetEnumerator())
						while (wms.MoveNext())
							if (wms.Current != null)
							{
								if (weaponManagers.TryGetValue(wms.Current.Team.Name, out var teamManagers))
									teamManagers.Add(wms.Current);
								else
									weaponManagers.Add(wms.Current.Team.Name, new List<MissileFire> { wms.Current });
								break;
							}
				}
		}

		private void OnDestroy()
		{
			if (toolbarButton)
			{
				ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
				toolbarButton = null;
			}
			if (maySavethisInstance)
			{
				SaveConfig();
			}
		}

		IEnumerator ToolbarButtonRoutine()
		{
			if (toolbarButton || (!HighLogic.LoadedSceneIsEditor)) yield break;
			while (!ApplicationLauncher.Ready)
			{
				yield return null;
			}
			AddToolbarButton();
		}

		void AddToolbarButton()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (toolbarButton == null)
				{
					Texture buttonTexture = GameDatabase.Instance.GetTexture("BDATeamIcons/Icons/icon", false);
					toolbarButton = ApplicationLauncher.Instance.AddModApplication(ShowToolbarGUI, HideToolbarGUI, null, null, null, null, ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
				}
			}
		}

		public void ShowToolbarGUI()
		{
			showTeamIconGUI = true;
			LoadConfig();
		}

		public void HideToolbarGUI()
		{
			showTeamIconGUI = false;
			SaveConfig();
		}

		public static void LoadConfig()
		{
			try
			{
				Debug.Log("[BDTeamIcons]=== Loading settings.cfg ===");

				SettingsDataField.Load();
			}
			catch (NullReferenceException)
			{
				Debug.Log("[BDTeamIcons]=== Failed to load settings config ===");
			}
		}

		public static void SaveConfig()
		{
			try
			{
				Debug.Log("[BDTeamIcons] == Saving settings.cfg ==	");
				SettingsDataField.Save();
			}
			catch (NullReferenceException)
			{
				Debug.Log("[BDTeamIcons]: === Failed to save settings.cfg ====");
			}
		}
		
		GUIStyle title;

		void OnGUI()
		{
			if (showTeamIconGUI)
			{
				if (HighLogic.LoadedSceneIsFlight)
				{
					maySavethisInstance = true;
				}
				WindowRectGUI = new Rect(Screen.width - toolWindowWidth - 40, 150, toolWindowWidth, toolWindowHeight);
				WindowRectGUI = GUI.Window(this.GetInstanceID(), WindowRectGUI, TeamIconGUI, windowTitle, BDGuiSkin.window);
			}
			title = new GUIStyle(GUI.skin.label);
			title.fontSize = 30;
			title.alignment = TextAnchor.MiddleLeft;
			title.wordWrap = false;
		}
		public bool showTeamIconSelect = false;
		public bool showColorSelect = false;
		public string teamname;

		void TeamIconGUI(int windowID)
		{
			int line = 0;
			int i = 0;
			TeamIconSettings.TEAMICONS = GUI.Toggle(new Rect(5, 25, 200, 20), TeamIconSettings.TEAMICONS, "Enable Team Icons", BDGuiSkin.toggle);
			if (TeamIconSettings.TEAMICONS)
			{
				Rect IconOptionsGroup = new Rect(15, 55, toolWindowWidth - 20, 200);
				GUI.BeginGroup(IconOptionsGroup, GUIContent.none, BDGuiSkin.box);
				TeamIconSettings.TEAMNAMES = GUI.Toggle(new Rect(15, 0, toolWindowWidth - 20, 20), TeamIconSettings.TEAMNAMES, "Enable Team Labels", BDGuiSkin.toggle);
				TeamIconSettings.VESSELNAMES = GUI.Toggle(new Rect(15, 25, toolWindowWidth - 20, 20), TeamIconSettings.VESSELNAMES, "Enable Vessel Labels", BDGuiSkin.toggle);
				TeamIconSettings.MISSILES = GUI.Toggle(new Rect(15, 50, toolWindowWidth - 20, 20), TeamIconSettings.MISSILES, "Missile Icons", BDGuiSkin.toggle);
				TeamIconSettings.DEBRIS = GUI.Toggle(new Rect(15, 75, toolWindowWidth - 20, 20), TeamIconSettings.DEBRIS, "Debris Icons", BDGuiSkin.toggle);
				TeamIconSettings.PERSISTANT = GUI.Toggle(new Rect(15, 100, toolWindowWidth - 20, 20), TeamIconSettings.PERSISTANT, "Do not hide with UI", BDGuiSkin.toggle);
				TeamIconSettings.POINTERS = GUI.Toggle(new Rect(15, 130, toolWindowWidth - 20, 20), TeamIconSettings.POINTERS, "Offscreen Icon Pointers", BDGuiSkin.toggle);

				GUI.Label(new Rect(75, 150, toolWindowWidth - 20, 20), $"Icon scale: {(TeamIconSettings.ICONSCALE * 100f).ToString("0")}" + "%");
				TeamIconSettings.ICONSCALE = GUI.HorizontalSlider(new Rect(20, 175, toolWindowWidth - 20, 20), TeamIconSettings.ICONSCALE, 0.25f, 2f);
				GUI.EndGroup();
				line = 8;

				Rect TeamColorsGroup = new Rect(15, 265, toolWindowWidth - 20, teamWindowHeight);
				GUI.BeginGroup(TeamColorsGroup, GUIContent.none, BDGuiSkin.box);
				using (var teamManagers = weaponManagers.GetEnumerator())
					while (teamManagers.MoveNext())
					{
						i++;
						Rect buttonRect = new Rect(30, -20 + (i * 25), 190, 20);
						GUIStyle vButtonStyle = showColorSelect ? BDGuiSkin.box : BDGuiSkin.button;
						if (GUI.Button(buttonRect, $"{teamManagers.Current.Key}", vButtonStyle))
						{
							teamname = teamManagers.Current.Key;
							showColorSelect = !showColorSelect;
							LoadConfig();
							selectedTeam = i;
						}
						if (i == 1)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_1_COLOR);
						}
						else if (i == 2)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_2_COLOR);
						}
						else if (i == 3)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_3_COLOR);
						}
						else if (i == 4)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_4_COLOR);
						}
						else if (i == 5)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_5_COLOR);
						}
						else if (i == 6)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_6_COLOR);
						}
						else if (i == 7)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_7_COLOR);
						}
						else if (i == 8)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_8_COLOR);
						}
						else if (i == 9)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_9_COLOR);
						}
						else if (i == 10)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_10_COLOR);
						}
						else if (i == 11)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_11_COLOR);
						}
						else if (i == 12)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_12_COLOR);
						}
						else if (i == 13)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_13_COLOR);
						}
						else if (i == 14)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_14_COLOR);
						}
						else if (i == 15)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_15_COLOR);
						}
						else if (i == 16)
						{
							title.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_16_COLOR);
						}
						GUI.Label(new Rect(5, -20 + (i * 25), 25, 25), "*", title);
					}
				teamWindowHeight = Mathf.Lerp(teamWindowHeight, (i * 25)+5, 1);
				GUI.EndGroup();			
			}
			else
			{
				line = 0;
			}
			toolWindowHeight = Mathf.Lerp(toolWindowHeight, (50 + (line * 25)+(i*25)+5) + 15, 1);
			WindowRectGUI.height = toolWindowHeight;
		}
	}
}
