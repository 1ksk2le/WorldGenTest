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
        private GraphicsDevice graphicsDevice;
        private Dictionary<int, Color> tileColors;
        public int miniMapSize;
        public int isVisible;
        public Minimap(GraphicsDevice graphicsDevice, Dictionary<int, Color> tileColors)
        {
            this.miniMapSize = 246;
            this.graphicsDevice = graphicsDevice;
            miniMapRenderTarget = new RenderTarget2D(graphicsDevice, this.miniMapSize, this.miniMapSize);
            this.tileColors = tileColors;
            isVisible = 1;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 miniMapPos)
        {
            int borderWidth = 8;
            if (isVisible == 1)
            {
                spriteBatch.DrawRectangle(new Rectangle((int)miniMapPos.X - borderWidth / 2, (int)miniMapPos.Y - borderWidth / 2, miniMapSize + borderWidth, miniMapSize + borderWidth), Color.Black, 1f);
                spriteBatch.Draw(miniMapRenderTarget, miniMapPos, Color.White);
                spriteBatch.DrawStringWithOutline(Main.font, "Zoom: " + Main.miniMapZoom, miniMapPos + new Vector2(0, miniMapSize), Color.Black, Color.White, 1f, 1);
            }
        }

        public void Update(GameConsole console)
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
        }
        public void MinimapRenderTarget(SpriteBatch spriteBatch, Camera camera, World world, int miniMapZoom)
        {
            graphicsDevice.SetRenderTarget(miniMapRenderTarget);

            graphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();

            Vector2 cameraCenter = camera.center;
            int centerTileX = (int)cameraCenter.X / world.tileSize;
            int centerTileY = (int)cameraCenter.Y / world.tileSize;

            int halfSize = (miniMapSize / 2) / Main.miniMapZoom;
            int startX = Math.Max(centerTileX - halfSize, 0);
            int startY = Math.Max(centerTileY - halfSize, 0);

            startX = Math.Min(startX, world.size - (miniMapSize / Main.miniMapZoom));
            startY = Math.Min(startY, world.size - (miniMapSize / Main.miniMapZoom));

            for (int y = 0; y < miniMapSize / Main.miniMapZoom; y++)
            {
                for (int x = 0; x < miniMapSize / Main.miniMapZoom; x++)
                {
                    int tileX = startX + x;
                    int tileY = startY + y;

                    if (tileX >= 0 && tileY >= 0 && tileX < world.size && tileY < world.size)
                    {
                        int tileID = world.GetTileID(tileX, tileY);
                        if (tileColors.TryGetValue(tileID, out Color tileColor))
                        {
                            spriteBatch.Draw(Main.pixel, new Rectangle(x * Main.miniMapZoom, y * Main.miniMapZoom, Main.miniMapZoom, Main.miniMapZoom), tileColor);
                        }
                    }
                }
            }

            float cameraViewWidth = Main.screenDim.X / camera.zoom;
            float cameraViewHeight = Main.screenDim.Y / camera.zoom;
            float cameraViewWidthInTiles = cameraViewWidth / world.tileSize;
            float cameraViewHeightInTiles = cameraViewHeight / world.tileSize;
            float cameraViewWidthInPixels = cameraViewWidthInTiles * Main.miniMapZoom;
            float cameraViewHeightInPixels = cameraViewHeightInTiles * Main.miniMapZoom;

            float cameraViewX = (centerTileX - startX - cameraViewWidthInTiles / 2) * Main.miniMapZoom;
            float cameraViewY = (centerTileY - startY - cameraViewHeightInTiles / 2) * Main.miniMapZoom;

            Rectangle cameraViewRectangle = new Rectangle((int)cameraViewX, (int)cameraViewY, (int)cameraViewWidthInPixels, (int)cameraViewHeightInPixels);

            spriteBatch.DrawRectangleBorder(cameraViewRectangle, Color.Red, 0f, 1f);

            spriteBatch.End();

            graphicsDevice.SetRenderTarget(null);
        }

    }

}
