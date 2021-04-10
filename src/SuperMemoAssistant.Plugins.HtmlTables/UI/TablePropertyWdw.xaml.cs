using mshtml;
using SuperMemoAssistant.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SuperMemoAssistant.Plugins.HtmlTables.UI
{
  /// <summary>
  /// Interaction logic for TablePropertyWdw.xaml
  /// </summary>
  public partial class TablePropertyWdw : Window
  {

    public bool Confirmed { get; set; } = false;
    public HtmlTableProperty Value { get; set; }
    private static HtmlTablesCfg Config => Svc<HtmlTablesPlugin>.Plugin.Config;

    public string TableCaption { get; set; } = Config.TableCaption;
    public string BorderSize { get; set; } = Config.BorderSize;
    public string TableRows { get; set; } = Config.TableRows;
    public string TableColumns { get; set; } = Config.TableColumns;
    public string TableWidth { get; set; } = Config.TableWidth;
    public string CellPadding { get; set; } = Config.CellPadding;
    public string CellSpacing { get; set; } = Config.CellSpacing;

    public TablePropertyWdw(HtmlTableProperty props)
    {

      InitializeComponent();

      // Set Table 
      this.TableCaption = props.CaptionText;
      this.BorderSize = props.BorderSize.ToString();
      this.TableRows = props.TableRows.ToString();
      this.TableColumns = props.TableColumns.ToString();
      this.TableWidth = props.TableWidth.ToString();
      this.CellPadding = props.CellPadding.ToString();
      this.CellSpacing = props.CellSpacing.ToString();

      DataContext = this;

    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {

      Confirmed = false;
      Close();

    }

    private void CreateButtonClick(object sender, RoutedEventArgs e)
    {

      var props = new HtmlTableProperty();

      // Caption Text
      props.CaptionText = TableCaptionBox.Text ?? string.Empty;

      // Caption Alignment
      if (CaptionAlignmentDefault.IsChecked == true)
        props.CaptionAlignment = HorizontalAlignOption.Default;
      else if (CaptionAlignmentCenter.IsChecked == true)
        props.CaptionAlignment = HorizontalAlignOption.Center;
      else if (CaptionAlignmentLeft.IsChecked == true)
        props.CaptionAlignment = HorizontalAlignOption.Left;
      else if (CaptionAlignmentRight.IsChecked == true)
        props.CaptionAlignment = HorizontalAlignOption.Right;

      // Caption Location
      if (CaptionLocationBottom.IsChecked == true)
        props.CaptionLocation = VerticalAlignOption.Bottom;
      else if (CaptionLocationTop.IsChecked == true)
        props.CaptionLocation = VerticalAlignOption.Top;
      else if (CaptionLocationDefault.IsChecked == true)
        props.CaptionLocation = VerticalAlignOption.Default;

      // Table alignment
      if (TableAlignmentCenter.IsChecked == true)
        props.TableAlignment = HorizontalAlignOption.Center;
      else if (TableAlignmentDefault.IsChecked == true)
        props.TableAlignment = HorizontalAlignOption.Default;
      else if (TableAlignmentLeft.IsChecked == true)
        props.TableAlignment = HorizontalAlignOption.Left;
      else if (TableAlignmentRight.IsChecked == true)
        props.TableAlignment = HorizontalAlignOption.Right;

      // Border Size
      props.BorderSize = BorderSizeBox.Text.TryParseByte(2);

      // Table Rows
      props.TableRows = TableRowsBox.Text.TryParseByte(3);

      // Table Columns
      props.TableColumns = TableColumnsBox.Text.TryParseByte(3);

      // Table Width
      props.TableWidth = TableWidthBox.Text.TryParseUshort(50);

      // Table Width Measurement
      if (WidthMeasurementPercent.IsChecked == true)
        props.TableWidthMeasurement = MeasurementOption.Percent;
      else if (WidthMeasurementPixel.IsChecked == true)
        props.TableWidthMeasurement = MeasurementOption.Pixel;

      // Cell Padding
      props.CellPadding = CellPaddingBox.Text.TryParseByte(1);

      // Cell Spacing
      props.CellSpacing = CellSpacingBox.Text.TryParseByte(2);

      Value = props;
      Confirmed = true;
      Close();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
        CreateButtonClick(null, null);

      else if (e.Key == Key.Escape)
        Close();
    }
  }
}
