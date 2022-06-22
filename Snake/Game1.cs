using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Snake.Content;
using System.Collections.Generic;


namespace Snake
{
    public class Game1 : Game
    {
        static int GridSize = 20;
        static int TileSize = 50;

        Texture2D gridTexture1;
        Texture2D gridTexture2;

        Texture2D SnakeTexture;
        Texture2D SnakeBodyTexture;
        Texture2D FoodTexture;
        SpriteFont font;
        SoundEffect hurt;
        SoundEffect eat;

        GridTile[,] grid;
        GameEntity[,] EntitiesGrid;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int PlayerX;
        private int PlayerY;
        private Directions.DIRECTIONS PlayerDirection = Directions.DIRECTIONS.RIGHT;

        private int SnakeLength;
        private List<Vector2> SnakeCoordinateHistory;

        private bool IsGameOver = false;

        private float AutoMoveDelay;
        private bool WaitingForAutoMove;
        private float AutoMoveTimerStart;

        private float BonusFoodDelay;
        private bool WaitingForBonusFoodSpawn;
        private float BonusFoodSpawnTimerStart;

        private bool FoodIsSpawnedOnGrid;

        private KeyboardState PreviousKeyboardState = Keyboard.GetState();
        private KeyboardState CurrentKeyboardState = Keyboard.GetState();

        private int score = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.ApplyChanges();
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            gridTexture1 = Content.Load<Texture2D>("grid1");
            gridTexture2 = Content.Load<Texture2D>("grid2");
            SnakeTexture = Content.Load<Texture2D>("snake");
            SnakeBodyTexture = Content.Load<Texture2D>("snake-body");
            FoodTexture = Content.Load<Texture2D>("food");
            font = Content.Load<SpriteFont>("Score");
            hurt = Content.Load<SoundEffect>("hurt");
            eat = Content.Load<SoundEffect>("eat");

            PlayerX = 6;
            PlayerY = 9;
            SnakeLength = 5;
            SnakeCoordinateHistory = InitializeSnakeCoordinateHistory(SnakeLength, PlayerX, PlayerY);

            FoodIsSpawnedOnGrid = false;

            grid = InitializeGameTiles();
            EntitiesGrid = InstantiateEntityMap();
            EntitiesGrid[PlayerX, PlayerY] = GenerateSnakeAtCoordinate(PlayerX, PlayerY);

            AutoMoveDelay = 500; // Milliseconds
            WaitingForAutoMove = false;
            AutoMoveTimerStart = 0;

            BonusFoodDelay = 30000;
            WaitingForBonusFoodSpawn = false;
            BonusFoodSpawnTimerStart = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            ExitGameKeyboardInput();
            if (IsGameOver)
                return;

            AutoMoveOnTimer(gameTime);
            SpawnBonusFoodOnTimer(gameTime);

            if (!FoodIsSpawnedOnGrid)
            {
                SpawnFood();
            }
            KeyboardInput();
            AddSnakeBodiesToGrid();
            UpdateEntityGridLocations();
            base.Update(gameTime);
        }

        private void AutoMoveOnTimer(GameTime gameTime)
        {
            if (!WaitingForAutoMove)
            {
                AutoMoveTimerStart = (float)gameTime.TotalGameTime.TotalMilliseconds;
                WaitingForAutoMove = true;
            }
            else if (WaitingForAutoMove)
            {
                if ((float)gameTime.TotalGameTime.TotalMilliseconds - AutoMoveTimerStart > AutoMoveDelay)
                {
                    MovePlayerInPlayerDirection();
                    WaitingForAutoMove = false;
                }
            }
        }

        private void SpawnBonusFoodOnTimer(GameTime gameTime)
        {
            if (!WaitingForBonusFoodSpawn)
            {
                BonusFoodSpawnTimerStart = (float)gameTime.TotalGameTime.TotalMilliseconds;
                WaitingForBonusFoodSpawn = true;
            }
            else if (WaitingForAutoMove)
            {
                if ((float)gameTime.TotalGameTime.TotalMilliseconds - BonusFoodSpawnTimerStart > BonusFoodDelay)
                {
                    SpawnFood();
                    WaitingForBonusFoodSpawn = false;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DrawGrid();
            DrawEntityGrid();
            DrawScoreCount();

            if (IsGameOver)
                DrawGameOverText();

            base.Draw(gameTime);
        }

        private GridTile[,] InitializeGameTiles()
        {
            GridTile[,] grid = new GridTile[GridSize, GridSize];

            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    grid[i, j] = new GridTile(j * 50, i * 50, ((i + j) % 2 == 0) ? gridTexture1 : gridTexture2, GameEntity.ENTITY_TYPE.GRID_TILE);
                }
            }

            return grid;
        }

        private void ExitGameKeyboardInput()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
        }

        private void KeyboardInput()
        {
            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            if (CurrentKeyboardState.IsKeyDown(Keys.Right) && !PreviousKeyboardState.IsKeyDown(Keys.Right))
            {
                ChangeSnakeDirection(Directions.DIRECTIONS.RIGHT);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Left) && !PreviousKeyboardState.IsKeyDown(Keys.Left))
            {
                ChangeSnakeDirection(Directions.DIRECTIONS.LEFT);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Down) && !PreviousKeyboardState.IsKeyDown(Keys.Down))
            {
                ChangeSnakeDirection(Directions.DIRECTIONS.DOWN);
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Up) && !PreviousKeyboardState.IsKeyDown(Keys.Up))
            {
                ChangeSnakeDirection(Directions.DIRECTIONS.UP);
            }
            if (CurrentKeyboardState.IsKeyDown(Keys.Space) && !PreviousKeyboardState.IsKeyDown(Keys.Space))
            {
                SnakeLength += 1;
            }
        }


        private void ChangeSnakeDirection(Directions.DIRECTIONS direction)
        {
            if(IsDirectOppositeDirection(direction))
            {
                return;
            }
            PlayerDirection = direction;
            MovePlayerInPlayerDirection();
        }

        private bool IsDirectOppositeDirection(Directions.DIRECTIONS direction)
        {
            if(PlayerDirection == Directions.DIRECTIONS.RIGHT && direction == Directions.DIRECTIONS.LEFT)
            {
                return true;
            }
            if (PlayerDirection == Directions.DIRECTIONS.LEFT && direction == Directions.DIRECTIONS.RIGHT)
            {
                return true;
            }
            if (PlayerDirection == Directions.DIRECTIONS.UP && direction == Directions.DIRECTIONS.DOWN)
            {
                return true;
            }
            if (PlayerDirection == Directions.DIRECTIONS.DOWN && direction == Directions.DIRECTIONS.UP)
            {
                return true;
            }
            return false;
        }

        private void MovePlayerInPlayerDirection()
        {

            if (PlayerDirection == Directions.DIRECTIONS.RIGHT)
            {
                MovePlayer(PlayerX + 1, PlayerY);
            }
            else if (PlayerDirection == Directions.DIRECTIONS.LEFT)
            {

                MovePlayer(PlayerX - 1, PlayerY);
            }
            else if (PlayerDirection == Directions.DIRECTIONS.UP)
            {
                MovePlayer(PlayerX, PlayerY - 1);
            }
            else if (PlayerDirection == Directions.DIRECTIONS.DOWN)
            {

                MovePlayer(PlayerX, PlayerY + 1);
            }
        }

        private void MovePlayer(int newX, int newY)
        {
            if (newX > GridSize - 1 || newX < 0 || newY > GridSize - 1 || newY < 0)
            {
                GameOver();
                return;
            }
            CheckCollisionsWhenMoving(newX, newY);

            SnakeCoordinateHistory = UpdateSnakeCoordinateHistory(PlayerX, PlayerY);
            EntitiesGrid[PlayerX, PlayerY] = null;
            PlayerX = newX;
            PlayerY = newY;
            EntitiesGrid[PlayerX, PlayerY] = GenerateSnakeAtCoordinate(PlayerX, PlayerY);
        }

        private List<Vector2> UpdateSnakeCoordinateHistory(int startingX, int startingY)
        {
            var PreviousCoordinate = new Vector2(startingX, startingY);
            var NewHistory = new List<Vector2>();

            NewHistory.Add(PreviousCoordinate);
            for (int i = 0; i < SnakeLength - 1 && i < SnakeCoordinateHistory.Count; i++)
            {
                NewHistory.Add(SnakeCoordinateHistory[i]);
            }

            RemoveSnakeBodiesFromGrid();

            return NewHistory;
        }

        private void CheckCollisionsWhenMoving(int newX, int newY)
        {
            var entity = EntitiesGrid[newX, newY];
            if (entity == null)
                return;

            if (entity.EntityType == GameEntity.ENTITY_TYPE.SNAKE_BODY)
            {
                GameOver();
            }
            else if (entity.EntityType == GameEntity.ENTITY_TYPE.FOOD)
            {
                eat.Play();
                AutoMoveDelay -= 25;
                if (AutoMoveDelay < 17)
                {
                    AutoMoveDelay = 17;
                }
                SnakeLength += 1;
                score += 1;
                FoodIsSpawnedOnGrid = false;
            }

        }

        private Snake GenerateSnakeAtCoordinate(int x, int y)
        {
            return new Snake(x * TileSize, y * TileSize, SnakeTexture, GameEntity.ENTITY_TYPE.SNAKE);
        }

        private void DrawGrid()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    grid[i, j]?.Draw(_spriteBatch);
                }
            }
        }

        private void UpdateEntityGridLocations()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    if (EntitiesGrid[i, j] != null)
                    {
                        var entity = EntitiesGrid[i, j];
                        entity.X = i * TileSize;
                        entity.Y = j * TileSize;
                    }
                }
            }
        }

        private void DrawEntityGrid()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    if (EntitiesGrid[i, j] != null)
                    {
                        var entity = EntitiesGrid[i, j];
                        entity.Draw(_spriteBatch);
                    }
                }
            }
        }

        private void RemoveSnakeBodiesFromGrid()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    if (EntitiesGrid[i, j] != null)
                    {
                        if (EntitiesGrid[i, j].EntityType == GameEntity.ENTITY_TYPE.SNAKE_BODY)
                        {
                            EntitiesGrid[i, j] = null;
                        }
                    }
                }
            }
        }

        private List<Vector2> InitializeSnakeCoordinateHistory(int length, int startingX, int startingY)
        {
            var coordinateHistory = new List<Vector2>();

            for (int i = 1; i < length + 1; i++)
            {
                coordinateHistory.Add(new Vector2(startingX - i, startingY));
            }

            return coordinateHistory;
        }

        private void AddSnakeBodiesToGrid()
        {
            foreach (var body in SnakeCoordinateHistory)
            {
                EntitiesGrid[(int)body.X, (int)body.Y] = new SnakeBody((int)body.X * TileSize, (int)body.Y * TileSize, SnakeBodyTexture, GameEntity.ENTITY_TYPE.SNAKE_BODY);
            }
        }

        private GameEntity[,] InstantiateEntityMap()
        {
            GameEntity[,] entities = new GameEntity[GridSize, GridSize];
            return entities;
        }

        private void GameOver()
        {
            hurt.Play();
            IsGameOver = true;
        }

        private void SpawnFood()
        {
            var random = new System.Random();
            bool foodIsSpawned = false;
            while (!foodIsSpawned)
            {
                int randomX = random.Next(0, GridSize - 1);
                int randomY = random.Next(0, GridSize - 1);
                if (EntitiesGrid[randomX, randomY] == null)
                {
                    if (randomX != PlayerX - 1 && randomX != PlayerX + 1 && randomY != PlayerY - 1 && randomX != PlayerY + 1)
                    {
                        EntitiesGrid[randomX, randomY] = CreateFoodEntity();
                        foodIsSpawned = true;
                    }
                }
            }

            FoodIsSpawnedOnGrid = true;

        }

        private void DrawScoreCount()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "Score: " + score, new Vector2(1100, 100), Color.White);
            _spriteBatch.End();
        }

        private void DrawGameOverText()
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "Game over", new Vector2(1100, 200), Color.White);
            _spriteBatch.End();
        }

        private Food CreateFoodEntity()
        {
            return new Food(0, 0, FoodTexture, GameEntity.ENTITY_TYPE.FOOD);
        }
    }
}
