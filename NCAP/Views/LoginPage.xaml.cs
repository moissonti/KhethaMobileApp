using NCAP.Views;

namespace NCAP.Views
{
    public partial class LoginPage : ContentPage
    {
        private bool _passwordVisible = false;

        public LoginPage()
        {
            InitializeComponent();
        }

        // ── Toggle password visibility ─────────────────
        private void OnTogglePassword(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;
            PasswordEntry.IsPassword = !_passwordVisible;
            TogglePasswordLabel.Text = _passwordVisible ? "\uE8F5" : "\uE8F4"; // visibility / visibility_off
        }

        // ── Sign In ────────────────────────────────────
        private async void OnSignInTapped(object sender, EventArgs e)
        {
            // Reset errors
            EmailError.IsVisible = false;
            PasswordError.IsVisible = false;
            GeneralErrorBorder.IsVisible = false;

            // Highlight fields
            bool hasError = false;

            if (string.IsNullOrWhiteSpace(EmailEntry.Text) ||
                !EmailEntry.Text.Contains("@"))
            {
                EmailError.IsVisible = true;
                hasError = true;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                PasswordError.IsVisible = true;
                hasError = true;
            }

            if (hasError) return;

            // Show loading
            var btn = sender as View;
            if (btn != null) await btn.ScaleTo(0.97, 80);

            // TODO: Replace with real auth call
            // Simulate a quick check
            bool loginSuccess = true; // Replace with actual auth logic

            if (btn != null) await btn.ScaleTo(1.0, 80);

            if (loginSuccess)
            {

                await Shell.Current.GoToAsync("//DashboardTabs");
            }
            else
            {
                GeneralErrorBorder.IsVisible = true;
            }
        }

        // ── Forgot password ────────────────────────────
        private async void OnForgotPassword(object sender, EventArgs e)
        {
            await DisplayAlert("Forgot Password",
                "Please contact Khetha support:\ncareerhelp@dhet.gov.za\nor call 086 999 0123",
                "OK");
        }

        // ── Back ───────────────────────────────────────
        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}