namespace NCAP.Views
{
    public partial class RegisterPage : ContentPage
    {
        private bool _passwordVisible = false;
        private bool _confirmVisible = false;

        public RegisterPage()
        {
            InitializeComponent();
        }

        private void OnTogglePassword(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            PasswordEntry.IsPassword = !_passwordVisible;
            TogglePasswordLabel.Text = _passwordVisible ? "\uE8F5" : "\uE8F4";
        }

        private void OnToggleConfirmPassword(object sender, EventArgs e)
        {
            _confirmVisible = !_confirmVisible;
            ConfirmPasswordEntry.IsPassword = !_confirmVisible;
            ToggleConfirmLabel.Text = _confirmVisible ? "\uE8F5" : "\uE8F4";
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            // Reset errors
            FullNameError.IsVisible = false;
            EmailError.IsVisible = false;
            MobileError.IsVisible = false;
            PasswordError.IsVisible = false;
            ConfirmPasswordError.IsVisible = false;

            bool hasError = false;

            if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
            { FullNameError.IsVisible = true; hasError = true; }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                !EmailEntry.Text.Contains("@") || !EmailEntry.Text.Contains("."))
            { EmailError.IsVisible = true; hasError = true; }

            var mobile = MobileEntry.Text?.Replace(" ", "").Replace("-", "") ?? "";
            if (string.IsNullOrWhiteSpace(mobile) || mobile.Length < 9)
            { MobileError.IsVisible = true; hasError = true; }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 8)
            { PasswordError.IsVisible = true; hasError = true; }

            if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
            { ConfirmPasswordError.IsVisible = true; hasError = true; }

            if (hasError) return;

            // ── POPIA Modal ────────────────────────────────────────────
            bool accepted = await DisplayAlert(
                "Privacy Notice (POPIA)",
                "By registering, you consent to Khetha (DHET) collecting and processing " +
                "your personal information to provide career guidance services.\n\n" +
                "✓ Your data is used only for career guidance.\n" +
                "✓ Your data will not be sold to third parties.\n" +
                "✓ You may request deletion of your data at any time.\n\n" +
                "For queries: careerhelp@dhet.gov.za",
                "I Accept",
                "Decline");

            if (accepted)
            {
                // Go to profile setup
                await Navigation.PushAsync(new ProfileSetupPage());
            }
            else
            {
                // Decline → back to MainPage
                await Navigation.PopToRootAsync();
            }
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}