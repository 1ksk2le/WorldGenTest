using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace WorldGenTest.Content
{
    public class Minimap
    {
        public RenderTarget2D miniMapRenderTarget;
        public RenderTarget2D largeMapRenderTarget;
        private GraphicsDevice graphicsDevice;
        private Dictionary<int, Color> tileColors;
        public int miniMapSize;
        public int largeMapSize;
        public int isVisible;

        public Minimap(GraphicsDevice graphicsDevice, Dictionary<int, Color> tileColors)
        {
            this.miniMapSize = 246;
            this.largeMapSize = 500;
            this.graphicsDevice = graphicsDevice;
            miniMapRenderTarget = new RenderTarget2D(graphicsDevice, this.miniMapSize, this.miniMapSize);
            largeMapRenderTarget = new RenderTarget2D(graphicsDevice, this.largeMapSize, this.largeMapSize);
            this.tileColors = tileColors;
            isVisible = 1;
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, World world, Vector2 miniMapPos)
        {
            if (isVisible == 1)
            {
                DrawMap(spriteBatch, camera, world, miniMapPos, miniMapSize, miniMapRenderTarget);
            }
            else if (isVisible == 2)
            {
                Vector2 largeMapPos = new Vector2((Main.screenDim.X - largeMapSize) / 2, (Main.screenDim.Y - largeMapSize) / 2); // Center the large map
                DrawMap(spriteBatch, camera, world, largeMapPos, largeMapSize, largeMapRenderTarget);
            }
        }

        private void DrawMap(SpriteBatch spriteBatch, Camera camera, World world, Vector2 mapPos, int mapSize, RenderTarget2D renderTarget)
        {
            int borderWidth = 8;
            spriteBatch.DrawRectangle(new Rectangle((int)mapPos.X - borderWidth / 2, (int)mapPos.Y - borderWidth / 2, mapSize + borderWidth, mapSize + borderWidth), Color.Wheat, 1f);
            spriteBatch.Draw(renderTarget, mapPos, Color.White);
            spriteBatch.DrawStringWithOutline(Main.font, "Zoom: " + Main.mapZoom, mapPos + new Vector2(0, mapSize), Color.Black, Color.White, 1f, 1);
            spriteBatch.DrawStringWithOutline(Main.font, "Mouse Tile: " + GetMouseTileName(camera, world, mapPos, mapSize), mapPos + new Vector2(0, mapSize + 10), Color.Black, Color.White, 1f, 1);
            spriteBatch.DrawStringWithOutline(Main.font, "Mouse World POS: " + GetMousePosition(camera, world, mapPos, mapSize).ToString(), mapPos + new Vector2(0, mapSize + 20), Color.Black, Color.White, 1f, 1);
            Point mouseTilePos = new Point((int)(GetMousePosition(camera, world, mapPos, mapSize).X / world.tileSize), (int)(GetMousePosition(camera, world, mapPos, mapSize).Y / world.tileSize));
            spriteBatch.DrawStringWithOutline(Main.font, "Mouse Tile POS: " + mouseTilePos, mapPos + new Vector2(0, mapSize + 30), Color.Black, Color.White, 1f, 1);
        }

        private Vector2 GetMousePosition(Camera camera, World world, Vector2 mapPos, int mapSize)
        {
            var input = InputManager.Instance;

            Vector2 mousePos = input.mousePosition;
            if (mousePos.X >= mapPos.X && mousePos.X < mapPos.X + mapSize &&
                mousePos.Y >= mapPos.Y && mousePos.Y < mapPos.Y + mapSize)
            {
                int pixelSize = Main.mapZoom;
                Vector2 relativePos = (mousePos - mapPos) / pixelSize;

                int halfSize = (mapSize / 2) / pixelSize;
                int centerTileX = (int)camera.center.X / world.tileSize;
                int centerTileY = (int)camera.center.Y / world.tileSize;
                int startX = Math.Max(centerTileX - halfSize, 0);
                int startY = Math.Max(centerTileY - halfSize, 0);

                startX = Math.Min(startX, world.sizeX - (mapSize / pixelSize));
                startY = Math.Min(startY, world.sizeY - (mapSize / pixelSize));

                int tileX = startX + (int)relativePos.X;
                int tileY = startY + (int)relativePos.Y;

                float newCameraX = MathHelper.Clamp(tileX * world.tileSize, 0, world.sizeX * world.tileSize - Main.screenDim.X);
                float newCameraY = MathHelper.Clamp(tileY * world.tileSize, 0, world.sizeY * world.tileSize - Main.screenDim.Y);

                return new Vector2(newCameraX, newCameraY);
            }
            return Vector2.Zero;
        }

        private string GetMouseTileName(Camera camera, World world, Vector2 mapPos, int mapSize)
        {
            var input = InputManager.Instance;
            Vector2 mousePos = input.mousePosition;

            int pixelSize = Main.mapZoom;
            Vector2 relativePos = (mousePos - mapPos) / pixelSize;

            int halfSize = (mapSize / 2) / pixelSize;
            int centerTileX = (int)camera.center.X / world.tileSize;
            int centerTileY = (int)camera.center.Y / world.tileSize;
            int startX = Math.Max(centerTileX - halfSize, 0);
            int startY = Math.Max(centerTileY - halfSize, 0);

            startX = Math.Min(startX, world.sizeX - (mapSize / pixelSize));
            startY = Math.Min(startY, world.sizeY - (mapSize / pixelSize));

            int tileX = startX + (int)relativePos.X;
            int tileY = startY + (int)relativePos.Y;
            return Tile.GetTileName(world.GetTileID(tileX, tileY));
        }

        public void Update(GameConsole console, Camera camera, World world, Vector2 miniMapPos)
        {
            var input = InputManager.Instance;
            if (input.IsKeySinglePress(Keys.M) && !console.isVisible)
            {
                if (isVisible < 2)
                {
                    isVisible++;
                }
                else
                {
                    isVisible = 0;
                }
            }

            if (input.IsButtonSingleClick(true) && ((input.mousePosition.X >= miniMapPos.X && input.mousePosition.X < miniMapPos.X + miniMapSize &&
                input.mousePosition.Y >= miniMapPos.Y && input.mousePosition.Y < miniMapPos.Y + miniMapSize) ||
                (input.mousePosition.X >= (Main.screenDim.X - largeMapSize) / 2 && input.mousePosition.X < (Main.screenDim.X + largeMapSize) / 2 &&
                input.mousePosition.Y >= (Main.screenDim.Y - largeMapSize) / 2 && input.mousePosition.Y < (Main.screenDim.Y + largeMapSize) / 2)))
            {
                //HandleCameraMovement(camera, world, miniMapPos);
            }
        }

        public void MinimapRenderTarget(SpriteBatch spriteBatch, Camera camera, World world)
        {
            RenderTarget2D renderTarget;
            int mapSize;

            if (isVisible == 1)
            {
                renderTarget = miniMapRenderTarget;
                mapSize = miniMapSize;
            }
            else if (isVisible == 2)
            {
                renderTarget = largeMapRenderTarget;
                mapSize = largeMapSize;
            }
            else
            {
                return;
            }

            graphicsDevice.SetRenderTarget(renderTarget);

            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();

            Vector2 cameraCenter = camera.center;
            int centerTileX = (int)cameraCenter.X / world.tileSize;
            int centerTileY = (int)cameraCenter.Y / world.tileSize;

            int halfSize = (mapSize / 2) / Main.mapZoom;
            int startX = Math.Max(centerTileX - halfSize, 0);
            int startY = Math.Max(centerTileY - halfSize, 0);

            startX = Math.Min(startX, world.sizeX - (mapSize / Main.mapZoom));
            startY = Math.Min(startY, world.sizeY - (mapSize / Main.mapZoom));

            for (int y = 0; y < mapSize / Main.mapZoom; y++)
            {
                for (int x = 0; x < mapSize / Main.mapZoom; x++)
                {
                    int tileX = startX + x;
                    int tileY = startY + y;

                    if (tileX >= 0 && tileY >= 0 && tileX < world.sizeX && tileY < world.sizeY)
                    {
                        int tileID = world.GetTileID(tileX, tileY);
                        if (tileColors.TryGetValue(tileID, out Color tileColor))
                        {
                            spriteBatch.Draw(Main.pixel, new Rectangle(x * Main.mapZoom, y * Main.mapZoom, Main.mapZoom, Main.mapZoom), tileColor);
                        }
                    }
                }
            }

            float cameraViewWidth = Main.screenDim.X / camera.zoom;
            float cameraViewHeight = Main.screenDim.Y / camera.zoom;
            float cameraViewWidthInTiles = cameraViewWidth / world.tileSize;
            float cameraViewHeightInTiles = cameraViewHeight / world.tileSize;
            float cameraViewWidthInPixels = cameraViewWidthInTiles * Main.mapZoom;
            float cameraViewHeightInPixels = cameraViewHeightInTiles * Main.mapZoom;

            float cameraViewX = (centerTileX - startX - cameraViewWidthInTiles / 2) * Main.mapZoom;
            float cameraViewY = (centerTileY - startY - cameraViewHeightInTiles / 2) * Main.mapZoom;

            Rectangle cameraViewRectangle = new Rectangle((int)cameraViewX, (int)cameraViewY, (int)cameraViewWidthInPixels, (int)cameraViewHeightInPixels);

            spriteBatch.DrawRectangleBorder(cameraViewRectangle, Color.Red, 0f, 1f);

            spriteBatch.End();

            graphicsDevice.SetRenderTarget(null);
        }

        public void HandleCameraMovement(Camera camera, World world, Vector2 miniMapPos)
        {
            var input = InputManager.Instance;
            Vector2 mousePos = input.mousePosition;

            if (isVisible == 1)
            {
                if (mousePos.X >= miniMapPos.X && mousePos.X < miniMapPos.X + miniMapSize &&
                    mousePos.Y >= miniMapPos.Y && mousePos.Y < miniMapPos.Y + miniMapSize)
                {
                    Vector2 newCameraPos = GetMousePosition(camera, world, miniMapPos, miniMapSize);
                    camera.position = new Vector2(
                        MathHelper.Clamp(newCameraPos.X, 0, world.sizeX * world.tileSize - Main.screenDim.X),
                        MathHelper.Clamp(newCameraPos.Y, 0, world.sizeY * world.tileSize - Main.screenDim.Y)
                    );
                }
            }
            else if (isVisible == 2)
            {
                Vector2 largeMapPos = new Vector2((Main.screenDim.X - largeMapSize) / 2, (Main.screenDim.Y - largeMapSize) / 2);
                if (mousePos.X >= largeMapPos.X && mousePos.X < largeMapPos.X + largeMapSize &&
                    mousePos.Y >= largeMapPos.Y && mousePos.Y < largeMapPos.Y + largeMapSize)
                {
                    Vector2 newCameraPos = GetMousePosition(camera, world, largeMapPos, largeMapSize);
                    camera.position = new Vector2(
                        MathHelper.Clamp(newCameraPos.X, 0, world.sizeX * world.tileSize - Main.screenDim.X),
                        MathHelper.Clamp(newCameraPos.Y, 0, world.sizeY * world.tileSize - Main.screenDim.Y)
                    );
                }
            }
        }
    }
}
