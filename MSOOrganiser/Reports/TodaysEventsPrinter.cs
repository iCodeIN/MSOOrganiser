﻿using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MSOOrganiser.Reports
{
    public class TodaysEventsPrinter
    {
        public void Print(/* TODO date parameter */)
        {
           // var rg = new PentamindStandingsGenerator();
           // var results = rg.GetStandings();

            PrintDialog dlg = new PrintDialog();
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                using (new SpinnyCursor())
                {
                    FlowDocument doc = new FlowDocument();

                    doc.ColumnWidth = 770; // 96ths of an inch
                    doc.FontFamily = new FontFamily("Verdana");

                    /* ********** Header *********** */

                    Table headerTable = new Table() { CellSpacing = 0 };
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(220) });
                    headerTable.Columns.Add(new TableColumn() { Width = new GridLength(550) });
                    headerTable.RowGroups.Add(new TableRowGroup());

                    Image image = new Image();
                    image.Source = new BitmapImage(new Uri(@"pack://application:,,,/MSOOrganiser;component/Resources/Logo.png", UriKind.Absolute));

                    var trow = new TableRow();
                    trow.Cells.Add(new TableCell(new Paragraph(new InlineUIContainer(image)) { Margin = new Thickness(10), FontSize = 10, FontWeight = FontWeights.Bold }));
                    var cell = new TableCell();
                    cell.Blocks.Add(new Paragraph(new Run("18th Mind Sports Olympiad (2014)")) { Margin = new Thickness(10), FontSize = 24, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    cell.Blocks.Add(new Paragraph(new Run("Friday's Events")) { Margin = new Thickness(2), FontSize = 48, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center });
                    trow.Cells.Add(cell);
                    headerTable.RowGroups[0].Rows.Add(trow);

                    doc.Blocks.Add(headerTable);

                    /************ Main body *************/

                    Table bodyTable = new Table() { CellSpacing = 0 };
                    bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(60) });
                    bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(560) });
                    bodyTable.Columns.Add(new TableColumn() { Width = new GridLength(100) });
                    bodyTable.RowGroups.Add(new TableRowGroup());

                    var row = new TableRow();
                    row.Cells.Add(new TableCell(new Paragraph(new Run("A")) { Margin = new Thickness(8), FontSize = 36, FontWeight = FontWeights.Bold }));
                    var bodyCell = new TableCell();
                    bodyCell.Blocks.Add(new Paragraph(new Run("Abalone Olympiad Championship")) { Margin = new Thickness(2), FontSize = 18, FontWeight = FontWeights.Bold });
                    bodyCell.Blocks.Add(new Paragraph(new Run("        -> Learning Room 2")) { Margin = new Thickness(2), FontSize = 12 });
                    row.Cells.Add(bodyCell);
                    bodyCell = new TableCell();
                    bodyCell.Blocks.Add(new Paragraph(new Run(" ")) { Margin = new Thickness(2), FontSize = 18 });
                    bodyCell.Blocks.Add(new Paragraph(new Run("10.15 - 13.00")) { Margin = new Thickness(2), FontSize = 12 });
                    row.Cells.Add(bodyCell);

                    bodyTable.RowGroups[0].Rows.Add(row);

                    doc.Blocks.Add(bodyTable);

                    DocumentPaginator paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    dlg.PrintDocument(paginator, "Todays Events");
                }
            }
        }
    }
}
