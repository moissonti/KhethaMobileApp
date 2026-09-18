using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using NCAP.Views;

namespace NCAP
{
    public partial class MainPage : ContentPage
    {
        private readonly (string Image, string Badge, string Subtitle)[] _slides =
        {
            ("career.jpg",        "SELF-EXPLORATION",   "Discover who you are and what drives you"),
            ("main_banner_2.jpg", "CAREER GUIDANCE",    "Find the path that fits your strengths"),
            ("main_banner_3.jpg", "YOUR FUTURE STARTS", "Take the first step toward your dream career"),
        };

        private readonly (string Image, string Badge, string Subtitle)[] _slidesXhosa =
        {
            ("career.jpg",        "IZIFUNDO ZAKHO",     "Fumanisa ukuba ungubani nokuba yintoni ekukhuthazayo"),
            ("main_banner_2.jpg", "INKULISA YOMSEBENZI","Fumana indlela efanelekileyo kwezona zakhono zakho"),
            ("main_banner_3.jpg", "IKAMVA LAKHO",       "Ngenisa inyathelo lokuqala ekuphumeleleni"),
        };

        private int _currentSlide = 0;
        private System.Timers.Timer? _timer;
        private Ellipse[] _dots = Array.Empty<Ellipse>();
        private bool _isXhosa = false;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Always show language modal on every load
            await ShowLanguageModal();

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

        // ── Language Modal ─────────────────────────────
        private async Task ShowLanguageModal()
        {
            // Dim overlay
            var overlay = new Grid
            {
                BackgroundColor = Color.FromArgb("#80000000"),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };

            // Modal card
            var card = new Border
            {
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                Margin = new Thickness(24, 0),
                VerticalOptions = LayoutOptions.Center,
                TranslationY = 400,
                StrokeShape = new RoundRectangle { CornerRadius = 24 }
            };

            var content = new VerticalStackLayout { Padding = new Thickness(24), Spacing = 20 };

            // Header
            var headerStack = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center };
            headerStack.Children.Add(new Label
            {
                Text = "🌍",
                FontSize = 36,
                HorizontalOptions = LayoutOptions.Center
            });
            headerStack.Children.Add(new Label
            {
                Text = "Choose Your Language",
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#1A3C2E"),
                HorizontalOptions = LayoutOptions.Center
            });
            headerStack.Children.Add(new Label
            {
                Text = "Khetha ulwimi lwakho / Select your language",
                FontSize = 12,
                TextColor = Color.FromArgb("#888888"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            });
            content.Children.Add(headerStack);

            // Divider
            content.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F0F0F0") });

            // Language options
            var languages = new[]
            {
                ("🇿🇦", "English", "en", "Continue in English"),
                ("🇿🇦", "IsiXhosa", "xh", "Qhubeka ngeXhosa"),
                ("🇿🇦", "IsiZulu", "zu", "Qhubeka ngesiZulu"),
                ("🇿🇦", "Afrikaans", "af", "Gaan voort in Afrikaans"),
                ("🇿🇦", "Sesotho", "st", "Tswela pele ka Sesotho"),
            };

            var tcs = new TaskCompletionSource<string>();

            foreach (var (flag, name, code, subtitle) in languages)
            {
                var langCode = code;
                var btn = new Border
                {
                    BackgroundColor = Color.FromArgb("#F5F7F5"),
                    StrokeThickness = 1.5f,
                    Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")),
                    Padding = new Thickness(16, 12),
                    StrokeShape = new RoundRectangle { CornerRadius = 12 }
                };

                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                row.Children.Add(new Label { Text = flag, FontSize = 22, VerticalOptions = LayoutOptions.Center });

                var textStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
                textStack.Children.Add(new Label { Text = name, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });
                textStack.Children.Add(new Label { Text = subtitle, FontSize = 11, TextColor = Color.FromArgb("#888888") });
                Grid.SetColumn(textStack, 1);
                row.Children.Add(textStack);

                var arrow = new Label { Text = "›", FontSize = 20, TextColor = Color.FromArgb("#CCCCCC"), VerticalOptions = LayoutOptions.Center };
                Grid.SetColumn(arrow, 2);
                row.Children.Add(arrow);

                btn.Content = row;

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) =>
                {
                    // Highlight selected
                    btn.BackgroundColor = Color.FromArgb("#E8F5EE");
                    btn.Stroke = new SolidColorBrush(Color.FromArgb("#1A3C2E"));
                    await Task.Delay(150);
                    tcs.TrySetResult(langCode);
                };
                btn.GestureRecognizers.Add(tap);
                content.Children.Add(btn);
            }

            card.Content = content;
            overlay.Children.Add(card);

            // Add overlay to page
            var grid = (Grid)Content;
            grid.Add(overlay);
            Grid.SetRowSpan(overlay, 3);

            // Animate card in
            await card.TranslateTo(0, 0, 350, Easing.CubicOut);

            // Wait for user selection
            var selectedLang = await tcs.Task;

            // Animate out
            await card.TranslateTo(0, 400, 250, Easing.CubicIn);
            grid.Remove(overlay);

            // Save and apply
            Preferences.Set("app_language", selectedLang);
            _isXhosa = selectedLang == "xh";
            ApplyLanguage();
        }

        // ── Apply Language to MainPage only ────────────
        private void ApplyLanguage()
        {
            if (!_isXhosa) return;

            // Update all text labels on this page to Xhosa
            // Tagline
            TaglineLabel.Text = "Inkulisa Yakho Yomsebenzi, Naphina Ufana";
            TaglineSubLabel.Text = "Simahla · Ithenjwa · Yawo Wonke AmaSouth Africa";
            SectionLabel.Text = "UNAKHO UKWENZA";

            // Cards
            Card1Title.Text = "Umbuzo Wokukhetha";
            Card1Sub.Text = "Iingcebiso ezikhethekileyo";
            Card1Action.Text = "Qala →";

            Card2Title.Text = "Ukukhetha Izifundo";
            Card2Sub.Text = "Izifundo ezifanelekileyo";
            Card2Action.Text = "Qala →";

            Card3Title.Text = "Ukufaneleka Komsebenzi";
            Card3Sub.Text = "Fumana imisebenzi efanelekileyo";
            Card3Action.Text = "Qala →";

            Card4Title.Text = "Izahluko Zokufunda";
            Card4Sub.Text = "Iyunivesithi, TVET neziqu";
            Card4Action.Text = "Phanda →";

            Card5Title.Text = "Imisebenzi Emihlaza";
            Card5Sub.Text = "Imisebenzi yekamva";
            Card5Action.Text = "Jonga →";

            Card6Title.Text = "Isebenza Ungaxhobile";
            Card6Sub.Text = "Iyasebenza kwiindawo zasemaphandleni";
            Card6Action.Text = "Funda okuninzi →";

            // Footer
            FooterLabel.Text = "Inkonzo yasimahla yoMnyango weMfundo ePhakamileyo neSilimo";

            // Buttons
            RegisterBtn.Text = "Bhalisa";
            LoginBtn.Text = "Ngena";
        }

        // ── Carousel ───────────────────────────────────
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
            var slides = _isXhosa ? _slidesXhosa : _slides;
            var (img, badge, subtitle) = slides[index];
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

        // ── Navigation ─────────────────────────────────
        private async void OnLoginTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new LoginPage());

        private async void OnRegisterTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new RegisterPage());
    }
}