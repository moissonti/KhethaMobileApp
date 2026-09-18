using System.Text.Json;

namespace NCAP.Views.Dashboard
{
    public partial class GuidePage : ContentPage
    {
        // ── Career match data ──────────────────────────
        private readonly Dictionary<string, List<(string Title, string OFO, string Pathway)>> _careerMatches = new()
        {
            ["IT"] = new() { ("Software Developer", "251201", "BSc Computer Science · NQF 7"), ("Data Scientist", "252902", "BSc Data Science · NQF 7"), ("ICT Security Specialist", "252901", "BSc Cybersecurity + Certs") },
            ["Engineering"] = new() { ("Civil Engineer", "214201", "BEng Civil · NQF 7 · 4 years"), ("Electrical Engineer", "215101", "BEng Electrical · NQF 7 · 4 years"), ("Mechanical Engineer", "214401", "BEng Mechanical · NQF 7") },
            ["Health"] = new() { ("Professional Nurse", "222101", "Bachelor of Nursing · NQF 7"), ("General Medical Practitioner", "221101", "MBChB · NQF 7 · 6 years"), ("Pharmacist", "226201", "BPharm · NQF 7 · 4 years") },
            ["Finance"] = new() { ("Accountant (General)", "241101", "BCom Accounting · NQF 7"), ("Financial Analyst", "241201", "BCom Finance · NQF 7"), ("Tax Professional", "335901", "National Diploma Accounting · NQF 6") },
            ["Law"] = new() { ("Lawyer", "261101", "LLB · NQF 7 · 4 years"), ("Paralegal", "341101", "National Diploma Paralegal · NQF 6"), ("Advocate", "261106", "LLB + Bar Council Pupillage") },
            ["Education"] = new() { ("Secondary School Teacher", "232101", "BEd FET · NQF 7 · 4 years"), ("Primary School Teacher", "233101", "BEd Foundation · NQF 7"), ("Early Childhood Educator", "234101", "Diploma ECD · NQF 6") },
            ["Trades"] = new() { ("Electrician", "661101", "N1-N3 TVET + Trade Test (Red Seal)"), ("Plumber", "642601", "N1-N3 TVET + Trade Test"), ("Automotive Motor Mechanic", "671101", "N1-N3 TVET + Trade Test") },
            ["Social Sciences"] = new() { ("Social Worker", "263501", "Bachelor of Social Work · NQF 7"), ("Psychologist", "263401", "BA Psych + Masters · NQF 9"), ("Community Dev Worker", "341301", "National Diploma Community Dev") },
            ["Agriculture"] = new() { ("Agricultural Scientist", "213201", "BSc Agriculture · NQF 7"), ("Agricultural Farm Manager", "131101", "National Diploma Agriculture"), ("Environmental Scientist", "213301", "BSc Environmental Science") },
            ["Business"] = new() { ("Finance Manager", "121101", "BCom + Honours · NQF 8"), ("Sales and Marketing Manager", "122101", "BCom Marketing · NQF 7"), ("Management Consultant", "242101", "BCom + MBA · NQF 9") },
        };

        // ── Subject → careers ──────────────────────────
        private readonly Dictionary<string, (string[] Careers, string[] AlsoStudy)> _subjectMap = new()
        {
            ["Mathematics"] = (new[] { "Software Developer", "Civil Engineer", "Electrical Engineer", "Accountant", "Data Scientist", "Actuary", "Financial Analyst" }, new[] { "Physical Sciences", "Information Technology", "Accounting" }),
            ["Physical Sciences"] = (new[] { "Chemical Engineer", "Mechanical Engineer", "Pharmacist", "General Medical Practitioner", "Electrician (Trade)", "Physicist" }, new[] { "Mathematics", "Life Sciences" }),
            ["Life Sciences"] = (new[] { "General Medical Practitioner", "Professional Nurse", "Pharmacist", "Agricultural Scientist", "Dentist", "Veterinarian" }, new[] { "Physical Sciences", "Mathematics", "Agricultural Sciences" }),
            ["Information Technology"] = (new[] { "Software Developer", "ICT Security Specialist", "Data Scientist", "Computer Network Technician", "Web Developer", "Systems Analyst" }, new[] { "Mathematics", "Computer Applications Technology" }),
            ["Accounting"] = (new[] { "Accountant (General)", "Financial Analyst", "Tax Professional", "Auditor", "Finance Manager", "Bookkeeper" }, new[] { "Mathematics", "Business Studies", "Economics" }),
            ["Business Studies"] = (new[] { "Sales and Marketing Manager", "Entrepreneur", "Human Resource Manager", "Public Relations Professional", "Retail Manager" }, new[] { "Accounting", "Economics", "Mathematics" }),
            ["History"] = (new[] { "Lawyer", "Advocate", "Journalist", "Archivist", "Diplomat", "Policy Analyst" }, new[] { "English Home Language", "Geography", "Economics" }),
            ["Geography"] = (new[] { "Urban and Regional Planner", "Environmental Scientist", "Geologist", "Surveyor", "GIS Specialist" }, new[] { "Mathematics", "Agricultural Sciences", "Life Sciences" }),
            ["Economics"] = (new[] { "Economist", "Financial Adviser", "Policy Analyst", "Banker", "Investment Analyst" }, new[] { "Mathematics", "Accounting", "Business Studies" }),
            ["Engineering Graphics and Design"] = (new[] { "Architect", "Civil Engineer", "Draughtsman", "Interior Designer", "Mechanical Engineer" }, new[] { "Mathematics", "Physical Sciences" }),
            ["Agricultural Sciences"] = (new[] { "Agricultural Scientist", "Agricultural Farm Manager", "Food Scientist", "Veterinarian", "Environmental Manager" }, new[] { "Life Sciences", "Geography" }),
            ["Mathematical Literacy"] = (new[] { "Social Worker", "Teacher", "Police Officer", "Enrolled Nurse", "Tourism Officer", "Retail Manager" }, new[] { "Business Studies", "Economics", "Life Sciences" }),
            ["Computer Applications Technology"] = (new[] { "ICT User Support Technician", "Data Entry Operator", "Office Administrator", "Help Desk Technician" }, new[] { "Information Technology", "Mathematics" }),
            ["Visual Arts"] = (new[] { "Graphic Designer", "Architect", "Photographer", "Animator", "Interior Designer", "Art Teacher" }, new[] { "Engineering Graphics and Design", "Mathematics" }),
        };

        // ── Career → required subjects ─────────────────
        private readonly Dictionary<string, (string[] Compulsory, string[] Advisable)> _careerSubjectMap = new()
        {
            ["Software Developer"] = (new[] { "Mathematics", "English" }, new[] { "Information Technology", "Physical Sciences", "CAT" }),
            ["Civil Engineer"] = (new[] { "Mathematics", "Physical Sciences" }, new[] { "Engineering Graphics and Design" }),
            ["Medical Doctor"] = (new[] { "Mathematics", "Physical Sciences", "Life Sciences" }, new[] { "English Home Language" }),
            ["Accountant"] = (new[] { "Mathematics", "Accounting" }, new[] { "Business Studies", "Economics" }),
            ["Lawyer"] = (new[] { "English Home Language" }, new[] { "History", "Life Orientation" }),
            ["Professional Nurse"] = (new[] { "Life Sciences", "English" }, new[] { "Mathematics", "Physical Sciences" }),
            ["Architect"] = (new[] { "Mathematics", "Physical Sciences" }, new[] { "Engineering Graphics and Design", "Visual Arts" }),
            ["Teacher"] = (new[] { "English", "Life Orientation" }, new[] { "Any 3 relevant subjects" }),
            ["Electrician"] = (new[] { "Mathematics or Maths Literacy" }, new[] { "Physical Sciences", "EGD" }),
            ["Agricultural Scientist"] = (new[] { "Life Sciences", "Mathematics" }, new[] { "Agricultural Sciences", "Geography" }),
        };

        // ── Quiz questions ─────────────────────────────
        private readonly (string Question, string[] Fields)[] _questions =
        {
            ("I enjoy solving mathematical or logical problems", new[] { "IT", "Engineering", "Finance" }),
            ("I like working with people and helping them recover or grow", new[] { "Health", "Education", "Social Sciences" }),
            ("I enjoy creative activities like designing, writing or performing", new[] { "Business", "Social Sciences" }),
            ("I am interested in how machines, circuits and electronics work", new[] { "Engineering", "Trades", "IT" }),
            ("I enjoy learning about business, entrepreneurship or markets", new[] { "Business", "Finance", "Law" }),
            ("I like being outdoors and working with nature, animals or farming", new[] { "Agriculture" }),
            ("I am interested in law, justice and protecting people's rights", new[] { "Law", "Social Sciences", "Education" }),
            ("I enjoy physical work and building or repairing things with my hands", new[] { "Trades", "Engineering" }),
            ("I like researching topics, reading and writing detailed reports", new[] { "Law", "Social Sciences" }),
            ("I enjoy using computers, coding or solving tech problems every day", new[] { "IT", "Engineering" }),
            ("I am interested in health, medicine and caring for sick people", new[] { "Health" }),
            ("I enjoy teaching others, explaining concepts or mentoring people", new[] { "Education", "Social Sciences" }),
            ("I like working with numbers, spreadsheets and financial records", new[] { "Finance", "Business" }),
            ("I enjoy managing projects, leading teams or organising people", new[] { "Business", "Law", "Education" }),
            ("I am passionate about the environment and sustainability", new[] { "Agriculture", "Engineering" }),
        };

        private int _currentQuestion = 0;
        private readonly Dictionary<string, int> _fieldScores = new();
        private readonly HashSet<string> _selectedSubjects = new();
        private readonly Dictionary<string, Border> _subjectBorderMap = new();

        public GuidePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadQuizResults();
            BuildSubjectButtons();
            BuildCareerSubjectList();
        }

        // ── SUBJECT BUTTONS (built in code, no XAML taps) ──
        private void BuildSubjectButtons()
        {
            if (SubjectFlexLayout.Children.Any()) return; // already built

            _subjectBorderMap.Clear();
            var subjects = _subjectMap.Keys.ToList();

            foreach (var subject in subjects)
            {
                var subjectCopy = subject;
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 1.5f,
                    Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")),
                    Padding = new Thickness(14, 10),
                    Margin = new Thickness(0, 0, 8, 8),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 }
                };

                border.Content = new Label
                {
                    Text = subject.Length > 18 ? subject[..18] + "…" : subject,
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1A3C2E")
                };

                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => HandleSubjectTap(subjectCopy);
                border.GestureRecognizers.Add(tap);

                _subjectBorderMap[subject] = border;
                SubjectFlexLayout.Children.Add(border);
            }
        }




        private void HandleSubjectTap(string subject)
        {
            if (_selectedSubjects.Contains(subject))
            {
                _selectedSubjects.Remove(subject);
                if (_subjectBorderMap.TryGetValue(subject, out var b))
                {
                    b.BackgroundColor = Colors.White;
                    b.Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0"));
                }
            }
            else
            {
                _selectedSubjects.Add(subject);
                if (_subjectBorderMap.TryGetValue(subject, out var b))
                {
                    b.BackgroundColor = Color.FromArgb("#E8F5EE");
                    b.Stroke = new SolidColorBrush(Color.FromArgb("#1A3C2E"));
                }
            }
            UpdateSubjectResults();
        }

        private void UpdateSubjectResults()
        {
            if (!_selectedSubjects.Any()) { SubjectResultCard.IsVisible = false; return; }

            var careerCounts = new Dictionary<string, int>();
            var alsoStudy = new HashSet<string>();

            foreach (var subject in _selectedSubjects)
            {
                if (!_subjectMap.TryGetValue(subject, out var data)) continue;
                foreach (var career in data.Careers) { careerCounts.TryAdd(career, 0); careerCounts[career]++; }
                foreach (var s in data.AlsoStudy) if (!_selectedSubjects.Contains(s)) alsoStudy.Add(s);
            }

            SubjectResultContent.Children.Clear();
            SubjectResultTitle.Text = $"With {string.Join(" + ", _selectedSubjects.Take(2))}{(_selectedSubjects.Count > 2 ? " +" : "")} you can pursue:";

            foreach (var (career, count) in careerCounts.OrderByDescending(x => x.Value).Take(6))
            {
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                row.Children.Add(new BoxView
                {
                    WidthRequest = 7,
                    HeightRequest = 7,
                    Color = Color.FromArgb("#1A3C2E"),
                    CornerRadius = 3.5f,
                    VerticalOptions = LayoutOptions.Center
                });

                var lbl = new Label
                {
                    Text = career,
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#1A3C2E"),
                    VerticalOptions = LayoutOptions.Center
                };
                Grid.SetColumn(lbl, 1);
                row.Children.Add(lbl);

                if (count > 1)
                {
                    var badge = new Border
                    {
                        BackgroundColor = Color.FromArgb("#E8F5EE"),
                        StrokeThickness = 0,
                        Padding = new Thickness(6, 2),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                        Content = new Label { Text = $"{count} subjects", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") }
                    };
                    Grid.SetColumn(badge, 2);
                    row.Children.Add(badge);
                }

                SubjectResultContent.Children.Add(row);
            }

            if (alsoStudy.Any())
            {
                SubjectResultContent.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromArgb("#F0F0F0"), Margin = new Thickness(0, 8) });
                SubjectResultContent.Children.Add(new Label { Text = "Also consider:", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#C8972B") });
                SubjectResultContent.Children.Add(new Label { Text = string.Join(", ", alsoStudy.Take(4)), FontSize = 12, TextColor = Color.FromArgb("#555555"), LineBreakMode = LineBreakMode.WordWrap });
            }

            SubjectResultCard.IsVisible = true;
        }

        // ── CAREER → SUBJECTS ──────────────────────────
        private void BuildCareerSubjectList()
        {
            if (CareerSubjectList.Children.Any()) return;

            foreach (var (career, reqs) in _careerSubjectMap)
            {
                var card = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = new Thickness(16, 14),
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 }
                };
                var stack = new VerticalStackLayout { Spacing = 8 };
                stack.Children.Add(new Label { Text = career, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });

                var compRow = new VerticalStackLayout { Spacing = 2 };
                compRow.Children.Add(new Label { Text = "Compulsory", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#D32F2F") });
                compRow.Children.Add(new Label { Text = string.Join(", ", reqs.Compulsory), FontSize = 12, TextColor = Color.FromArgb("#555555"), LineBreakMode = LineBreakMode.WordWrap });
                stack.Children.Add(compRow);

                var advRow = new VerticalStackLayout { Spacing = 2 };
                advRow.Children.Add(new Label { Text = "Advisable", FontSize = 10, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#C8972B") });
                advRow.Children.Add(new Label { Text = string.Join(", ", reqs.Advisable), FontSize = 12, TextColor = Color.FromArgb("#555555"), LineBreakMode = LineBreakMode.WordWrap });
                stack.Children.Add(advRow);

                card.Content = stack;
                CareerSubjectList.Children.Add(card);
            }
        }

        // ── TAB SWITCHING ──────────────────────────────
        private void OnTabRoadmap(object sender, EventArgs e) => SwitchTab(0);
        private void OnTabQuiz(object sender, EventArgs e) => SwitchTab(1);
        private void OnTabSubjects(object sender, EventArgs e) => SwitchTab(2);

        private void SwitchTab(int tab)
        {
            RoadmapView.IsVisible = tab == 0;
            QuizView.IsVisible = tab == 1;
            SubjectsView.IsVisible = tab == 2;

            TabRoadmap.BackgroundColor = tab == 0 ? Color.FromArgb("#1A3C2E") : Colors.Transparent;
            TabQuiz.BackgroundColor = tab == 1 ? Color.FromArgb("#1A3C2E") : Colors.Transparent;
            TabSubjects.BackgroundColor = tab == 2 ? Color.FromArgb("#1A3C2E") : Colors.Transparent;

            LblRoadmap.TextColor = tab == 0 ? Colors.White : Color.FromArgb("#888888");
            LblRoadmap.FontAttributes = tab == 0 ? FontAttributes.Bold : FontAttributes.None;
            LblQuiz.TextColor = tab == 1 ? Colors.White : Color.FromArgb("#888888");
            LblQuiz.FontAttributes = tab == 1 ? FontAttributes.Bold : FontAttributes.None;
            LblSubjects.TextColor = tab == 2 ? Colors.White : Color.FromArgb("#888888");
            LblSubjects.FontAttributes = tab == 2 ? FontAttributes.Bold : FontAttributes.None;
        }

        // ── SUBJECT MODE TOGGLE ────────────────────────
        private void OnModeSubjectToCareer(object sender, EventArgs e)
        {
            SubjectPickerView.IsVisible = true;
            CareerPickerView.IsVisible = false;
            ModeSubjectToCareer.BackgroundColor = Color.FromArgb("#1A3C2E");
            ModeCareerToSubject.BackgroundColor = Colors.Transparent;
            LblCareerMode.TextColor = Color.FromArgb("#888888");
            LblCareerMode.FontAttributes = FontAttributes.None;
        }

        private void OnModeCareerToSubject(object sender, EventArgs e)
        {
            SubjectPickerView.IsVisible = false;
            CareerPickerView.IsVisible = true;
            ModeCareerToSubject.BackgroundColor = Color.FromArgb("#1A3C2E");
            ModeSubjectToCareer.BackgroundColor = Colors.Transparent;
            LblCareerMode.TextColor = Colors.White;
            LblCareerMode.FontAttributes = FontAttributes.Bold;
        }

        // ── QUIZ RESULTS ───────────────────────────────
        private void LoadQuizResults()
        {
            var resultsJson = Preferences.Get("quiz_results", "");
            if (string.IsNullOrEmpty(resultsJson)) return;

            try
            {
                var results = JsonSerializer.Deserialize<Dictionary<string, int>>(resultsJson);
                if (results == null || !results.Any()) return;

                QuizResultsCard.IsVisible = true;
                MatchedCareersContainer.Children.Clear();

                foreach (var (field, _) in results.OrderByDescending(x => x.Value).Take(3))
                {
                    if (!_careerMatches.TryGetValue(field, out var careers)) continue;
                    var top = careers.First();

                    var card = new Border
                    {
                        BackgroundColor = Color.FromArgb("#F5F7F5"),
                        StrokeThickness = 0,
                        Padding = new Thickness(14, 12),
                        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 }
                    };
                    var stack = new VerticalStackLayout { Spacing = 3 };
                    stack.Children.Add(new Label { Text = top.Title, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A3C2E") });
                    stack.Children.Add(new Label { Text = $"OFO: {top.OFO} · {top.Pathway}", FontSize = 11, TextColor = Color.FromArgb("#888888") });
                    card.Content = stack;
                    MatchedCareersContainer.Children.Add(card);
                }
            }
            catch { }
        }

        // ── QUIZ MODAL ─────────────────────────────────
        private void OnStartCareerQuiz(object sender, EventArgs e) { QuizModalTitle.Text = "Career Choice Quiz"; QuizModalSubtitle.Text = "15 questions · Interest-based"; StartQuiz(); }
        private void OnStartJobFit(object sender, EventArgs e) { QuizModalTitle.Text = "Job Fit Assessment"; QuizModalSubtitle.Text = "15 questions · Skills-based"; StartQuiz(); }
        private void OnRetakeQuiz(object sender, EventArgs e) { QuizModalTitle.Text = "Career Choice Quiz"; QuizModalSubtitle.Text = "15 questions · Interest-based"; StartQuiz(); }

        private void StartQuiz()
        {
            _currentQuestion = 0;
            _fieldScores.Clear();
            QuizModal.IsVisible = true;
            ShowModalQuestion(0);
        }

        private void ShowModalQuestion(int index)
        {
            if (index >= _questions.Length) return;
            ModalQuestionText.Text = _questions[index].Question;
            ModalQuestionCounter.Text = $"Question {index + 1} of {_questions.Length}";
            double progress = (double)index / _questions.Length;
            double maxWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density - 40;
            ModalProgressBar.WidthRequest = Math.Max(8, maxWidth * progress);
        }

        private async void OnModalAnswerYes(object sender, EventArgs e)
        {
            var (_, fields) = _questions[_currentQuestion];
            foreach (var field in fields) { _fieldScores.TryAdd(field, 0); _fieldScores[field]++; }
            await NextModalQuestion();
        }

        private async void OnModalAnswerNo(object sender, EventArgs e) => await NextModalQuestion();

        private async Task NextModalQuestion()
        {
            await ModalQuestionText.TranslateTo(-20, 0, 80);
            _currentQuestion++;

            if (_currentQuestion >= _questions.Length)
            {
                Preferences.Set("quiz_results", JsonSerializer.Serialize(_fieldScores));
                QuizModal.IsVisible = false;
                LoadQuizResults();
                SwitchTab(1);
                return;
            }

            ModalQuestionText.TranslationX = 20;
            ShowModalQuestion(_currentQuestion);
            await ModalQuestionText.TranslateTo(0, 0, 120);
        }

        private void OnCloseQuiz(object sender, EventArgs e)
        {
            if (_currentQuestion > 0) Preferences.Set("quiz_results", JsonSerializer.Serialize(_fieldScores));
            QuizModal.IsVisible = false;
            LoadQuizResults();
        }

        // ── NAVIGATION ─────────────────────────────────
        private async void OnExploreStudy(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//StudyPage");

        private void OnChatTapped(object sender, EventArgs e)
        {
            ChatModal.IsVisible = true;
        }

        private void OnCloseChat(object sender, EventArgs e)
        {
            ChatModal.IsVisible = false;
        }
    }
}