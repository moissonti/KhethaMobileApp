namespace NCAP.Views.Dashboard
{
    public partial class DashboardPage : ContentPage
    {
        public DashboardPage()
        {
            InitializeComponent();
            SetGreeting();
            LoadSettings();
        }

        private void SetGreeting()
        {
            var hour = DateTime.Now.Hour;
            GreetingLabel.Text = hour switch
            {
                < 12 => "Good morning 👋",
                < 17 => "Good afternoon 👋",
                _ => "Good evening 👋"
            };
            var name = Preferences.Get("user_name", "");
            UserNameLabel.Text = string.IsNullOrEmpty(name) ? "Welcome back" : $"Hello, {name}";
        }

        private void LoadSettings()
        {
            NotificationsToggle.IsToggled = Preferences.Get("notifications_on", true);
            //OfflineToggle.IsToggled = Preferences.Get("offline_mode", true);
            LargeTextToggle.IsToggled = Preferences.Get("large_text", false);
            HighContrastToggle.IsToggled = Preferences.Get("high_contrast", false);

            var lang = Preferences.Get("app_language", "en");
            CurrentLanguageLabel.Text = lang switch
            {
                "xh" => "IsiXhosa",
                "zu" => "IsiZulu",
                "af" => "Afrikaans",
                "st" => "Sesotho",
                _ => "English"
            };
        }

        private void OnHighContrastToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("high_contrast", e.Value);

            if (e.Value)
            {
                // High contrast — black background, white text, yellow accents
                Application.Current!.Resources["PrimaryColor"] = Color.FromArgb("#000000");
                Application.Current!.Resources["AccentColor"] = Color.FromArgb("#FFD700");
                Application.Current!.Resources["SurfaceColor"] = Color.FromArgb("#1A1A1A");
            }
            else
            {
                // Reset to normal Khetha theme
                Application.Current!.Resources["PrimaryColor"] = Color.FromArgb("#1A3C2E");
                Application.Current!.Resources["AccentColor"] = Color.FromArgb("#C8972B");
                Application.Current!.Resources["SurfaceColor"] = Color.FromArgb("#F5F7F5");
            }
        }

        // ── Notifications ──────────────────────────────
        private async void OnNotificationsTapped(object sender, EventArgs e)
        {
            await DisplayAlert(
                "🔔 Notifications",
                "• NSFAS applications open in January 2026\n" +
                "• Complete your Career Quiz to get matched\n" +
                "• New: Green Careers added to the directory\n" +
                "• Khetha career fair — 15 March 2026",
                "OK");
        }

        // ── Settings modal ─────────────────────────────
        private async void OnSettingsTapped(object sender, EventArgs e)
        {
            SettingsOverlay.IsVisible = true;
            await Task.CompletedTask;
        }

        private async void OnCloseSettings(object sender, EventArgs e)
        {
            SettingsOverlay.IsVisible = false;
            await Task.CompletedTask;
        }

        // ── Toggles ────────────────────────────────────
        private void OnNotificationsToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("notifications_on", e.Value);
        }

        private void OnOfflineToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("offline_mode", e.Value);
        }

        private void OnLargeTextToggled(object sender, ToggledEventArgs e)
        {
            Preferences.Set("large_text", e.Value);
            // Apply large text immediately
            if (e.Value)
                Application.Current!.Resources["DefaultFontSize"] = 18.0;
            else
                Application.Current!.Resources["DefaultFontSize"] = 14.0;
        }

        // ── Language change ────────────────────────────
        private async void OnLanguageTapped(object sender, EventArgs e)
        {
            var result = await DisplayActionSheet(
                "Choose Language", "Cancel", null,
                "English", "IsiXhosa", "IsiZulu", "Afrikaans", "Sesotho");

            if (result == null || result == "Cancel") return;

            var code = result switch
            {
                "IsiXhosa" => "xh",
                "IsiZulu" => "zu",
                "Afrikaans" => "af",
                "Sesotho" => "st",
                _ => "en"
            };

            Preferences.Set("app_language", code);
            CurrentLanguageLabel.Text = result;

            await DisplayAlert("Language Updated",
                $"Language set to {result}. Restart the app to apply changes to all screens.",
                "OK");
        }

        // ── Navigation ─────────────────────────────────
        private async void OnExploreTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//ExplorePage");

        private async void OnGuideTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//GuidePage");
    }
}