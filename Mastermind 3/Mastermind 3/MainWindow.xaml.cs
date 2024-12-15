﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();
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
                    continue;
                }
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
            SpelersListBox.ItemsSource = spelers;

            MessageBox.Show($"Spelers toegevoegd: {string.Join(", ", spelers)}", "Spelerslijst", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void UpdateSpelerInfo()
        {
            string huidigeSpeler = spelers[huidigeSpelerIndex];
            int pogingen = totalAttempts;
            int score = currentScore;

            // Update de labels
            SpelerLabel.Content = $"Actieve speler: {huidigeSpeler}";
            ScoreLabel.Content = $"Pogingen: {pogingen} | Score: {score}";
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
            UpdateSpelerInfo(); // Voeg dit toe om spelerinformatie bij te werken

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
        private void HintButton_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBoxResult keuze = MessageBox.Show(
                "Wil je een hint voor een juiste kleur (kost 15 strafpunten) of een juiste kleur op de juiste plaats (kost 25 strafpunten)?\n\n" +
                "Ja = Juiste kleur (15 punten)\nNee = Juiste kleur op juiste plaats (25 punten)\nAnnuleren = Geen hint kopen.",
                "Hint Kopen",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
            );

            if (keuze == MessageBoxResult.Cancel) return; 

            if (keuze == MessageBoxResult.Yes)
            {
                
                if (currentScore < 15)
                {
                    MessageBox.Show("Je hebt niet genoeg punten om een hint te kopen!", "Niet genoeg punten", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GeefHintJuisteKleur();
                currentScore -= 15; 
            }
            else if (keuze == MessageBoxResult.No)
            {
                
                if (currentScore < 25)
                {
                    MessageBox.Show("Je hebt niet genoeg punten om een hint te kopen!", "Niet genoeg punten", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                GeefHintJuistePlaats();
                currentScore -= 25;
            }

            UpdateUI(); 
        }

        private void GeefHintJuisteKleur()
        {
            
            Random rand = new Random();
            string hintKleur = geheimeCode[rand.Next(geheimeCode.Count)];
            MessageBox.Show($"Hint: Eén van de kleuren in de geheime code is '{hintKleur}'.", "Hint - Juiste Kleur", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GeefHintJuistePlaats()
        {
            
            for (int i = 0; i < geheimeCode.Count; i++)
            {
                MessageBox.Show($"Hint: Op positie {i + 1} zit de kleur '{geheimeCode[i]}'.", "Hint - Juiste Kleur op Juiste Plaats", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
            }
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
                ? "Gefeliciteerd! Je hebt de code gekraakt!"
                : $"Helaas, je hebt de code niet kunnen kraken. De geheime code was: {string.Join(", ", Geheime_code)}";

            var result = MessageBox.Show(message + "\nWil je opnieuw spelen?", "Spel beëindigd", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResetGame();
            }
            else
            {
                Application.Current.Shutdown();
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

