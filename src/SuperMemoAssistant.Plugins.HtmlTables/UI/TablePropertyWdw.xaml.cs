using mshtml;
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

    // Values
    public string TableCaption { get; set; } = string.Empty;
    public string BorderSize { get; set; } = "2";
    public string TableRows { get; set; } = "3";
    public string TableColumns { get; set; } = "3";
    public string TableWidth { get; set; } = "50";
    public string CellPadding { get; set; } = "1";
    public string CellSpacing { get; set; } = "2";

    public TablePropertyWdw(HtmlTableProperty props)
    {

      InitializeComponent();


      DataContext = this;

    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {

      Confirmed = false;
      Close();

    }

    private void CreateButtonClick(object sender, RoutedEventArgs e)
    {

      //Value = new HtmlTableProperty();
      //Value.CaptionText = TableCaptionBox.Text ?? string.Empty;

      //if (CaptionAlignmentDefault.IsChecked == true)
      //  Value.CaptionAlignment = HorizontalAlignOption.Default;
      //else if (CaptionAlignmentDefault)
      //Value.CaptionAlignment = 


    }
  }
}
