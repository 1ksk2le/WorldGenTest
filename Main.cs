using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using WorldGenTest.Content;

namespace WorldGenTest
{
    public class Main : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public static Texture2D pixel;

        public static Camera camera;

        public static SpriteFont font;

        private int frameRate;
        private int frameCounter;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        public static Vector2 screenDim;

        private World world;
        public static int chosenTile;
        public static bool devMode;

        #region MINIMAP VARIABLES
        public static int mapZoom = 2;
        private Vector2 miniMapPosition = Vector2.Zero;
        private Minimap miniMap;
        #endregion

        Dictionary<int, Texture2D> tileTextures;

        private GameConsole console;

        float textDuration;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            screenDim = new Vector2(1000, 1000);
            graphics.PreferredBackBufferHeight = (int)screenDim.Y;
            graphics.PreferredBackBufferWidth = (int)screenDim.X;
            graphics.ApplyChanges();

            world = new World();
            camera = new Camera(Vector2.Zero, world.tileSize * world.sizeX, world.tileSize * world.sizeY);

            tileTextures = new Dictionary<int, Texture2D>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            font = Content.Load<SpriteFont>("Fonts/Font_Test");

            for (int i = 0; i < 4; i++)
            {
                string texturePath = $"Textures/Tiles/Tile_{i}";
                Texture2D texture = Content.Load<Texture2D>(texturePath);
                tileTextures[i] = texture;
            }

            miniMap = new Minimap(GraphicsDevice, Tile.MinimapColors);
            mapZoom = 2;
            miniMapPosition = new Vector2(Main.screenDim.X - miniMap.miniMapSize - 6, 4);
            chosenTile = 1;

            world.LoadFromFile("world1.csv");

            devMode = true;
            console = new GameConsole(world, font);
        }

        protected override void Update(GameTime gameTime)
        {
            var input = InputManager.Instance;
            input.PreUpdate();
            input.PostUpdate(gameTime, camera);

            camera.Update(gameTime, console);
            miniMap.Update(console, camera, world, miniMapPosition);

            if (devMode)
            {
                Build();
                if (input.IsKeySinglePress(Keys.B))
                {
                    for (int i = 0; i < world.sizeX; i++)
                    {
                        for (int j = 0; j < world.sizeY; j++)
                        {
                            world.SetTileID(i, j, 0);
                        }
                    }
                    world.GenerateDungeon(35, 8, 24, new Vector2(50, 10), 5, 2);
                }
            }

            console.Update(input);

            #region CONTROLS
            if (!console.isVisible)
            {
                if (input.IsKeySinglePress(Keys.PageUp))
                {
                    mapZoom++;
                }
                else if (input.IsKeySinglePress(Keys.PageDown) && mapZoom > 1)
                {
                    mapZoom--;
                }
                if (input.IsKeySinglePress(Keys.F5))
                {
                    textDuration = 2;
                    world.SaveToFile("world1.csv");
                }
                if (input.IsKeySinglePress(Keys.F9))
                {
                    world.LoadFromFile("world1.csv");
                }
            }
            #endregion

            if (textDuration > 0)
            {
                textDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var input = InputManager.Instance;
            Point mouseTile = new Point((int)(input.mouseWorldPosition.X / world.tileSize), (int)(input.mouseWorldPosition.Y / world.tileSize));

            GraphicsDevice.Clear(Color.Black);

            miniMap.MinimapRenderTarget(spriteBatch, camera, world);

            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.TransformMatrix);
            world.Draw(spriteBatch, camera, tileTextures);

            string tileInfo = $"[{world.GetTileID(mouseTile.X, mouseTile.Y)}] {Tile.GetTileName(world.GetTileID(mouseTile.X, mouseTile.Y))}";
            spriteBatch.DrawStringWithOutline(font, tileInfo, input.mouseWorldPosition + new Vector2(12, 0), Color.Black, Color.White, 1f, 1f);

            spriteBatch.End();

            spriteBatch.Begin();
            miniMap.Draw(spriteBatch, camera, world, miniMapPosition);
            spriteBatch.End();

            spriteBatch.Begin();
            console.Draw(spriteBatch, graphics);
            spriteBatch.End();

            spriteBatch.Begin();
            if (!console.isVisible)
            {
                DrawFPS(gameTime);
                spriteBatch.DrawStringWithOutline(font, "Camera POS: " + camera.position, new Vector2(10, 20), Color.Black, Color.White, 1f, 1f);
                spriteBatch.DrawStringWithOutline(font, "Camera Center: " + camera.center, new Vector2(10, 30), Color.Black, Color.White, 1f, 1f);
                spriteBatch.DrawStringWithOutline(font, "Mouse POS: " + input.mousePosition, new Vector2(10, 40), Color.Black, Color.White, 1f, 1f);
                spriteBatch.DrawStringWithOutline(font, "Mouse World POS: " + input.mouseWorldPosition, new Vector2(10, 50), Color.Black, Color.White, 1f, 1f);
                spriteBatch.DrawStringWithOutline(font, "Mouse Tile POS: " + mouseTile, new Vector2(10, 60), Color.Black, Color.White, 1f, 1f);
            }

            int visibleTileX = (int)((Main.screenDim.X / world.tileSize) / camera.zoom);
            int visibleTileY = (int)((Main.screenDim.Y / world.tileSize) / camera.zoom);

            spriteBatch.DrawStringWithOutline(font, "vis X: " + visibleTileX, new Vector2(10, 200), Color.Black, Color.White, 1f, 1f);
            spriteBatch.DrawStringWithOutline(font, "vis Y: " + visibleTileY, new Vector2(10, 210), Color.Black, Color.White, 1f, 1f);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawFPS(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;
            frameCounter++;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            string fps = string.Format("FPS: {0}", frameRate);
            spriteBatch.DrawStringWithOutline(font, fps, new Vector2(10, 10), Color.Black, Color.White, 1f, 1f);
        }

        private void Build()
        {
            var input = InputManager.Instance;
            if (input.IsButtonPressed(true) &&
                (!input.IsMouseOnUI(new Rectangle((int)miniMapPosition.X, (int)miniMapPosition.Y, miniMap.miniMapSize, miniMap.miniMapSize)) && miniMap.isVisible != 2))
            {
                Point mouseTile = new Point((int)(input.mouseWorldPosition.X / world.tileSize), (int)(input.mouseWorldPosition.Y / world.tileSize));

                if (mouseTile.X >= 0 && mouseTile.X < world.sizeX && mouseTile.Y >= 0 && mouseTile.Y < world.sizeY)
                {
                    world.SetTileID(mouseTile.X, mouseTile.Y, chosenTile);
                }
            }
        }
    }
}
