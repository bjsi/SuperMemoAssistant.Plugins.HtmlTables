using Forge.Forms.Annotations;
using Newtonsoft.Json;
using SuperMemoAssistant.Services.UI.Configuration;
using SuperMemoAssistant.Sys.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Plugins.HtmlTables
{
  [Form(Mode = DefaultFields.None)]
  [Title("Dictionary Settings",
       IsVisible = "{Env DialogHostContext}")]
  [DialogAction("cancel",
      "Cancel",
      IsCancel = true)]
  [DialogAction("save",
      "Save",
      IsDefault = true,
      Validates = true)]
  public class HtmlTablesCfg : CfgBase<HtmlTablesCfg>, INotifyPropertyChangedEx
  {
    [Title("Html Tables Plugin")]
    [Heading("By Jamesb | Experimental Learning")]

    [Heading("Features:")]
    [Text(@"- Easily create and modify HTML tables.
- Create hotkey bindings for HTML table-related operations.
- Optionally integrates with DevContextMenu.")]

    [Heading("Plugin Integration Settings")]
    [Heading("Dev Context Menu")]

    [Field(Name = @"Add ""Insert Table"" command to Dev Context Menu?")]
    public bool AddInsertTableMenuItem { get; set; } = true;

    [Field(Name = @"Add ""Modify Table"" command to Dev Context Menu?")]
    public bool AddModifyTableMenuItem { get; set; } = true;

    [Field(Name = @"Add ""Insert Row"" command to Dev Context Menu?")]
    public bool AddInsertRowMenuItem { get; set; } = true;

    [Field(Name = @"Add ""Delete Row"" command to Dev Context Menu?")]
    public bool AddDeleteRowMenuItem { get; set; } = true;

    [JsonIgnore]
    public bool IsChanged { get; set; }

    public override string ToString()
    {
      return "Html Tables Settings";
    }

    public event PropertyChangedEventHandler PropertyChanged;
  }
}
