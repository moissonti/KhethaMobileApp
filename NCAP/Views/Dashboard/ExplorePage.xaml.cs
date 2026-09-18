using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCAP.Views.Dashboard
{
    public class Career
    {
        [JsonPropertyName("occupation_code")] public string OccupationCode { get; set; } = "";
        [JsonPropertyName("title")] public string title { get; set; } = "";
        [JsonPropertyName("major_group")] public string major_group { get; set; } = "";
        [JsonPropertyName("description")] public string description { get; set; } = "";
        [JsonPropertyName("tasks")] public List<string> tasks { get; set; } = new();
        [JsonPropertyName("learning_pathways")] public List<string> learning_pathways { get; set; } = new();
        [JsonPropertyName("is_high_demand")] public bool is_high_demand { get; set; }
        [JsonPropertyName("is_green_career")] public bool is_green_career { get; set; }
        [JsonPropertyName("is_trade")] public bool is_trade { get; set; }
    }

    public partial class ExplorePage : ContentPage
    {
        private List<Career> _allCareers = new();
        private List<Career> _filtered = new();
        private string _activeFilter = "All";
        private bool _drawerOpen = false;

        public ExplorePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_allCareers.Any())
                await LoadCareers();
        }

        // ── Load seed data ─────────────────────────────
        private async Task LoadCareers()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("seed_careers.json");
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                _allCareers = JsonSerializer.Deserialize<List<Career>>(json) ?? new();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load careers: {ex.Message}", "OK");
            }
        }

        // ── Search ─────────────────────────────────────
        private void OnSearchChanged(object sender, TextChangedEventArgs e)
        {
            ClearSearch.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
            ApplyFilter();
        }

        private void OnClearSearch(object sender, EventArgs e)
        {
            SearchEntry.Text = "";
            ClearSearch.IsVisible = false;
            ApplyFilter();
        }

        // ── Filter chips ───────────────────────────────
        private void OnFilterTapped(object sender, TappedEventArgs e)
        {
            _activeFilter = e.Parameter?.ToString() ?? "All";
            UpdateChipStyles();
            ApplyFilter();
        }

        private void UpdateChipStyles()
        {
            var chips = new Dictionary<string, Border>
            {
                { "All",        ChipAll },
                { "HighDemand", ChipDemand },
                { "Green",      ChipGreen },
                { "Trades",     ChipTrades },
            };

            foreach (var (key, chip) in chips)
            {
                if (key == _activeFilter)
                {
                    chip.BackgroundColor = Color.FromArgb("#1A3C2E");
                    chip.StrokeThickness = 0;
                    // Set inner label color to white
                    if (chip.Content is Label lbl) lbl.TextColor = Colors.White;
                    if (chip.Content is HorizontalStackLayout hsl)
                        foreach (var child in hsl.Children)
                            if (child is Label l) l.TextColor = Colors.White;
                }
                else
                {
                    chip.BackgroundColor = Color.FromArgb("#F5F7F5");
                    chip.StrokeThickness = 1.5f;
                    chip.Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0"));
                    if (chip.Content is Label lbl) lbl.TextColor = Color.FromArgb("#1A3C2E");
                    if (chip.Content is HorizontalStackLayout hsl)
                        foreach (var child in hsl.Children)
                            if (child is Label l) l.TextColor = Color.FromArgb("#1A3C2E");
                }
            }
        }

        // ── Apply search + filter ──────────────────────
        private void ApplyFilter()
        {
            var query = SearchEntry.Text?.ToLower().Trim() ?? "";

            _filtered = _allCareers.Where(c =>
            {
                // Search
                bool matchesSearch = string.IsNullOrEmpty(query) ||
                    c.title.ToLower().Contains(query) ||
                    c.major_group.ToLower().Contains(query) ||
                    c.description.ToLower().Contains(query);

                // Filter
                bool matchesFilter = _activeFilter switch
                {
                    "HighDemand" => c.is_high_demand,
                    "Green" => c.is_green_career,
                    "Trades" => c.is_trade,
                    _ => true
                };

                return matchesSearch && matchesFilter;
            }).ToList();

            CareerList.ItemsSource = _filtered;
            EmptyState.IsVisible = !_filtered.Any();
        }

        // ── Career tapped → open drawer ────────────────
        private async void OnCareerTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not Career career) return;
            await OpenDrawer(career);
        }

        private async Task OpenDrawer(Career career)
        {
            // Populate drawer
            DrawerTitle.Text = career.title;
            DrawerOfoCode.Text = $"OFO: {career.OccupationCode}";
            DrawerGroup.Text = career.major_group;
            DrawerDescription.Text = string.IsNullOrEmpty(career.description)
                ? "No description available."
                : career.description;

            // Tasks
            DrawerTasks.Children.Clear();
            if (career.tasks.Any())
            {
                DrawerTasksSection.IsVisible = true;
                foreach (var task in career.tasks)
                {
                    DrawerTasks.Children.Add(new HorizontalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Label { Text = "•", FontSize = 13, TextColor = Color.FromArgb("#1A3C2E"), VerticalOptions = LayoutOptions.Start },
                            new Label { Text = task, FontSize = 13, TextColor = Color.FromArgb("#555555"), LineBreakMode = LineBreakMode.WordWrap, HorizontalOptions = LayoutOptions.FillAndExpand }
                        }
                    });
                }
            }
            else DrawerTasksSection.IsVisible = false;

            // Pathways
            DrawerPathways.Children.Clear();
            if (career.learning_pathways.Any())
            {
                DrawerPathwaysSection.IsVisible = true;
                foreach (var pathway in career.learning_pathways)
                {
                    var badge = new Border
                    {
                        BackgroundColor = Color.FromArgb("#E8F5EE"),
                        StrokeThickness = 0,
                        Padding = new Thickness(10, 6),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
                        Content = new Label { Text = pathway, FontSize = 12, TextColor = Color.FromArgb("#1A3C2E") }
                    };
                    DrawerPathways.Children.Add(badge);
                }
            }
            else DrawerPathwaysSection.IsVisible = false;

            // Animate in
            DrawerOverlay.IsVisible = true;
            _drawerOpen = true;
            await DrawerPanel.TranslateTo(0, 0, 280, Easing.CubicOut);
        }

        // ── Close drawer ───────────────────────────────
        private async void OnCloseDrawer(object sender, EventArgs e)
        {
            await DrawerPanel.TranslateTo(320, 0, 220, Easing.CubicIn);
            DrawerOverlay.IsVisible = false;
            _drawerOpen = false;
        }

        // Handle back button closing drawer first
        protected override bool OnBackButtonPressed()
        {
            if (_drawerOpen)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                    await DrawerPanel.TranslateTo(320, 0, 220, Easing.CubicIn));
                DrawerOverlay.IsVisible = false;
                _drawerOpen = false;
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}