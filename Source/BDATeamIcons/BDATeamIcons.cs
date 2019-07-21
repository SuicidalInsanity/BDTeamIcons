using System.Collections.Generic;
using BDArmory.Modules;
using UnityEngine;
using BDArmory.UI;

namespace BDTeamIcons
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class BDATeamIcons : MonoBehaviour
	{
		private SortedList<string, List<MissileFire>> weaponManagers = new SortedList<string, List<MissileFire>>();

		public BDATeamIcons Instance;

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
		static void DrawOnScreenIcon(Vector3 worldPos, Texture texture, Vector2 size, Color Teamcolor)
		{
			bool offscreen = false;
			Vector3 screenPos = BDGUIUtils.GetMainCamera().WorldToViewportPoint(worldPos);
			if (screenPos.z < 0 )
			{ 
				offscreen = true;
				screenPos.x *= -1;
				screenPos.y *= -1;
			}
			if (screenPos.x != Mathf.Clamp01(screenPos.x))
			{
				screenPos.x = Mathf.Clamp01(screenPos.x);
				offscreen = true;
			}
			if (screenPos.y != Mathf.Clamp01(screenPos.y))
			{
				screenPos.y = Mathf.Clamp01(screenPos.y);
				offscreen = true;
			}
			float xPos = screenPos.x * Screen.width - (0.5f * size.x);
			float yPos = (1 - screenPos.y) * Screen.height - (0.5f * size.y);
			float xtPos = 1* (Screen.width / 2);
			float ytPos = 1* (Screen.height / 2);
			if (offscreen)
			{
				xPos = Mathf.Clamp(xPos, (Screen.width * 0) + 20, (Screen.width * 1) - 20);

				if ((1 - screenPos.y) * Screen.height < (Screen.height / 2))
				{
					yPos = Mathf.Clamp(yPos, ((1 - screenPos.y)* Screen.height * 1)+20, ((1 - screenPos.y)* Screen.height * 1)+20);	
				}
				if ((1 - screenPos.y) * Screen.height > (Screen.height / 2))
				{
					yPos = Mathf.Clamp(yPos, ((1 - screenPos.y) * Screen.height * 1)-20, ((1 - screenPos.y) * Screen.height * 1)-20);
				}
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
				GUI.depth = 0;
				Rect iconRect = new Rect(xPos, yPos, size.x, size.y);
				GUI.DrawTexture(iconRect, texture);
			}
		}
		public static void DrawPointer(Vector3 Pointer, Vector3 Tail, float width, Color color)
		{
			Camera cam = BDGUIUtils.GetMainCamera();

			if (cam == null) return;

			GUI.matrix = Matrix4x4.identity;

			float angle = Vector2.Angle(Vector3.up, Tail - Pointer);
			if (Tail.x < Pointer.x)
			{
				angle = -angle;
			}

			Vector2 vector = Tail - Pointer;
			float length = 60;

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
				float size = 45;

				List<Vessel>.Enumerator v = FlightGlobals.Vessels.GetEnumerator();
				while (v.MoveNext())
				{
					if (v.Current == null) continue;
					if (!v.Current.loaded || v.Current.packed) continue;

					if (TeamIconSettings.MISSILES)
					{
						List<MissileBase>.Enumerator ml = v.Current.FindPartModulesImplementing<MissileBase>().GetEnumerator();
						while (ml.MoveNext())
						{
							if (ml.Current == null) continue;
							if (ml.Current.MissileState == MissileBase.MissileStates.Boost || ml.Current.MissileState == MissileBase.MissileStates.Cruise)
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
						ml.Dispose();
					}

					List<MissileFire>.Enumerator mf = v.Current.FindPartModulesImplementing<MissileFire>().GetEnumerator();
					while (mf.MoveNext())
					{
						if (mf.Current == null) continue;
						if (!mf.Current.vessel.loaded || mf.Current.vessel.packed) continue;

						if (!mf.Current.vessel.isActiveVessel)
						{
							Vector3 selfPos = FlightGlobals.ActiveVessel.CoM;
							Vector3 targetPos = (mf.Current.vessel.CoM);
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
								if (mf.Current.Team.Name == "A")
								{
									IconUIStyle.normal.textColor = XKCDColors.Red;
									Teamcolor = XKCDColors.Red;
									if ((mf.Current.vessel.vesselType == VesselType.Ship && !mf.Current.vessel.Splashed) || mf.Current.vessel.vesselType == VesselType.Plane)
									{
										icon = BDATISetup.Instance.TextureIconPlaneA;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Base || mf.Current.vessel.vesselType == VesselType.Lander)
									{
										icon = BDATISetup.Instance.TextureIconBaseA;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Rover)
									{
										icon = BDATISetup.Instance.TextureIconRoverA;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Probe)
									{
										icon = BDATISetup.Instance.TextureIconProbeA;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Ship && mf.Current.vessel.Splashed)
									{
										icon = BDATISetup.Instance.TextureIconShipA;
										if (mf.Current.vessel.vesselType == VesselType.Ship && mf.Current.vessel.altitude < -10)
										{
											icon = BDATISetup.Instance.TextureIconSubA;
										}
									}
									else if (mf.Current.vessel.vesselType == VesselType.Debris)
									{
										icon = BDATISetup.Instance.TextureIconDebris;
									}
									else
									{
										icon = BDATISetup.Instance.TextureIconGenericA;
									}
								}
								else if (mf.Current.Team.Name == "B")
								{
									IconUIStyle.normal.textColor = XKCDColors.Blue;
									Teamcolor = XKCDColors.Blue;
									if ((mf.Current.vessel.vesselType == VesselType.Ship && !mf.Current.vessel.Splashed) || mf.Current.vessel.vesselType == VesselType.Plane)
									{
										icon = BDATISetup.Instance.TextureIconPlaneB;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Base || mf.Current.vessel.vesselType == VesselType.Lander)
									{
										icon = BDATISetup.Instance.TextureIconBaseB;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Rover)
									{
										icon = BDATISetup.Instance.TextureIconRoverB;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Probe)
									{
										icon = BDATISetup.Instance.TextureIconProbeB;
									}
									else if (mf.Current.vessel.vesselType == VesselType.Ship && mf.Current.vessel.Splashed)
									{
										icon = BDATISetup.Instance.TextureIconShipB;
										if (mf.Current.vessel.vesselType == VesselType.Ship && mf.Current.vessel.altitude < -10)
										{
											icon = BDATISetup.Instance.TextureIconSubB;
										}
									}
									else if (mf.Current.vessel.vesselType == VesselType.Debris)
									{
										icon = BDATISetup.Instance.TextureIconDebris;
									}
									else
									{
										icon = BDATISetup.Instance.TextureIconGenericB;
									}
								}
								else
								{
									IconUIStyle.normal.textColor = XKCDColors.Grey;
									Teamcolor = XKCDColors.Grey;
									icon = BDATISetup.Instance.TextureIconGeneric;
								}
								// if string teamname != mf.Current.Team.name
								//		i ++
								// string teamname = mf.Current.Team.name
								// IconUIStyle.normal.textcolor = colors[i]

								//alternitively, link into the team counting code from vesselswitcher and have the counter count from there?

								DrawOnScreenIcon(mf.Current.vessel.CoM, icon, new Vector2(size, size), Teamcolor);
								if (BDGUIUtils.WorldToGUIPos(mf.Current.vessel.CoM, out guiPos))
								{
									if (TeamIconSettings.VESSELNAMES)
									{
										vName = mf.Current.vessel.vesselName;
										Rect nameRect = new Rect((guiPos.x + 24), guiPos.y - 4, 100, 32);
										GUI.Label(nameRect, vName, IconUIStyle);
									}
									Rect distRect = new Rect((guiPos.x - 12), (guiPos.y + 20), 100, 32);
									GUI.Label(distRect, UIdist + UoM, IconUIStyle);
									if (TeamIconSettings.TEAMNAMES)
									{
										Rect teamRect = new Rect((guiPos.x + 11), (guiPos.y - 19), 16, 16);
										GUI.Label(teamRect, "Team: " + $"{mf.Current.Team.Name}", IconUIStyle);
									}

								}
							}
						}
					}
					mf.Dispose();
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
				v.Dispose();
			}
		}
	}
}

