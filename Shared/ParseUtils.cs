using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Shared;
public static partial class ParseUtils
{
    public static string ParseString(this XElement element, string name, string undefined = "")
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return value;
    }

    public static int ParseInt(this XElement element, string name, int undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return (value.StartsWith("0x") ? int.Parse(value[2..], NumberStyles.HexNumber) : int.Parse(value));
    }

    public static long ParseLong(this XElement element, string name, long undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return long.Parse(value);
    }

    public static uint ParseUInt(this XElement element, string name, bool isHex = false, uint undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return Convert.ToUInt32(value, isHex ? 16 : 10);
    }

    public static float ParseFloat(this XElement element, string name, float undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return float.Parse(value, CultureInfo.InvariantCulture);
    }

    public static bool ParseBool(this XElement element, string name, bool undefined = false)
    {
        var isAttr = name[0].Equals('@');
        var id = name[0].Equals('@') ? name.Remove(0, 1) : name;
        var value = isAttr ? element.Attribute(id)?.Value : element.Element(id)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            if (isAttr && element.Attribute(id) != null || !isAttr && element.Element(id) != null)
                return true;
            return undefined;
        }
        return bool.Parse(value);
    }

    public static ushort ParseUshort(this XElement element, string name, ushort undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return (ushort)(value.StartsWith("0x") ? Int32.Parse(value.Substring(2), NumberStyles.HexNumber) : Int32.Parse(value));
    }
    public static T ParseEnum<T>(this XElement element, string name, T undefined) where T : Enum
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;

        try {
            return (T)Enum.Parse(typeof(T), value.Replace(" ", ""), true);
        }catch (Exception) {
            SLog.Error($"{value} not found in {typeof(T).Name} enum");
            return undefined;
        }
    }

    public static string[] ParseStringArray(this XElement element, string name, char seperator, string[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return value.Split(seperator);
    }

    public static int[] ParseIntArray(this XElement element, string name, char seperator, int[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return ParseStringArray(element, name, seperator).Select(k => int.Parse(k)).ToArray();
    }
    public static uint[] ParseUIntArray(this XElement element, string name, char seperator, uint[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return ParseStringArray(element, name, seperator).Select(uint.Parse).ToArray();
    }

    public static ushort[] ParseUshortArray(this XElement element, string name, char seperator, ushort[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return ParseStringArray(element, name, seperator).Select(k => (ushort)(k.StartsWith("0x") ? Int32.Parse(k.Substring(2), NumberStyles.HexNumber) : Int32.Parse(k))).ToArray();
    }

    public static List<int> ParseIntList(this XElement element, string name, char seperator, List<int> undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return ParseStringArray(element, name, seperator).Select(k => int.Parse(k)).ToList();
    }

    public static List<uint> ParseUIntList(this XElement element, string name, char seperator, List<uint> undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = SpaceRegex().Replace(value, "");
        return ParseStringArray(element, name, seperator).Select(k => uint.Parse(k)).ToList();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
}
