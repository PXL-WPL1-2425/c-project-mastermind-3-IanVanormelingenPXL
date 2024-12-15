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
            private List<string> spelers = new List<string>(); // Nieuwe lijst voor spelersnamen
            private DispatcherTimer timer = new DispatcherTimer();
            private int remainingTime = 10;
            private int totalAttempts = 0;
            private const int MaxAttempts = 10;
            private List<string> attemptHistory = new List<string>();
            private int currentScore = 0;
            private int huidigeSpelerIndex = 0; // Houdt de index van de huidige speler bij

            public MainWindow()
            {
                InitializeComponent();
            }

            private void StartGame_Click(object sender, RoutedEventArgs e)
            {
                MenuPanel.Visibility = Visibility.Collapsed; // Verberg het menu
                GamePanel.Visibility = Visibility.Visible; // Toon het spel
                VraagSpelers(); // Vraag spelersnamen op
                ResetGame();
            }

            private void VraagSpelers()
            {
                spelers.Clear(); // Maak de lijst met spelers leeg voor een nieuw spel
                SpelersListBox.ItemsSource = null; // Reset ListBox

                bool nieuweSpelerToevoegen = true;

                while (nieuweSpelerToevoegen)
                {
                    string spelerNaam = Microsoft.VisualBasic.Interaction.InputBox(
                        "Voer de naam van de speler in:",
                        "Nieuwe Speler",
                        "Speler 1"
                    );

                    if (!string.IsNullOrWhiteSpace(spelerNaam))
                    {
                        spelers.Add(spelerNaam);
                    }
                    else
                    {
                        MessageBox.Show("Naam mag niet leeg zijn. Probeer opnieuw.");
                        continue; // Vraag opnieuw om een naam
                    }

                    // Vraag of nog een speler wil meedoen
                    MessageBoxResult result = MessageBox.Show(
                        "Wil je nog een speler toevoegen?",
                        "Nieuwe Speler Toevoegen",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.No)
                    {
                        nieuweSpelerToevoegen = false;
                    }
                }

                // Update de UI met spelersnamen
                SpelersListBox.ItemsSource = spelers;

                MessageBox.Show($"Spelers toegevoegd: {string.Join(", ", spelers)}", "Spelerslijst", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            private void ResetGame()
            {
                totalAttempts = 0;
                remainingTime = 10;
                currentScore = 0;
                attemptHistory.Clear();

                GenerateGeheime_code();
                FillComboBoxes();
                UpdateHistoryView();
                UpdateScoreLabel();
                CountdownTextBox.Text = remainingTime.ToString();

                SetupTimer();
            }

            private void GenerateGeheime_code()
            {
                Random random = new Random();
                Geheime_code.Clear();

                for (int i = 0; i < 4; i++)
                {
                    Geheime_code.Add(colors[random.Next(colors.Count)]);
                }
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

                totalAttempts++;

                if (guess.TrueForAll(color => Geheime_code.Contains(color)))
                {
                    EndGame(true);
                    return;
                }

                if (totalAttempts >= MaxAttempts)
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
                    if (guess[i] == Geheime_code[i])
                    {
                        correctPosition++;
                    }
                    else if (Geheime_code.Contains(guess[i]))
                    {
                        correctColor++;
                    }
                }

                score = correctPosition * 10 + correctColor * 5;
                return $"{correctPosition} goed geplaatst, {correctColor} juiste kleur";
            }

            private void UpdateHistoryView()
            {
                HistoryListBox.ItemsSource = null;
                HistoryListBox.ItemsSource = attemptHistory;
            }

            private void UpdateScoreLabel()
            {
                ScoreLabel.Content = currentScore.ToString();
            }

            private void EndGame(bool codeCracked)
            {
                timer.Stop();
                string message = codeCracked
                    ? $"Gefeliciteerd, {spelers[huidigeSpelerIndex]}! Je hebt de code gekraakt!"
                    : $"Helaas, {spelers[huidigeSpelerIndex]}, je hebt de code niet kunnen kraken. De geheime code was: {string.Join(", ", Geheime_code)}";

                // Volgende speler bepalen
                int volgendeSpelerIndex = (huidigeSpelerIndex + 1) % spelers.Count; // Zorg ervoor dat het teruggaat naar de eerste speler als iedereen heeft gespeeld

                // Toon de melding voor de huidige speler en de volgende speler
                message += $"\nDe volgende speler is: {spelers[volgendeSpelerIndex]}";

                var result = MessageBox.Show(message + "\nWil je opnieuw spelen?", "Spel beëindigd", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Zet de nieuwe speler aan het begin van de lijst
                    huidigeSpelerIndex = volgendeSpelerIndex;
                    ResetGame();  // Reset het spel voor de volgende speler
                }
                else
                {
                    Application.Current.Shutdown();  // Sluit de applicatie als er geen nieuwe ronde is
                }
            }

            private void ShowHighscores_Click(object sender, RoutedEventArgs e)
            {
                MessageBox.Show("Highscores: \n[Naam speler] - [Pogingen] - [Score]");
            }

            private void ExitGame_Click(object sender, RoutedEventArgs e)
            {
                Application.Current.Shutdown();
            }
        }
    }


