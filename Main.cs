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

        public static int screenDimX, screenDimY;

        private World world;

        private int miniMapSize = 246;
        private Minimap miniMap;

        Dictionary<int, Texture2D> tileTextures;

        float textDuration;

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            screenDimX = 1000;
            screenDimY = 1000;
            graphics.PreferredBackBufferHeight = screenDimY;
            graphics.PreferredBackBufferWidth = screenDimX;
            graphics.ApplyChanges();

            world = new World();
            int worldArea = world.worldSize * world.tileSize;
            camera = new Camera(Vector2.Zero, worldArea, worldArea);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            string filePath = "Content/TileID.csv";
            miniMap = new Minimap(miniMapSize, GraphicsDevice, TileLoader.LoadTileColors(filePath));

            world.LoadFromFile("world1.csv");

            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            font = Content.Load<SpriteFont>("Fonts/Font_Test");


            tileTextures = TileLoader.LoadTileTextures(filePath, Content);

            foreach (var kvp in tileTextures)
            {
                int tileID = kvp.Key;
                string texturePath = $"Textures/Tiles/Tile_{tileID}";
                Texture2D texture = Content.Load<Texture2D>(texturePath);
                tileTextures[tileID] = texture;
            }


        }

        protected override void Update(GameTime gameTime)
        {
            var input = Input_Manager.Instance;
            input.PreUpdate();
            input.PostUpdate(gameTime);

            camera.Update(gameTime);

            Build();

            if (input.IsKeySinglePress(Keys.C))
            {
                for (int i = 0; i < world.worldSize; i++)
                {
                    for (int j = 0; j < world.worldSize; j++)
                    {
                        world.SetTileID(i, j, 1);
                    }
                }
            }

            if (textDuration > 0)
            {
                textDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds;
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

            base.Update(gameTime);
        }

        void Build()
        {
            var input = Input_Manager.Instance;
            if (input.IsButtonPressed(true))
            {

                Vector2 mouseWorldPosition = Input_Manager.Instance.mousePosition / camera.zoom + camera.position;

                Point mouseTile = new Point((int)(mouseWorldPosition.X / world.tileSize), (int)(mouseWorldPosition.Y / world.tileSize));

                if (mouseTile.X >= 0 && mouseTile.X < world.worldSize && mouseTile.Y >= 0 && mouseTile.Y < world.worldSize)
                {
                    world.SetTileID(mouseTile.X, mouseTile.Y, 2);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            Vector2 mouseWorldPosition = Input_Manager.Instance.mousePosition / camera.zoom + camera.position;
            Point mouseTile = new Point((int)(mouseWorldPosition.X / world.tileSize), (int)(mouseWorldPosition.Y / world.tileSize));

            GraphicsDevice.Clear(Color.Black);

            miniMap.DrawRenderTarget(spriteBatch, camera, world, 2);

            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, camera.TransformMatrix);
            world.Draw(spriteBatch, camera, tileTextures);


            string tileInfo = $"[{world.GetTileID(mouseTile.X, mouseTile.Y)}] {TileLoader.LoadTileName(world.GetTileID(mouseTile.X, mouseTile.Y))}";
            spriteBatch.DrawStringWithOutline(font, tileInfo, mouseWorldPosition + new Vector2(12, 0), Color.Black, Color.White, 1f, 1f);

            spriteBatch.End();

            spriteBatch.Begin();
            spriteBatch.DrawRectangle(new Rectangle(Main.screenDimX - miniMapSize - 10, 0, 254, 254), Color.Black, 1f);
            miniMap.Draw(spriteBatch, new Vector2(Main.screenDimX - miniMapSize - 6, 4));
            spriteBatch.End();

            spriteBatch.Begin();
            DrawFPS(gameTime);
            spriteBatch.DrawStringWithOutline(font, "Camera POS: " + camera.position, new Vector2(10, 20), Color.Black, Color.White, 1f, 1f);
            spriteBatch.DrawStringWithOutline(font, "Mouse POS: " + mouseWorldPosition, new Vector2(10, 30), Color.Black, Color.White, 1f, 1f);
            spriteBatch.DrawStringWithOutline(font, "Mouse Tile POS: " + mouseTile, new Vector2(10, 40), Color.Black, Color.White, 1f, 1f);

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
    }
}