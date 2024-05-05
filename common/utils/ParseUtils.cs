using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace common.utils;

public static class ParseUtils
{
    public static string ParseString(this XElement element, string name, string undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return value;
    }

    public static int ParseInt(this XElement element, string name, int undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return int.Parse(value);
    }

    public static long ParseLong(this XElement element, string name, long undefined = 0)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        return long.Parse(value);
    }

    public static uint ParseUInt(this XElement element, string name, bool isHex = true, uint undefined = 0)
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

    //public static ConditionEffectIndex ParseConditionEffect(this XElement element, string name, ConditionEffectIndex undefined = ConditionEffectIndex.Dead)
    //{
    //    var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
    //    if (string.IsNullOrWhiteSpace(value))
    //        return undefined;
    //    return (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), value.Replace(" ", ""));
    //}

    public static T ParseEnum<T>(this XElement element, string name, T undefined) where T : Enum
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;

        try {
            return (T)Enum.Parse(typeof(T), value.Replace(" ", ""), true);
        }catch (Exception) {
            Console.WriteLine($"{value} not found in {nameof(T)} enum");
            return undefined;
        }
    }

    public static string[] ParseStringArray(this XElement element, string name, char seperator, string[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = Regex.Replace(value, @"\s+", "");
        return value.Split(seperator);
    }

    public static int[] ParseIntArray(this XElement element, string name, char seperator, int[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = Regex.Replace(value, @"\s+", "");
        return ParseStringArray(element, name, seperator).Select(k => int.Parse(k)).ToArray();
    }

    public static ushort[] ParseUshortArray(this XElement element, string name, char seperator, ushort[] undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = Regex.Replace(value, @"\s+", "");
        return ParseStringArray(element, name, seperator).Select(k => (ushort)(k.StartsWith("0x") ? Int32.Parse(k.Substring(2), NumberStyles.HexNumber) : Int32.Parse(k))).ToArray();
    }

    public static List<int> ParseIntList(this XElement element, string name, char seperator, List<int> undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = Regex.Replace(value, @"\s+", "");
        return ParseStringArray(element, name, seperator).Select(k => int.Parse(k)).ToList();
    }

    public static List<uint> ParseUIntList(this XElement element, string name, char seperator, List<uint> undefined = null)
    {
        var value = name[0].Equals('@') ? element.Attribute(name.Remove(0, 1))?.Value : element.Element(name)?.Value;
        if (string.IsNullOrWhiteSpace(value)) return undefined;
        value = Regex.Replace(value, @"\s+", "");
        return ParseStringArray(element, name, seperator).Select(k => uint.Parse(k)).ToList();
    }
}
