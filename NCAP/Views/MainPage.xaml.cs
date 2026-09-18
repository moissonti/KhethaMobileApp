using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NCAP.Views;

namespace NCAP
{
    public partial class MainPage : ContentPage
    {
        // ── Carousel data ──────────────────────────────────────────────
        private readonly (string Image, string Badge, string Subtitle)[] _slides =
        {
            ("career.jpg",  "SELF-EXPLORATION",   "Discover who you are and what drives you"),
            ("main_banner_2.jpg",  "CAREER GUIDANCE",    "Find the path that fits your strengths"),
            ("main_banner_3.jpg",  "YOUR FUTURE STARTS", "Take the first step toward your dream career"),
        };

        private int _currentSlide = 0;
        private System.Timers.Timer? _timer;
        private Ellipse[] _dots = Array.Empty<Ellipse>();

        // ── Constructor ────────────────────────────────────────────────
        public MainPage()
        {
            InitializeComponent();
        }

        // ── Lifecycle ──────────────────────────────────────────────────
        protected override void OnAppearing()
        {
            base.OnAppearing();
            BuildDots();
            ShowSlide(0);
            StartTimer();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        // ── Carousel helpers ───────────────────────────────────────────
        private void BuildDots()
        {
            DotsContainer.Children.Clear();
            _dots = new Ellipse[_slides.Length];
            for (int i = 0; i < _slides.Length; i++)
            {
                var dot = new Ellipse
                {
                    WidthRequest = 7,
                    HeightRequest = 7,
                    Fill = i == 0 ? Brush.White : new SolidColorBrush(Color.FromArgb("#66FFFFFF")),
                };
                _dots[i] = dot;
                DotsContainer.Children.Add(dot);
            }
        }

        private void ShowSlide(int index)
        {
            var (img, badge, subtitle) = _slides[index];
            SlideImage.Source = img;
            SlideBadgeLabel.Text = badge;
            SlideSubtitleLabel.Text = subtitle;

            for (int i = 0; i < _dots.Length; i++)
                _dots[i].Fill = i == index
                    ? Brush.White
                    : new SolidColorBrush(Color.FromArgb("#66FFFFFF"));
        }

        private void StartTimer()
        {
            _timer = new System.Timers.Timer(3500);
            _timer.Elapsed += (_, _) =>
            {
                _currentSlide = (_currentSlide + 1) % _slides.Length;
                MainThread.BeginInvokeOnMainThread(() => ShowSlide(_currentSlide));
            };
            _timer.AutoReset = true;
            _timer.Start();
        }

        // ── Navigation ─────────────────────────────────────────────────
        private async void OnLoginTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage());
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}