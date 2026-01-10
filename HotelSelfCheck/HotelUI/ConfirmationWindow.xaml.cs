using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HotelUI
{
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow(string message, string title = "Confirmation", bool isError = false)
        {
            InitializeComponent();

            TxtTitle.Text = title;
            TxtMessage.Text = message;

            // 1. MOD NOTIFICARE (Doar buton OK)
            if (isError)
            {
                TxtSubMessage.Visibility = Visibility.Collapsed;
                BtnCancel.Visibility = Visibility.Collapsed;
                BtnConfirm.Content = "OK";

                if (title == "Visit Completed")
                {
                    // CHECK-OUT COMPLET (Salut)
                    TxtIcon.Text = "👋";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));
                }
                else if (title.Contains("Confirmed") || title.Contains("Success") || title.Contains("Update"))
                {
                    // SUCCES STANDARD (Verde)
                    TxtIcon.Text = "✅";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                }
                else
                {
                    // EROARE (Rosu)
                    TxtIcon.Text = "⛔";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53E3E"));
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748"));
                }
            }
            // 2. MOD CONFIRMARE (Butoane YES / NO)
            else
            {
                BtnCancel.Visibility = Visibility.Visible;
                TxtSubMessage.Visibility = Visibility.Visible;

                if (title == "Confirm Booking")
                {
                    // BOOKING (Verde)
                    TxtIcon.Text = "📅";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    
                    BtnConfirm.Content = "Book Now";
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    TxtSubMessage.Text = "Please review the details above.";
                }
                else if (title.Contains("Check-out"))
                {
                    // PREDARE CHEIE (Albastru)
                    TxtIcon.Text = "🗝️"; 
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));

                    BtnConfirm.Content = "Confirm";
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));
                    TxtSubMessage.Text = "Are you sure you want to leave?";
                }
                else if (title.Contains("Modify"))
                {
                    // MODIFICARE (Albastru)
                    TxtIcon.Text = "✏️";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));

                    BtnConfirm.Content = "Yes, Modify";
                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3182CE"));
                    TxtSubMessage.Text = "Changes will be saved immediately.";
                }
                else
                {
                    // STERGERE/ANULARE (Rosu)
                    TxtIcon.Text = "⚠️";
                    TxtIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706"));

                    BtnConfirm.Content = "Delete";
                    if (title.Contains("Cancel")) BtnConfirm.Content = "Yes, Cancel";

                    BtnConfirm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E53E3E"));
                    TxtSubMessage.Text = "This action cannot be undone.";
                }
            }
        }

        public ConfirmationWindow() : this("Are you sure?", "Confirm", false) { }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}