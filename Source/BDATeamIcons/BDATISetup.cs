using System;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;
using BDArmory.Modules;

/*
 * Milestone 1: Toolbar icon + GUI for toggling Team Icons - Done
 * Milestone 2: Unique icons for different vessel types - link into KSP VESSELTYPE - Done
 * Milestone 3: User selected team colors - have the chooser, need to figure out how to get selected colors to save to settings
 * Milestone 4: getting colorized icons working - will likely have to make each icon a gameobject, then have it load the icon texture as a material and tint the material
 *	
 * 
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
		public int selectedTeam;
		public bool UpdateTeamColor = false;
		//private float updateList = 0;
		private bool maySavethisInstance = false;

		private SortedList<string, List<MissileFire>> weaponManagers = new SortedList<string, List<MissileFire>>();

		public static string textureDir = "BDATeamIcons/Icons/";
		
		private Texture2D ti1A;
		public Texture2D TextureIconShipA
		{
			get { return ti1A ? ti1A : ti1A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Ship_A", false); }
		}
		private Texture2D ti2A;
		public Texture2D TextureIconPlaneA
		{
			get { return ti2A ? ti2A : ti2A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Plane_A", false); }
		}
		private Texture2D ti3A;
		public Texture2D TextureIconRoverA
		{
			get { return ti3A ? ti3A : ti3A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Rover_A", false); }
		}
		private Texture2D ti4A;
		public Texture2D TextureIconBaseA
		{
			get { return ti4A ? ti4A : ti4A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Base_A", false); }
		}
		private Texture2D ti5A;
		public Texture2D TextureIconProbeA
		{
			get { return ti5A ? ti5A : ti5A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Probe_A", false); }
		}
		private Texture2D ti6A;
		public Texture2D TextureIconSubA
		{
			get { return ti6A ? ti6A : ti6A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Sub_A", false); }
		}
		private Texture2D ti7A;
		public Texture2D TextureIconGenericA
		{
			get { return ti7A ? ti7A : ti7A = GameDatabase.Instance.GetTexture(textureDir + "Icon_Generic_A", false); }
		}
		private Texture2D ti1B;
		public Texture2D TextureIconShipB
		{
			get { return ti1B ? ti1B : ti1B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Ship_B", false); }
		}
		private Texture2D ti2B;
		public Texture2D TextureIconPlaneB
		{
			get { return ti2B ? ti2B : ti2B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Plane_B", false); }
		}
		private Texture2D ti3B;
		public Texture2D TextureIconRoverB
		{
			get { return ti3B ? ti3B : ti3B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Rover_B", false); }
		}
		private Texture2D ti4B;
		public Texture2D TextureIconBaseB
		{
			get { return ti4B ? ti4B : ti4B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Base_B", false); }
		}
		private Texture2D ti5B;
		public Texture2D TextureIconProbeB
		{
			get { return ti5B ? ti5B : ti5B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Probe_B", false); }
		}
		private Texture2D ti6B;
		public Texture2D TextureIconSubB
		{
			get { return ti6B ? ti6B : ti6B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Sub_B", false); }
		}
		private Texture2D ti7B;
		public Texture2D TextureIconGenericB
		{
			get { return ti7B ? ti7B : ti7B = GameDatabase.Instance.GetTexture(textureDir + "Icon_Generic_B", false); }
		}
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

		/*
		private Texture2D tia;
		public Texture2D TextureIconA
		{
			get { return tia ? tia : tia = GameDatabase.Instance.GetTexture(textureDir + "A_Team", false); }
		}
		private Texture2D tib;
		public Texture2D TextureIconB
		{
			get { return tib ? tib : tib = GameDatabase.Instance.GetTexture(textureDir + "B_Team", false); }
		}
		*/
		void Start()
		{
			Instance = this;
			AddToolbarButton();
			LoadConfig();
			//UpdateList(); implement once multi-team support working
		}
		/*
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

			List<Vessel>.Enumerator v = FlightGlobals.Vessels.GetEnumerator();
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
			v.Dispose();
		}
		*/
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
			TeamIconSettings.TEAMICONS = GUI.Toggle(new Rect(5, 25, 100, 20), TeamIconSettings.TEAMICONS, "Enable Team Icons", BDGuiSkin.toggle);
			if (TeamIconSettings.TEAMICONS)
			{
				Rect IconOptionsGroup = new Rect(15, 55, toolWindowWidth - 20, 130);
				GUI.BeginGroup(IconOptionsGroup, GUIContent.none, BDGuiSkin.box);
				TeamIconSettings.TEAMNAMES = GUI.Toggle(new Rect(15, 0, toolWindowWidth - 20, 20), TeamIconSettings.TEAMNAMES, "Enable Team Labels", BDGuiSkin.toggle);
				TeamIconSettings.VESSELNAMES = GUI.Toggle(new Rect(15, 25, toolWindowWidth - 20, 20), TeamIconSettings.VESSELNAMES, "Enable Vessel Labels", BDGuiSkin.toggle);
				TeamIconSettings.MISSILES = GUI.Toggle(new Rect(15, 50, toolWindowWidth - 20, 20), TeamIconSettings.MISSILES, "Missile icons", BDGuiSkin.toggle);
				TeamIconSettings.DEBRIS = GUI.Toggle(new Rect(15, 75, toolWindowWidth - 20, 20), TeamIconSettings.DEBRIS, "Debris Icons", BDGuiSkin.toggle);
				TeamIconSettings.PERSISTANT = GUI.Toggle(new Rect(15, 100, toolWindowWidth - 20, 20), TeamIconSettings.PERSISTANT, "Do not hide with UI", BDGuiSkin.toggle);
				GUI.EndGroup();
				line = 5;
				/*
				Rect TeamColorsGroup = new Rect(15, 190, toolWindowWidth - 20, 60);
				GUI.BeginGroup(TeamColorsGroup, GUIContent.none, BDGuiSkin.box);
				using (var teamManagers = weaponManagers.GetEnumerator())
					while (teamManagers.MoveNext())
					{
						i++;
						Rect buttonRect = new Rect(30, -20 + (i * 25), 190, 20);
						GUIStyle vButtonStyle = showColorSelect ? BDArmorySetup.BDGuiSkin.box : BDArmorySetup.BDGuiSkin.button;
						if (GUI.Button(buttonRect, $"{teamManagers.Current.Key}", vButtonStyle))
						{
							teamname = teamManagers.Current.Key;
							showColorSelect = !showColorSelect;
							LoadConfig();
							selectedTeam = i;
						}
						if (i == 1)
						{
							title.normal.textColor = TeamIconSettings.TEAM_1_COLOR;
						}
						else if (i == 2)
						{
							title.normal.textColor = TeamIconSettings.TEAM_2_COLOR;
						}
						GUI.Label(new Rect(5, -20 + (i * 25), 25, 25), "*", title);
					}	
				GUI.EndGroup();
				*/
			}
			else
			{
				line = 0;
			}
			toolWindowHeight = Mathf.Lerp(toolWindowHeight, (100 + (line * 25)+(i*25)) + 5, 1);
			WindowRectGUI.height = toolWindowHeight;
		}
	}
}
