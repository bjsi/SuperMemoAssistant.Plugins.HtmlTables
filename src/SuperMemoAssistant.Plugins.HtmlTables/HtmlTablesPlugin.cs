#region License & Metadata

// The MIT License (MIT)
// 
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// 
// 
// Created On:   7/12/2020 7:16:38 PM
// Modified By:  james

#endregion




namespace SuperMemoAssistant.Plugins.HtmlTables
{
  using System;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using System.Runtime.Remoting;
  using System.Windows;
  using System.Windows.Input;
  using mshtml;
  using SuperMemoAssistant.Plugins.DevContextMenu.Interop;
  using SuperMemoAssistant.Plugins.HtmlTables.UI;
  using SuperMemoAssistant.Services;
  using SuperMemoAssistant.Services.IO.HotKeys;
  using SuperMemoAssistant.Services.IO.Keyboard;
  using SuperMemoAssistant.Services.Sentry;
  using SuperMemoAssistant.Services.UI.Configuration;
  using SuperMemoAssistant.Sys.IO.Devices;
  using SuperMemoAssistant.Sys.Remoting;

  // ReSharper disable once UnusedMember.Global
  // ReSharper disable once ClassNeverInstantiated.Global
  [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
  public class HtmlTablesPlugin : SentrySMAPluginBase<HtmlTablesPlugin>
  {
    #region Constructors

    /// <inheritdoc />
    public HtmlTablesPlugin() : base("Enter your Sentry.io api key (strongly recommended)") { }

    #endregion

    #region Properties Impl - Public

    /// <inheritdoc />
    public override string Name => "HtmlTables";

    /// <inheritdoc />
    public override bool HasSettings => false;

    public HtmlTablesCfg Config;

    #endregion

    #region Methods Impl

    private void LoadConfig()
    {
      Config = Svc.Configuration.Load<HtmlTablesCfg>() ?? new HtmlTablesCfg();
    }


    /// <inheritdoc />
    protected override void PluginInit()
    {

      LoadConfig();

      RegisterDummyHotkeys();

      IntegrateOptionalServices();

    }

    private void IntegrateOptionalServices()
    {
      
      var svc = GetService<IDevContextMenu>();
      if (svc.IsNull())
        return;

      if (Config.AddDeleteRowMenuItem)
        svc.AddMenuItem(Name, "Delete Row", new ActionProxy(TableDeleteRow));

      if (Config.AddInsertRowMenuItem)
        svc.AddMenuItem(Name, "Insert Row", new ActionProxy(TableInsertRow));

      if (Config.AddInsertTableMenuItem)
        svc.AddMenuItem(Name, "Insert Table", new ActionProxy(TableInsertPrompt));

      if (Config.AddModifyTableMenuItem)
        svc.AddMenuItem(Name, "Modify Table", new ActionProxy(TableModifyPrompt));

    }

    private void RegisterDummyHotkeys()
    {

      // INSERT TABLE
      Svc.HotKeyManager
      .RegisterGlobal(
        "InsertHTMLTable",
        "Insert an HTML Table",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.DbeAlphanumeric),
        TableInsertPrompt
      )

      // MODIFY TABLE
      .RegisterGlobal(
        "ModifyHTMLTable",
        "Insert an HTML Table",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.DbeCodeInput),
        TableModifyPrompt
      )

      // INSERT ROW 
      .RegisterGlobal(
        "InsertHTMLTableRow",
        "Insert an HTML Table Row",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.DbeDbcsChar),
        TableInsertRow
      )

      // DELETE ROW
      .RegisterGlobal(
        "DeleteHTMLTableRow",
        "Delete an HTML Table Row",
        HotKeyScopes.SMBrowser,
        new HotKey(Key.DbeDetermineString),
        TableDeleteRow
      );

    }

    /// <inheritdoc />
    public override void ShowSettings()
    {
      ConfigurationWindow.ShowAndActivate(HotKeyManager.Instance, Config);
    }

    #endregion

    #region Table Processing Operations

    /// <summary>
    /// Method to create a table class
    /// Insert method then works on this table
    /// </summary>
    public void TableInsert(HtmlTableProperty tableProperties)
    {
      // call the private insert table method with a null table entry
      ProcessTable(null, tableProperties);

    }

    /// <summary>
    /// Method to modify a tables properties
    /// Ensure a table is currently selected or insertion point is within a table
    /// </summary>
    public bool TableModify(HtmlTableProperty tableProperties)
    {
      // define the Html Table element
      IHTMLTable table = GetTableElement();

      // if a table has been selected then process
      if (!table.IsNull())
      {
        ProcessTable(table, tableProperties);
        return true;
      }
      return false;
    }

    /// <summary>
    /// Get the first Html Control element from the current selection
    /// </summary>
    /// <returns>IHTMLElement or null</returns>
    private IHTMLElement GetFirstControl()
    {

      IHTMLElement control = null;

      try
      {

        var selection = ContentUtils.GetControlSelectionObject();

        // get the first control
        control = !selection.IsNull() && selection.length > 0
          ? selection.item(0)
          : null;

      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

      return control;

    }

    /// <summary>
    /// Method to present to the user the table properties dialog
    /// Uses all the default properties for the table based on an insert operation
    /// </summary>
    public void TableInsertPrompt()
    {
      // if user has selected a table create a reference
      var table = GetFirstControl() as IHTMLTable;
      ProcessTablePrompt(table);

    }

    /// <summary>
    /// Method to present to the user the table properties dialog
    /// Ensure a table is currently selected or insertion point is within a table
    /// </summary>
    public void TableModifyPrompt()
    {
      // define the Html Table element
      IHTMLTable table = GetTableElement();

      // if a table has been selected then process
      if (!table.IsNull())
        ProcessTablePrompt(table);

    }

    /// <summary>
    /// Method to insert a new row into the table
    /// Based on the current user row and insertion after
    /// </summary>
    public void TableInsertRow()
    {

      // see if a table selected or insertion point inside a table
      IHTMLTable table;
      IHTMLTableRow row;
      GetTableElement(out table, out row);

      // process according to table being defined
      if (!table.IsNull() && !row.IsNull())
      {
        try
        {

          // find the existing row the user is on and perform the insertion
          int index = row.rowIndex + 1;
          var insertedRow = (IHTMLTableRow)table.insertRow(index);
          // add the new columns to the end of each row
          int numberCols = row.cells.length;

          for (int idxCol = 0; idxCol < numberCols; idxCol++)
            insertedRow.insertCell();

        }
        catch (RemotingException) { }
        catch (UnauthorizedAccessException) { }
      }
    }

    /// <summary>
    /// Method to delete the currently selected row
    /// Operation based on the current user row location
    /// </summary>
    public void TableDeleteRow()
    {

      // see if a table selected or insertion point inside a table
      IHTMLTable table;
      IHTMLTableRow row;
      GetTableElement(out table, out row);

      // process according to table being defined
      if (!table.IsNull() && !row.IsNull())
      {

        try
        {
          // find the existing row the user is on and perform the deletion
          int index = row.rowIndex;
          table.deleteRow(index);

        }
        catch (RemotingException) { }
        catch (UnauthorizedAccessException) { }
      }

    }

    /// <summary>
    /// Method to present to the user the table properties dialog
    /// Uses all the default properties for the table based on an insert operation
    /// </summary>
    private void ProcessTablePrompt(IHTMLTable table)
    {

      HtmlTableProperty tableProperties = GetTableProperties(table);

      var res = Application.Current.Dispatcher.Invoke(() =>
      {

        var wdw = new TablePropertyWdw(tableProperties);
        wdw.ShowDialog();
        return wdw;

      });

      if (!res.Confirmed)
        return;

      tableProperties = res.Value;

      if (table.IsNull())
      {
        TableInsert(tableProperties);
      }
      else
      {
        ProcessTable(table, tableProperties);
      }

    }

    /// <summary>
    /// Method to insert a basic table
    /// Will honour the existing table if passed in
    /// </summary>
    private void ProcessTable(IHTMLTable table, HtmlTableProperty tableProperties)
    {
      try
      {

        // obtain a reference to the body node and indicate table present
        var document = ContentUtils.GetFocusedHtmlDocument();
        var bodyNode = document?.body;
        bool tableCreated = false;

        // ensure a table node has been defined to work with
        if (table.IsNull())
        {
          // create the table and indicate it was created
          table = (IHTMLTable)document.createElement("<table>");
          tableCreated = true;
        }

        // define the table border, width, cell padding and spacing
        table.border = tableProperties.BorderSize;
        if (tableProperties.TableWidth > 0) table.width = (tableProperties.TableWidthMeasurement == MeasurementOption.Pixel) ? string.Format("{0}", tableProperties.TableWidth) : string.Format("{0}%", tableProperties.TableWidth);
        else table.width = string.Empty;
        table.align = tableProperties.TableAlignment != HorizontalAlignOption.Default ? tableProperties.TableAlignment.ToString().ToLower() : string.Empty;
        table.cellPadding = tableProperties.CellPadding.ToString(CultureInfo.InvariantCulture);
        table.cellSpacing = tableProperties.CellSpacing.ToString(CultureInfo.InvariantCulture);

        // define the given table caption and alignment
        string caption = tableProperties.CaptionText;
        IHTMLTableCaption tableCaption = table.caption;
        if (!caption.IsNullOrEmpty())
        {
          // ensure table caption correctly defined
          if (tableCaption.IsNull()) tableCaption = table.createCaption();
          ((IHTMLElement)tableCaption).innerText = caption;
          if (tableProperties.CaptionAlignment != HorizontalAlignOption.Default) tableCaption.align = tableProperties.CaptionAlignment.ToString().ToLower();
          if (tableProperties.CaptionLocation != VerticalAlignOption.Default) tableCaption.vAlign = tableProperties.CaptionLocation.ToString().ToLower();
        }
        else
        {
          // if no caption specified remove the existing one
          if (!tableCaption.IsNull())
          {
            // prior to deleting the caption the contents must be cleared
            ((IHTMLElement)tableCaption).innerText = null;
            table.deleteCaption();
          }
        }

        // determine the number of rows one has to insert
        int numberRows, numberCols;
        if (tableCreated)
        {
          numberRows = Math.Max((int)tableProperties.TableRows, 1);
        }
        else
        {
          numberRows = Math.Max((int)tableProperties.TableRows, 1) - table.rows.length;
        }

        // layout the table structure in terms of rows and columns
        table.cols = tableProperties.TableColumns;
        if (tableCreated)
        {
          // this section is an optimization based on creating a new table
          // the section below works but not as efficiently
          numberCols = Math.Max((int)tableProperties.TableColumns, 1);
          // insert the appropriate number of rows
          IHTMLTableRow tableRow;
          for (int idxRow = 0; idxRow < numberRows; idxRow++)
          {
            tableRow = (IHTMLTableRow)table.insertRow();
            // add the new columns to the end of each row
            for (int idxCol = 0; idxCol < numberCols; idxCol++)
            {
              tableRow.insertCell();
            }
          }
        }
        else
        {
          // if the number of rows is increasing insert the decrepency
          if (numberRows > 0)
          {
            // insert the appropriate number of rows
            for (int idxRow = 0; idxRow < numberRows; idxRow++)
            {
              table.insertRow();
            }
          }
          else
          {
            // remove the extra rows from the table
            for (int idxRow = numberRows; idxRow < 0; idxRow++)
            {
              table.deleteRow(table.rows.length - 1);
            }
          }
          // have the rows constructed
          // now ensure the columns are correctly defined for each row
          IHTMLElementCollection rows = table.rows;
          foreach (IHTMLTableRow tableRow in rows)
          {
            numberCols = Math.Max((int)tableProperties.TableColumns, 1) - tableRow.cells.length;
            if (numberCols > 0)
            {
              // add the new column to the end of each row
              for (int idxCol = 0; idxCol < numberCols; idxCol++)
              {
                tableRow.insertCell();
              }
            }
            else
            {
              // reduce the number of cells in the given row
              // remove the extra rows from the table
              for (int idxCol = numberCols; idxCol < 0; idxCol++)
              {
                tableRow.deleteCell(tableRow.cells.length - 1);
              }
            }
          }
        }

        // if the table was created then it requires insertion into the DOM
        // otherwise property changes are sufficient
        if (tableCreated)
        {
          // table processing all complete so insert into the DOM
          var tableNode = (IHTMLDOMNode)table;
          var tableElement = (IHTMLElement)table;
          IHTMLTxtRange textRange = ContentUtils.GetTextSelectionObject();
          // final insert dependant on what user has selected
          if (!textRange.IsNull())
          {
            // text range selected so overwrite with a table
            try
            {
              string selectedText = textRange.text;
              if (!selectedText.IsNull())
              {
                // place selected text into first cell
                var tableRow = (IHTMLTableRow)table.rows.item(0, null);
                ((IHTMLElement)tableRow.cells.item(0, null)).innerText = selectedText;
              }
              textRange.pasteHTML(tableElement.outerHTML);
            }
            catch (RemotingException) { }
            catch (UnauthorizedAccessException) { }
          }
          else
          {
            IHTMLControlRange controlRange = ContentUtils.GetControlSelectionObject();
            if (!controlRange.IsNull())
            {
              // overwrite any controls the user has selected
              try
              {
                // clear the selection and insert the table
                // only valid if multiple selection is enabled
                for (int idx = 1; idx < controlRange.length; idx++)
                {
                  controlRange.remove(idx);
                }
                controlRange.item(0).outerHTML = tableElement.outerHTML;
                // this should work with initial count set to zero
                // controlRange.add((mshtmlControlElement)table);
              }
              catch (RemotingException) { }
              catch (UnauthorizedAccessException) { }
            }
            else
            {
              // insert the table at the end of the HTML
              ((IHTMLDOMNode)bodyNode).appendChild(tableNode);
            }
          }
        }
        else
        {
          // table has been correctly defined as being the first selected item
          // need to remove other selected items
          IHTMLControlRange controlRange = ContentUtils.GetControlSelectionObject();
          if (!controlRange.IsNull())
          {
            // clear the controls selected other than than the first table
            // only valid if multiple selection is enabled
            for (int idx = 1; idx < controlRange.length; idx++)
              controlRange.remove(idx);
          }
        }
      }
      catch (RemotingException) { }
      catch (UnauthorizedAccessException) { }

    } //ProcessTable

    /// <summary>
    /// Method to determine if the current selection is a table
    /// If found will return the table element
    /// </summary>
    private void GetTableElement(out IHTMLTable table, out IHTMLTableRow row)
    {
      row = null;
      var range = ContentUtils.GetTextSelectionObject();

      try
      {
        // first see if the table element is selected
        table = GetFirstControl() as IHTMLTable;
        // if table not selected then parse up the selection tree
        if (table.IsNull() && !range.IsNull())
        {
          var element = range.parentElement();
          // parse up the tree until the table element is found
          while (!element.IsNull() && table.IsNull())
          {
            element = element.parentElement;
            // extract the Table properties
            var htmlTable = element as IHTMLTable;
            if (!htmlTable.IsNull())
            {
              table = htmlTable;
            }
            // extract the Row  properties
            var htmlTableRow = element as IHTMLTableRow;
            if (!htmlTableRow.IsNull())
            {
              row = htmlTableRow;
            }
          }
        }
      }
      catch (Exception)
      {
        // have unknown error so set return to null
        table = null;
        row = null;
      }

    } //GetTableElement

    /// <summary>
    /// Method to return the currently selected Html Table Element
    /// </summary>
    private IHTMLTable GetTableElement()
    {
      // define the table and row elements and obtain there values
      IHTMLTable table;
      IHTMLTableRow row;
      GetTableElement(out table, out row);

      // return the defined table element
      return table;

    }

    /// <summary>
    /// Given an Html Table Element determines the table properties
    /// Returns the properties as an HtmlTableProperty class
    /// </summary>
    private HtmlTableProperty GetTableProperties(IHTMLTable table)
    {
      // define a set of base table properties
      var tableProperties = new HtmlTableProperty(true);

      // if user has selected a table extract those properties
      if (!table.IsNull())
      {
        try
        {
          // have a table so extract the properties
          IHTMLTableCaption caption = table.caption;
          // if have a caption persist the values
          if (!caption.IsNull())
          {
            tableProperties.CaptionText = ((IHTMLElement)table.caption).innerText;
            if (!caption.align.IsNull()) tableProperties.CaptionAlignment = (HorizontalAlignOption)typeof(HorizontalAlignOption).TryParseEnum(caption.align, HorizontalAlignOption.Default);
            if (!caption.vAlign.IsNull()) tableProperties.CaptionLocation = (VerticalAlignOption)typeof(VerticalAlignOption).TryParseEnum(caption.vAlign, VerticalAlignOption.Default);
          }
          // look at the table properties
          if (!GeneralUtils.IsNull(table.border)) tableProperties.BorderSize = GeneralUtils.TryParseByte(table.border.ToString(), tableProperties.BorderSize);
          if (!table.align.IsNull()) tableProperties.TableAlignment = (HorizontalAlignOption)typeof(HorizontalAlignOption).TryParseEnum(table.align, HorizontalAlignOption.Default);
          // define the table rows and columns
          int rows = Math.Min(table.rows.length, Byte.MaxValue);
          int cols = Math.Min(table.cols, Byte.MaxValue);
          if (cols == 0 && rows > 0)
          {
            // cols value not set to get the maxiumn number of cells in the rows
            foreach (IHTMLTableRow tableRow in table.rows)
            {
              cols = Math.Max(cols, tableRow.cells.length);
            }
          }
          tableProperties.TableRows = (byte)Math.Min(rows, byte.MaxValue);
          tableProperties.TableColumns = (byte)Math.Min(cols, byte.MaxValue);
          // define the remaining table properties
          if (!GeneralUtils.IsNull(table.cellPadding)) tableProperties.CellPadding = GeneralUtils.TryParseByte(table.cellPadding.ToString(), tableProperties.CellPadding);
          if (!GeneralUtils.IsNull(table.cellSpacing)) tableProperties.CellSpacing = GeneralUtils.TryParseByte(table.cellSpacing.ToString(), tableProperties.CellSpacing);
          if (!GeneralUtils.IsNull(table.width))
          {
            string tableWidth = table.width.ToString();
            if (tableWidth.TrimEnd(null).EndsWith("%"))
            {
              tableProperties.TableWidth = tableWidth.Remove(tableWidth.LastIndexOf("%", StringComparison.Ordinal), 1).TryParseUshort(tableProperties.TableWidth);
              tableProperties.TableWidthMeasurement = MeasurementOption.Percent;
            }
            else
            {
              tableProperties.TableWidth = tableWidth.TryParseUshort(tableProperties.TableWidth);
              tableProperties.TableWidthMeasurement = MeasurementOption.Pixel;
            }
          }
          else
          {
            tableProperties.TableWidth = 0;
            tableProperties.TableWidthMeasurement = MeasurementOption.Pixel;
          }
        }
        catch (RemotingException) { }
        catch (UnauthorizedAccessException) { }
      }

      return tableProperties;

    }

    /// <summary>
    /// Method to return  a table defintion based on the user selection
    /// If table selected (or insertion point within table) returns these values
    /// </summary>
    public void GetTableDefinition(out HtmlTableProperty table, out bool tableFound)
    {
      // see if a table selected or insertion point inside a table
      IHTMLTable htmlTable = GetTableElement();

      // process according to table being defined
      if (htmlTable.IsNull())
      {
        table = new HtmlTableProperty(true);
        tableFound = false;
      }
      else
      {
        table = GetTableProperties(htmlTable);
        tableFound = true;
      }

    }

    /// <summary>
    /// Method to determine if the insertion point or selection is a table
    /// </summary>
    private bool IsParentTable()
    {
      // see if a table selected or insertion point inside a table
      IHTMLTable htmlTable = GetTableElement();

      // process according to table being defined
      if (htmlTable.IsNull())
      {
        return false;
      }
      return true;
    }

    #endregion
  }
}
