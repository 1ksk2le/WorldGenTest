using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

        public void DrawRenderTarget(SpriteBatch spriteBatch, Camera camera, World world, int miniMapPixelSize)
        {
            graphicsDevice.SetRenderTarget(renderTarget);

            spriteBatch.Begin();

            // Calculate the position of the camera in tile coordinates
            int cameraTopLeftX = (int)(camera.position.X / world.tileSize);
            int cameraTopLeftY = (int)(camera.position.Y / world.tileSize);

            // Calculate the start position for rendering based on the camera's position
            int startX = cameraTopLeftX;
            int startY = cameraTopLeftY;

            // Calculate the number of tiles that fit in the minimap based on its pixel size
            int numTilesX = miniMapSize / miniMapPixelSize;
            int numTilesY = miniMapSize / miniMapPixelSize;

            // Calculate the maximum start position for rendering based on the camera's position and the size of the minimap
            int maxStartX = Math.Max(0, Math.Min(world.worldSize - numTilesX, cameraTopLeftX));
            int maxStartY = Math.Max(0, Math.Min(world.worldSize - numTilesY, cameraTopLeftY));

            // Adjust the start position if it exceeds the maximum allowed
            startX = Math.Min(startX, maxStartX);
            startY = Math.Min(startY, maxStartY);

            for (int x = 0; x < numTilesX; x++)
            {
                for (int y = 0; y < numTilesY; y++)
                {
                    // Calculate the world coordinates for the current tile in the minimap
                    int worldX = startX + x;
                    int worldY = startY + y;

                    // Check if the current world coordinates are within the bounds of the world
                    if (worldX >= 0 && worldX < world.worldSize && worldY >= 0 && worldY < world.worldSize)
                    {
                        // Get the tile ID and color for the current world coordinates
                        int tileID = world.tileID[worldX, worldY];
                        Color tileColor = tileColors.ContainsKey(tileID) ? tileColors[tileID] : Color.Transparent;

                        // Calculate the destination rectangle for rendering the tile in the minimap
                        Rectangle destinationRect = new Rectangle(x * miniMapPixelSize, y * miniMapPixelSize, miniMapPixelSize, miniMapPixelSize);

                        // Draw the tile using a 1x1 pixel texture
                        spriteBatch.Draw(Main.pixel, destinationRect, tileColor);
                    }
                }
            }

            spriteBatch.End();

            graphicsDevice.SetRenderTarget(null);
        }

    }
}
