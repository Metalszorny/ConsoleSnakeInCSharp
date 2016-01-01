using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSnake
{
    /// <summary>
    /// Interaction logic for Program.
    /// </summary>
    class Program
    {
        #region Fields

        #region Map

        // The map txt access.
        private static TxtAccess txtAccess = new TxtAccess(@"..\..\Resources\map.txt");

        // The map's width.
        private const int mapWidth = 40;

        // The map's height.
        private const int mapHeight = 20;

        // The map.
        public static char[,] map = new char[mapHeight, mapWidth];

        // Draw the map in color.
        private static bool coloredMap = true;

        #endregion Map

        #region Highscore

        // The limit of the highscores.
        private const int scoreLimit = 20;

        // The xml highscores access.
        private static XmlAccess xmlAccess = new XmlAccess(@"..\..\Resources\highscores.xml");

        // The list of highscores.
        private static List<Highscore> highscoresList = new List<Highscore>();

        #endregion Highscore

        #region Character

        // The snake.
        private static List<SnakePart> snake = new List<SnakePart>();

        #endregion Character

        #region CharacterFunction

        // The player's score.
        private static int playerScore = 0;

        // The snake's movement.
        private static MoveDirections snakeDirection = MoveDirections.Left;

        // The enum of move directions.
        private enum MoveDirections
        {
            Left,
            Right,
            Up,
            Down
        }

        #endregion CharacterFunction

        #region MapFunction

        // Indicates that the game is in session or not.
        private static bool gameFinished = false;

        // The game needs to preset some values.
        private static bool gamePreset = true;

        // The status of the game.
        private static GameStages gameStatus = GameStages.Menu;

        // Refresh the display.
        public static bool refreshDisplay = false;

        // The random.
        private static Random rnd = new Random();

        // The number of the options.
        private const int optionsNumber = 2;

        // The time limit of the obstacle generation.
        private const int obstableTime = 100;

        // The time limit of the bonus generation.
        private const int generateBonusTime = 100;

        // The time limit of the bonus removeal.
        private const int removeBonusTime = 50;

        // The time delay of the bonus generation.
        private static int generateBonusCounter = 0;

        // The position of the bonus.
        private static Position bonusPosition = new Position(-1, -1);

        // The time delay of the bonus removeal.
        private static int removeBonusCounter = 0;

        // The multiplier of the score based on the difficulty.
        private static int scoreMultiplier = 1;

        // The bonus is generated or removed.
        private static bool generateBonus = true;

        // The time delay of the obstacle generation.
        private static int obstacleCounter = 0;

        // The curzor for the options.
        private static int optionsPointer = 1;

        // Indicates that the user's score should be checked or not.
        private static bool checkHighscores = false;

        // Object generate limit to prevent infinate loops.
        private const int tryLimit = 20;

        // The area around the snake where no obstacles shoud generate.
        private const int snakeRadius = 5;

        // The refresh rate of the game.
        private static int sleepTime = 50;

        // The difficulty of the game.
        private static GameDifficulties gameDifficulty = GameDifficulties.Normal;

        // The difficulties of the game.
        private enum GameDifficulties
        {
            Easy,
            Normal,
            Hard
        }

        // The enum of game stages.
        private enum GameStages
        {
            Menu,
            Highsores,
            Game,
            Options,
            Exit
        }

        #endregion MapFunction

        #endregion Fields

        #region Methods

        /// <summary>
        /// The main method of the Program.
        /// </summary>
        /// <param name="args">The input arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                while (!gameFinished)
                {
                    #region Menu

                    // The menu stage.
                    if (gameStatus == GameStages.Menu)
                    {
                        #region RefreshDisplay

                        DisplayMenu();

                        #endregion RefreshDisplay

                        // Repeat the key read until a valid key is pushed.
                        while (gameStatus == GameStages.Menu)
                        {
                            #region HandleInput

                            // A key was pushed.
                            if (Console.KeyAvailable)
                            {
                                // Handle the key input.
                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.D1:
                                    case ConsoleKey.NumPad1:
                                        gameStatus = GameStages.Game;
                                        break;
                                    case ConsoleKey.D2:
                                    case ConsoleKey.NumPad2:
                                        gameStatus = GameStages.Highsores;
                                        break;
                                    case ConsoleKey.D3:
                                    case ConsoleKey.NumPad3:
                                        gameStatus = GameStages.Options;
                                        break;
                                    case ConsoleKey.D4:
                                    case ConsoleKey.NumPad4:
                                        gameStatus = GameStages.Exit;
                                        break;
                                }

                                // Delete the read key input.
                                DeleteKeyInput();
                            }

                            #endregion HandleInput
                        }
                    }

                    #endregion Menu

                    #region Game

                    // The game stage.
                    else if (gameStatus == GameStages.Game)
                    {
                        #region PreSet

                        // Refresh the snake.
                        snake.Clear();
                        snake.Add(new SnakePart(new Position(11, 30), SnakePart.BodyParts.Head));
                        snake.Add(new SnakePart(new Position(11, 31), SnakePart.BodyParts.Body));
                        snake.Add(new SnakePart(new Position(11, 32), SnakePart.BodyParts.Body));
                        snake.Add(new SnakePart(new Position(11, 33), SnakePart.BodyParts.Body));

                        // Pre-set values.
                        if (gamePreset)
                        {
                            try
                            {
                                // Read and store the map.
                                if (txtAccess.FileExists())
                                {
                                    // Load the map.
                                    map = txtAccess.LoadMap(mapWidth, mapHeight);

                                    // Put the snake on the map.
                                    map[snake[0].Position.X, snake[0].Position.Y] = snake[0].Draw();
                                    map[snake[1].Position.X, snake[1].Position.Y] = snake[1].Draw();
                                    map[snake[2].Position.X, snake[2].Position.Y] = snake[2].Draw();
                                    map[snake[3].Position.X, snake[3].Position.Y] = snake[3].Draw();
                                    GenerateCollectable();
                                }
                                // The map txt file can't be found.
                                else
                                {
                                    Console.WriteLine("Map not found.");
                                    Console.ReadLine();
                                    gameFinished = true;
                                }
                            }
                            catch
                            { }

                            // Set the game difficulty and the score multiplier.
                            switch (gameDifficulty)
                            {
                                case GameDifficulties.Easy:
                                    sleepTime = 65;
                                    scoreMultiplier = 1;
                                    break;
                                case GameDifficulties.Normal:
                                    sleepTime = 50;
                                    scoreMultiplier = 2;
                                    break;
                                case GameDifficulties.Hard:
                                    sleepTime = 40;
                                    scoreMultiplier = 3;
                                    break;
                            }

                            playerScore = 0;
                            obstacleCounter = 0;
                            generateBonus = true;
                            generateBonusCounter = 0;
                            removeBonusCounter = 0;
                            gamePreset = false;
                            checkHighscores = false;
                            snakeDirection = MoveDirections.Left;
                            bonusPosition = new Position(-1, -1);
                        }

                        #endregion PreSet

                        #region RefreshDisplay

                        DisplayMap();

                        #endregion RefreshDisplay

                        // Repeat while the game is active.
                        while (gameStatus == GameStages.Game)
                        {
                            #region HandleInput

                            // A key was pushed.
                            if (Console.KeyAvailable)
                            {
                                // Handle the key input.
                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.W:
                                    case ConsoleKey.UpArrow:
                                        if (snakeDirection != MoveDirections.Down)
                                        {
                                            snakeDirection = MoveDirections.Up;
                                        }
                                        break;
                                    case ConsoleKey.A:
                                    case ConsoleKey.LeftArrow:
                                        if (snakeDirection != MoveDirections.Right)
                                        {
                                            snakeDirection = MoveDirections.Left;
                                        }
                                        break;
                                    case ConsoleKey.D:
                                    case ConsoleKey.RightArrow:
                                        if (snakeDirection != MoveDirections.Left)
                                        {
                                            snakeDirection = MoveDirections.Right;
                                        }
                                        break;
                                    case ConsoleKey.S:
                                    case ConsoleKey.DownArrow:
                                        if (snakeDirection != MoveDirections.Up)
                                        {
                                            snakeDirection = MoveDirections.Down;
                                        }
                                        break;
                                    case ConsoleKey.Escape:
                                        gameStatus = GameStages.Menu;
                                        checkHighscores = false;
                                        gamePreset = true;
                                        break;
                                }

                                // Delete the read key input.
                                DeleteKeyInput();
                            }

                            #endregion HandleInput

                            #region HandleChanges

                            // Only handle the changes in the game stage.
                            if (gameStatus == GameStages.Game)
                            {
                                #region MoveSnake

                                // The snake moves.
                                MoveSnake();

                                #endregion MoveSnake

                                #region GenerateOrRemoveBonus

                                // Generate the bonus.
                                if (generateBonus)
                                {
                                    // It's time to generate a bonus.
                                    if (generateBonusCounter == generateBonusTime)
                                    {
                                        generateBonusCounter = 0;
                                        GenerateBonus();
                                        generateBonus = false;
                                    }
                                    // Continue increasing the timer's value.
                                    else
                                    {
                                        generateBonusCounter++;
                                    }
                                }
                                // Remove the bonus.
                                else
                                {
                                    // It's time to remove a bonus.
                                    if (removeBonusCounter == removeBonusTime)
                                    {
                                        removeBonusCounter = 0;
                                        RemoveBonus();
                                        generateBonus = true;
                                    }
                                    // Continue increasing the timer's value.
                                    else
                                    {
                                        removeBonusCounter++;
                                    }
                                }

                                #endregion GenerateOrRemoveBonus

                                #region GenerateObstacle

                                // It's time to generate an obstacle.
                                if (obstacleCounter == obstableTime)
                                {
                                    obstacleCounter = 0;
                                    GenerateObstacle();
                                }
                                // Continue increasing the timer's value.
                                else
                                {
                                    obstacleCounter++;
                                }

                                #endregion GenerateObstacle
                            }

                            #endregion HandleChanges

                            Thread.Sleep(sleepTime);

                            #region GameRefresh

                            // The display needs to be refreshed.
                            if (refreshDisplay)
                            {
                                #region Text

                                #region Score

                                // Display the player's score.
                                if (playerScore / 10000 > 0)
                                {
                                    Console.SetCursorPosition(8, 20);
                                }
                                else if (playerScore / 1000 > 0)
                                {
                                    Console.SetCursorPosition(8, 20);
                                    Console.Write("0");
                                    Console.SetCursorPosition(9, 20);
                                }
                                else if (playerScore / 100 > 0)
                                {
                                    Console.SetCursorPosition(8, 20);
                                    Console.Write("00");
                                    Console.SetCursorPosition(10, 20);
                                }
                                else if (playerScore / 10 > 0)
                                {
                                    Console.SetCursorPosition(8, 20);
                                    Console.Write("000");
                                    Console.SetCursorPosition(11, 20);
                                }
                                else
                                {
                                    Console.SetCursorPosition(8, 20);
                                    Console.Write("0000");
                                    Console.SetCursorPosition(12, 20);
                                }

                                Console.Write(playerScore);
                                Console.SetCursorPosition(0, 21);

                                #endregion Score

                                #endregion Text

                                refreshDisplay = false;
                            }

                            #endregion GameRefresh
                        }

                        // Check the player's score if it's in the highscores.
                        if (checkHighscores)
                        {
                            #region RefreshDisplay

                            StoreHighscore();

                            #endregion RefreshDisplay

                            gamePreset = true;
                        }
                    }

                    #endregion Game

                    #region Highscores

                    // The highscore stage.
                    else if (gameStatus == GameStages.Highsores)
                    {
                        #region RefreshDisplay

                        DisplayHighscores();

                        #endregion RefreshDisplay

                        // Repeat the key read until the escape is pushed.
                        while (gameStatus == GameStages.Highsores)
                        {
                            #region HandleInput

                            // A key was pushed.
                            if (Console.KeyAvailable)
                            {
                                // Handle the key input.
                                if (Console.ReadKey().Key == ConsoleKey.Escape)
                                {
                                    gameStatus = GameStages.Menu;
                                }

                                // Delete the read key input.
                                DeleteKeyInput();
                            }

                            #endregion HandleInput
                        }
                    }

                    #endregion Highscores

                    #region Options

                    // The options stage.
                    else if (gameStatus == GameStages.Options)
                    {
                        #region RefreshDisplay

                        DisplayOptions();
                        optionsPointer = 1;

                        #endregion RefreshDisplay

                        // Repeat the key read until the escape is pushed.
                        while (gameStatus == GameStages.Options)
                        {
                            #region HandleInput

                            // A key was pushed.
                            if (Console.KeyAvailable)
                            {
                                // Handle the key input.
                                switch (Console.ReadKey().Key)
                                {
                                    case ConsoleKey.Escape:
                                        gameStatus = GameStages.Menu;
                                        break;
                                    case ConsoleKey.LeftArrow:
                                        switch (optionsPointer)
                                        {
                                            case 1:
                                                if (coloredMap)
                                                {
                                                    coloredMap = false;
                                                    ChangeText(new Position(2, 7), "Off");
                                                }
                                                else
                                                {
                                                    coloredMap = true;
                                                    ChangeText(new Position(2, 7), "On ");
                                                }
                                                break;
                                            case 2:
                                                if (gameDifficulty == GameDifficulties.Hard)
                                                {
                                                    gameDifficulty = GameDifficulties.Normal;
                                                    ChangeText(new Position(4, 12), "Normal");
                                                }
                                                else if (gameDifficulty == GameDifficulties.Normal)
                                                {
                                                    gameDifficulty = GameDifficulties.Easy;
                                                    ChangeText(new Position(4, 12), " Easy ");
                                                }
                                                break;
                                        }
                                        break;
                                    case ConsoleKey.RightArrow:
                                        switch (optionsPointer)
                                        {
                                            case 1:
                                                if (coloredMap)
                                                {
                                                    coloredMap = false;
                                                    ChangeText(new Position(2, 7), "Off");
                                                }
                                                else
                                                {
                                                    coloredMap = true;
                                                    ChangeText(new Position(2, 7), "On ");
                                                }
                                                break;
                                            case 2:
                                                if (gameDifficulty == GameDifficulties.Easy)
                                                {
                                                    gameDifficulty = GameDifficulties.Normal;
                                                    ChangeText(new Position(4, 12), "Normal");
                                                }
                                                else if (gameDifficulty == GameDifficulties.Normal)
                                                {
                                                    gameDifficulty = GameDifficulties.Hard;
                                                    ChangeText(new Position(4, 12), " Hard ");
                                                }
                                                break;
                                        }
                                        break;
                                    case ConsoleKey.UpArrow:
                                        if (optionsPointer > 1)
                                        {
                                            optionsPointer--;
                                            ChangeOption(optionsPointer, optionsPointer + 1);
                                        }
                                        else
                                        {
                                            optionsPointer = optionsNumber;
                                            ChangeOption(optionsNumber, 1);
                                        }
                                        break;
                                    case ConsoleKey.DownArrow:
                                        if (optionsPointer < optionsNumber)
                                        {
                                            optionsPointer++;
                                            ChangeOption(optionsPointer, optionsPointer - 1);
                                        }
                                        else
                                        {
                                            optionsPointer = 1;
                                            ChangeOption(1, optionsNumber);
                                        }
                                        break;
                                }

                                // Delete the read key input.
                                DeleteKeyInput();
                            }

                            #endregion HandleInput
                        }
                    }

                    #endregion Options

                    #region Exit

                    // The exit stage.
                    else if (gameStatus == GameStages.Exit)
                    {
                        // End the program.
                        gameFinished = true;
                    }

                    #endregion Exit
                }
            }
            catch
            { }
        }

        #region Functions

        /// <summary>
        /// Genetares a bonus.
        /// </summary>
        private static void GenerateBonus()
        {
            try
            {
                Position randomPosition = new Position(-1, -1);
                int tryCount = 0;

                // Generate position.
                do
                {
                    randomPosition.X = rnd.Next(0, mapHeight);
                    randomPosition.Y = rnd.Next(0, mapWidth);
                    tryCount++;
                } while (map[randomPosition.X, randomPosition.Y] != ' ' && tryCount <= tryLimit);

                if (tryCount <= tryLimit)
                {
                    // Set the bonus to the generated position.
                    map[randomPosition.X, randomPosition.Y] = '*';
                    RefreshChar(randomPosition, '*');
                    bonusPosition.X = randomPosition.X;
                    bonusPosition.Y = randomPosition.Y;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Removes the bonus.
        /// </summary>
        private static void RemoveBonus()
        {
            try
            {
                map[bonusPosition.X, bonusPosition.Y] = ' ';
                RefreshChar(bonusPosition, ' ');
            }
            catch
            { }
        }

        /// <summary>
        /// Genetares a obstacle.
        /// </summary>
        private static void GenerateObstacle()
        {
            try
            {
                Position randomPosition = new Position(-1, -1);
                int tryCount = 0;

                // Generate position, that shouldn't be near the snake.
                do
                {
                    randomPosition.X = rnd.Next(0, mapHeight);
                    randomPosition.Y = rnd.Next(0, mapWidth);
                    tryCount++;
                } while (map[randomPosition.X, randomPosition.Y] != ' ' && tryCount <= tryLimit ||
                    ((randomPosition.X >= snake[0].Position.X - snakeRadius && randomPosition.X <= snake[0].Position.X + snakeRadius) &&
                    (randomPosition.Y >= snake[0].Position.Y - snakeRadius && randomPosition.Y <= snake[0].Position.Y + snakeRadius)));

                if (tryCount <= tryLimit)
                {
                    // Set the obstacle to the generated position.
                    map[randomPosition.X, randomPosition.Y] = '#';
                    RefreshChar(randomPosition, '#');
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Genetares a collectable.
        /// </summary>
        private static void GenerateCollectable()
        {
            try
            {
                Position randomPosition = new Position(-1, -1);
                int tryCount = 0;

                // Generate position.
                do
                {
                    randomPosition.X = rnd.Next(0, mapHeight);
                    randomPosition.Y = rnd.Next(0, mapWidth);
                    tryCount++;
                } while (map[randomPosition.X, randomPosition.Y] != ' ' && tryCount <= tryLimit);

                if (tryCount <= tryLimit)
                {
                    // Set the collectable to the generated position.
                    map[randomPosition.X, randomPosition.Y] = '.';
                    RefreshChar(randomPosition, '.');
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Changes the selected option.
        /// </summary>
        /// <param name="currentpointer">The current selection.</param>
        /// <param name="previouspointer">The previous selection.</param>
        private static void ChangeOption(int currentpointer, int previouspointer)
        {
            try
            {
                // Don't reposition with the same position.
                if (currentpointer != previouspointer)
                {
                    // Make the current selection have the curzor.
                    switch (currentpointer)
                    {
                        case 1:
                            Console.SetCursorPosition(7, 2);
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                            if (coloredMap)
                            {
                                Console.Write("On ");
                            }
                            else
                            {
                                Console.Write("Off");
                            }
                            break;
                        case 2:
                            Console.SetCursorPosition(12, 4);
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.ForegroundColor = ConsoleColor.Black;
                            switch (gameDifficulty)
                            {
                                case GameDifficulties.Easy:
                                    Console.Write(" Easy ");
                                    break;
                                case GameDifficulties.Normal:
                                    Console.Write("Normal");
                                    break;
                                case GameDifficulties.Hard:
                                    Console.Write(" Hard ");
                                    break;
                            }
                            break;
                    }

                    // Make the previous selection the default color.
                    switch (previouspointer)
                    {
                        case 1:
                            Console.SetCursorPosition(7, 2);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            if (coloredMap)
                            {
                                Console.Write("On ");
                            }
                            else
                            {
                                Console.Write("Off");
                            }
                            break;
                        case 2:
                            Console.SetCursorPosition(12, 4);
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            switch (gameDifficulty)
                            {
                                case GameDifficulties.Easy:
                                    Console.Write(" Easy ");
                                    break;
                                case GameDifficulties.Normal:
                                    Console.Write("Normal");
                                    break;
                                case GameDifficulties.Hard:
                                    Console.Write(" Hard ");
                                    break;
                            }
                            break;
                    }

                    Console.SetCursorPosition(1, 9);
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Changes the text at the given position.
        /// </summary>
        /// <param name="position">The position to write.</param>
        /// <param name="text">The test to write.</param>
        private static void ChangeText(Position position, string text)
        {
            try
            {
                Console.SetCursorPosition(position.Y, position.X);
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(text);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.SetCursorPosition(1, 9);
            }
            catch
            { }
        }

        /// <summary>
        /// Validates the next step of the snake.
        /// </summary>
        /// <param name="newposition">The new position.</param>
        private static void ValidateNextStep(Position newposition)
        {
            try
            {
                switch (map[newposition.X, newposition.Y])
                {
                    case ' ':
                        // Remove the last part of the snake from the map and the display.
                        RefreshChar(snake[snake.Count - 1].Position, ' ');
                        map[snake[snake.Count - 1].Position.X, snake[snake.Count - 1].Position.Y] = ' ';

                        // Make each part's position to the position of the part before it and the head gets the new position.
                        for (int i = snake.Count - 1; i > 0; i--)
                        {
                            snake[i].Reposition(snake[i - 1].Position);
                        }
                        snake[0].Reposition(newposition);

                        // Draw the snake's head on the map and the display.
                        RefreshChar(snake[0].Position, snake[0].Draw());
                        map[snake[0].Position.X, snake[0].Position.Y] = snake[0].Draw();
                        RefreshChar(snake[1].Position, snake[1].Draw());
                        map[snake[1].Position.X, snake[1].Position.Y] = snake[1].Draw();
                        break;
                    case 'O':
                        gameStatus = GameStages.Menu;
                        checkHighscores = true;
                        break;
                    case '.':
                        // Add a new part to the snake.
                        snake.Add(new SnakePart(snake[snake.Count - 1].Position, SnakePart.BodyParts.Body));

                        // Make each part's position to the position of the part before it and the head gets the new position.
                        for (int i = snake.Count - 2; i > 0; i--)
                        {
                            snake[i].Reposition(snake[i - 1].Position);
                        }
                        snake[0].Reposition(newposition);

                        // Draw the snake's head on the map and the display.
                        RefreshChar(snake[0].Position, snake[0].Draw());
                        map[snake[0].Position.X, snake[0].Position.Y] = snake[0].Draw();
                        RefreshChar(snake[1].Position, snake[1].Draw());
                        map[snake[1].Position.X, snake[1].Position.Y] = snake[1].Draw();

                        playerScore += (5 * scoreMultiplier);
                        refreshDisplay = true;
                        GenerateCollectable();
                        break;
                    case '*':
                        // Add a new part to the snake.
                        snake.Add(new SnakePart(snake[snake.Count - 1].Position, SnakePart.BodyParts.Body));

                        // Make each part's position to the position of the part before it and the head gets the new position.
                        for (int i = snake.Count - 2; i > 0; i--)
                        {
                            snake[i].Reposition(snake[i - 1].Position);
                        }
                        snake[0].Reposition(newposition);

                        // Draw the snake's head on the map and the display.
                        RefreshChar(snake[0].Position, snake[0].Draw());
                        map[snake[0].Position.X, snake[0].Position.Y] = snake[0].Draw();
                        RefreshChar(snake[1].Position, snake[1].Draw());
                        map[snake[1].Position.X, snake[1].Position.Y] = snake[1].Draw();

                        playerScore += (50 * scoreMultiplier);
                        refreshDisplay = true;
                        generateBonus = true;
                        generateBonusCounter = 0;
                        removeBonusCounter = 0;
                        break;
                    case '#':
                        gameStatus = GameStages.Menu;
                        checkHighscores = true;
                        break;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Moves the snake.
        /// </summary>
        private static void MoveSnake()
        {
            try
            {
                switch (snakeDirection)
                {
                    case MoveDirections.Left:
                        // The snake is at tunnel.
                        if (snake[0].Position.Y - 1 < 0)
                        {
                            ValidateNextStep(new Position(snake[0].Position.X, mapWidth - 1));
                        }
                        // Move left.
                        else
                        {
                            ValidateNextStep(new Position(snake[0].Position.X, snake[0].Position.Y - 1));
                        }
                        break;
                    case MoveDirections.Right:
                        // The snake is at tunnel.
                        if (snake[0].Position.Y + 1 > mapWidth - 1)
                        {
                            ValidateNextStep(new Position(snake[0].Position.X, 0));
                        }
                        // Move right.
                        else
                        {
                            ValidateNextStep(new Position(snake[0].Position.X, snake[0].Position.Y + 1));
                        }
                        break;
                    case MoveDirections.Up:
                        // The snake is at tunnel.
                        if (snake[0].Position.X - 1 < 0)
                        {
                            ValidateNextStep(new Position(mapHeight - 1, snake[0].Position.Y));
                        }
                        // Move up.
                        else
                        {
                            ValidateNextStep(new Position(snake[0].Position.X - 1, snake[0].Position.Y));
                        }
                        break;
                    case MoveDirections.Down:
                        // The snake is at tunnel.
                        if (snake[0].Position.X + 1 > mapHeight - 1)
                        {
                            ValidateNextStep(new Position(0, snake[0].Position.Y));
                        }
                        // Move up.
                        else
                        {
                            ValidateNextStep(new Position(snake[0].Position.X + 1, snake[0].Position.Y));
                        }
                        break;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// Displays the menu.
        /// </summary>
        private static void DisplayMenu()
        {
            try
            {
                // Clear the console.
                Console.Clear();

                // Display the menu.
                Console.WriteLine("Press the number of the option.");
                Console.WriteLine();
                Console.WriteLine("1. Start Game");
                Console.WriteLine();
                Console.WriteLine("2. Highscores");
                Console.WriteLine();
                Console.WriteLine("3. Options");
                Console.WriteLine();
                Console.WriteLine("4. Exit");
                Console.WriteLine();
            }
            catch
            { }
        }

        /// <summary>
        /// Overwrites the given position with the given character.
        /// </summary>
        /// <param name="position">The position to write to.</param>
        /// <param name="character">The character to write.</param>
        private static void RefreshChar(Position position, char character)
        {
            try
            {
                Console.SetCursorPosition(position.Y, position.X);

                // Display the ghost in color.
                if (coloredMap)
                {
                    // Handles the characters.
                    switch (character)
                    {
                        case '#':
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;
                        case '.':
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case 'O':
                        case '@':
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;
                        case '*':
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case ' ':
                            break;
                    }

                    Console.Write(character);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                // Display the ghost in black and white.
                else
                {
                    Console.Write(character);
                }

                Console.SetCursorPosition(0, 21);
            }
            catch
            { }
        }

        /// <summary>
        /// Deletes the read key input.
        /// </summary>
        private static void DeleteKeyInput()
        {
            try
            {
                Console.CursorLeft -= 1;
                Console.Write(" ");
                Console.CursorLeft -= 1;
            }
            catch
            { }
        }

        /// <summary>
        /// Displays the options.
        /// </summary>
        private static void DisplayOptions()
        {
            try
            {
                // Clear the console.
                Console.Clear();

                // Display the menu.
                Console.WriteLine("Options");
                Console.WriteLine();
                Console.Write("Color: ");
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.Black;
                if (coloredMap)
                {
                    Console.Write("On ");
                }
                else
                {
                    Console.Write("Off");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
                Console.WriteLine();
                Console.Write("Difficulty: ");
                switch (gameDifficulty)
                {
                    case GameDifficulties.Easy:
                        Console.Write(" Easy ");
                        break;
                    case GameDifficulties.Normal:
                        Console.Write("Normal");
                        break;
                    case GameDifficulties.Hard:
                        Console.Write(" Hard ");
                        break;
                }
                Console.WriteLine();
                Console.WriteLine();

                // Display the return message.
                Console.WriteLine();
                Console.WriteLine("Press Up or Down to select an option and Left or Right to change it.");
                Console.WriteLine("Press Esc to return to menu.");
            }
            catch
            { }
        }

        /// <summary>
        /// Displays the highscores.
        /// </summary>
        private static void DisplayHighscores()
        {
            try
            {
                // Clear the console.
                Console.Clear();

                // Display the highscores.
                Console.WriteLine("Highscores.");
                Console.WriteLine();

                try
                {
                    // Get the highscores from the file.
                    highscoresList = xmlAccess.LoadScores();

                    // There is at leat one highsore in the list.
                    if (highscoresList != null && highscoresList.Count > 0)
                    {
                        // Sort the highscores in the list in a descending order.
                        highscoresList.Sort((x, y) => y.Score.CompareTo(x.Score));

                        // Check each highscore.
                        foreach (var oneItem in highscoresList)
                        {
                            // Show the highscore.
                            Console.WriteLine(oneItem.ToString());
                        }
                    }
                }
                catch
                { }

                // Display the return message.
                Console.WriteLine();
                Console.WriteLine("Press Esc to return to menu.");
            }
            catch
            { }
        }

        /// <summary>
        /// Displays the map.
        /// </summary>
        private static void DisplayMap()
        {
            try
            {
                // Clear the console.
                Console.Clear();

                // The rows of the map.
                for (int i = 0; i < mapHeight; i++)
                {
                    // The columns of the map.
                    for (int j = 0; j < mapWidth; j++)
                    {
                        // Show colored map.
                        if (coloredMap)
                        {
                            // Set the color based on the map item.
                            switch (map[i, j])
                            {
                                case '#':
                                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                                    break;
                                case '.':
                                    Console.ForegroundColor = ConsoleColor.White;
                                    break;
                                case 'O':
                                case '@':
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    break;
                                case '*':
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    break;
                                case ' ':
                                    break;
                            }

                            // Show the item and reset the color.
                            Console.Write(map[i, j]);
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        // Show black and white map.
                        else
                        {
                            Console.Write(map[i, j]);
                        }
                    }

                    // Brake the line.
                    Console.WriteLine();
                }

                // Show the information line.
                Console.WriteLine("Score: {0:00000}, Exit: Press Esc.", playerScore);
            }
            catch
            { }
        }

        /// <summary>
        /// Stores a new highscore.
        /// </summary>
        private static void StoreHighscore()
        {
            try
            {
                // Clear the console.
                Console.Clear();

                List<Highscore> scoreList = new List<Highscore>();

                try
                {
                    // Load the scores.
                    scoreList = xmlAccess.LoadScores();

                    // The scores are valid.
                    if (scoreList != null && scoreList.Count > 0)
                    {
                        // Sort the scores in descending order.
                        scoreList.Sort((x, y) => y.Score.CompareTo(x.Score));

                        bool found = false;
                        checkHighscores = false;
                        string name = "";

                        // Check each highscore.
                        for (int i = 0; i < scoreList.Count; i++)
                        {
                            // The player's score is bigger then any of the highscores.
                            if (scoreList[i].Score < playerScore)
                            {
                                // No mach found previously. 
                                if (!found)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }

                        // Try to submit it.
                        if (found)
                        {
                            // Ask if the player would like to submit the score.
                            Console.WriteLine("Your score made it to the highscores, would you like to submit it? Y or N.");
                            ConsoleKey pressed;

                            // Read the answer.
                            do
                            {
                                // Store the given answer.
                                pressed = (ConsoleKey)Console.ReadKey().Key;

                                // Delete the read key input.
                                DeleteKeyInput();
                            } while (pressed != ConsoleKey.Y && pressed != ConsoleKey.N);

                            // Submit.
                            if (pressed == ConsoleKey.Y)
                            {
                                // Get the player's name.
                                Console.WriteLine("Enter your name: ");
                                name = Console.ReadLine();

                                // The player entered a name.
                                if (!string.IsNullOrEmpty(name))
                                {
                                    // Add the highscore to the list.
                                    scoreList.Add(new Highscore(0, name, playerScore));
                                    scoreList.Sort((x, y) => y.Score.CompareTo(x.Score));

                                    // Remove the extra items.
                                    if (scoreList.Count >= scoreLimit)
                                    {
                                        int j = scoreList.Count - 1;

                                        while (j >= scoreLimit)
                                        {
                                            scoreList.RemoveAt(j);
                                            j--;
                                        }
                                    }

                                    // Save the highscores.
                                    xmlAccess.SaveScores(scoreList);
                                }
                            }
                        }
                    }
                }
                catch
                { }
            }
            catch
            { }
        }

        #endregion Functions

        #endregion Methods
    }
}
