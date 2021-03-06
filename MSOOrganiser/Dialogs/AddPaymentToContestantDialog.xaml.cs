using MSOCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MSOOrganiser.Dialogs
{
    /// <summary>
    /// Interaction logic for AddPaymentToContestantDialog.xaml
    /// </summary>
    public partial class AddPaymentToContestantDialog : Window
    {
        public AddPaymentToContestantDialog()
        {
            InitializeComponent();
            DataContext = new AddPaymentToContestantVm();
        }

        public AddPaymentToContestantVm ViewModel
        {
            get { return (AddPaymentToContestantVm)DataContext; }
        }

        public AddPaymentToContestantVm Payment { get { return ViewModel; } }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void addEvent_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Payment_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            if (PaymentRB.IsChecked.Value)
            {
                ViewModel.IsRefund = false;
                MethodPanel.Visibility = Visibility.Visible;
                BankedPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ViewModel.IsRefund = true;
                MethodPanel.Visibility = Visibility.Hidden;
                BankedPanel.Visibility = Visibility.Hidden;
            }
        }
    }

    public class AddPaymentToContestantVm
    {
        public class PaymentMethodVm {
            public string Text { get; set; }
        }

        public bool IsRefund { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public bool Banked { get; set; }

        public ObservableCollection<PaymentMethodVm> PaymentMethods { get; set; }

        public AddPaymentToContestantVm()
        {
            PaymentMethods = new ObservableCollection<PaymentMethodVm>();

            var context = DataEntitiesProvider.Provide();

            foreach (var p in context.Payment_Methods)
                PaymentMethods.Add(new PaymentMethodVm() { Text = p.Payment_Method1 });

            PaymentMethod = "Cash";
        }
    }
}
