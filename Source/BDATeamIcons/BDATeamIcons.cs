using System.Collections.Generic;
using BDArmory.Modules;
using UnityEngine;
using BDArmory.UI;
using BDArmory.Misc;

namespace BDTeamIcons
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class BDATeamIcons : MonoBehaviour
	{
		public BDATeamIcons Instance;
		public SortedList<string, List<MissileFire>> weaponManagers = new SortedList<string, List<MissileFire>>();

		void Awake()
		{
			if (Instance)
			{
				Destroy(this);
			}
			else
				Instance = this;
		}
		GUIStyle IconUIStyle;
		
		GUIStyle mIStyle;
		Color Teamcolor;
		private float updateList = 0;

		private void Start()
		{
			IconUIStyle = new GUIStyle();
			IconUIStyle.fontStyle = FontStyle.Bold;
			IconUIStyle.fontSize = 10;
			IconUIStyle.normal.textColor = XKCDColors.Red;//replace with BDATISetup defined value varable.

			mIStyle = new GUIStyle();
			mIStyle.fontStyle = FontStyle.Normal;
			mIStyle.fontSize = 10;
			mIStyle.normal.textColor = XKCDColors.Yellow;
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
			if (!HighLogic.LoadedSceneIsFlight)
			{
				return;
			}
			if (TeamIconSettings.TEAMICONS)
			{
				updateList -= Time.fixedDeltaTime;
				if (updateList < 0)
				{
					UpdateList();
					updateList = 2f; // check team lists less often than every frame
				}
			}
		}
		private void UpdateList() // would love if I could get the list from BDATISetup instead so I'm not making a duplicate list for here
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

		private static void DrawOnScreenIcon(Vector3 worldPos, Texture texture, Vector2 size, Color Teamcolor)
		{
			if (Event.current.type.Equals(EventType.Repaint))
			{
				Material IconMat;
				bool offscreen = false;
				Vector3 screenPos = BDGUIUtils.GetMainCamera().WorldToViewportPoint(worldPos);
				if (screenPos.z < 0)
				{
					offscreen = true;
					screenPos.x *= -1;
					screenPos.y *= -1; 				
				}
				if (screenPos.x != Mathf.Clamp01(screenPos.x))
				{
					offscreen = true;
				}
				if (screenPos.y != Mathf.Clamp01(screenPos.y))
				{
					offscreen = true;
				}
				float xPos = (screenPos.x * Screen.width) - (0.5f * size.x);
				float yPos = ((1 - screenPos.y) * Screen.height) - (0.5f * size.y);
				float xtPos = 1 * (Screen.width / 2);
				float ytPos = 1 * (Screen.height / 2);
				if (offscreen && TeamIconSettings.POINTERS)
				{
					Vector2 pointer;
					Vector2 tail;

					pointer.x = xPos;
					pointer.y = yPos;
					tail.x = xtPos;
					tail.y = ytPos;

					DrawPointer(pointer, tail, 4, Teamcolor);
				}
				else
				{
					IconMat = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
					IconMat.SetColor("_TintColor", Teamcolor);
					IconMat.mainTexture = texture;
					Rect iconRect = new Rect(xPos, yPos, size.x, size.y);
					Rect sourceRect = new Rect(0, 0, 40 * TeamIconSettings.ICONSCALE, 40 * TeamIconSettings.ICONSCALE);
					Graphics.DrawTexture(iconRect, texture, IconMat); 
				}
			}
		}
		public static void DrawPointer(Vector2 Pointer, Vector2 Tail, float width, Color color)
		{
			Camera cam = BDGUIUtils.GetMainCamera();

			if (cam == null) return;

			GUI.matrix = Matrix4x4.identity;

			float angle = Vector2.Angle(Vector3.up, Tail - Pointer);
			if (Tail.x < Pointer.x)
			{
				angle = -angle;
			}
			float theta = Mathf.Abs(angle);
			if (theta > 90)
			{
				theta -= 90;
			}
			theta = theta * Mathf.Deg2Rad; //needs to be in radians for Mathf. trig
			float length = 60;
			float HalfWidth = Screen.width / 2;
			float HalfHeight = Screen.height / 2;
			float Cos = Mathf.Cos(theta); 
			float Sin = Mathf.Sin(theta);
			
			if (Pointer.y >= HalfHeight)
			{
				if (Pointer.x >= HalfWidth) // set up Quads 3-4
				{
					Pointer.x = (Cos * (0.75f * HalfWidth)) + HalfWidth;
				}
				else
				{
					Pointer.x = HalfWidth - ((Cos * 0.75f) * HalfWidth);
				}
				Pointer.y = (Sin * (0.75f * HalfHeight)) + HalfHeight;
			}
			else
			{
				if (Pointer.x >= HalfWidth) // set up Quads 1-2 
				{
					Pointer.x = (Sin * (0.75f * HalfWidth)) + HalfWidth;
				}
				else
				{
					Pointer.x = HalfWidth - ((Sin * 0.75f) * HalfWidth);
				}
				Pointer.y = HalfHeight - ((Cos * 0.75f) * HalfHeight);
			}
			Rect upRect = new Rect(Pointer.x - (width / 2), Pointer.y - length, width, length);
			GUIUtility.RotateAroundPivot(-angle + 180, Pointer);
			BDGUIUtils.DrawRectangle(upRect, color);
			GUI.matrix = Matrix4x4.identity;
		}
		void OnGUI()
		{
			if ((HighLogic.LoadedSceneIsFlight && BDArmorySetup.GAME_UI_ENABLED && !MapView.MapIsEnabled && TeamIconSettings.TEAMICONS) || HighLogic.LoadedSceneIsFlight && !BDArmorySetup.GAME_UI_ENABLED && !MapView.MapIsEnabled && TeamIconSettings.TEAMICONS && TeamIconSettings.PERSISTANT)
			{
				Texture icon;
				float size = 40;

				using (List<Vessel>.Enumerator v = FlightGlobals.Vessels.GetEnumerator())
					while (v.MoveNext())
					{
						if (v.Current == null) continue;
						if (!v.Current.loaded || v.Current.packed || v.Current.isActiveVessel) continue;

						if (TeamIconSettings.MISSILES)
						{
							using (List<MissileBase>.Enumerator ml = v.Current.FindPartModulesImplementing<MissileBase>().GetEnumerator())
								while (ml.MoveNext())
								{
									if (ml.Current == null) continue;
									if (ml.Current.MissileState != MissileBase.MissileStates.Idle && ml.Current.MissileState != MissileBase.MissileStates.Drop)
									{
										Vector3 sPos = FlightGlobals.ActiveVessel.vesselTransform.position;
										Vector3 tPos = v.Current.vesselTransform.position;
										Vector3 Dist = (tPos - sPos);
										Vector2 guiPos;
										string UIdist;
										string UoM;
										if (Dist.magnitude > 100)
										{
											if ((Dist.magnitude / 1000) >= 1)
											{
												UoM = "km";
												UIdist = (Dist.magnitude / 1000).ToString("0.00");
											}
											else
											{
												UoM = "m";
												UIdist = Dist.magnitude.ToString("0.0");
											}
											BDGUIUtils.DrawTextureOnWorldPos(v.Current.CoM, BDATISetup.Instance.TextureIconMissile, new Vector2(20, 20), 0);
											if (BDGUIUtils.WorldToGUIPos(ml.Current.vessel.CoM, out guiPos))
											{
												Rect distRect = new Rect((guiPos.x - 12), (guiPos.y + 10), 100, 32);
												GUI.Label(distRect, UIdist + UoM, mIStyle);
											}

										}
									}
								}
						}
						if (TeamIconSettings.DEBRIS)
						{
							if (v.Current.vesselType != VesselType.Debris && !v.Current.isActiveVessel) continue;
							if (v.Current.LandedOrSplashed) continue;
							{
								Vector3 sPos = FlightGlobals.ActiveVessel.vesselTransform.position;
								Vector3 tPos = v.Current.vesselTransform.position;
								Vector3 Dist = (tPos - sPos);
								if (Dist.magnitude > 100)
								{
									BDGUIUtils.DrawTextureOnWorldPos(v.Current.CoM, BDATISetup.Instance.TextureIconDebris, new Vector2(20, 20), 0);
								}
							}
						}
					}
				int Teamcount = 0;
				using (var teamManagers = weaponManagers.GetEnumerator())
					while (teamManagers.MoveNext())
					{
						Teamcount++;
						using (var wm = teamManagers.Current.Value.GetEnumerator())
							while (wm.MoveNext())
							{
								if (wm.Current == null) continue;

								if (wm.Current.vessel.isActiveVessel) continue;

								Vector3 selfPos = FlightGlobals.ActiveVessel.CoM;
								Vector3 targetPos = (wm.Current.vessel.CoM);
								Vector3 targetRelPos = (targetPos - selfPos);
								Vector2 guiPos;
								float distance;
								string UIdist;
								string UoM;
								string vName;
								distance = targetRelPos.magnitude;
								if (distance >= 100)
								{
									if ((distance / 1000) >= 1)
									{
										UoM = "km";
										UIdist = (distance / 1000).ToString("0.00");
									}
									else
									{
										UoM = "m";
										UIdist = distance.ToString("0.0");
									}
									if ((wm.Current.vessel.vesselType == VesselType.Ship && !wm.Current.vessel.Splashed) || wm.Current.vessel.vesselType == VesselType.Plane)
									{
										icon = BDATISetup.Instance.TextureIconPlane;
									}
									else if (wm.Current.vessel.vesselType == VesselType.Base || wm.Current.vessel.vesselType == VesselType.Lander)
									{
										icon = BDATISetup.Instance.TextureIconBase;
									}
									else if (wm.Current.vessel.vesselType == VesselType.Rover)
									{
										icon = BDATISetup.Instance.TextureIconRover;
									}
									else if (wm.Current.vessel.vesselType == VesselType.Probe)
									{
										icon = BDATISetup.Instance.TextureIconProbe;
									}
									else if (wm.Current.vessel.vesselType == VesselType.Ship && wm.Current.vessel.Splashed)
									{
										icon = BDATISetup.Instance.TextureIconShip;
										if (wm.Current.vessel.vesselType == VesselType.Ship && wm.Current.vessel.altitude < -10)
										{
											icon = BDATISetup.Instance.TextureIconSub;
										}
									}
									else if (wm.Current.vessel.vesselType == VesselType.Debris)
									{
										icon = BDATISetup.Instance.TextureIconDebris;
										size = 20;
									}
									else
									{
										icon = BDATISetup.Instance.TextureIconGeneric;
									}
									if (Teamcount == 1)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_1_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_1_COLOR);
									}
									else if (Teamcount == 2)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_2_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_2_COLOR);
									}
									else if (Teamcount == 3)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_3_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_3_COLOR);
									}
									else if (Teamcount == 4)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_4_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_4_COLOR);
									}
									else if (Teamcount == 5)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_5_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_5_COLOR);
									}
									else if (Teamcount == 6)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_6_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_6_COLOR);
									}
									else if (Teamcount == 7)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_7_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_7_COLOR);
									}
									else if (Teamcount == 8)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_8_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_8_COLOR);
									}
									else if (Teamcount == 9)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_1_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_9_COLOR);
									}
									else if (Teamcount == 10)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_2_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_10_COLOR);
									}
									else if (Teamcount == 11)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_3_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_11_COLOR);
									}
									else if (Teamcount == 12)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_4_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_12_COLOR);
									}
									else if (Teamcount == 13)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_5_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_13_COLOR);
									}
									else if (Teamcount == 14)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_6_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_14_COLOR);
									}
									else if (Teamcount == 15)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_7_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_15_COLOR);
									}
									else if (Teamcount == 16)
									{
										IconUIStyle.normal.textColor = Misc.ParseColor255(TeamIconSettings.TEAM_8_COLOR);
										Teamcolor = Misc.ParseColor255(TeamIconSettings.TEAM_16_COLOR);
									}
									else
									{
										IconUIStyle.normal.textColor = XKCDColors.Grey;
										Teamcolor = XKCDColors.Grey;
									}
									DrawOnScreenIcon(wm.Current.vessel.CoM, icon, new Vector2((size * TeamIconSettings.ICONSCALE), (size * TeamIconSettings.ICONSCALE)), Teamcolor);
									if (BDGUIUtils.WorldToGUIPos(wm.Current.vessel.CoM, out guiPos))
									{
										if (TeamIconSettings.VESSELNAMES)
										{
											vName = wm.Current.vessel.vesselName;
											Rect nameRect = new Rect((guiPos.x + (24 * TeamIconSettings.ICONSCALE)), guiPos.y - 4, 100, 32);
											GUI.Label(nameRect, vName, IconUIStyle);
										}
										Rect distRect = new Rect((guiPos.x - 12), (guiPos.y + (20 * TeamIconSettings.ICONSCALE)), 100, 32);
										GUI.Label(distRect, UIdist + UoM, IconUIStyle);
										if (TeamIconSettings.TEAMNAMES)
										{
											Rect teamRect = new Rect((guiPos.x + (11 * TeamIconSettings.ICONSCALE)), (guiPos.y - (19 * TeamIconSettings.ICONSCALE)), 16, 16);
											GUI.Label(teamRect, "Team: " + $"{wm.Current.Team.Name}", IconUIStyle);
										}
									}
								}

							}

					}
			}
		}
	}	
}

