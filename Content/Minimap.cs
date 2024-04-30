using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WorldGenTest.Content
{
    public class Minimap
    {
        public RenderTarget2D renderTarget;
        private GraphicsDevice graphicsDevice;
        private Dictionary<int, Color> tileColors;
        private int miniMapSize;

        public Minimap(int miniMapSize, GraphicsDevice graphicsDevice, Dictionary<int, Color> tileColors)
        {
            this.miniMapSize = miniMapSize;
            this.graphicsDevice = graphicsDevice;
            this.renderTarget = new RenderTarget2D(graphicsDevice, miniMapSize, miniMapSize, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            this.tileColors = tileColors;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Draw(renderTarget, position, Color.White);
        }

        public void DrawRenderTarget(SpriteBatch spriteBatch, Camera camera, World world, int tileSize)
        {
            graphicsDevice.SetRenderTarget(renderTarget);

            spriteBatch.Begin();

            int cameraTopLeftX = (int)(camera.position.X / world.tileSize);
            int cameraTopLeftY = (int)(camera.position.Y / world.tileSize);

            int startX = cameraTopLeftX;
            int startY = cameraTopLeftY;

            for (int x = 0; x < miniMapSize; x += tileSize)
            {
                for (int y = 0; y < miniMapSize; y += tileSize)
                {
                    int worldX = startX + (x / tileSize);
                    int worldY = startY + (y / tileSize);

                    if (worldX >= 0 && worldX < world.worldSize && worldY >= 0 && worldY < world.worldSize)
                    {
                        int tileID = world.tileID[worldX, worldY];
                        Color tileColor = tileColors.ContainsKey(tileID) ? tileColors[tileID] : Color.Transparent;

                        Rectangle destinationRect = new Rectangle(x, y, tileSize, tileSize);
                        spriteBatch.Draw(Main.pixel, destinationRect, tileColor);
                    }
                }
            }

            spriteBatch.End();

            graphicsDevice.SetRenderTarget(null);
        }
    }
}
