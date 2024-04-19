using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

class Program
{
    static readonly object _lock = new object();
    static Random _random = new Random();
    static List<Player> _players = new List<Player>();
    static string _outputFilePath = "report.txt";
    static int _numOfPotentialPlayers;

    static void Main()
    {
        Console.WriteLine("Введіть кількість потенційних гравців за день (від 20 до 100):");
        _numOfPotentialPlayers = int.Parse(Console.ReadLine());

        // Запуск гри
        StartGame();

        // Запис результатів у файл
        WriteResultsToFile();

        Console.WriteLine("Гра завершена. Результати збережено в файлі report.txt");
    }

    static void StartGame()
    {
        // Запускаємо потоки для гравців
        for (int i = 0; i < _numOfPotentialPlayers; i++)
        {
            var player = new Player(i + 1, _random.Next(100, 1000)); // Номер гравця і початкова сума грошей
            _players.Add(player);

            Thread thread = new Thread(() => Play(player));
            thread.Start();
        }

        // Очікуємо, доки всі потенційні гравці не зіграють хоча б один раунд
        while (_players.Exists(p => !p.HasPlayed))
        {
            Thread.Sleep(100);
        }
    }

    static void Play(Player player)
    {
        int bet = _random.Next(10, 101); // Ставка
        int number = _random.Next(1, 37); // Обране число
        bool win = _random.Next(0, 2) == 0; // Чи виграє гравець

        lock (_lock)
        {
            // Запис результатів гравця
            using (StreamWriter writer = new StreamWriter(_outputFilePath, true))
            {
                writer.WriteLine($"Гравець {player.Id}: початкова сума - {player.StartingAmount}, ставка - {bet}, обране число - {number}, результат - {(win ? "виграш" : "програш")}");
            }

            // Зміна суми грошей гравця
            if (win)
            {
                player.CurrentAmount += bet;
            }
            else
            {
                player.CurrentAmount -= bet;
            }

            // Позначаємо, що гравець зіграв хоча б один раунд
            player.HasPlayed = true;

            // Перевірка, чи може гравець зіграти ще один раунд або відкрити місце для нового гравця
            if (player.CurrentAmount <= 0)
            {
                _players.Remove(player);
            }
        }
    }

    static void WriteResultsToFile()
    {
        using (StreamWriter writer = new StreamWriter(_outputFilePath))
        {
            foreach (var player in _players)
            {
                writer.WriteLine($"Гравець {player.Id}: початкова сума - {player.StartingAmount}, кінцева сума - {player.CurrentAmount}");
            }
        }
    }
}

class Player
{
    public int Id { get; set; }
    public int StartingAmount { get; }
    public int CurrentAmount { get; set; }
    public bool HasPlayed { get; set; }

    public Player(int id, int startingAmount)
    {
        Id = id;
        StartingAmount = startingAmount;
        CurrentAmount = startingAmount;
        HasPlayed = false;
    }
}

