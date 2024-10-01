// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.PowerShell.ConsoleGuiTools.Models;

using NStack;

using Terminal.Gui;

namespace Microsoft.PowerShell.ConsoleGuiTools;

internal sealed class GridViewDataSource(IEnumerable<GridViewRow> gridViewRowList) : IListDataSource
{
    internal List<GridViewRow> GridViewRowList { get; init; } = gridViewRowList.ToList();

    public int Count => GridViewRowList.Count;

    public int Length { get; }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start)
    {
        container.Move(col, line);
        RenderUstr(driver, GridViewRowList[item].DisplayString, width);
    }

    public bool IsMarked(int item) => GridViewRowList[item].IsMarked;

    public void SetMark(int item, bool value)
    {
        var oldValue = GridViewRowList[item].IsMarked;
        GridViewRowList[item].IsMarked = value;
        var args = new RowMarkedEventArgs()
        {
            Row = GridViewRowList[item],
            OldValue = oldValue
        };
        MarkChanged?.Invoke(this, args);
    }

    public sealed class RowMarkedEventArgs : EventArgs
    {
        public GridViewRow Row { get; set; }
        public bool OldValue { get; set; }

    }

    public event EventHandler<RowMarkedEventArgs> MarkChanged;

    public IList ToList()
    {
        return GridViewRowList;
    }

    // A slightly adapted method from gui.cs: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
    private static void RenderUstr(ConsoleDriver driver, ustring ustr, int width)
    {
        int used = 0;
        int index = 0;
        while (index < ustr.Length)
        {
            (var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
            var count = Rune.ColumnWidth(rune);
            if (used + count > width) break;
            driver.AddRune(rune);
            used += count;
            index += size;
        }

        while (used < width)
        {
            driver.AddRune(' ');
            used++;
        }
    }
}
