using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace HotelUI
{
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationWindow(string message, string title, bool isAlert = false, string icon = "❓", string okText = "Confirm")
        {
            InitializeComponent();
    
            TxtBigIcon.Text = icon;
            TxtMessage.Text = message;
            TxtTitle.Text = title;

            BtnCancel.Content = "Cancel";

            if (icon == "❌" || icon == "🚫" || icon == "⚠️" || title.ToLower().Contains("error"))
            {
                var redBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                TxtBigIcon.Foreground = redBrush;
                TxtTitle.Foreground = redBrush;
                BtnOk.Background = redBrush;
                BtnOk.Content = (okText == "Confirm") ? "Got it" : okText;
            }
            else
            {
                BtnOk.Content = okText;
            }

            if (isAlert)
            {
                BtnCancel.Visibility = Visibility.Collapsed;
                BtnOk.Width = 200;
            }
            else
            {
                BtnCancel.Visibility = Visibility.Visible;
                BtnOk.Width = 110;
            }
        }

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