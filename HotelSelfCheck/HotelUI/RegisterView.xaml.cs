using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HotelUI
{
    public partial class RegisterView : Window
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var data = DataStore.Load();
            string user = RegUser.Text.Trim();
            string pass = RegPass.Password;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                new ConfirmationWindow("Please fill in all fields to continue.", "Incomplete Data", true, "⚠️").ShowDialog();
                return;
            }

            if (data.Users.Any(u => u.Username == user))
            {
                new ConfirmationWindow("This username is already taken. Please try another one.", "Registration Error", true, "🚫").ShowDialog();
                return;
            }

            data.Users.Add(new User { Username = user, Password = pass, Role = "Guest" });
            DataStore.Save(data);

            // Mesajul de succes tradus
            new ConfirmationWindow("Your account has been created successfully! ✨", "Welcome", true, "🎉").ShowDialog();
    
            new LoginView().Show();
            this.Close();
        }

        private void RegPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = sender as PasswordBox;
            if (pb == null) return;
            ControlTemplate template = pb.Template;
            TextBlock placeholder = (TextBlock)template.FindName("placeholder", pb);
            if (placeholder != null) placeholder.Visibility = (pb.Password.Length > 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new LoginView().Show();
            this.Close();
        }
    }
}