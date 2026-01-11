using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HotelUI
{
    public partial class UserView : Window
    {
        private HotelData _data;
        private string _currentUser;
        private ObservableCollection<Reservation> _myActiveRes;
        private ObservableCollection<Reservation> _myHistoryRes;

        public UserView(string username = "Guest Client")
        {
            InitializeComponent();
            _currentUser = username;
            _data = DataStore.Load();
            
            StartClock();
            InitializeUILists();
            
            // Render inițial al camerelor
            RenderRooms(_data.Rooms);
            Navigate(BtnExplore, null);
        }

        private void InitializeUILists()
        {
            var myRes = _data.Reservations.Where(r => r.ClientName == _currentUser).ToList();
            _myActiveRes = new ObservableCollection<Reservation>(myRes.Where(r => r.Status == "Booked" || r.Status == "Checked-in"));
            _myHistoryRes = new ObservableCollection<Reservation>(myRes.Where(r => r.Status == "Completed" || r.Status == "Cancelled"));
            ActiveResList.ItemsSource = _myActiveRes;
            HistoryGrid.ItemsSource = _myHistoryRes;
        }

        private void RenderRooms(List<Room> rooms)
        {
            if (RoomsList == null) return;
            RoomsList.Items.Clear();
            
            if (rooms.Count == 0)
            {
                TxtNoRooms.Visibility = Visibility.Visible;
                return;
            }
            
            TxtNoRooms.Visibility = Visibility.Collapsed;
            foreach (var r in rooms)
            {
                decimal price = _data.Prices.ContainsKey(r.Type) ? _data.Prices[r.Type] : 50;
                RoomsList.Items.Add(CreateRoomCard(r, price));
            }
        }

        private Border CreateRoomCard(Room r, decimal price)
        {
            var card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(20), Margin = new Thickness(0, 0, 0, 20), Padding = new Thickness(5) };
            card.Effect = new System.Windows.Media.Effects.DropShadowEffect { BlurRadius = 15, Opacity = 0.1, ShadowDepth = 2, Color = Colors.Gray };
            
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });

            string emoji = "🏨"; string color = "#EDF2F7";
            if (r.Type.Contains("Single")) emoji = "🛌";
            else if (r.Type.Contains("Double")) { emoji = "🏩"; color = "#EBF8FF"; }
            else if (r.Type.Contains("Presidential")) { emoji = "👑"; color = "#FEFCBF"; }

            var iconBox = new Border { Background = (Brush)new BrushConverter().ConvertFrom(color), CornerRadius = new CornerRadius(18), Margin = new Thickness(10) };
            iconBox.Child = new TextBlock { Text = emoji, FontSize = 32, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(iconBox, 0); grid.Children.Add(iconBox);

            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 10, 0) };
            info.Children.Add(new TextBlock { Text = r.Type, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#2D3748") });
            info.Children.Add(new TextBlock { Text = $"Room {r.RoomNumber} • {r.Facilities}", FontSize = 13, Foreground = (Brush)new BrushConverter().ConvertFrom("#718096"), Margin = new Thickness(0, 4, 0, 8) });
            
            var tag = new Border { Background = (Brush)new BrushConverter().ConvertFrom("#F0FFF4"), CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 2, 8, 2), HorizontalAlignment = HorizontalAlignment.Left };
            tag.Child = new TextBlock { Text = "Available Now", FontSize = 11, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#38A169") };
            info.Children.Add(tag);
            Grid.SetColumn(info, 1); grid.Children.Add(info);

            var actions = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 20, 0) };
            actions.Children.Add(new TextBlock { Text = $"€{price}", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#10B981"), HorizontalAlignment = HorizontalAlignment.Right });
            actions.Children.Add(new TextBlock { Text = "per night", FontSize = 11, Foreground = Brushes.LightGray, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, -2, 0, 12) });

            var btn = new Button { Content = "BOOK NOW", Height = 35, Width = 120, Cursor = Cursors.Hand, Foreground = Brushes.White, FontWeight = FontWeights.Bold, Tag = r };
            var btnTemplate = new ControlTemplate(typeof(Button));
            var btnBorder = new FrameworkElementFactory(typeof(Border));
            btnBorder.SetValue(Border.BackgroundProperty, (Brush)new BrushConverter().ConvertFrom("#10B981"));
            btnBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));
            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            btnBorder.AppendChild(contentPresenter);
            btnTemplate.VisualTree = btnBorder;
            btn.Template = btnTemplate;
            btn.Click += Book_Click;
            actions.Children.Add(btn);
            Grid.SetColumn(actions, 2); grid.Children.Add(actions);

            card.Child = grid;
            return card;
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            var room = (sender as Button).Tag as Room;
            if (room == null) return;

            var dateWin = new DateSelectionWindow();
            if (dateWin.ShowDialog() == true)
            {
                DateTime start = dateWin.StartDate;
                DateTime end = dateWin.EndDate;

                if (start < DateTime.Today || end <= start)
                {
                    new ConfirmationWindow("Please select valid future dates.", "Invalid Dates", true, "🚫").ShowDialog();
                    return;
                }

                bool isTaken = _data.Reservations.Any(res => 
                    res.RoomNumber == room.RoomNumber && 
                    res.Status != "Cancelled" && 
                    start < res.CheckOut && end > res.CheckIn);

                if (isTaken)
                {
                    new ConfirmationWindow("Room is already booked for this period.", "Unavailable", true, "🚫").ShowDialog();
                    return;
                }

                decimal price = _data.Prices.ContainsKey(room.Type) ? _data.Prices[room.Type] : 50;
                decimal total = price * (end - start).Days;

                if (new ConfirmationWindow($"Total cost: €{total}. Confirm booking?", "Confirm", false, "🏨", "Book Now").ShowDialog() == true)
                {
                    var res = new Reservation { ClientName = _currentUser, RoomNumber = room.RoomNumber, RoomType = room.Type, CheckIn = start, CheckOut = end, Status = "Booked", TotalPrice = total };
                    _data.Reservations.Add(res);
                    _myActiveRes.Add(res);
                    
                    if (start == DateTime.Today) {
                        var r = _data.Rooms.FirstOrDefault(x => x.RoomNumber == room.RoomNumber);
                        if (r != null) r.Status = "Ocupată";
                    }

                    DataStore.Save(_data);
                    new ConfirmationWindow("Booking successful! ✨", "Success", true, "✨").ShowDialog();
                    Navigate(BtnMyBooking, null);
                }
            }
        }
        
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
            BtnFac.IsChecked = false;
        }

        private void ApplyFilters()
        {
            if (_data == null) return;

            string query = SearchBox.Text.ToLower();
            string selectedType = (SearchType.SelectedItem as ComboBoxItem)?.Content.ToString();
    
            List<string> selectedFacs = new List<string>();
            foreach (var item in FilterFacilities.Children)
            {
                if (item is CheckBox cb && cb.IsChecked == true)
                {
                    selectedFacs.Add(cb.Content.ToString().ToLower());
                }
            }

            var filtered = _data.Rooms.Where(r =>
                (string.IsNullOrEmpty(query) || r.RoomNumber.ToString().Contains(query) || r.Type.ToLower().Contains(query)) &&
                (selectedType == "All Types" || r.Type == selectedType) &&
                (selectedFacs.Count == 0 || selectedFacs.All(f => r.Facilities.ToLower().Contains(f)))
            ).ToList();

            RenderRooms(filtered);
        }

        public void Modify_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null)
            {
                decimal price = _data.Prices.ContainsKey(res.RoomType) ? _data.Prices[res.RoomType] : 50;
                if (new ConfirmationWindow($"Extend stay by 1 night? Cost: €{price}", "Extend", false, "➕", "Extend").ShowDialog() == true)
                {
                    res.CheckOut = res.CheckOut.AddDays(1);
                    res.TotalPrice += price;
                    DataStore.Save(_data);
                    RefreshList();
                }
            }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null) 
            { 
                res.Status = "Checked-in"; 
                DataStore.Save(_data); 
                RefreshList();
                new ConfirmationWindow("Checked-in successfully! 🔑", "Success", true, "🔑").ShowDialog(); 
            }
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null)
            {
                var confirm = new ConfirmationWindow(
                    "Would you like to check out now? 💼", 
                    "Check-out", 
                    false, 
                    "💼", 
                    "Yes, check out");

                if (confirm.ShowDialog() == true)
                {
                    res.Status = "Completed";

                    var r = _data.Rooms.FirstOrDefault(x => x.RoomNumber == res.RoomNumber);
                    if (r != null) r.Status = "Curățenie";

                    _myActiveRes.Remove(res);
                    _myHistoryRes.Insert(0, res); 

                    DataStore.Save(_data);
            
                    RefreshList();
            
                    new ConfirmationWindow(
                        "Check-out successful! We hope to see you again soon. 👋", 
                        "Finished", 
                        true, 
                        "👋").ShowDialog();
                }
            }
        }
        
        private void CancelRes_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
    
            if (res != null)
            {
                var confirm = new ConfirmationWindow(
                    "Are you sure you want to cancel this booking? 😟", 
                    "Cancel Booking", 
                    false, 
                    "↩️", 
                    "Yes, cancel");

                if (confirm.ShowDialog() == true)
                {
                    res.Status = "Cancelled";

                    if (res.CheckIn.Date <= DateTime.Today && res.CheckOut.Date > DateTime.Today)
                    {
                        var room = _data.Rooms.FirstOrDefault(r => r.RoomNumber == res.RoomNumber);
                        if (room != null)
                        {
                            room.Status = "Liberă";
                        }
                    }

                    _myActiveRes.Remove(res);
                    _myHistoryRes.Insert(0, res);

                    DataStore.Save(_data);
            
                    RefreshList();
            
                    new ConfirmationWindow("Booking cancelled. The room is now available! ✅", "Success", true, "✅").ShowDialog();
                }
            }
        }

        private void RefreshList() 
        { 
            ActiveResList.ItemsSource = null; 
            ActiveResList.ItemsSource = _myActiveRes;
    
            HistoryGrid.ItemsSource = null;
            HistoryGrid.ItemsSource = _myHistoryRes;
        }
        
        private void StartClock()
        {
            DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            t.Tick += (s, ev) => { TxtClock.Text = DateTime.Now.ToString("hh:mm"); };
            t.Start();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button; if (b == null) return;
            BtnExplore.Tag = ""; BtnMyBooking.Tag = ""; b.Tag = "Active";
            TxtTitle.Text = b.Content.ToString();
            ExplorePanel.Visibility = (b == BtnExplore) ? Visibility.Visible : Visibility.Collapsed;
            BookingPanel.Visibility = (b == BtnMyBooking) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Logout_Click(object sender, RoutedEventArgs e) { new LoginView().Show(); this.Close(); }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }
    }
}