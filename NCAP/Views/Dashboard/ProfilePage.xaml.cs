namespace NCAP.Views.Dashboard
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProfile();
        }

        private void LoadProfile()
        {
            // Load from Preferences (saved during ProfileSetupPage)
            UserNameLabel.Text = Preferences.Get("user_name", "My Profile");
            CategoryLabel.Text = Preferences.Get("user_category", "Learner");
            ProvinceLabel.Text = Preferences.Get("user_province", "Eastern Cape");
            LanguageLabel.Text = Preferences.Get("user_language", "IsiXhosa");

            // Load quiz results
            LoadInterests();
        }

        private void LoadInterests()
        {
            var resultsJson = Preferences.Get("quiz_results", "");
            if (string.IsNullOrEmpty(resultsJson))
            {
                InterestsPlaceholder.IsVisible = true;
                return;
            }

            try
            {
                InterestsPlaceholder.IsVisible = false;
                var results = System.Text.Json.JsonSerializer
                    .Deserialize<Dictionary<string, int>>(resultsJson);

                if (results == null || !results.Any()) return;

                var top = results.OrderByDescending(x => x.Value).Take(5).ToList();
                int maxScore = top.First().Value;

                InterestsContainer.Children.Clear();

                foreach (var (field, score) in top)
                {
                    double percent = maxScore > 0 ? (double)score / maxScore : 0;

                    var row = new VerticalStackLayout { Spacing = 6 };

                    // Label + percent
                    var labelRow = new Grid();
                    labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    labelRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var fieldLabel = new Label
                    {
                        Text = field,
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1A3C2E")
                    };
                    Grid.SetColumn(fieldLabel, 0);

                    var pctLabel = new Label
                    {
                        Text = $"{(int)(percent * 100)}%",
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#C8972B")
                    };
                    Grid.SetColumn(pctLabel, 1);

                    labelRow.Children.Add(fieldLabel);
                    labelRow.Children.Add(pctLabel);

                    // Progress bar
                    var barBg = new Grid();
                    barBg.Children.Add(new BoxView
                    {
                        HeightRequest = 6,
                        Color = Color.FromArgb("#E8F5EE"),
                        CornerRadius = 3
                    });
                    barBg.Children.Add(new BoxView
                    {
                        HeightRequest = 6,
                        Color = Color.FromArgb("#1A3C2E"),
                        CornerRadius = 3,
                        HorizontalOptions = LayoutOptions.Start,
                        WidthRequest = 260 * percent
                    });

                    row.Children.Add(labelRow);
                    row.Children.Add(barBg);
                    InterestsContainer.Children.Add(row);
                }
            }
            catch { InterestsPlaceholder.IsVisible = true; }
        }

        // ── Contact actions ────────────────────────────
        private void OnCallTapped(object sender, EventArgs e)
        {
            try { PhoneDialer.Default.Open("0869990123"); }
            catch { }
        }

        private async void OnWhatsAppTapped(object sender, EventArgs e)
        {
            var url = "https://wa.me/27722045056";
            await Launcher.Default.OpenAsync(new Uri(url));
        }

        private async void OnEmailTapped(object sender, EventArgs e)
        {
            var message = new EmailMessage
            {
                Subject = "Career Guidance Enquiry",
                To = new List<string> { "careerhelp@dhet.gov.za" }
            };
            await Microsoft.Maui.ApplicationModel.Communication.Email.Default.ComposeAsync(message);
        }

        // ── Sign Out ───────────────────────────────────
        private async void OnSignOutTapped(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert(
                "Sign Out",
                "Are you sure you want to sign out?",
                "Sign Out", "Cancel");

            if (confirm)
            {
                Preferences.Remove("user_name");
                Preferences.Remove("user_category");
                Preferences.Remove("user_province");
                Preferences.Remove("user_language");
                Preferences.Remove("quiz_results");

                await Shell.Current.GoToAsync("//MainPage");
            }
        }
    }
}