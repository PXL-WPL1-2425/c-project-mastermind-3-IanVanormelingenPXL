using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mastermind_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private List<string> colors = new List<string> { "Rood", "Geel", "Oranje", "Wit", "Groen", "Blauw" };
        private List<string> Geheime_code = new List<string>();
        private List<string> spelers = new List<string>();
        private DispatcherTimer timer = new DispatcherTimer();
        private int remainingTime = 10;
        private int totalAttempts = 0;
        private const int MaxAttempts = 10;
        private List<string> attemptHistory = new List<string>();
        private int currentScore = 0;
        private int aantalKleuren = 4;
        private List<string> beschikbareKleuren = new List<string> { "Rood", "Blauw", "Groen", "Geel", "Oranje", "Paars" };
        private int huidigeSpelerIndex = 0;  

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AantalKleurenComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AantalKleurenComboBox.SelectedItem != null)
            {
                string selectie = (AantalKleurenComboBox.SelectedItem as ComboBoxItem).Content.ToString();

                aantalKleuren = selectie switch
                {
                    "4 Kleuren" => 4,
                    "5 Kleuren" => 5,
                    "6 Kleuren" => 6,
                    _ => aantalKleuren
                };

                UpdateKleurkeuze();
                GenereerGeheimeCode();
            }
        }

        private void UpdateKleurkeuze()
        {
           
            beschikbareKleuren = aantalKleuren switch
            {
                4 => new List<string> { "Rood", "Blauw", "Groen", "Geel" },
                5 => new List<string> { "Rood", "Blauw", "Groen", "Geel", "Oranje" },
                6 => new List<string> { "Rood", "Blauw", "Groen", "Geel", "Oranje", "Paars" },
                _ => beschikbareKleuren
            };

            
            Kleurcode1.ItemsSource = beschikbareKleuren;
            Kleurcode2.ItemsSource = beschikbareKleuren;
            Kleurcode3.ItemsSource = beschikbareKleuren;
            Kleurcode4.ItemsSource = beschikbareKleuren;
        }

        private void GenereerGeheimeCode()
        {
            Random random = new Random();
            Geheime_code.Clear();

            for (int i = 0; i < aantalKleuren; i++)
            {
                string gekozenKleur = beschikbareKleuren[random.Next(beschikbareKleuren.Count)];
                Geheime_code.Add(gekozenKleur);
            }

            ToonGeheimeCode();
        }

        private void GeefFeedbackOpGok(List<string> gok)
        {
            if (gok.Count != aantalKleuren)
            {
                MessageBox.Show("Je gok moet hetzelfde aantal kleuren bevatten als de geheime code.");
                return;
            }

            for (int i = 0; i < gok.Count; i++)
            {
                string gokKleur = gok[i];
                string juisteKleur = Geheime_code[i];
                Brush borderColor = Brushes.Transparent;
                string feedbackText = "Foute kleur";

                if (gokKleur == juisteKleur)
                {
                    borderColor = Brushes.Red;
                    feedbackText = "Juiste kleur, juiste positie";
                }
                else if (Geheime_code.Contains(gokKleur))
                {
                    borderColor = Brushes.White;
                    feedbackText = "Juiste kleur, foute positie";
                }

                SetRandEnTooltip(i, feedbackText, borderColor);
            }
        }

        private void SetRandEnTooltip(int index, string feedbackText, Brush borderColor)
        {
            
            ComboBox targetComboBox = index switch
            {
                0 => Kleurcode1,
                1 => Kleurcode2,
                2 => Kleurcode3,
                3 => Kleurcode4,
                _ => null
            };

            if (targetComboBox != null)
            {
                targetComboBox.BorderBrush = borderColor;
                ToolTip toolTip = index switch
                {
                    0 => ToolTipKleurcode1,
                    1 => ToolTipKleurcode2,
                    2 => ToolTipKleurcode3,
                    3 => ToolTipKleurcode4,
                    _ => null
                };

                if (toolTip != null)
                    toolTip.Content = feedbackText;
            }
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MenuPanel.Visibility = Visibility.Collapsed;
            GamePanel.Visibility = Visibility.Visible;
            VraagSpelers();
            ResetGame();
        }

        private void VraagSpelers()
        {
            spelers.Clear();
            SpelersListBox.ItemsSource = null;

            bool nieuweSpelerToevoegen = true;

            while (nieuweSpelerToevoegen)
            {
                string spelerNaam = Microsoft.VisualBasic.Interaction.InputBox(
                    "Voer de naam van de speler in:", "Nieuwe Speler", "Speler 1");

                if (!string.IsNullOrWhiteSpace(spelerNaam))
                {
                    spelers.Add(spelerNaam);
                }
                else
                {
                    MessageBox.Show("Naam mag niet leeg zijn. Probeer opnieuw.");
                    continue;
                }

                MessageBoxResult result = MessageBox.Show(
                    "Wil je nog een speler toevoegen?", "Nieuwe Speler Toevoegen",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    nieuweSpelerToevoegen = false;
                }
            }

            SpelersListBox.ItemsSource = spelers;
            MessageBox.Show($"Spelers toegevoegd: {string.Join(", ", spelers)}", "Spelerslijst", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void UpdateSpelerInfo()
        {
            string huidigeSpeler = spelers[huidigeSpelerIndex];
            SpelerLabel.Content = $"Actieve speler: {huidigeSpeler}";
            ScoreLabel.Content = $"Pogingen: {totalAttempts} | Score: {currentScore}";
        }

        private void ResetGame()
        {
            totalAttempts = 0;
            remainingTime = 10;
            currentScore = 0;
            attemptHistory.Clear();
            GenereerGeheimeCode();
            FillComboBoxes();
            UpdateHistoryView();
            UpdateScoreLabel();
            UpdateSpelerInfo();
            CountdownTextBox.Text = remainingTime.ToString();
            SetupTimer();
        }

        private void FillComboBoxes()
        {
            var comboBoxes = new List<ComboBox> { Kleurcode1, Kleurcode2, Kleurcode3, Kleurcode4 };
            foreach (var comboBox in comboBoxes)
            {
                comboBox.ItemsSource = colors;
                comboBox.SelectedIndex = -1;
            }
        }

        private void SetupTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime--;
            CountdownTextBox.Text = remainingTime.ToString();

            if (remainingTime <= 0)
            {
                timer.Stop();
                MessageBox.Show("De tijd is om! Probeer opnieuw.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Warning);
                EndGame(false);
            }
        }

        private void CheckCode_Click(object sender, RoutedEventArgs e)
        {
            if (totalAttempts >= MaxAttempts)
            {
                EndGame(false);
                return;
            }

            var guess = new List<string>
        {
            Kleurcode1.SelectedItem?.ToString(),
            Kleurcode2.SelectedItem?.ToString(),
            Kleurcode3.SelectedItem?.ToString(),
            Kleurcode4.SelectedItem?.ToString()
        };

            if (guess.Contains(null))
            {
                MessageBox.Show("Selecteer alle kleuren voordat je verdergaat.", "Fout", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string feedback = EvaluateGuess(guess, out int score);
            attemptHistory.Add(string.Join(", ", guess) + " - Feedback: " + feedback);
            UpdateHistoryView();

            currentScore += score;
            UpdateScoreLabel();
            UpdateSpelerInfo();

            totalAttempts++;

            if (guess.TrueForAll(color => Geheime_code.Contains(color)))
            {
                EndGame(true);
            }
            else if (totalAttempts >= MaxAttempts)
            {
                EndGame(false);
            }
        }

        private string EvaluateGuess(List<string> guess, out int score)
        {
            int correctPosition = 0;
            int correctColor = 0;
            score = 0;

            for (int i = 0; i < 4; i++)
            {
                if (guess[i] == Geheime_code[i]) correctPosition++;
                else if (Geheime_code.Contains(guess[i])) correctColor++;
            }

            score = correctPosition * 10 + correctColor * 5;
            return $"{correctPosition} goed geplaatst, {correctColor} juiste kleur";
        }

        private void EndGame(bool codeCracked)
        {
            timer.Stop();
            string message = codeCracked ?
                "Gefeliciteerd! Je hebt de code gekraakt!" :
                $"Helaas, je hebt de code niet kunnen kraken. De geheime code was: {string.Join(", ", Geheime_code)}";

            var result = MessageBox.Show(message + "\nWil je opnieuw spelen?", "Spel beëindigd", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes) ResetGame();
            else Application.Current.Shutdown();
        }
    }
}

