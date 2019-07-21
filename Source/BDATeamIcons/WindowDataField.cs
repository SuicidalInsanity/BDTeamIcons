using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;

namespace BDTeamIcons
{
	[AttributeUsage(AttributeTargets.Field)]
	public class WindowDataField : Attribute
	{
		public WindowDataField()
		{
		}

		public static void Save()
		{
			ConfigNode fileNode = ConfigNode.Load(TeamIconSettings.settingsConfigURL);

			if (!fileNode.HasNode("Windows"))
			{
				fileNode.AddNode("Windows");
			}

			ConfigNode settings = fileNode.GetNode("Windows");

			IEnumerator<FieldInfo> field = typeof(BDATISetup).GetFields().AsEnumerable().GetEnumerator();
			while (field.MoveNext())
			{
				if (field.Current == null) continue;
				if (!field.Current.IsDefined(typeof(WindowDataField), false)) continue;

				settings.SetValue(field.Current.Name, field.Current.GetValue(null).ToString(), true);
			}
			field.Dispose();
			fileNode.Save(TeamIconSettings.settingsConfigURL);
		}

		public static void Load()
		{
			ConfigNode fileNode = ConfigNode.Load(TeamIconSettings.settingsConfigURL);
			if (!fileNode.HasNode("Windows")) return;

			ConfigNode settings = fileNode.GetNode("Windows");

			IEnumerator<FieldInfo> field = typeof(BDATISetup).GetFields().AsEnumerable().GetEnumerator();
			while (field.MoveNext())
			{
				if (field.Current == null) continue;
				if (!field.Current.IsDefined(typeof(WindowDataField), false)) continue;
				if (!settings.HasValue(field.Current.Name)) continue;

				object parsedValue = SettingsDataField.ParseValue(field.Current.FieldType, settings.GetValue(field.Current.Name));
				if (parsedValue != null)
				{
					field.Current.SetValue(null, parsedValue);
				}
			}
			field.Dispose();
		}
	}
}