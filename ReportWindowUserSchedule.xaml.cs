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

namespace AppointmentsC969_TimothyUpchurch
{
    /// <summary>
    /// Interaction logic for ReportWindowUserSchedule.xaml
    /// </summary>
    public partial class ReportWindowUserSchedule : Window
    {
        public ReportWindowUserSchedule()
        {
            InitializeComponent();
            DataContext = this;
            // Set item source to AppointmentsByUser
            userScheduleDataGrid.ItemsSource = Appointment.AppointmentsByUser;
        }
        private void DraggableWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        private void ExitToReportWindow(object sender, MouseButtonEventArgs e)
        {
            ReportWindow reportWindow = new ReportWindow();
            reportWindow.Show();
            Close();
        }
    }
}
