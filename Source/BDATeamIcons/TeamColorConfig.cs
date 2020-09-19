using UnityEngine;

// credit to Brian Jones (https://github.com/boj)& KSP ForumMember TaxiService
namespace BDTeamIcons
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class TeamColorConfig : MonoBehaviour
	{
		private Texture2D displayPicker;
		public int displayTextureWidth = 360;
		public int displayTextureHeight = 360;

		public int HorizPos;
		public int VertPos;

		public Color selectedColor;
		private Texture2D selectedColorPreview;

		private float hueSlider = 0f;
		private float prevHueSlider = 0f;
		private Texture2D hueTexture;

		protected void Awake()
		{
			HorizPos = (Screen.width / 2) - (displayTextureWidth / 2);
			VertPos = (Screen.height / 2) - (displayTextureHeight / 2);

			renderColorPicker();

			hueTexture = new Texture2D(10, displayTextureHeight, TextureFormat.ARGB32, false);
			for (int x = 0; x < hueTexture.width; x++)
			{
				for (int y = 0; y < hueTexture.height; y++)
				{
					float h = (y / (hueTexture.height * 1.0f)) * 1f;
					hueTexture.SetPixel(x, y, new ColorHSV(h, 1f, 1f).ToColor());
				}
			}
			hueTexture.Apply();

			selectedColorPreview = new Texture2D(1, 1);
			selectedColorPreview.SetPixel(0, 0, selectedColor);
		}

		private void renderColorPicker()
		{
			Texture2D colorPicker = new Texture2D(displayTextureWidth, displayTextureHeight, TextureFormat.ARGB32, false);
			for (int x = 0; x < displayTextureWidth; x++)
			{
				for (int y = 0; y < displayTextureHeight; y++)
				{
					float h = hueSlider;
					float v = (y / (displayTextureHeight * 1.0f)) * 1f;
					float s = (x / (displayTextureWidth * 1.0f)) * 1f;
					colorPicker.SetPixel(x, y, new ColorHSV(h, s, v).ToColor());
				}
			}

			colorPicker.Apply();
			displayPicker = colorPicker;
		}

		protected void OnGUI()
		{
			if (!BDATISetup.Instance.showColorSelect) return;

			GUI.Box(new Rect(HorizPos - 3, VertPos - 3, displayTextureWidth + 60, displayTextureHeight + 60), "");

			if (hueSlider != prevHueSlider) // new Hue value
			{
				prevHueSlider = hueSlider;
				renderColorPicker();
			}

			if (GUI.RepeatButton(new Rect(HorizPos, VertPos, displayTextureWidth, displayTextureHeight), displayPicker))
			{
				int a = (int)Input.mousePosition.x;
				int b = Screen.height - (int)Input.mousePosition.y;

				selectedColor = displayPicker.GetPixel(a - HorizPos, -(b - VertPos));
			}

			hueSlider = GUI.VerticalSlider(new Rect(HorizPos + displayTextureWidth + 3, VertPos, 10, displayTextureHeight), hueSlider, 1, 0);
			GUI.Box(new Rect(HorizPos + displayTextureWidth + 20, VertPos, 20, displayTextureHeight), hueTexture);

			if (GUI.Button(new Rect(HorizPos + displayTextureWidth - 60, VertPos + displayTextureHeight + 10, 60, 25), "Apply"))
			{
				selectedColor = selectedColorPreview.GetPixel(0, 0);
				BDATISetup.Instance.showColorSelect = false;
				BDATISetup.Instance.UpdateTeamColor = true;
			}

			// box for chosen color
			GUIStyle style = new GUIStyle();
			selectedColorPreview.SetPixel(0, 0, selectedColor);
			selectedColorPreview.Apply();
			style.normal.background = selectedColorPreview;
			GUI.Box(new Rect(HorizPos + displayTextureWidth + 10, VertPos + displayTextureHeight + 10, 30, 30), new GUIContent(""), style);
		}
		float updateTimer;

		void Update()
		{
			if (!HighLogic.LoadedSceneIsFlight)
			{
				return;
			}
			if (BDATISetup.Instance.UpdateTeamColor)
			{
				updateTimer -= Time.fixedDeltaTime;
				{
					if (BDATISetup.Instance.UpdateTeamColor && updateTimer < 0)
					{
						updateTimer = 0.5f;    //next update in half a sec only
						if (BDATISetup.Instance.selectedTeam == 1)
						{
							TeamIconSettings.TEAM_1_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 2)
						{
							TeamIconSettings.TEAM_2_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 3)
						{
							TeamIconSettings.TEAM_3_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 4)
						{
							TeamIconSettings.TEAM_4_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 5)
						{
							TeamIconSettings.TEAM_5_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 6)
						{
							TeamIconSettings.TEAM_6_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 7)
						{
							TeamIconSettings.TEAM_7_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 8)
						{
							TeamIconSettings.TEAM_8_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 9)
						{
							TeamIconSettings.TEAM_9_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 10)
						{
							TeamIconSettings.TEAM_10_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 11)
						{
							TeamIconSettings.TEAM_11_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 12)
						{
							TeamIconSettings.TEAM_12_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 13)
						{
							TeamIconSettings.TEAM_13_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 14)
						{
							TeamIconSettings.TEAM_14_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 15)
						{
							TeamIconSettings.TEAM_15_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						else if (BDATISetup.Instance.selectedTeam == 16)
						{
							TeamIconSettings.TEAM_16_COLOR = $"{Mathf.RoundToInt(selectedColor.r * 255)},{Mathf.RoundToInt(selectedColor.g * 255)},{Mathf.RoundToInt(selectedColor.b * 255)},{Mathf.RoundToInt(selectedColor.a * 255)}";
						}
						BDATISetup.Instance.UpdateTeamColor = !BDATISetup.Instance.UpdateTeamColor;
					}
				}
			}
		}
	}
}