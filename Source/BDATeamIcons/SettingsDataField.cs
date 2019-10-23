using System;
using System.Collections.Generic;
using System.Reflection;
using UniLinq;
using UnityEngine;
using BDArmory.Misc;

namespace BDTeamIcons
{
	[AttributeUsage(AttributeTargets.Field)]
	public class SettingsDataField : Attribute
	{
		public SettingsDataField()
		{
		}
		public static void Save()
		{
			ConfigNode fileNode = ConfigNode.Load(TeamIconSettings.settingsConfigURL);

			if (!fileNode.HasNode("IconSettings"))
			{
				fileNode.AddNode("IconSettings");
			}

			ConfigNode settings = fileNode.GetNode("IconSettings");
			IEnumerator<FieldInfo> field = typeof(TeamIconSettings).GetFields().AsEnumerable().GetEnumerator();
			while (field.MoveNext())
			{
				if (field.Current == null) continue;
				if (!field.Current.IsDefined(typeof(SettingsDataField), false)) continue;

				settings.SetValue(field.Current.Name, field.Current.GetValue(null).ToString(), true);
			}
			field.Dispose();
			fileNode.Save(TeamIconSettings.settingsConfigURL);
		}
		public static void Load()
		{
			ConfigNode fileNode = ConfigNode.Load(TeamIconSettings.settingsConfigURL);
			if (!fileNode.HasNode("IconSettings")) return;

			ConfigNode settings = fileNode.GetNode("IconSettings");

			IEnumerator<FieldInfo> field = typeof(TeamIconSettings).GetFields().AsEnumerable().GetEnumerator();
			while (field.MoveNext())
			{
				if (field.Current == null) continue;
				if (!field.Current.IsDefined(typeof(SettingsDataField), false)) continue;

				if (!settings.HasValue(field.Current.Name)) continue;
				object parsedValue = ParseValue(field.Current.FieldType, settings.GetValue(field.Current.Name));
				if (parsedValue != null)
				{
					field.Current.SetValue(null, parsedValue);
				}
			}
			field.Dispose();
		}
		public static object ParseValue(Type type, string value)
		{
			if (type == typeof(bool))
			{
				return Boolean.Parse(value);
			}
			else if (type == typeof(float))
			{
				return Single.Parse(value);
			}
			else if (type == typeof(string))
			{
				return value;
			}
			Debug.LogError("[BDArmory]: BDAPersistantSettingsField to parse settings field of type " + type +
						   " and value " + value);

			return null;
		}
	}
}