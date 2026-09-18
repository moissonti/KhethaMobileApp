using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCAP.Views.Dashboard
{
    // ── Display models for the list ────────────────────
    public class StudyItem
    {
        public string DisplayTitle { get; set; } = "";
        public string DisplaySubtitle { get; set; } = "";
        public string BadgeText { get; set; } = "";
        public bool HasBadge { get; set; }
        public object? RawData { get; set; }
    }

    // ── Qualification model ────────────────────────────
    public class Qualification
    {
        [JsonPropertyName("title")] public string title { get; set; } = "";
        [JsonPropertyName("nqf_level")] public int nqf_level { get; set; }
        [JsonPropertyName("type")] public string type { get; set; } = "";
        [JsonPropertyName("duration")] public string duration { get; set; } = "";
        [JsonPropertyName("field")] public string field { get; set; } = "";
    }

    // ── Provider model ─────────────────────────────────
    public class Provider
    {
        [JsonPropertyName("name")] public string name { get; set; } = "";
        [JsonPropertyName("type")] public string type { get; set; } = "";
        [JsonPropertyName("province")] public string province { get; set; } = "";
        [JsonPropertyName("city")] public string city { get; set; } = "";
        [JsonPropertyName("phone")] public string phone { get; set; } = "";
        [JsonPropertyName("website")] public string website { get; set; } = "";
    }

    public class Bursary
    {
        public string Name { get; set; } = "";
        public string Provider { get; set; } = "";
        public string Description { get; set; } = "";
        public string Fields { get; set; } = "";
        public string Eligibility { get; set; } = "";
        public string Deadline { get; set; } = "";
        public string ApplyUrl { get; set; } = "";
        public string Color { get; set; } = "#1A3C2E";
    }

    public partial class StudyPage : ContentPage
    {
        private List<Qualification> _qualifications = new();
        private List<Provider> _providers = new();
        private bool _isWhatTab = true;
        private string _activeFilter = "All";
        private bool _drawerOpen = false;

        public StudyPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!_qualifications.Any()) await LoadData();
        }

        // ── Load seed data ─────────────────────────────
        private async Task LoadData()
        {
            try
            {
                // Load qualifications
                using var qStream = await FileSystem.OpenAppPackageFileAsync("seed_qualifications.json");
                using var qReader = new StreamReader(qStream);
                _qualifications = JsonSerializer.Deserialize<List<Qualification>>(
                    await qReader.ReadToEndAsync()) ?? new();

                // Load providers
                using var pStream = await FileSystem.OpenAppPackageFileAsync("seed_providers.json");
                using var pReader = new StreamReader(pStream);
                _providers = JsonSerializer.Deserialize<List<Provider>>(
                    await pReader.ReadToEndAsync()) ?? new();

                BuildFilterChips();
                ApplyFilter();
                BuildBursaryChips();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load data: {ex.Message}", "OK");
            }
        }

        // ── Tab switching ──────────────────────────────
        private void OnTabWhat(object sender, EventArgs e)
        {
            if (_isWhatTab) return;
            _isWhatTab = true;
            _activeFilter = "All";
            SearchEntry.Text = "";

            // Update tab styles
            TabWhat.BackgroundColor = Colors.White;
            TabWhere.BackgroundColor = Colors.Transparent;

            BuildFilterChips();
            ApplyFilter();
            BuildBursaryChips();
        }

        private void OnTabWhere(object sender, EventArgs e)
        {
            if (!_isWhatTab) return;
            _isWhatTab = false;
            _activeFilter = "All";
            SearchEntry.Text = "";

            // Update tab styles
            TabWhere.BackgroundColor = Colors.White;
            TabWhat.BackgroundColor = Colors.Transparent;

            BuildFilterChips();
            ApplyFilter();
            BuildBursaryChips();
        }

        // ── Build filter chips dynamically ─────────────
        private void BuildFilterChips()
        {
            FilterChipsContainer.Children.Clear();

            var filters = _isWhatTab
                ? new[] { "All", "NQF 4", "NQF 6", "NQF 7", "NQF 8+" }
                : new[] { "All", "University", "University of Technology", "TVET College" };

            foreach (var filter in filters)
            {
                var isActive = filter == _activeFilter;
                var chip = new Border
                {
                    BackgroundColor = isActive ? Color.FromArgb("#1A3C2E") : Color.FromArgb("#F5F7F5"),
                    StrokeThickness = isActive ? 0 : 1.5f,
                    Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")),
                    Padding = new Thickness(14, 8),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 }
                };

                var label = new Label
                {
                    Text = filter,
                    FontSize = 12,
                    FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = isActive ? Colors.White : Color.FromArgb("#1A3C2E")
                };

                chip.Content = label;
                chip.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    CommandParameter = filter,
                    Command = new Command<string>(f =>
                    {
                        _activeFilter = f;
                        BuildFilterChips();
                        ApplyFilter();
                        BuildBursaryChips();
                    })
                });

                FilterChipsContainer.Children.Add(chip);
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

        // ── Apply search + filter ──────────────────────
        private void ApplyFilter()
        {
            var query = SearchEntry.Text?.ToLower().Trim() ?? "";
            List<StudyItem> items;

            if (_isWhatTab)
            {
                items = _qualifications
                    .Where(q =>
                    {
                        bool matchSearch = string.IsNullOrEmpty(query) ||
                            q.title.ToLower().Contains(query) ||
                            q.field.ToLower().Contains(query) ||
                            q.type.ToLower().Contains(query);

                        bool matchFilter = _activeFilter switch
                        {
                            "NQF 4" => q.nqf_level == 4,
                            "NQF 6" => q.nqf_level == 6,
                            "NQF 7" => q.nqf_level == 7,
                            "NQF 8+" => q.nqf_level >= 8,
                            _ => true
                        };

                        return matchSearch && matchFilter;
                    })
                    .Select(q => new StudyItem
                    {
                        DisplayTitle = q.title,
                        DisplaySubtitle = $"{q.type} · {q.duration}",
                        BadgeText = $"NQF {q.nqf_level}",
                        HasBadge = true,
                        RawData = q
                    }).ToList();
            }
            else
            {
                items = _providers
                    .Where(p =>
                    {
                        bool matchSearch = string.IsNullOrEmpty(query) ||
                            p.name.ToLower().Contains(query) ||
                            p.province.ToLower().Contains(query) ||
                            p.city.ToLower().Contains(query);

                        bool matchFilter = _activeFilter switch
                        {
                            "University" => p.type == "University",
                            "University of Technology" => p.type == "University of Technology",
                            "TVET College" => p.type == "TVET College",
                            _ => true
                        };

                        return matchSearch && matchFilter;
                    })
                    .Select(p => new StudyItem
                    {
                        DisplayTitle = p.name,
                        DisplaySubtitle = $"{p.province} · {p.city}",
                        BadgeText = p.type,
                        HasBadge = true,
                        RawData = p
                    }).ToList();
            }

            ItemList.ItemsSource = items;
            EmptyState.IsVisible = !items.Any();
            ResultsCount.Text = $"{items.Count} result{(items.Count == 1 ? "" : "s")} found";
        }

        // ── Item tapped → open drawer ──────────────────
        private async void OnItemTapped(object sender, TappedEventArgs e)
        {
            if (e.Parameter is not StudyItem item) return;
            await OpenDrawer(item);
        }

        private async Task OpenDrawer(StudyItem item)
        {
            DrawerContent.Children.Clear();

            if (item.RawData is Qualification q)
            {
                DrawerCategory.Text = "QUALIFICATION";
                DrawerTitle.Text = q.title;
                AddDrawerRow("NQF Level", $"Level {q.nqf_level}");
                AddDrawerRow("Type", q.type);
                AddDrawerRow("Duration", q.duration);
                AddDrawerRow("Field of Study", q.field);
                AddDrawerInfo("To enrol, contact institutions in your province that offer this qualification. Ensure the institution is registered with DHET and the qualification is accredited.");
            }
            else if (item.RawData is Provider p)
            {
                DrawerCategory.Text = "INSTITUTION";
                DrawerTitle.Text = p.name;
                AddDrawerRow("Type", p.type);
                AddDrawerRow("Province", p.province);
                AddDrawerRow("City", p.city);
                if (!string.IsNullOrEmpty(p.phone))
                    AddDrawerRowTappable("Phone", p.phone, async () =>
                    {
                        try { PhoneDialer.Default.Open(p.phone.Replace(" ", "")); }
                        catch { }
                        await Task.CompletedTask;
                    });
                if (!string.IsNullOrEmpty(p.website))
                    AddDrawerRowTappable("Website", p.website, async () =>
                        await Launcher.Default.OpenAsync(new Uri(p.website)));
            }

            DrawerOverlay.IsVisible = true;
            _drawerOpen = true;
            await DrawerPanel.TranslateTo(0, 0, 280, Easing.CubicOut);
        }

        private void AddDrawerRow(string label, string value)
        {
            var row = new VerticalStackLayout { Spacing = 3 };
            row.Children.Add(new Label { Text = label, FontSize = 11, TextColor = Color.FromArgb("#999999") });
            row.Children.Add(new Label { Text = value, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });

            var wrapper = new Border
            {
                BackgroundColor = Color.FromArgb("#F5F7F5"),
                StrokeThickness = 0,
                Padding = new Thickness(14, 12),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = row
            };
            DrawerContent.Children.Add(wrapper);
        }

        private void AddDrawerRowTappable(string label, string value, Func<Task> onTap)
        {
            var row = new VerticalStackLayout { Spacing = 3 };
            row.Children.Add(new Label { Text = label, FontSize = 11, TextColor = Color.FromArgb("#999999") });
            row.Children.Add(new Label { Text = value, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });

            var wrapper = new Border
            {
                BackgroundColor = Color.FromArgb("#E8F5EE"),
                StrokeThickness = 0,
                Padding = new Thickness(14, 12),
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Content = row
            };
            wrapper.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(async () => await onTap())
            });
            DrawerContent.Children.Add(wrapper);
        }

        private void AddDrawerInfo(string text)
        {
            DrawerContent.Children.Add(new Label
            {
                Text = text,
                FontSize = 12,
                TextColor = Color.FromArgb("#888888"),
                LineBreakMode = LineBreakMode.WordWrap
            });
        }

        // ── Close drawer ───────────────────────────────
        private async void OnCloseDrawer(object sender, EventArgs e)
        {
            await DrawerPanel.TranslateTo(320, 0, 220, Easing.CubicIn);
            DrawerOverlay.IsVisible = false;
            _drawerOpen = false;
        }

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

        // Add this list inside the StudyPage class

        private readonly List<Bursary> _bursaries = new()
{
    new Bursary
    {
        Name = "NSFAS",
        Provider = "National Student Financial Aid Scheme",
        Description = "NSFAS provides financial aid to eligible South African students at public universities and TVET colleges who cannot afford to pay for their studies.",
        Fields = "All fields of study",
        Eligibility = "SA citizen · Household income ≤ R350,000/year · Registered at public university or TVET",
        Deadline = "Applications open annually (Jan–Mar)",
        ApplyUrl = "https://my.nsfas.org.za",
        Color = "#1A3C2E"
    },
    new Bursary
    {
        Name = "Funza Lushaka",
        Provider = "Department of Basic Education",
        Description = "The Funza Lushaka Bursary Programme funds students who want to become teachers, specifically in scarce subjects like Mathematics, Science and Languages.",
        Fields = "Education (BEd / PGCE)",
        Eligibility = "SA citizen · Studying towards a teaching qualification · Willing to teach in a public school after graduation",
        Deadline = "Applications open in September each year",
        ApplyUrl = "https://www.funzalushaka.doe.gov.za",
        Color = "#C8972B"
    },
    new Bursary
    {
        Name = "ISFAP",
        Provider = "Ikusasa Student Financial Aid Programme",
        Description = "ISFAP supports students from the 'missing middle' — students whose families earn too much to qualify for NSFAS but too little to afford university fees.",
        Fields = "All fields of study at participating universities",
        Eligibility = "SA citizen · Household income R350,000–R600,000/year · Strong academic record",
        Deadline = "Applications open in May each year",
        ApplyUrl = "https://applyonline.isfap.org.za",
        Color = "#2D5A40"
    },
    new Bursary
    {
        Name = "Capitec",
        Provider = "Capitec Bank",
        Description = "Capitec Bank offers bursaries to talented South African students studying towards qualifications in finance, technology and business-related fields.",
        Fields = "Finance, IT, Engineering, Business",
        Eligibility = "SA citizen · Excellent academic results · Financial need",
        Deadline = "31 July annually",
        ApplyUrl = "https://www.capitecbank.co.za/careers/bursaries",
        Color = "#1A3C2E"
    },
    new Bursary
    {
        Name = "Nedbank",
        Provider = "Nedbank Group",
        Description = "Nedbank offers bursaries to students studying in scarce skills fields, with potential employment opportunities upon graduation.",
        Fields = "Finance, Accounting, IT, Engineering",
        Eligibility = "SA citizen · Minimum 65% academic average · Studying at an accredited SA university",
        Deadline = "Applications open in August each year",
        ApplyUrl = "https://www.nedbank.co.za/content/nedbank/desktop/gt/en/careers/bursaries.html",
        Color = "#C8972B"
    },
    new Bursary
    {
        Name = "Enel Green Power",
        Provider = "Enel Green Power SA",
        Description = "Enel Green Power offers bursaries for students in green and renewable energy fields, supporting South Africa's transition to sustainable energy.",
        Fields = "Electrical Engineering, Mechanical Engineering, Environmental Science",
        Eligibility = "SA citizen · Studying towards an engineering or environmental qualification · Financial need",
        Deadline = "Applications open in 2025",
        ApplyUrl = "https://www.enelgreenpower.com/south-africa",
        Color = "#2D5A40"
    },
    new Bursary
    {
        Name = "Santam",
        Provider = "Santam Insurance",
        Description = "Santam offers bursaries to students in insurance, risk management, actuarial science and related fields, with possible internship placement.",
        Fields = "Actuarial Science, Finance, Risk Management",
        Eligibility = "SA citizen · Strong Mathematics results · Studying at a public university",
        Deadline = "Applications open in September each year",
        ApplyUrl = "https://www.santam.co.za/careers/bursaries",
        Color = "#1A3C2E"
    },
    new Bursary
    {
        Name = "DALRRD",
        Provider = "Dept. of Agriculture, Land Reform & Rural Development",
        Description = "The DALRRD bursary supports students with a passion for agriculture, food security and rural development to study at public universities.",
        Fields = "Agriculture, Food Science, Veterinary Science, Forestry",
        Eligibility = "SA citizen · Financial need · Studying an agriculture-related qualification",
        Deadline = "Applications open annually",
        ApplyUrl = "https://www.dalrrd.gov.za",
        Color = "#C8972B"
    },
};

        // Add these methods inside the StudyPage class

        private void BuildBursaryChips()
        {
            BursaryChips.Children.Clear();
            foreach (var bursary in _bursaries)
            {
                var chip = new Border
                {
                    BackgroundColor = Color.FromArgb(bursary.Color),
                    StrokeThickness = 0,
                    Padding = new Thickness(16, 10),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                    MinimumWidthRequest = 100
                };

                var stack = new VerticalStackLayout { Spacing = 2 };
                stack.Children.Add(new Label
                {
                    Text = bursary.Name,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White
                });
                stack.Children.Add(new Label
                {
                    Text = bursary.Fields.Length > 25 ? bursary.Fields[..25] + "…" : bursary.Fields,
                    FontSize = 10,
                    TextColor = Color.FromArgb("#CCFFFFFF")
                });

                chip.Content = stack;
                chip.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => OpenBursaryModal(bursary))
                });

                BursaryChips.Children.Add(chip);
            }
        }

        private string _currentBursaryUrl = "";

        private void OpenBursaryModal(Bursary bursary)
        {
            _currentBursaryUrl = bursary.ApplyUrl;
            BursaryTitle.Text = bursary.Name;
            BursaryProvider.Text = bursary.Provider;
            BursaryDescription.Text = bursary.Description;

            BursaryDetails.Children.Clear();
            AddBursaryDetail("📚 Fields", bursary.Fields);
            AddBursaryDetail("✅ Eligibility", bursary.Eligibility);
            AddBursaryDetail("📅 Deadline", bursary.Deadline);
            AddBursaryDetail("🔗 Apply at", bursary.ApplyUrl);

            BursaryOverlay.IsVisible = true;
        }

        private void AddBursaryDetail(string label, string value)
        {
            var row = new VerticalStackLayout { Spacing = 2 };
            row.Children.Add(new Label { Text = label, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });
            row.Children.Add(new Label { Text = value, FontSize = 12, TextColor = Color.FromArgb("#555555"), LineBreakMode = LineBreakMode.WordWrap });
            BursaryDetails.Children.Add(row);
        }

        private void OnCloseBursary(object sender, EventArgs e)
        {
            BursaryOverlay.IsVisible = false;
        }

        private async void OnApplyBursary(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentBursaryUrl))
                await Launcher.Default.OpenAsync(new Uri(_currentBursaryUrl));
        }
    }
}