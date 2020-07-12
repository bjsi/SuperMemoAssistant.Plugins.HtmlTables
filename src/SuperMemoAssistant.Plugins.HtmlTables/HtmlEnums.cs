using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.HtmlTables
{

  /// <summary>
  /// Enum used to define the text alignment property
  /// </summary>
  public enum HorizontalAlignOption
  {

    Default,
    Left,
    Center,
    Right

  }

  /// <summary>
  /// Enum used to define the vertical alignment property
  /// </summary>
  public enum VerticalAlignOption
  {

    Default,
    Top,
    Bottom

  }

  /// <summary>
  /// Enum used to define the unit of measure for a value
  /// </summary>
  public enum MeasurementOption
  {

    Pixel,
    Percent

  }

}
