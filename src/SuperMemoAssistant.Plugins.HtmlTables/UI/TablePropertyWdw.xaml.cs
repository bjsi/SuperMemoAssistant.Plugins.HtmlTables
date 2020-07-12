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

    public TablePropertyWdw(HtmlTableProperty props)
    {
      InitializeComponent();
    }
  }
}
