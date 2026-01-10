using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HotelUI
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb == null) return;
            ControlTemplate template = pb.Template;
            TextBlock placeholder = (TextBlock)template.FindName("placeholder", pb);
            if (placeholder != null) placeholder.Visibility = (pb.Password.Length > 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string user = UsernameBox.Text.Trim();
            string pass = PasswordBox.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowError("Vă rugăm să introduceți toate datele!");
                return;
            }
            
            if (user == "admin" && pass == "admin")
            {
                new AdminView().Show();
                this.Close();
            }
            else if (user == "client" && pass == "1234")
            {
                new UserView().Show();
                this.Close();
            }
            else
            {
                ShowError("Utilizator sau parolă incorectă!");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            Storyboard shake = (Storyboard)this.Resources["ShakeAnimation"];
            shake.Begin(MainCard);
        }
    }
}