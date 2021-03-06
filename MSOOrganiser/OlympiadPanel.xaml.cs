using JuliaHayward.Common.Environment;
using JuliaHayward.Common.Logging;
using MSOCore;
using MSOCore.Calculators;
using MSOCore.Models;
using MSOOrganiser.Dialogs;
using MSOOrganiser.Events;
using MSOOrganiser.UIUtilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Validation;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSOOrganiser
{
    /// <summary>
    /// Interaction logic for EventPanel.xaml
    /// </summary>
    public partial class OlympiadPanel : UserControl
    {
        public OlympiadPanel()
        {
            InitializeComponent();
            DataContext = new OlympiadPanelVm();

            if (!JuliaEnvironment.CurrentEnvironment.IsDebug())
            {
                dataGrid.Columns[0].Visibility = Visibility.Collapsed;
                locationsDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            }
        }

        // A delegate type for hooking up change notifications.
        public delegate void EventEventHandler(object sender, EventEventArgs e);

        public event EventEventHandler EventSelected;

        public OlympiadPanelVm ViewModel
        {
            get { return (OlympiadPanelVm)DataContext; }
        }

        public void Populate()
        {
            ViewModel.PopulateDropdown();
        }

        private void olympiadCombo_Changed(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PopulateGame();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PopulateGame();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ViewModel.EditingThePast)
                {
                    if (MessageBox.Show("You are editing data for a past Olympiad. Are you sure this is right?",
                        "MSOOrganiser", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No)
                        == MessageBoxResult.No) return;
                }
                var errors = ViewModel.Validate();
                if (!errors.Any())
                {
                    using (new SpinnyCursor())
                    {
                        ViewModel.Save();
                    }
                }
                else
                {
                    errors.Insert(0, "Could not save:");
                    MessageBox.Show(string.Join(Environment.NewLine, errors));
                }
            }
            catch (Exception ex)
            {
                string message = (ex is DbEntityValidationException)
                    ? ((DbEntityValidationException)ex).EntityValidationErrors.First().ValidationErrors.First().ErrorMessage
                    : ex.Message;

                MessageBox.Show("Something went wrong  - data not saved (" + message + ")");

                var trelloKey = ConfigurationManager.AppSettings["TrelloKey"];
                var trelloAuthKey = ConfigurationManager.AppSettings["TrelloAuthKey"];
                var logger = new TrelloLogger(trelloKey, trelloAuthKey);
                logger.Error("MSOWeb", message, ex.StackTrace);
            }
        }

        private void deleteEvent_Click(object sender, RoutedEventArgs e)
        {
            var eventToDelete = ((FrameworkElement)sender).DataContext as OlympiadPanelVm.EventVm;
            ViewModel.Events.Remove(eventToDelete);
            ViewModel.IsDirty = true;
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEventToOlympiadWindow();
            dialog.ShowDialog();

            if (dialog.DialogResult.Value)
            {
                var evt = dialog.SelectedEvent;
                if (ViewModel.Events.Any(x => x.Code == evt.Code))
                    MessageBox.Show("Olympiad already contains event " + evt.Code);

                ViewModel.Events.Add(new OlympiadPanelVm.EventVm() { Code = evt.Code, Name = evt.Name, Id = 0, Status = "New" });
                ViewModel.IsDirty = true;
            }
        }

        private void editEvent_Click(object sender, RoutedEventArgs e)
        {    
            var evt = ((FrameworkElement)sender).DataContext as OlympiadPanelVm.EventVm;
            if (EventSelected != null)
            {
                var args = new EventEventArgs() { EventCode = evt.Code, OlympiadId = ViewModel.OlympiadId };
                EventSelected(this, args);
            }
        }

        private void addLocation_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddLocationDialog();
            if (dialog.ShowDialog().Value)
            {
                ViewModel.Locations.Add(new OlympiadPanelVm.LocationVm() { Id = 0, Name = dialog.LocationName });
                ViewModel.IsDirty = true;
            }
        }

        private void addNew_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Olympiads.Insert(0, new OlympiadPanelVm.OlympiadVm { Text = "New Olympiad", Id = 0 });
            ViewModel.OlympiadId = 0;
        }
    }

    public class OlympiadPanelVm : VmBase
    {

        public class OlympiadVm
        {
            public string Text { get; set; }
            public int Id { get; set; }
        }

        public class EventVm
        {
            public int SequenceNumber { get; set; }
            public int Id { get; set; }
            public string Code { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public bool CanDelete { get; set; }
            public string Dates { get; set; }

            public EventVm()
            {
            }

            public EventVm(Event e)
            {
                SequenceNumber = e.Number;
                Id = e.EIN;
                Code = e.Code;
                Name = e.Mind_Sport;
                CanDelete = !e.Entrants.Any(x => x.Rank.HasValue && x.Rank > 0);
                Status = e.Status();

                if (e.Event_Sess.Any())
                {
                    Dates = e.Event_Sess.Min(s => s.ActualStart).ToString("ddd dd MMM hh:mm")
                        + " - " + e.Event_Sess.Max(s => s.ActualEnd).ToString("hh:mm");
                }
            }
        }

        public class LocationVm
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #region bindable properties
        public ObservableCollection<OlympiadVm> Olympiads { get; set; }
        public ObservableCollection<EventVm> Events { get; set; }
        public ObservableCollection<LocationVm> Locations { get; set; }
        public bool EditingThePast { get; set; }

        private int _OlympiadId;
        public int OlympiadId
        {
            get
            {
                return _OlympiadId;
            }
            set
            {
                if (_OlympiadId != value)
                {
                    _OlympiadId = value;
                    OnPropertyChanged("OlympiadId");
                }
            }
        }

        private bool _IsDirty;
        public bool IsDirty
        {
            get
            {
                return _IsDirty;
            }
            set
            {
                if (_IsDirty != value)
                {
                    _IsDirty = value;
                    OnPropertyChanged("IsDirty");
                    OnPropertyChanged("IsNotDirty");
                }
            }
        }

        public bool IsNotDirty { get { return !IsDirty; } }

        private string _YearOf;
        public string YearOf 
        { 
            get 
            {
                return _YearOf; 
            }
            set
            {
                if (_YearOf != value)
                {
                    _YearOf = value;
                    IsDirty = true;
                    OnPropertyChanged("YearOf");
                }
            }
        }
        private string _Number;
        public string Number
        {
            get
            {
                return _Number;
            }
            set
            {
                if (_Number != value)
                {
                    _Number = value;
                    IsDirty = true;
                    OnPropertyChanged("Number");
                }
            }
        }
        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    IsDirty = true;
                    OnPropertyChanged("Title");
                }
            }
        }
        private string _Venue;
        public string Venue
        {
            get
            {
                return _Venue;
            }
            set
            {
                if (_Venue != value)
                {
                    _Venue = value;
                    IsDirty = true;
                    OnPropertyChanged("Venue");
                }
            }
        }
        private DateTime? _StartDate;
        public DateTime? StartDate
        {
            get
            {
                return _StartDate;
            }
            set
            {
                if (_StartDate != value)
                {
                    _StartDate = value;
                    IsDirty = true;
                    OnPropertyChanged("StartDate");
                }
            }
        }
        private DateTime? _FinishDate;
        public DateTime? FinishDate
        {
            get
            {
                return _FinishDate;
            }
            set
            {
                if (_FinishDate != value)
                {
                    _FinishDate = value;
                    IsDirty = true;
                    OnPropertyChanged("FinishDate");
                }
            }
        }
        private string _MaxFee;
        public string MaxFee
        {
            get
            {
                return _MaxFee;
            }
            set
            {
                if (_MaxFee != value)
                {
                    _MaxFee = value;
                    IsDirty = true;
                    OnPropertyChanged("MaxFee");
                }
            }
        }
        private string _MaxCon;
        public string MaxCon
        {
            get
            {
                return _MaxCon;
            }
            set
            {
                if (_MaxCon != value)
                {
                    _MaxCon = value;
                    IsDirty = true;
                    OnPropertyChanged("MaxCon");
                }
            }
        }
        private DateTime? _AgeDate;
        public DateTime? AgeDate
        {
            get
            {
                return _AgeDate;
            }
            set
            {
                if (_AgeDate != value)
                {
                    _AgeDate = value;
                    IsDirty = true;
                    OnPropertyChanged("AgeDate");
                }
            }
        }
        private string _JnrAge;
        public string JnrAge
        {
            get
            {
                return _JnrAge;
            }
            set
            {
                if (_JnrAge != value)
                {
                    _JnrAge = value;
                    IsDirty = true;
                    OnPropertyChanged("JnrAge");
                }
            }
        }
        private string _SnrAge;
        public string SnrAge
        {
            get
            {
                return _SnrAge;
            }
            set
            {
                if (_SnrAge != value)
                {
                    _SnrAge = value;
                    IsDirty = true;
                    OnPropertyChanged("SnrAge");
                }
            }
        }
         private string _PentaLong;
        public string PentaLong
        {
            get
            {
                return _PentaLong;
            }
            set
            {
                if (_PentaLong != value)
                {
                    _PentaLong = value;
                    IsDirty = true;
                    OnPropertyChanged("PentaLong");
                }
            }
        }
            private string _PentaTotal;
        public string PentaTotal
        {
            get
            {
                return _PentaTotal;
            }
            set
            {
                if (_PentaTotal != value)
                {
                    _PentaTotal = value;
                    IsDirty = true;
                    OnPropertyChanged("PentaTotal");
                }
            }
        }
        #endregion

        public OlympiadPanelVm()
        {
            Olympiads = new ObservableCollection<OlympiadVm>();
            Events = new ObservableCollection<EventVm>();
            Locations = new ObservableCollection<LocationVm>();
        }

        public void PopulateDropdown()
        {
            Olympiads.Clear();
            var context = DataEntitiesProvider.Provide();
            foreach (var o in context.Olympiad_Infoes.OrderByDescending(x => x.StartDate))
                Olympiads.Add(new OlympiadVm { Text = o.FullTitle(), Id = o.Id });

            OlympiadId = Olympiads.First().Id;
        }

        public void PopulateGame()
        {
            var id = OlympiadId;
            if (id == 0)
            {
                YearOf = "";
                Number = "";
                Title = ""; Venue = ""; StartDate = null; FinishDate = null; MaxFee = ""; MaxCon = ""; AgeDate= null;
                JnrAge = ""; SnrAge = "";
                PentaLong = ""; PentaTotal = "";
                Events.Clear();
                Locations.Clear();
            }
            else
            {
                var context = DataEntitiesProvider.Provide();
                // TODO This join could be eliminated if an Event had a do-not-delete flag. Still, it's
                // better than not Including it.
                var o = context.Olympiad_Infoes.Include("Events").Include("Events.Entrants")
                    .FirstOrDefault(x => x.Id == id);
                EditingThePast = !o.Current;
                YearOf = o.YearOf.ToString();
                Number = o.Number;
                Title = o.Title;
                Venue = o.Venue;
                StartDate = o.StartDate;
                FinishDate = o.FinishDate;
                MaxFee = (o.MaxFee.HasValue) ? o.MaxFee.Value.ToString("F2") : "";
                MaxCon = (o.MaxCon.HasValue) ? o.MaxCon.Value.ToString("F2") : "";
                AgeDate = o.AgeDate;
                JnrAge = o.JnrAge.ToString();
                SnrAge = o.SnrAge.ToString();
                PentaLong = o.PentaLong.ToString();
                PentaTotal = o.PentaTotal.ToString();
                Events.Clear();
                foreach (var e in o.Events.OrderBy(x => x.Code))
                {
                    Events.Add(new EventVm(e));
                }
                Locations.Clear();
                foreach (var l in o.Locations.OrderBy(x => x.Location1))
                {
                    Locations.Add(new LocationVm() { Id = l.Id, Name = l.Location1 });
                }
            }
            IsDirty = false;
        }

        public List<string> Validate()
        {
            int i;
            DateTime startDt, endDt, ageDt;
            decimal d;
            var errors = new List<string>();
            if (string.IsNullOrEmpty(Number))
                errors.Add("Number must be specified");
            if (string.IsNullOrEmpty(Title))
                errors.Add("Title must be specified");
            if (string.IsNullOrEmpty(Venue))
                errors.Add("Venue must be specified");
            if (!int.TryParse(YearOf, out i))
                errors.Add("Invalid year");
            if (!decimal.TryParse(MaxFee, out d))
                errors.Add("Invalid MaxFee");
            if (!decimal.TryParse(MaxCon, out d))
                errors.Add("Invalid Max Concession");
            if (!int.TryParse(JnrAge, out i))
                errors.Add("Invalid junior age");
            if (!int.TryParse(SnrAge, out i))
                errors.Add("Invalid senior age");
            if (!int.TryParse(PentaLong, out i))
                errors.Add("Invalid penamind long events");
            if (!int.TryParse(PentaTotal, out i))
                errors.Add("Invalid pentamind total events");

            if (StartDate > FinishDate)
                errors.Add("Start date must be prior to End Date");

            return errors;
        }

        public void Save()
        {
            var context = DataEntitiesProvider.Provide();
            var id = OlympiadId;
            Olympiad_Info o;
            if (id == 0)
            {
                o = new Olympiad_Info()
                {
                    YearOf = int.Parse(this.YearOf),
                    Number = this.Number,
                    Title = this.Title,
                    Venue = this.Venue,
                    StartDate = this.StartDate,
                    FinishDate = this.FinishDate,
                    MaxFee = decimal.Parse(this.MaxFee),
                    MaxCon = decimal.Parse(this.MaxCon),
                    AgeDate = this.AgeDate,
                    JnrAge = int.Parse(this.JnrAge),
                    SnrAge = int.Parse(this.SnrAge),
                    PentaLong = int.Parse(this.PentaLong),
                    PentaTotal = int.Parse(this.PentaTotal),
                    Events = new List<Event>(),
                    Current = false,    // will be sorted out later                      
                };
                context.Olympiad_Infoes.Add(o);
                context.SaveChanges();
                // So we don't have to do a full refresh of the combo
                Olympiads.RemoveAt(0);
                Olympiads.Insert(0, new OlympiadVm() { Text = o.FullTitle(), Id = o.Id });
                id = o.Id;
            }
            else
            {
                o = context.Olympiad_Infoes.FirstOrDefault(x => x.Id == id);
                o.YearOf = int.Parse(this.YearOf);
                o.Number = this.Number;
                o.Title = this.Title;
                o.Venue = this.Venue;
                o.StartDate = this.StartDate;
                o.FinishDate = this.FinishDate;
                o.MaxFee = decimal.Parse(this.MaxFee);
                o.MaxCon = decimal.Parse(this.MaxCon);
                o.AgeDate = this.AgeDate;
                o.JnrAge = int.Parse(this.JnrAge);
                o.SnrAge = int.Parse(this.SnrAge);
                o.PentaLong = int.Parse(this.PentaLong);
                o.PentaTotal = int.Parse(this.PentaTotal);
                o.Current = false;      // will be sorted out later
            }
            context.SaveChanges();
            // Now update the events and locations. Need to do here to have the reference back to the Olympiad
            foreach (var existingEvent in o.Events.ToList())
            {
                if (!Events.Any(x => x.Id == existingEvent.EIN))
                {
                    o.Events.Remove(existingEvent);
                    context.Events.Remove(existingEvent);
                }
            }
            foreach (var existingLocation in o.Locations.ToList())
            {
                if (!Locations.Any(x => x.Id == existingLocation.Id))
                {
                    o.Locations.Remove(existingLocation);
                    context.Locations.Remove(existingLocation);
                }
            }
            foreach (var evm in Events)
            {
                if (evm.Id == 0)
                {
                    var game = context.Games.FirstOrDefault(x => evm.Code.StartsWith(x.Code));
                    if (game == null)
                        throw new ArgumentOutOfRangeException("No Game for code " + evm.Code);

                    var evt = new Event()
                    {
                        Mind_Sport = evm.Name,
                        Code = evm.Code,
                        Olympiad_Info = o,
                        Game = game,
                        MAX_Number = 70,
                        ConsistentWithBoardability = true,
                        PentamindFactor = 1.0f
                        // TODO more stuff here
                    };
                    o.Events.Add(evt);
                }
                else
                {
                    var evt = context.Events.Find(evm.Id);
                    // We're not doing any update here?
                }
            }
            foreach (var loc in Locations)
            {
                if (loc.Id == 0)
                {
                    o.Locations.Add(new Location() { Location1 = loc.Name, Olympiad_Info = o, YEAR = o.YearOf });
                }
                // Not doing updates here
            }
            context.SaveChanges();

            // Make sure Current is set properly
            var oldCurrents = context.Olympiad_Infoes.Where(x => x.Current).ToList();
            oldCurrents.ForEach(ol => ol.Current = false);
            var newCurrent = context.Olympiad_Infoes.OrderByDescending(x => x.StartDate).First();
            newCurrent.Current = true;
            context.SaveChanges();

            var eventIndexer = new EventIndexer();
            eventIndexer.IndexEvents(id);

            IsDirty = false;
            OlympiadId = id;
        }
    }
}
