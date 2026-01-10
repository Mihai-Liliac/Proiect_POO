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
        private string _currentUser = "Guest Client";
        private ObservableCollection<Reservation> _myActiveRes;
        private ObservableCollection<Reservation> _myHistoryRes;

        public UserView()
        {
            InitializeComponent();
            StartClock();
            _data = DataStore.Load();
            InitializeUILists();
            DateStart.SelectedDate = DateTime.Now;
            DateEnd.SelectedDate = DateTime.Now.AddDays(1);
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
            RoomsList.Items.Clear();
            if (rooms.Count == 0) { TxtNoRooms.Visibility = Visibility.Visible; return; }
            TxtNoRooms.Visibility = Visibility.Collapsed;

            foreach (var r in rooms)
            {
                string icon = "🛏️";
                string desc = "Perfect stay.";
                decimal price = _data.Prices.ContainsKey(r.Type) ? _data.Prices[r.Type] : 50;

                if (r.Type == "Single Room") { icon = "🛏️"; desc = "Solo traveler comfort."; }
                else if (r.Type == "Double Deluxe") { icon = "🛏️🛏️"; desc = "King bed, city view."; }
                else if (r.Type == "Presidential Suite") { icon = "👑"; desc = "Luxury experience."; }

                Border card = CreateRoomCard(r, icon, desc, price);
                RoomsList.Items.Add(card);
            }
        }

        private Border CreateRoomCard(Room r, string icon, string desc, decimal price)
        {
            var card = new Border { Background = Brushes.White, CornerRadius = new CornerRadius(20), Margin = new Thickness(0, 0, 0, 20) };
            card.Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Gray, BlurRadius = 20, Opacity = 0.1, ShadowDepth = 5 };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            var iconB = new Border { Background = (Brush)new BrushConverter().ConvertFrom("#F7FAFC"), CornerRadius = new CornerRadius(20, 0, 0, 20) };
            iconB.Child = new TextBlock { Text = icon, FontSize = 35, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(iconB, 0); grid.Children.Add(iconB);
            var info = new StackPanel { Margin = new Thickness(20), VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock { Text = r.Type, FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#2D3748") });
            info.Children.Add(new TextBlock { Text = desc, Foreground = (Brush)new BrushConverter().ConvertFrom("#718096"), Margin = new Thickness(0, 5, 0, 10) });
            var b = new Border { Background = (Brush)new BrushConverter().ConvertFrom("#F0FFF4"), CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 4, 10, 4), HorizontalAlignment = HorizontalAlignment.Left };
            b.Child = new TextBlock { Text = r.Facilities, Foreground = (Brush)new BrushConverter().ConvertFrom("#10B981"), FontWeight = FontWeights.SemiBold, FontSize = 12 };
            info.Children.Add(b); Grid.SetColumn(info, 1); grid.Children.Add(info);
            var priceGrid = new Grid { Margin = new Thickness(0, 0, 25, 0) };
            priceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            priceGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            priceGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            var act = new StackPanel { HorizontalAlignment = HorizontalAlignment.Right }; Grid.SetRow(act, 1);
            act.Children.Add(new TextBlock { Text = $"€ {price}", FontSize = 32, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#10B981"), HorizontalAlignment = HorizontalAlignment.Right });
            act.Children.Add(new TextBlock { Text = "per night", FontSize = 13, Foreground = (Brush)new BrushConverter().ConvertFrom("#A0AEC0"), HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, -5, 0, 15) });
            var btn = new Button { Content = "BOOK NOW", Width = 160, Height = 50, Cursor = Cursors.Hand, Tag = r }; btn.Click += Book_Click; act.Children.Add(btn);
            Grid.SetColumn(priceGrid, 2); grid.Children.Add(priceGrid); card.Child = grid; return card;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) return;
            string type = (SearchType.SelectedItem as ComboBoxItem)?.Content.ToString();
            var filtered = _data.Rooms.Where(r => (type == "All Types" || r.Type == type)).ToList();
            RenderRooms(filtered);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_data == null || SearchBox == null) return;
            string q = SearchBox.Text.ToLower();
            RenderRooms(_data.Rooms.Where(r => r.RoomNumber.ToString().Contains(q) || r.Type.ToLower().Contains(q)).ToList());
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            var room = (sender as Button).Tag as Room;
            decimal price = _data.Prices.ContainsKey(room.Type) ? _data.Prices[room.Type] : 50;
            int nights = (DateEnd.SelectedDate.Value - DateStart.SelectedDate.Value).Days;
            if (nights < 1) nights = 1;
            decimal total = price * nights;
            if (new ConfirmationWindow($"Book Room {room.RoomNumber}?\nTotal: €{total}", "Confirm").ShowDialog() == true)
            {
                var n = new Reservation { ClientName = _currentUser, RoomType = room.Type, RoomNumber = room.RoomNumber, CheckIn = DateStart.SelectedDate.Value, CheckOut = DateEnd.SelectedDate.Value, Status = "Booked", TotalPrice = total };
                _data.Reservations.Add(n); _myActiveRes.Add(n); DataStore.Save(_data);
                Navigate(BtnMyBooking, null);
            }
        }

        private void CheckIn_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null) { res.Status = "Checked-in"; RefreshList(); DataStore.Save(_data); }
        }

        private void CheckOut_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null && new ConfirmationWindow("Check out?", "Confirm").ShowDialog() == true)
            {
                res.Status = "Completed"; _myActiveRes.Remove(res); _myHistoryRes.Add(res); DataStore.Save(_data);
            }
        }

        private void CancelRes_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null && new ConfirmationWindow("Cancel?", "Confirm").ShowDialog() == true)
            {
                res.Status = "Cancelled"; _myActiveRes.Remove(res); _myHistoryRes.Add(res); DataStore.Save(_data);
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            var res = (sender as Button).Tag as Reservation;
            if (res != null)
            {
                decimal pricePerNight = _data.Prices.ContainsKey(res.RoomType) ? _data.Prices[res.RoomType] : 50;
                if (new ConfirmationWindow($"Extend 1 night for {res.RoomType}?\nCost: €{pricePerNight}", "Modify Booking").ShowDialog() == true)
                {
                    res.CheckOut = res.CheckOut.AddDays(1);
                    res.TotalPrice += pricePerNight;
                    RefreshList();
                    DataStore.Save(_data);
                }
            }
        }

        private void RefreshList() { ActiveResList.ItemsSource = null; ActiveResList.ItemsSource = _myActiveRes; }
        
        private void StartClock() 
        { 
            DispatcherTimer t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) }; 
            t.Tick += (s, ev) => {
                TxtClock.Text = DateTime.Now.ToString("hh:mm"); 
                var parent = TxtClock.Parent as StackPanel;
                if (parent != null && parent.Children.Count > 1 && parent.Children[1] is TextBlock amPmTxt)
                    amPmTxt.Text = " " + DateTime.Now.ToString("tt");
            }; 
            t.Start(); 
        }

        private void Navigate(object sender, RoutedEventArgs e) { Button b = (Button)sender; BtnExplore.Tag = ""; BtnMyBooking.Tag = ""; b.Tag = "Active"; TxtTitle.Text = b.Content.ToString(); ExplorePanel.Visibility = (b == BtnExplore) ? Visibility.Visible : Visibility.Collapsed; BookingPanel.Visibility = (b == BtnMyBooking) ? Visibility.Visible : Visibility.Collapsed; SearchContainer.Visibility = ExplorePanel.Visibility; }
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Logout_Click(object sender, RoutedEventArgs e) { new LoginView().Show(); this.Close(); }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }
    }
}