using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Linq;

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
            var data = DataStore.Load();
            string user = UsernameBox.Text.Trim();
            string pass = PasswordBox.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowError("Please enter your credentials!");
                return;
            }

            var foundUser = data.Users.FirstOrDefault(u => u.Username == user && u.Password == pass);

            if (foundUser != null)
            {
                if (foundUser.Role == "Admin") new AdminView().Show();
                else new UserView(foundUser.Username).Show();
                this.Close();
            }
            else
            {
                ShowError("Invalid username or password!");
            }
        }

        private void OpenRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterView().Show();
            this.Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            
            Storyboard shake = (Storyboard)this.Resources["ShakeAnimation"];
            if (shake != null)
            {
                shake.Begin(MainCard);
            }
        }
    }
}