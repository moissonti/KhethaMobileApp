namespace NCAP.Views
{
    public partial class ProfileSetupPage : ContentPage
    {
        // ── Quiz questions (NCAP Interest Questionnaire) ───────────────
        private readonly (string Question, string[] Fields)[] _questions =
        {
            ("I enjoy solving mathematical or logical problems",
                new[] { "Engineering", "IT", "Finance", "Science" }),
            ("I like working with people and helping them",
                new[] { "Health", "Social Sciences", "Education" }),
            ("I enjoy creative activities like drawing, designing or writing",
                new[] { "Arts", "Design", "Media" }),
            ("I am interested in how machines and electronics work",
                new[] { "Engineering", "Trades", "IT" }),
            ("I enjoy learning about business and how companies operate",
                new[] { "Business", "Finance", "Marketing" }),
            ("I like being outdoors and working with nature or animals",
                new[] { "Agriculture", "Environmental Science", "Veterinary" }),
            ("I am interested in law, justice and human rights",
                new[] { "Law", "Public Administration", "Social Sciences" }),
            ("I enjoy physical work and building things with my hands",
                new[] { "Trades", "Construction", "Engineering" }),
            ("I like researching and investigating topics in depth",
                new[] { "Science", "Medicine", "Social Sciences" }),
            ("I enjoy using computers and technology every day",
                new[] { "IT", "Data Science", "Engineering" }),
            ("I am interested in health and caring for people",
                new[] { "Health", "Medicine", "Nursing" }),
            ("I enjoy public speaking, debating or performing",
                new[] { "Law", "Education", "Media", "Politics" }),
            ("I like cooking, preparing food or hospitality",
                new[] { "Hospitality", "Tourism", "Food Science" }),
            ("I enjoy working with numbers and financial records",
                new[] { "Finance", "Accounting", "Administration" }),
            ("I am interested in protecting the environment",
                new[] { "Environmental Science", "Agriculture", "Green Careers" }),
        };

        private int _currentStep = 1;
        private int _currentQuestion = 0;
        private string _selectedCategory = "";

        // Field scores: tracks how many Yes answers map to each field
        private readonly Dictionary<string, int> _fieldScores = new();

        // Category border tracking
        private Border? _selectedCategoryBorder;

        public ProfileSetupPage()
        {
            InitializeComponent();
            ShowQuestion(_currentQuestion);
        }

        // ── CATEGORY SELECTION ─────────────────────────────────────────
        private void OnCategoryTapped(object sender, TappedEventArgs e)
        {
            _selectedCategory = e.Parameter?.ToString() ?? "";

            // Reset all
            var borders = new[] { CatLearner, CatMatric, CatJobSeeker, CatCareerChanger };
            foreach (var b in borders)
            {
                b.Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0"));
                b.BackgroundColor = Colors.White;
            }

            // Highlight selected
            if (sender is Border selected)
            {
                selected.Stroke = new SolidColorBrush(Color.FromArgb("#1A3C2E"));
                selected.BackgroundColor = Color.FromArgb("#E8F5EE");
            }

            CategoryError.IsVisible = false;
        }

        // ── BOTTOM BUTTON ──────────────────────────────────────────────
        private async void OnBottomButtonTapped(object sender, EventArgs e)
        {
            if (_currentStep == 1)
            {
                // Validate Step 1
                bool hasError = false;

                if (string.IsNullOrEmpty(_selectedCategory))
                {
                    CategoryError.IsVisible = true;
                    hasError = true;
                }
                if (ProvincePicker.SelectedIndex < 0)
                {
                    ProvinceError.IsVisible = true;
                    hasError = true;
                }
                if (LanguagePicker.SelectedIndex < 0)
                {
                    LanguageError.IsVisible = true;
                    hasError = true;
                }

                if (hasError) return;

                await GoToStep2();
            }
            else if (_currentStep == 3)
            {
                // Navigate to Dashboard
                await Shell.Current.GoToAsync("//DashboardTabs");
            }
        }

        // ── STEP TRANSITIONS ───────────────────────────────────────────
        private async Task GoToStep2()
        {
            _currentStep = 2;
            HeaderSubtitle.Text = "Quick interest check";
            BottomButtonLabel.Text = "Skip Quiz";

            // Update step indicator
            Step2Dot.BackgroundColor = Color.FromArgb("#1A3C2E");
            Step2Label.TextColor = Colors.White;
            Step2Label.FontAttributes = FontAttributes.Bold;
            StepLine1.Color = Color.FromArgb("#1A3C2E");

            await Step1View.FadeTo(0, 150);
            Step1View.IsVisible = false;
            Step2View.IsVisible = true;
            await Step2View.FadeTo(1, 200);

            ShowQuestion(0);
        }

        private async Task GoToStep3()
        {
            _currentStep = 3;
            HeaderSubtitle.Text = "Your career profile";
            BottomButtonLabel.Text = "Go to Dashboard";

            // Update step indicator
            Step3Dot.BackgroundColor = Color.FromArgb("#1A3C2E");
            Step3Label.TextColor = Colors.White;
            StepLine2.Color = Color.FromArgb("#1A3C2E");

            await Step2View.FadeTo(0, 150);
            Step2View.IsVisible = false;
            Step3View.IsVisible = true;
            await Step3View.FadeTo(1, 200);

            ShowResults();
        }

        // ── QUIZ LOGIC ─────────────────────────────────────────────────
        private void ShowQuestion(int index)
        {
            if (index >= _questions.Length) return;

            var (question, _) = _questions[index];
            QuestionText.Text = question;
            QuestionCounter.Text = $"Question {index + 1} of {_questions.Length}";

            // Update progress bar width
            double progress = (double)(index) / _questions.Length;
            double maxWidth = DeviceDisplay.Current.MainDisplayInfo.Width /
                              DeviceDisplay.Current.MainDisplayInfo.Density - 48;
            QuizProgressBar.WidthRequest = maxWidth * progress;
        }

        private async void OnAnswerYes(object sender, EventArgs e)
        {
            var (_, fields) = _questions[_currentQuestion];
            foreach (var field in fields)
            {
                _fieldScores.TryAdd(field, 0);
                _fieldScores[field]++;
            }
            await NextQuestion();
        }

        private async void OnAnswerNo(object sender, EventArgs e)
        {
            await NextQuestion();
        }

        private async Task NextQuestion()
        {
            // Animate card
            await QuestionText.TranslateTo(-30, 0, 100);
            _currentQuestion++;

            if (_currentQuestion >= _questions.Length)
            {
                await GoToStep3();
                return;
            }

            QuestionText.TranslationX = 30;
            ShowQuestion(_currentQuestion);
            await QuestionText.TranslateTo(0, 0, 150);
        }

        // ── RESULTS ────────────────────────────────────────────────────
        private void ShowResults()
        {
            ResultsContainer.Children.Clear();

            var top = _fieldScores
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            if (!top.Any())
            {
                ResultsContainer.Children.Add(new Label
                {
                    Text = "Complete the interest quiz to see your top career fields.",
                    FontSize = 13,
                    TextColor = Color.FromArgb("#888888"),
                    HorizontalTextAlignment = TextAlignment.Center
                });
                return;
            }

            int maxScore = top.First().Value;

            foreach (var (field, score) in top)
            {
                double percent = maxScore > 0 ? (double)score / maxScore : 0;

                var card = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = new Thickness(16, 14),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 }
                };

                var stack = new VerticalStackLayout { Spacing = 8 };

                // Field name + score
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var fieldLabel = new Label
                {
                    Text = field,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1A3C2E")
                };
                Grid.SetColumn(fieldLabel, 0);

                var scoreLabel = new Label
                {
                    Text = $"{(int)(percent * 100)}%",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#C8972B")
                };
                Grid.SetColumn(scoreLabel, 1);

                row.Children.Add(fieldLabel);
                row.Children.Add(scoreLabel);

                // Progress bar
                var barBg = new Grid();
                var barBgBox = new BoxView
                {
                    HeightRequest = 6,
                    Color = Color.FromArgb("#E8F5EE"),
                    CornerRadius = 3
                };
                var barFill = new BoxView
                {
                    HeightRequest = 6,
                    Color = Color.FromArgb("#1A3C2E"),
                    CornerRadius = 3,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 240 * percent
                };
                barBg.Children.Add(barBgBox);
                barBg.Children.Add(barFill);

                stack.Children.Add(row);
                stack.Children.Add(barBg);
                card.Content = stack;
                ResultsContainer.Children.Add(card);
            }
        }
    }
}