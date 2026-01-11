using System;
using System.Windows;

namespace HotelUI
{
    public partial class DateSelectionWindow : Window
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateSelectionWindow()
        {
            InitializeComponent();
            PickStart.SelectedDate = DateTime.Today;
            PickEnd.SelectedDate = DateTime.Today.AddDays(1);
        }
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            StartDate = PickStart.SelectedDate.Value;
            EndDate = PickEnd.SelectedDate.Value;
            this.DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}