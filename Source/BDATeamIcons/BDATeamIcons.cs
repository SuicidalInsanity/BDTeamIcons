using System.Collections.Generic;
using BDArmory.Modules;
using UnityEngine;
using BDArmory.UI;
using BDArmory.Misc;
using BDArmory.Control;
using BDArmory.Competition;
using LibNoise;

namespace BDTeamIcons
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class BDATeamIcons : MonoBehaviour
	{
		public BDATeamIcons Instance;
		public SortedList<string, List<MissileFire>> weaponManagers = new SortedList<string, List<MissileFire>>();

		public Material IconMat;

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

			IconMat = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
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

		private void DrawOnScreenIcon(Vector3 worldPos, Texture texture, Vector2 size, Color Teamcolor, bool targeticon, bool ShowPointer)
		{
			if (Event.current.type.Equals(EventType.Repaint))
			{
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

				if (!offscreen)
				{
					IconMat.SetColor("_TintColor", Teamcolor);
					IconMat.mainTexture = texture;
					if (targeticon)
					{
						yPos = yPos - (24 * TeamIconSettings.ICONSCALE);
					}
					Rect iconRect = new Rect(xPos, yPos, size.x, size.y);
					Graphics.DrawTexture(iconRect, texture, IconMat);
				}
				else
				{
					if (TeamIconSettings.POINTERS)
					{
						Vector2 head;
						Vector2 tail;

						head.x = xPos;
						head.y = yPos;
						tail.x = xtPos;
						tail.y = ytPos;
						float angle = Vector2.Angle(Vector3.up, tail - head);
						if (tail.x < head.x)
						{
							angle = -angle;
						}
						if (ShowPointer)
						{
							DrawPointer(calculateRadialCoords(head, tail, angle, 0.75f), angle, 4, Teamcolor);
						}
						if (targeticon)
						{
							IconMat.SetColor("_TintColor", Teamcolor);
							IconMat.mainTexture = texture;
							xPos = (calculateRadialCoords(head, tail, angle, 0.78f)).x - (0.5f * size.x);
							yPos = (calculateRadialCoords(head, tail, angle, 0.78f)).y - (0.5f * size.y);
							Rect targetRect = new Rect(xPos, yPos, size.x, size.y);
							Graphics.DrawTexture(targetRect, texture, IconMat);
						}
					}
				}

			}
		}
		public Vector2 calculateRadialCoords(Vector2 RadialCoord, Vector2 Tail, float angle, float edgeDistance)
		{
			float theta = Mathf.Abs(angle);
			if (theta > 90)
			{
				theta -= 90;
			}
			theta = theta * Mathf.Deg2Rad; //needs to be in radians for Mathf. trig
			float HalfWidth = Screen.width / 2;
			float HalfHeight = Screen.height / 2;
			float Cos = Mathf.Cos(theta);
			float Sin = Mathf.Sin(theta);

			if (RadialCoord.y >= HalfHeight)
			{
				if (RadialCoord.x >= HalfWidth) // set up Quads 3-4
				{
					RadialCoord.x = (Cos * (edgeDistance * HalfWidth)) + HalfWidth;
				}
				else
				{
					RadialCoord.x = HalfWidth - ((Cos * edgeDistance) * HalfWidth);
				}
				RadialCoord.y = (Sin * (edgeDistance * HalfHeight)) + HalfHeight;
			}
			else
			{
				if (RadialCoord.x >= HalfWidth) // set up Quads 1-2 
				{
					RadialCoord.x = (Sin * (edgeDistance * HalfWidth)) + HalfWidth;
				}
				else
				{
					RadialCoord.x = HalfWidth - ((Sin * edgeDistance) * HalfWidth);
				}
				RadialCoord.y = HalfHeight - ((Cos * edgeDistance) * HalfHeight);
			}
			return RadialCoord;
		}
		public static void DrawPointer(Vector2 Pointer, float angle, float width, Color color)
		{
			Camera cam = BDGUIUtils.GetMainCamera();

			if (cam == null) return;

			GUI.matrix = Matrix4x4.identity;
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
				float Teamcount = 0;
				float TotalTeams = 0;

				// First let's count the total teams for color-picking
				using (var teamManagers = weaponManagers.GetEnumerator())
					while (teamManagers.MoveNext())
						TotalTeams++;

				using (var teamManagers = weaponManagers.GetEnumerator())
					while (teamManagers.MoveNext())
					{
						Teamcount++;
						using (var wm = teamManagers.Current.Value.GetEnumerator())
							while (wm.MoveNext())
							{
								if (wm.Current == null) continue;

								float h = (Teamcount - 1) / TotalTeams;
								Teamcolor = Color.HSVToRGB(h, 1f, 1f);
								IconUIStyle.normal.textColor = Teamcolor;

								if (wm.Current.vessel.isActiveVessel)
								{
									if (TeamIconSettings.THREATICON)
									{
										if (wm.Current.currentTarget == null) continue;
										Vector3 sPos = FlightGlobals.ActiveVessel.CoM;
										Vector3 tPos = (wm.Current.currentTarget.Vessel.CoM);
										Vector3 RelPos = (tPos - sPos);
										if (RelPos.magnitude >= 100)
										{
											DrawOnScreenIcon(wm.Current.currentTarget.Vessel.CoM, BDATISetup.Instance.TextureIconThreat, new Vector2((24 * TeamIconSettings.ICONSCALE), (24 * TeamIconSettings.ICONSCALE)), Teamcolor, true, false);
										}
									}
								}
								else
								{
									Vector3 selfPos = FlightGlobals.ActiveVessel.CoM;
									Vector3 targetPos = (wm.Current.vessel.CoM);
									Vector3 targetRelPos = (targetPos - selfPos);
									Vector2 guiPos;
									float distance;

									distance = targetRelPos.magnitude;
									string UIdist;
									string UoM;
									string vName;
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
										// Set color

										DrawOnScreenIcon(wm.Current.vessel.CoM, icon, new Vector2((size * TeamIconSettings.ICONSCALE), (size * TeamIconSettings.ICONSCALE)), Teamcolor, false, true);
										if (TeamIconSettings.THREATICON)
										{
											if (wm.Current.currentTarget != null)
											{
												if (!wm.Current.currentTarget.Vessel.isActiveVessel)
												{
													DrawOnScreenIcon(wm.Current.currentTarget.Vessel.CoM, BDATISetup.Instance.TextureIconThreat, new Vector2((24 * TeamIconSettings.ICONSCALE), (24 * TeamIconSettings.ICONSCALE)), Teamcolor, true, false);
												}
												//else
												//{
												//	DrawOnScreenIcon(wm.Current.currentTarget.Vessel.CoM, BDATISetup.Instance.TextureIconThreat, new Vector2((20 * TeamIconSettings.ICONSCALE), (24 * TeamIconSettings.ICONSCALE)), Teamcolor, true, false);
												//}
											}
										}
										if (BDGUIUtils.WorldToGUIPos(wm.Current.vessel.CoM, out guiPos))
										{
											if (TeamIconSettings.VESSELNAMES)
											{
												vName = wm.Current.vessel.vesselName;
												Rect nameRect = new Rect((guiPos.x + (24 * TeamIconSettings.ICONSCALE)), guiPos.y - 4, 100, 32);
												GUI.Label(nameRect, vName, IconUIStyle);
											}
											if (TeamIconSettings.TEAMNAMES)
											{
												Rect teamRect = new Rect((guiPos.x + (16 * TeamIconSettings.ICONSCALE)), (guiPos.y - (19 * TeamIconSettings.ICONSCALE)), 100, 32);
												GUI.Label(teamRect, "Team: " + $"{wm.Current.Team.Name}", IconUIStyle);
											}

											if (TeamIconSettings.SCORE)
											{
												BDArmory.Control.ScoringData scoreData = null;
												int Score = 0;

												if (BDACompetitionMode.Instance.Scores.ContainsKey(wm.Current.vessel.vesselName))
												{
													scoreData = BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName];
													Score = scoreData.Score;
												}
												if (VesselSpawner.Instance.vesselsSpawningContinuously)
												{
													if (VesselSpawner.Instance.continuousSpawningScores.ContainsKey(wm.Current.vessel.vesselName))
													{
														Score += VesselSpawner.Instance.continuousSpawningScores[wm.Current.vessel.vesselName].cumulativeHits;
													}
												}

												Rect scoreRect = new Rect((guiPos.x + (16 * TeamIconSettings.ICONSCALE)), (guiPos.y + (14 * TeamIconSettings.ICONSCALE)), 100, 32);
												GUI.Label(scoreRect, "Score: " + Score, IconUIStyle);
											}

											if (TeamIconSettings.HEALTHBAR)
											{

												double hpPercent = 1;
												/* //parts can take more damage than they have HP from overkill, this artificially inflates the damage taken count and throws off the remaining HP %
												if (BDACompetitionMode.Instance.Scores.ContainsKey(wm.Current.vessel.vesselName)) 
												{
													if (BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromBullets.Count > 0)
													{
														foreach (var vesselName in BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromBullets.Keys)
															damagetaken += BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromBullets[vesselName];
													}
													if (BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromMissiles.Count > 0)
													{
														foreach (var vesselName in BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromMissiles.Keys)
															damagetaken += BDACompetitionMode.Instance.Scores[wm.Current.vessel.vesselName].damageFromMissiles[vesselName];
													}
												}
												hpPercent = (wm.Current.totalHP - damagetaken ) / wm.Current.totalHP;										
												*/ //so lets just count how much of the ship is left instead
												hpPercent = Mathf.Clamp((1 - ((wm.Current.totalHP - wm.Current.vessel.parts.Count) / wm.Current.totalHP)), 0, 1);
												if (hpPercent > 0)
												{
													Rect barRect = new Rect((guiPos.x - (32 * TeamIconSettings.ICONSCALE)), (guiPos.y + (30 * TeamIconSettings.ICONSCALE)), (64 * TeamIconSettings.ICONSCALE), 12);
													Rect healthRect = new Rect((guiPos.x - (30 * TeamIconSettings.ICONSCALE)), (guiPos.y + (32 * TeamIconSettings.ICONSCALE)), (60 * (float)hpPercent * TeamIconSettings.ICONSCALE), 8);
													//GUI.Label(healthRect, "Team: " + $"{wm.Current.Team.Name}", IconUIStyle);
													BDGUIUtils.DrawRectangle(barRect, XKCDColors.Grey);
													BDGUIUtils.DrawRectangle(healthRect, Color.HSVToRGB((85f * (float)hpPercent) / 255, 1f, 1f));

												}
												Rect distRect = new Rect((guiPos.x - 12), (guiPos.y + (45 * TeamIconSettings.ICONSCALE)), 100, 32);
												GUI.Label(distRect, UIdist + UoM, IconUIStyle);
											}
											else
											{
												Rect distRect = new Rect((guiPos.x - 12), (guiPos.y + (20 * TeamIconSettings.ICONSCALE)), 100, 32);
												GUI.Label(distRect, UIdist + UoM, IconUIStyle);
											}
										}
									}
								}
							}

					}
			}
		}
	}	
}

