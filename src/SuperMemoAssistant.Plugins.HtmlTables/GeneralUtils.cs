using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.HtmlTables
{
  public static class GeneralUtils
  {
    /// <summary>
    /// Determine whether the object is null
    /// </summary>
    /// <param name="obj"></param>
    public static bool IsNull(this object obj)
    {
      return obj == null;
    }

    /// <summary>
    /// Determine whether the string is null or empty
    /// </summary>
    /// <param name="str"></param>
    public static bool IsNullOrEmpty(this string str)
    {
      return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// Method to perform a parse of a string into a byte number
    /// </summary>
    public static byte TryParseByte(this string stringValue, byte defaultValue)
    {
      // define the return type
      byte result;

      // try the conversion to a double number
      if (!byte.TryParse(stringValue, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result))
      {
        // default value will be returned
        result = defaultValue;
      }

      // return the byte value
      return result;

    }
    /// <summary>
    /// Method to perform a parse of a string into a ushort number
    /// </summary>
    public static ushort TryParseUshort(this string stringValue, ushort defaultValue)
    {
      // define the return type
      ushort result;

      // try the conversion to a double number
      if (!ushort.TryParse(stringValue, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out result))
      {
        // default value will be returned
        result = defaultValue;
      }

      // return the ushort value
      return result;

    }


    /// <summary>
    /// Method to perform a parse of the string into an enum
    /// </summary>
    public static object TryParseEnum(this Type enumType, string stringValue, object defaultValue)
    {
      // try the enum parse and return the default 
      object result;
      try
      {
        // try the enum parse operation
        result = Enum.Parse(enumType, stringValue, true);
      }
      catch (Exception)
      {
        // default value will be returned
        result = defaultValue;
      }

      // return the enum value
      return result;

    }
  }
}
