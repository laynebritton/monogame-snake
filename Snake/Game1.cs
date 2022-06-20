using Microsoft.Xna.Framework;
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
        Texture2D FoodTexture;

        GridTile[,] grid;
        GameEntity[,] EntitiesGrid;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private int PlayerX = 0;
        private int PlayerY = 0;
        private Directions.DIRECTIONS PlayerDirection = Directions.DIRECTIONS.RIGHT;

        private bool IsGameOver = false;

        private KeyboardState PreviousKeyboardState = Keyboard.GetState();
        private KeyboardState CurrentKeyboardState = Keyboard.GetState();

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

            grid = InitializeGameTiles();
            EntitiesGrid = InstantiateEntityMap();
            EntitiesGrid[PlayerX, PlayerY] = GenerateSnakeAtCoordinate(PlayerX, PlayerY);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (IsGameOver)
                return;

            KeyboardInput();
            UpdateEntityGridLocations();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DrawGrid();
            DrawEntityGrid();

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

        private void KeyboardInput()
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            PreviousKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Keyboard.GetState();

            if (CurrentKeyboardState.IsKeyDown(Keys.Right) && !PreviousKeyboardState.IsKeyDown(Keys.Right))
            {
                if (PlayerX < GridSize - 1)
                {
                    ChangeSnakeDirection(Directions.DIRECTIONS.RIGHT);
                }
            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Left) && !PreviousKeyboardState.IsKeyDown(Keys.Left))
            {

                if (PlayerX > 0)
                {
                    ChangeSnakeDirection(Directions.DIRECTIONS.LEFT);
                }

            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Down) && !PreviousKeyboardState.IsKeyDown(Keys.Down))
            {

                if (PlayerY < GridSize - 1)
                {
                    ChangeSnakeDirection(Directions.DIRECTIONS.DOWN);
                }

            }

            if (CurrentKeyboardState.IsKeyDown(Keys.Up) && !PreviousKeyboardState.IsKeyDown(Keys.Up))
            {

                if (PlayerY > 0)
                {
                    ChangeSnakeDirection(Directions.DIRECTIONS.UP);
                }

            }
        }


        private void ChangeSnakeDirection(Directions.DIRECTIONS direction)
        {
            PlayerDirection = direction;

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
            EntitiesGrid[PlayerX, PlayerY] = null;
            PlayerX = newX;
            PlayerY = newY;
            EntitiesGrid[PlayerX, PlayerY] = GenerateSnakeAtCoordinate(PlayerX, PlayerY);

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

        private GameEntity[,] InstantiateEntityMap()
        {
            GameEntity[,] entities = new GameEntity[GridSize, GridSize];
            return entities;
        }
    }
}
