﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using MSOCore;
using MSOOrganiser.Reports;
using MSOOrganiser.Dialogs;
using MSOOrganiser.UIUtilities;
using System.Diagnostics;
using AutoUpdaterForWPF;
using MSOOrganiser.Data;
using Newtonsoft.Json;
using MSOCore.Calculators;
using System.Timers;
using MSOCore.Models;
using System.Windows.Threading;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _updateCheckDone = false;
        private DispatcherTimer _databaseCheckTimer = new DispatcherTimer();

        private int LoggedInUserId { get; set; }
        private int UserLoginId { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowVm();

            _databaseCheckTimer.Interval = new TimeSpan(0, 0, 20);
            _databaseCheckTimer.Tick += _databaseCheckTimer_Tick;
        }

        public MainWindowVm ViewModel
        {
            get { return DataContext as MainWindowVm;  }
        }

        void _databaseCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var context = DataEntitiesProvider.Provide();
                var thisOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
                ViewModel.DbStatus = "Connected: " + DateTime.Now.ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                ViewModel.DbStatus = "Connection failed: " + DateTime.Now.ToString("HH:mm:ss");
            }
        }

        private void window_Loaded(object sender, EventArgs e)
        {
            CheckForUpdates();

            ConnectionStringUpdater.Update();

            var lastLoggedIn = GetLastLoggedInUser();

            var loginBox = new LoginWindow(lastLoggedIn);
            loginBox.ShowDialog();
            if (loginBox.UserId == 0)
            {
                MessageBox.Show("Invalid user/password");
                this.Close();
            }

            WriteLoggedInUserToRegistry(loginBox.UserName);

            _databaseCheckTimer.Start();

            LoggedInUserId = loginBox.UserId;
            UserLoginId = loginBox.UserLoginId;
            this.Title += " --- logged in as " + loginBox.UserName
                + " --- " + DataEntitiesProvider.Description();

            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new StartupPanel();
            dockPanel.Children.Add(panel);
        }

        private void WriteLoggedInUserToRegistry(string userId)
        {
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\JuliaHayward\MSOOrganiser");
            key.SetValue("LastLoginId", userId);
            key.Close();
        }

        private string GetLastLoggedInUser()
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\JuliaHayward\MSOOrganiser");
            if (key == null) return "";

            var value = key.GetValue("LastLoginId");
            key.Close();
            return (string)value ?? "";
        }

        private void CheckForUpdates()
        {
            try
            {
                var autoUpdater = new AutoUpdater();
                var result = autoUpdater.DoUpdate("http://apps.juliahayward.com/MSOOrganiser");
                if (result == AutoUpdateResult.UpdateInitiated)
                {
                    this.Close();
                    Environment.Exit(0);
                }
            }
            catch (Exception) { /* don't bother logging */ }
            _updateCheckDone = true;
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_updateCheckDone)
                return;

            var context = DataEntitiesProvider.Provide();
            var user = context.Users.Find(LoggedInUserId);
            if (user != null)
            {
                var login = user.UserLogins.Where(x => x.Id == UserLoginId && x.LogOutDate == null)
                    .OrderByDescending(x => x.LogInDate).FirstOrDefault();
                if (login != null)
                {
                    login.LogOutDate = DateTime.UtcNow;
                    context.SaveChanges();
                }
            }
        }


        private void changePasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var passwordDialog = new PasswordChangeWindow() { UserId = LoggedInUserId };
            passwordDialog.ShowDialog();
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void olympiadsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
                var panel = new OlympiadPanel();
                panel.Populate();
                panel.EventSelected += panel_EventSelected;
                dockPanel.Children.Add(panel);
            }
        }

        private void competitorsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
                var panel = new ContestantPanel();
                panel.Populate();
                panel.EventSelected += panel_EventSelected;
                dockPanel.Children.Add(panel);
            }
        }

        void panel_EventSelected(object sender, Events.EventEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
                var panel = new EventPanel();
                panel.Populate(e.EventCode, e.OlympiadId);
                panel.ContestantSelected += panel_ContestantSelected;
                dockPanel.Children.Add(panel);
            }
        }


        private void gamesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
                var panel = new GamePanel();
                panel.Populate();
                dockPanel.Children.Add(panel);
            }
        }

        private void nationalitiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new NationalityReport();
           // panel.Populate();
            dockPanel.Children.Add(panel);

            Cursor = Cursors.Arrow;
        }

        private void resultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new EventPanel();
            panel.Populate();
            panel.ContestantSelected += panel_ContestantSelected;
            dockPanel.Children.Add(panel);
        }

        void panel_ContestantSelected(object sender, Events.ContestantEventArgs e)
        {
            using (new SpinnyCursor())
            {
                if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
                var panel = new ContestantPanel();
                panel.Populate(e.ContestantId);
                panel.EventSelected += panel_EventSelected;
                dockPanel.Children.Add(panel);
            }
        }

        private void displayResultsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dockPanel.Children.Count > 2) dockPanel.Children.RemoveAt(2);
            var panel = new DisplayResultsPanel();
            dockPanel.Children.Add(panel);
        }

        private void printEventEntriesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EventEntriesReportPicker();
            if (dialog.ShowDialog().Value)
            {
                var printer = new PrintEventEntriesReportPrinter();
                if (dialog.UseEvent)
                    printer.Print(dialog.EventCode);
                else
                    printer.Print(dialog.StartDate, dialog.EndDate);
            }
        }

        private void parkingListsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This does not appear to work in the Access version - please contact Julia");
        }

        private void entrySummaryMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var documentPrinter = new FlowDocumentPrinter();
            var printer = new PrintEventEntriesSummaryReportPrinter();
            documentPrinter.PrintFlowDocument(() => printer.Print());
        }

        private void contactsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PrintContactsReportPrinter();
            printer.Print();
        }

        private void maxFeePentaCards_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This does not appear to work in the Access version - please contact Julia");
        }

        private void medalTable_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new MedalTablePrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());
        }

        private void donationsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new DonationPrinter();
            printer.Print();
        }

        private void medalFormsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EventEntriesReportPicker();
            if (dialog.ShowDialog().Value)
            {
                var printer = new MedalFormsPrinter();
                if (dialog.UseEvent)
                    printer.Print(dialog.EventCode);
                else
                    printer.Print(dialog.StartDate, dialog.EndDate);
            }
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AboutDialog();
            dialog.ShowDialog();
        }

        private void pentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.Print();
        }

        private void juniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintJunior();
        }

        private void seniorPentamindStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintSenior();
        }

        private void pokerStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PokerStandingsPrinter();
            printer.Print();
        }

        private void eurogamesStandings_Click(object sender, RoutedEventArgs e)
        {
            var printer = new PentamindStandingsPrinter();
            printer.PrintEuro();
        }

        private void eventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventIncomeReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(true));
        }

        private void nonEventIncomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventIncomeReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print(false));
        }

        private void gamePlanMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var printer = new GamePlanPrinter();
            printer.Print();
        }

        private void locationUse_Click(object sender, RoutedEventArgs e)
        {
            var printer = new LocationUsePrinter();
            printer.Print();
        }

        private void arbiterSchedule_Click(object sender, RoutedEventArgs e)
        {
            var printer = new ArbiterSchedulePrinter();
            printer.Print();
        }


        private void printTodaysEventsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SelectDateDialog();
            if (dlg.ShowDialog().Value)
            {
                var docPrinter = new FlowDocumentPrinter();
                var printer = new TodaysEventsPrinter();    
                docPrinter.PrintFlowDocument(() => printer.Print(dlg.SelectedDate));
            }
        }

        private void daysReportMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SelectDateDialog();
            if (dlg.ShowDialog().Value)
            {
                var docPrinter = new FlowDocumentPrinter();
                docPrinter.PrintFlowDocuments(() => FlowDocumentsForDay(dlg.SelectedDate));
            }
        }

        private IEnumerable<FlowDocument> FlowDocumentsForDay(DateTime selectedDate)
        {
            //  1. Events per Session (for the right day; not normally independent)
            var printer1 = new TrafficReportPrinter();
            yield return printer1.PrintEventsPerSession(selectedDate);

            //  2. Todays Events (for the right day)
            var printer2 = new TodaysEventsPrinter();
            yield return printer2.Print(selectedDate);

            // TODO TODO TODO 
            // This is a combination of existing reports (1,2,3,5 are for today, rest are as at now
            //  3.

            //  4.  Event Results for today, concatenated
            var printer4 = new TodaysEventsResultsPrinter();
            yield return printer4.Print(selectedDate);

            //  5.  Traffic report (for the right day)
            yield return printer1.Print(selectedDate);

            //  6.  Income summary
            var printer6 = new EventIncomeReportPrinter();
            yield return printer6.Print(true);

            //  7.  Entry Summary
            var printer7 = new PrintEventEntriesSummaryReportPrinter();
            yield return printer7.Print();
            
            //  8.  Medal table
            var printer8 = new MedalTablePrinter();
            yield return printer8.Print();

            //  9.  Event Entries (for the right day TODO TODO doesn't work in Access??)
        }

        private void trafficReport_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new TrafficReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());
        }

        private void printEventsPerSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var docPrinter = new FlowDocumentPrinter();
            var printer = new TrafficReportPrinter();
            docPrinter.PrintFlowDocument(() => printer.PrintEventsPerSession());
        }

        private void eventLabelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please ensure that sticky labels are in the printer");

            var docPrinter = new FlowDocumentPrinter();
            var printer = new EventLabelsPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());

            MessageBox.Show("Please take the labels out of the printer now");
        }

        private void arbitersBadgesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please ensure that YELLOW paper is in the printer");

            var docPrinter = new FlowDocumentPrinter();
            var printer = new ArbitersBadgesPrinter();
            docPrinter.PrintFlowDocument(() => printer.Print());

            MessageBox.Show("Please take the yellow paper out of the printer now");
        }


        private void contestantList_Click(object sender, RoutedEventArgs e)
        {
            var exporter = new ContestantListCsvExporter();
            exporter.ExportThisYearsContestants();
        }

        private void help_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.juliahayward.com/MSO/Help.html");
        }

        private void copyEventsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var thisOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();

            if (thisOlympiad.Events.Any())
            {
                MessageBox.Show("You cannot do this if the current Olympiad already has events.");
                return;
            }

            var previousOlympiad = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).Skip(1).First();

            this.Status.Text = "Copying from " + previousOlympiad.YearOf + " to " + thisOlympiad.YearOf;
            var dateShift = thisOlympiad.StartDate.Value.Subtract(previousOlympiad.StartDate.Value);

            foreach (var evt in previousOlympiad.Events)
            {
                var newE = evt.CopyTo(thisOlympiad);
                // This is a copy, so move the dates along by the same amount
                foreach (var es in newE.Event_Sess)
                {
                    es.Date = es.Date.Value.Add(dateShift);
                }
                thisOlympiad.Events.Add(newE);
            }

            context.SaveChanges();
            this.Status.Text = "";
        }

        private void printResults_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Event results can be printed from the individual event panel");
        }

        private void prepayments_Click(object sender, RoutedEventArgs e)
        {
            using (new SpinnyCursor())
            {
                var processor = new PaymentProcessor();
                processor.ProcessAll();
            }
        }

        private void unspentFee_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiadId = context.Olympiad_Infoes.First(x => x.Current).Id;

            var payments = context.Payments.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.MindSportsID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Payment1));
            var fees = context.Entrants.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.Mind_Sport_ID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Fee));
            var contestants = payments.Where(x => !fees.Keys.Contains(x.Key) || x.Value > fees[x.Key])
                .Select(x => x.Key);

            // Warning - can't do comparison inside SQL as Sum() can be NULL
            var names = context.Contestants.Where(x => contestants.Contains(x.Mind_Sport_ID))
                .ToList()
                .Select(x => x.FullName());


            if (names.Any())
            {
                MessageBox.Show("The following contestants have unspent fees:"
                    + Environment.NewLine + string.Join(Environment.NewLine, names)); 
            }
            else
            {
                MessageBox.Show("There are no contestants with unspent fees");
            }
        }

        private void unpaidFee_Click(object sender, RoutedEventArgs e)
        {
            var context = DataEntitiesProvider.Provide();
            var olympiadId = context.Olympiad_Infoes.First(x => x.Current).Id;

            var payments = context.Payments.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.MindSportsID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Payment1));
            var fees = context.Entrants.Where(p => p.OlympiadId == olympiadId)
                .GroupBy(x => x.Mind_Sport_ID)
                .ToDictionary(x => x.Key, x => x.Sum(p => p.Fee));
            var contestants = fees.Where(x => !payments.Keys.Contains(x.Key) || x.Value > payments[x.Key])
                .Select(x => x.Key);

            // Warning - can't do comparison inside SQL as Sum() can be NULL
            var names = context.Contestants.Where(x => contestants.Contains(x.Mind_Sport_ID))
                .ToList()
                .Select(x => x.FullName());

            if (names.Any())
            {
                MessageBox.Show("The following contestants have unpaid fees:"
                    + Environment.NewLine + string.Join(Environment.NewLine, names));
            }
            else
            {
                MessageBox.Show("There are no contestants with unpaid fees");
            }
        }

   
 
    }

    public class MainWindowVm : VmBase
    {
        private string _dbStatus = "";

        public string DbStatus
        {
            get { return _dbStatus; }
            set
            {
                if (_dbStatus != value)
                {
                    _dbStatus = value;
                    OnPropertyChanged("DbStatus");
                }
            }
        }
    }
}

