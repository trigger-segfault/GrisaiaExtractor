using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GrisaiaExtractor.Extensions;

namespace GrisaiaExtractor {
	[AttributeUsage(AttributeTargets.Field)]
	public class CodeAttribute : Attribute {

		//public string Code { get; set; }
		public string[] Codes { get; }
		public bool IgnoreCase { get; set; }
		
		public CodeAttribute(params string[] codes) {
			Codes = codes;
			IgnoreCase = false;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class NameAttribute : Attribute {
		public string Name { get; set; }
		
		public NameAttribute(string name) {
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class GroupAttribute : Attribute {
		public string Group { get; set; }

		public GroupAttribute(string group) {
			Group = group;
		}
	}

	public static class AttributeExtensions {
		public static string GetName(this MemberInfo member) {
			NameAttribute attr = member.GetCustomAttribute<NameAttribute>();
			return attr?.Name ?? member.Name;
		}
		public static string GetGroup(this MemberInfo member) {
			GroupAttribute attr = member.GetCustomAttribute<GroupAttribute>();
			return attr?.Group ?? "";
		}
		public static string GetDescription(this MemberInfo member) {
			DescriptionAttribute attr = member.GetCustomAttribute<DescriptionAttribute>();
			return attr?.Description;
		}

		/*public static string GetCode(this FieldInfo field) {
			CodeAttribute attr = field.GetCustomAttribute<CodeAttribute>();
			return attr?.Code ?? throw new CodeNotFoundException(field);
		}*/

		public static string[] GetCodes(this FieldInfo field) {
			CodeAttribute attr = field.GetCustomAttribute<CodeAttribute>();
			return attr?.Codes ?? throw new CodeNotFoundException(field);
		}

		public static bool HasCode(this FieldInfo field) {
			return field.GetCustomAttribute<CodeAttribute>() != null;
		}

		/*public static bool EqualsCode(this FieldInfo field, string code) {
			CodeAttribute attr = field.GetCustomAttribute<CodeAttribute>();
			return code?.Equals2(attr.Code, attr.IgnoreCase) ?? false;
		}*/

		/*public static string ToCode(this Enum value) {
			foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Static)) {
				string code = field.GetCode();
				if (code != null)
					return code;
			}
			return null;
		}*/

		public static string ToName(this Enum value) {
			foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Static)) {
				if (field.GetValue(null) == value)
					return field.GetName() ?? value.ToString();
			}
			return value.ToString();
		}

		public static FieldInfo GetField(this Enum value) {
			foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Static)) {
				if (field.GetValue(null) == value)
					return field;
			}
			return null;
		}

		public static IEnumerable<FieldInfo> GetFlagFields(this Enum value) {
			foreach (FieldInfo field in value.GetType().GetFields()) {
				Enum fieldValue = (Enum) field.GetValue(null);
				// Ignore default no-flag values when value is non-zero
				if (Convert.ToInt32(fieldValue) == 0 && Convert.ToInt32(value) != 0)
					continue;
				if (value.HasFlag(fieldValue))
					yield return field;
			}
		}
	}

	public class AttributeInfo {
		// General:
		/// <summary>The field for this enum value.</summary>
		public FieldInfo Field { get; }
		/// <summary>The enum's object value.</summary>
		public object EnumValue { get; }
		/// <summary>The enum's integer value.</summary>
		public int IntValue { get; }

		// Attributes:
		/// <summary>The code associated with the enum.</summary>
		public string[] Codes { get; }
		/// <summary>The readable name of the enum.</summary>
		public string Name { get; }
		/// <summary>The group of the enum.</summary>
		public string Group { get; }
		/// <summary>The description of the enum.</summary>
		public string Description { get; }

		/// <summary>True if a group is specified.</summary>
		public bool HasGroup => !string.IsNullOrWhiteSpace(Group);
		/// <summary>True if a description is specified.</summary>
		public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
		/// <summary>The longest length of the codes.</summary>
		public int CodeLength => Codes.Max(c => c.Length);

		/// <summary>The enum's object value.</summary>
		/*public object EnumValue {
			get { return Enum.ToObject(Field.DeclaringType, IntValue); }
		}*/

		/// <summary>Constructs the attribute info from the field.</summary>
		public AttributeInfo(FieldInfo field) {
			Field = field;
			EnumValue = field.GetValue(null);
			IntValue = Convert.ToInt32(EnumValue);

			Codes = field.GetCodes();
			Name = field.GetName();
			Group = field.GetGroup();
			Description = field.GetDescription();
		}
	}

	public class EnumInfo {
		// General:
		/// <summary>The type of the enum.</summary>
		public Type Type { get; }
		/// <summary>True if the enum is a flags type.</summary>
		public bool IsFlags { get; }

		// Codes:
		/// <summary>The default value if the enum is a flags type.</summary>
		public AttributeInfo DefaultValue { get; }
		/// <summary>The collection of codes.</summary>
		public Dictionary<string, AttributeInfo> Codes { get; }
		/// <summary>The sorted codes with longer code lengths appearing first.</summary>
		public List<AttributeInfo> SortedCodes { get; }

		/// <summary>Constructs the enum info from the type.</summary>
		public EnumInfo(Type type) {
			Type = type;
			IsFlags = Type.GetTypeInfo().GetCustomAttribute<FlagsAttribute>() != null;
			Codes = new Dictionary<string, AttributeInfo>();
			SortedCodes = new List<AttributeInfo>();

			foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public)) {
				AttributeInfo attrInfo = new AttributeInfo(field);
				// Ignore default no-flag values
				if (attrInfo.IntValue == 0 && IsFlags) {
					DefaultValue = attrInfo;
				}
				else {
					foreach (string code in attrInfo.Codes)
						Codes.Add(code, attrInfo);
					//Codes.Add(attrInfo.Code, attrInfo);
					SortedCodes.Add(attrInfo);
				}
			}
			SortedCodes.Sort((a, b) => b.CodeLength - a.CodeLength);
		}
	}

	public static class AttributeHelper {

		private static Dictionary<Type, EnumInfo> CachedEnums =
			new Dictionary<Type, EnumInfo>();
		
		/// <summary>Gets information about the enum and it's values.</summary>
		public static EnumInfo GetEnumInfo(Type type) {
			EnumInfo enumInfo;
			if (!CachedEnums.TryGetValue(type, out enumInfo)) {
				CachedEnums.Add(type, new EnumInfo(type));
			}
			return enumInfo;
		}

		/// <summary>Gets information about the enum and it's values.</summary>
		public static EnumInfo GetEnumInfo<TEnum>() {
			EnumInfo enumInfo;
			Type type = typeof(TEnum);
			if (!CachedEnums.TryGetValue(type, out enumInfo)) {
				enumInfo = new EnumInfo(type);
				CachedEnums.Add(type, enumInfo);
			}
			return enumInfo;
		}

		/// <summary>Parses the code and returns it as an enum.</summary>
		public static TEnum ParseCode<TEnum>(string code, out string unknownCode, TEnum defaultValue = default(TEnum)) {
			code = code ?? "";
			EnumInfo enumInfo = GetEnumInfo<TEnum>();
			if (enumInfo.IsFlags) {
				int value = 0;
				bool foundAny = false;
				foreach (AttributeInfo attrInfo in enumInfo.SortedCodes) {
					if (code.Length == 0)
						break;
					foreach (string attrCode in attrInfo.Codes) {
						int index = code.IndexOf(attrCode);
						if (index != -1) {
							foundAny = true;
							value |= attrInfo.IntValue;
							code = code.Remove(index, attrCode.Length);
							break;
						}
					}
				}
				unknownCode = code;
				if (foundAny)
					return (TEnum) Enum.ToObject(typeof(TEnum), value);
				return defaultValue;
			}
			else if (enumInfo.Codes.TryGetValue(code, out var attrInfo)) {
				unknownCode = "";
				return (TEnum) attrInfo.EnumValue;
			}
			else {
				unknownCode = code;
				return defaultValue;
			}
		}

		public static IEnumerable<AttributeInfo> GetAttributeInfos<TEnum>(TEnum enumValue, bool includeDefault = true) {
			int intValue = Convert.ToInt32(enumValue);
			EnumInfo enumInfo = GetEnumInfo<TEnum>();
			foreach (AttributeInfo attrInfo in enumInfo.Codes.Values) {
				if (enumInfo.IsFlags) {
					if ((intValue & attrInfo.IntValue) == intValue) {
						yield return attrInfo;
						includeDefault = false;
					}
				}
				else if (intValue == attrInfo.IntValue) {
					yield return attrInfo;
					includeDefault = false;
					break;
				}
			}
			if (includeDefault && enumInfo.IsFlags)
				yield return enumInfo.DefaultValue;
		}

		public static IEnumerable<string> GetNames<TEnum>(TEnum enumValue, bool includeDefault = true) {
			return GetAttributeInfos(enumValue, includeDefault).Select(a => a.Name);
		}

		public static IEnumerable<string> GetGroups<TEnum>(TEnum enumValue, bool includeDefault = true) {
			return GetAttributeInfos(enumValue, includeDefault).Select(a => a.Group);
		}

		public static AttributeInfo GetAttributeInfo<TEnum>(TEnum enumValue, bool includeDefault = true) {
			int intValue = Convert.ToInt32(enumValue);
			EnumInfo enumInfo = GetEnumInfo<TEnum>();
			foreach (AttributeInfo attrInfo in enumInfo.Codes.Values) {
				if (enumInfo.IsFlags) {
					if ((intValue & attrInfo.IntValue) == intValue)
						return attrInfo;
				}
				else if (intValue == attrInfo.IntValue)
					return attrInfo;
			}
			if (includeDefault && enumInfo.IsFlags)
				return enumInfo.DefaultValue;
			return null;
		}

		public static string GetName<TEnum>(TEnum enumValue, string defaultValue = null) {
			return GetAttributeInfo(enumValue).Name ?? defaultValue;
		}

		public static string GetGroup<TEnum>(TEnum enumValue, string defaultValue = null) {
			return GetAttributeInfo(enumValue).Group ?? defaultValue;
		}

		public static string GetCode<TEnum>(TEnum enumValue) {
			string code = "";
			foreach (AttributeInfo attrInfo in GetAttributeInfos(enumValue)) {
				code += attrInfo.Codes[0];
			}
			return code;
		}


		/*public static TEnum FromCode<TEnum>(string code, TEnum defaultValue = default(TEnum)) {
			foreach (FieldInfo field in typeof(TEnum).GetFields()) {
				if (field.EqualsCode(code))
					return (TEnum) field.GetValue(null);
			}
			return defaultValue;
		}

		public static Dictionary<string, int> GetCodeFlags<TEnum>() {
			Dictionary<string, int> flags = new Dictionary<string, int>();
			foreach (FieldInfo field in typeof(TEnum).GetFields()) {

			}
		}

		public static TEnum FromCodeFlags<TEnum>(string code, TEnum defaultValue = default(TEnum)) {
			int value = 0;
			bool foundAny = false;
			foreach (FieldInfo field in typeof(TEnum).GetFields()) {
				if (field.EqualsCode(code)) {
					value |= Convert.ToInt32(field.GetValue(null));
					foundAny = true;
				}
			}
			if (foundAny)
				return (TEnum) Enum.ToObject(typeof(TEnum), value);
			return defaultValue;
		}

		public static bool TryFromCode<TEnum>(string code, out TEnum result) {
			foreach (FieldInfo field in typeof(TEnum).GetFields()) {
				if (EqualsCode(field, code)) {
					result = (TEnum) field.GetValue(null);
					return true;
				}
			}
			result = default(TEnum);
			return false;
		}

		public static bool TryFromFlagsCode<TEnum>(string code, out TEnum result) {
			foreach (FieldInfo field in typeof(TEnum).GetFields()) {
				if (EqualsCode(field, code)) {
					result = (TEnum) field.GetValue(null);
					return true;
				}
			}
			result = default(TEnum);
			return false;
		}*/
	}
}
