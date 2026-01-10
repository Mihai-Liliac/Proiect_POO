using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace HotelUI
{
    public partial class AdminView : Window
    {
        private HotelData _data;
        private Room _roomToEdit = null;
        private bool _isRoomMode = true;

        public AdminView()
        {
            InitializeComponent();
            StartClock();
            
            _data = DataStore.Load();
            
            PopulateTimeCombos();
            Navigate(BtnDash, null);
        }

        private void PopulateTimeCombos()
        {
            List<string> hours = new List<string>();
            for (int i = 10; i <= 18; i++)
            {
                hours.Add($"{i:00}:00");
                if (i < 18) hours.Add($"{i:00}:30");
            }
            if(ComboCheckIn != null) { ComboCheckIn.ItemsSource = hours; ComboCheckIn.SelectedItem = "14:00"; }
            if(ComboCheckOut != null) { ComboCheckOut.ItemsSource = hours; ComboCheckOut.SelectedItem = "12:00"; }
        }

        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => 
            {
                TxtClock.Text = DateTime.Now.ToString("hh:mm");
                var parent = TxtClock.Parent as StackPanel;
                if (parent != null && parent.Children.Count > 1 && parent.Children[1] is TextBlock amPmTxt)
                {
                    amPmTxt.Text = " " + DateTime.Now.ToString("tt");
                }
            };
            timer.Start();
        }

        private void Navigate(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Tag != null && (btn.Tag.ToString() == "🛏️" || btn.Tag.ToString() == "📅"))
            {
                 if(btn.Tag.ToString() == "🛏️") btn = BtnRooms;
                 if(btn.Tag.ToString() == "📅") btn = BtnRes;
            }

            BtnDash.Tag = ""; BtnRooms.Tag = ""; BtnRes.Tag = ""; BtnConfig.Tag = "";
            btn.Tag = "Active";
            TxtTitle.Text = btn.Content.ToString();

            DashboardPanel.Visibility = Visibility.Collapsed;
            TablePanel.Visibility = Visibility.Collapsed;
            ConfigPanel.Visibility = Visibility.Collapsed;
            SearchContainer.Visibility = Visibility.Collapsed;
            SearchBox.Text = "";

            if (btn == BtnDash)
            {
                DashboardPanel.Visibility = Visibility.Visible;
                UpdateDashboardStats();
            }
            else if (btn == BtnConfig)
            {
                ConfigPanel.Visibility = Visibility.Visible;
                if(_data.Prices.ContainsKey("Single Room")) PriceSingle.Text = _data.Prices["Single Room"].ToString();
                if(_data.Prices.ContainsKey("Double Deluxe")) PriceDouble.Text = _data.Prices["Double Deluxe"].ToString();
                if(_data.Prices.ContainsKey("Presidential Suite")) PriceSuite.Text = _data.Prices["Presidential Suite"].ToString();
            }
            else
            {
                TablePanel.Visibility = Visibility.Visible;
                SearchContainer.Visibility = Visibility.Visible;
                _isRoomMode = (btn == BtnRooms);
                
                UpdateFilterOptions();
                GenerateColumns(_isRoomMode);
                ApplyFilters();
                
                RoomFormPanel.Visibility = _isRoomMode ? Visibility.Visible : Visibility.Collapsed;
            }
            ResetForm();
        }

        private void UpdateDashboardStats()
        {
            if (_data == null) return;

            decimal revenue = _data.Reservations.Where(r => r.Status != "Cancelled").Sum(r => r.TotalPrice);
            TxtRevenue.Text = $"€ {revenue:N0}";

            int totalRooms = _data.Rooms.Count;
            int clean = _data.Rooms.Count(r => r.Status == "Liberă");
            int occupied = _data.Rooms.Count(r => r.Status == "Ocupată");
            int dirty = _data.Rooms.Count(r => r.Status == "Curățenie");

            double occRate = totalRooms > 0 ? (double)occupied / totalRooms * 100 : 0;
            TxtOccupancy.Text = $"{occRate:0}%";
            PbOccupancy.Value = occRate;

            int activeGuests = _data.Reservations.Count(r => r.Status == "Booked" || r.Status == "Checked-in");
            TxtGuests.Text = activeGuests.ToString();

            int todayCheckins = _data.Reservations.Count(r => r.CheckIn.Date == DateTime.Today && r.Status == "Booked");
            TxtCheckIns.Text = $"{todayCheckins} checking in today";

            double cleanPct = totalRooms > 0 ? (double)clean / totalRooms * 100 : 0;
            double occPct = totalRooms > 0 ? (double)occupied / totalRooms * 100 : 0;
            double dirtyPct = totalRooms > 0 ? (double)dirty / totalRooms * 100 : 0;

            PbClean.Value = cleanPct; TxtClean.Text = $"{cleanPct:0}%";
            PbOccupied.Value = occPct; TxtOccupied.Text = $"{occPct:0}%";
            PbDirty.Value = dirtyPct; TxtDirty.Text = $"{dirtyPct:0}%";
        }

        private void UpdateFilterOptions()
        {
            FilterBox.ItemsSource = null;
            if (_isRoomMode)
                FilterBox.ItemsSource = new List<string> { "All", "Status: Liberă", "Status: Ocupată", "Status: Curățenie" };
            else
                FilterBox.ItemsSource = new List<string> { "All", "Status: Booked", "Status: Checked-in", "Status: Completed", "Status: Cancelled" };
            FilterBox.SelectedIndex = 0;
        }

        private void GenerateColumns(bool isRoomMode)
        {
            DG.Columns.Clear();
            if (isRoomMode)
            {
                DG.Columns.Add(new DataGridTextColumn { Header = "NR.", Binding = new Binding("RoomNumber"), Width = 90 });
                DG.Columns.Add(new DataGridTextColumn { Header = "TYPE", Binding = new Binding("Type"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                DG.Columns.Add(new DataGridTextColumn { Header = "FACILITIES", Binding = new Binding("Facilities"), Width = 150 });

                var statusCol = new DataGridTemplateColumn { Header = "STATUS", Width = 160 };
                var factory = new FrameworkElementFactory(typeof(ComboBox));
                factory.SetValue(ComboBox.ItemsSourceProperty, new string[] { "Liberă", "Ocupată", "Curățenie", "Indisponibilă" });
                factory.SetBinding(ComboBox.SelectedItemProperty, new Binding("Status") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                factory.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler((s, e) => { DataStore.Save(_data); UpdateDashboardStats(); }));
                factory.SetValue(ComboBox.StyleProperty, this.Resources["ModernComboBoxStyle"]);
                statusCol.CellTemplate = new DataTemplate { VisualTree = factory };
                DG.Columns.Add(statusCol);

                var actionCol = new DataGridTemplateColumn { Header = "ACTIONS", Width = 100 };
                var stack = new FrameworkElementFactory(typeof(StackPanel));
                stack.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                stack.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                
                var btnEdit = new FrameworkElementFactory(typeof(Button));
                btnEdit.SetValue(Button.ContentProperty, "✏️");
                btnEdit.SetResourceReference(Button.StyleProperty, "EditButtonStyle");
                btnEdit.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditRoom_Click));
                btnEdit.SetBinding(Button.TagProperty, new Binding("."));
                
                var btnDel = new FrameworkElementFactory(typeof(Button));
                btnDel.SetValue(Button.ContentProperty, "🗑️");
                btnDel.SetResourceReference(Button.StyleProperty, "DeleteButtonStyle");
                btnDel.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteRoom_Click));
                btnDel.SetBinding(Button.TagProperty, new Binding("."));

                stack.AppendChild(btnEdit);
                stack.AppendChild(btnDel);
                actionCol.CellTemplate = new DataTemplate { VisualTree = stack };
                DG.Columns.Add(actionCol);
            }
            else
            {
                DG.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("ReservationID"), Width = 90 });
                DG.Columns.Add(new DataGridTextColumn { Header = "CLIENT", Binding = new Binding("ClientName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                DG.Columns.Add(new DataGridTextColumn { Header = "ROOM", Binding = new Binding("RoomType"), Width = new DataGridLength(0.8, DataGridLengthUnitType.Star) });
                DG.Columns.Add(new DataGridTextColumn { Header = "PERIOD", Binding = new Binding("DateRange"), Width = new DataGridLength(1.2, DataGridLengthUnitType.Star) });
                DG.Columns.Add(new DataGridTextColumn { Header = "TOTAL", Binding = new Binding("TotalPrice") { StringFormat = "€{0}" }, Width = 90 });

                var statusCol = new DataGridTemplateColumn { Header = "STATUS", Width = 150 };
                var factory = new FrameworkElementFactory(typeof(ComboBox));
                factory.SetValue(ComboBox.ItemsSourceProperty, new string[] { "Booked", "Checked-in", "Completed", "Cancelled" });
                factory.SetBinding(ComboBox.SelectedItemProperty, new Binding("Status") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                factory.AddHandler(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler((s, e) => { DataStore.Save(_data); UpdateDashboardStats(); }));
                factory.SetValue(ComboBox.StyleProperty, this.Resources["ModernComboBoxStyle"]);
                statusCol.CellTemplate = new DataTemplate { VisualTree = factory };
                DG.Columns.Add(statusCol);
            }
        }

        private void ApplyFilters()
        {
            string query = SearchBox.Text.ToLower();
            string filter = FilterBox.SelectedItem as string;

            if (_isRoomMode)
            {
                var result = _data.Rooms.Where(r => 
                    (string.IsNullOrWhiteSpace(query) || r.RoomNumber.ToString().Contains(query)) &&
                    (string.IsNullOrEmpty(filter) || filter == "All" || r.Status == filter.Split(':')[1].Trim())
                ).ToList();
                DG.ItemsSource = result;
            }
            else
            {
                var result = _data.Reservations.Where(r => 
                    (string.IsNullOrWhiteSpace(query) || r.ClientName.ToLower().Contains(query) || r.ReservationID.ToLower().Contains(query)) &&
                    (string.IsNullOrEmpty(filter) || filter == "All" || r.Status == filter.Split(':')[1].Trim())
                ).ToList();
                DG.ItemsSource = result;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        
        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void SaveRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(InpNum.Text, out int num)) return;
            string type = (InpType.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            List<string> facs = new List<string>();
            foreach(var item in StackFacilities.Children)
            {
                if(item is CheckBox cb && cb.IsChecked == true) facs.Add(cb.Content.ToString());
            }
            string facilitiesString = string.Join(", ", facs);
            if(string.IsNullOrEmpty(facilitiesString)) facilitiesString = "Standard";

            if (_roomToEdit == null) 
            {
                _data.Rooms.Add(new Room { RoomNumber = num, Type = type, Status = "Liberă", Facilities = facilitiesString });
            }
            else 
            { 
                _roomToEdit.RoomNumber = num; 
                _roomToEdit.Type = type;
                _roomToEdit.Facilities = facilitiesString;
            }
            
            DataStore.Save(_data);
            ApplyFilters();
            UpdateDashboardStats();
            ResetForm();
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            var room = (sender as Button).Tag as Room;
            if(room != null)
            {
                _roomToEdit = room;
                InpNum.Text = room.RoomNumber.ToString();
                
                foreach(ComboBoxItem item in InpType.Items) if(item.Content.ToString() == room.Type) { InpType.SelectedItem = item; break; }
                
                foreach(var item in StackFacilities.Children)
                {
                    if(item is CheckBox cb) cb.IsChecked = room.Facilities.Contains(cb.Content.ToString());
                }

                BtnSaveRoom.Content = "Update Room";
                BtnCancelEdit.Visibility = Visibility.Visible;
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            var room = (sender as Button).Tag as Room;
            if (room != null)
            {
                if(new ConfirmationWindow($"Delete Room {room.RoomNumber}?", "Delete").ShowDialog() == true)
                {
                    _data.Rooms.Remove(room);
                    DataStore.Save(_data);
                    ApplyFilters();
                    UpdateDashboardStats();
                }
            }
        }

        private void SaveConfig_Click(object sender, RoutedEventArgs e)
        {
            if(double.TryParse(PriceSingle.Text, out double p1)) _data.Prices["Single Room"] = (decimal)p1;
            if(double.TryParse(PriceDouble.Text, out double p2)) _data.Prices["Double Deluxe"] = (decimal)p2;
            if(double.TryParse(PriceSuite.Text, out double p3)) _data.Prices["Presidential Suite"] = (decimal)p3;
            
            DataStore.Save(_data);
            new ConfirmationWindow("Prices updated successfully!", "System Update", true).ShowDialog();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Logout_Click(object sender, RoutedEventArgs e) { new LoginView().Show(); this.Close(); }
        private void CancelEdit_Click(object sender, RoutedEventArgs e) { ResetForm(); InpNum.Clear(); }
        private void ResetForm() 
        { 
            _roomToEdit = null; InpNum.Clear(); BtnSaveRoom.Content = "Add Room"; BtnCancelEdit.Visibility = Visibility.Collapsed;
            foreach(var item in StackFacilities.Children) if(item is CheckBox cb) cb.IsChecked = false;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }
    }
}