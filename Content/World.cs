using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WorldGenTest.Content
{

    public class World
    {
        public int tileSize = 32;
        public int worldSize = 150;
        public int[,] tileID;
        private Rectangle[,] tileRectangle;

        public World()
        {
            tileRectangle = new Rectangle[worldSize, worldSize];
            tileID = new int[worldSize, worldSize];
            GenerateWorldGrid();
        }

        private void GenerateWorldGrid()
        {
            for (int x = 0; x < worldSize; x++)
            {
                for (int y = 0; y < worldSize; y++)
                {
                    tileRectangle[x, y] = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    tileID[x, y] = 3;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Dictionary<int, Texture2D> tileTextures)
        {
            int startX = (int)MathHelper.Clamp(camera.position.X / tileSize, 0, worldSize - 1);
            int startY = (int)MathHelper.Clamp(camera.position.Y / tileSize, 0, worldSize - 1);

            int endX = (int)MathHelper.Clamp((camera.position.X + spriteBatch.GraphicsDevice.Viewport.Width / camera.zoom) / tileSize + 1, 0, worldSize);
            int endY = (int)MathHelper.Clamp((camera.position.Y + spriteBatch.GraphicsDevice.Viewport.Height / camera.zoom) / tileSize + 1, 0, worldSize);

            Vector2 mouseWorldPosition = Input_Manager.Instance.mousePosition / camera.zoom + camera.position;
            Point mouseTile = new Point((int)(mouseWorldPosition.X / tileSize), (int)(mouseWorldPosition.Y / tileSize));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Texture2D texture = tileTextures[tileID[x, y]];
                    Vector2 position = new Vector2(x * tileSize, y * tileSize);
                    spriteBatch.Draw(texture, position, Color.White);

                    if (x == mouseTile.X && y == mouseTile.Y)
                    {
                        spriteBatch.DrawRectangle(tileRectangle[x, y], Color.White * 0.2f, 0.99f);//Draw a white transparent box on the tile our mouse is hovering
                    }
                }
            }
        }

        public int GetTileID(int x, int y)
        {
            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                return this.tileID[x, y];
            }
            return 0;
        }

        public void SetTileID(int x, int y, int tileID)
        {
            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                this.tileID[x, y] = tileID;
            }
        }

        public void SaveToFile(string fileName)
        {
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "WhirlingRealms");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < worldSize; y++)
                {
                    int currentTile = tileID[0, y];
                    int count = 1;

                    for (int x = 1; x < worldSize; x++)
                    {
                        if (tileID[x, y] == currentTile)
                        {
                            count++;
                        }
                        else
                        {
                            writer.Write($"{currentTile}:{count} ");
                            currentTile = tileID[x, y];
                            count = 1;
                        }
                    }

                    writer.Write($"{currentTile}:{count} ");
                    writer.WriteLine();
                }
            }
        }

        public void LoadFromFile(string fileName)
        {
            string appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "WhirlingRealms");
            string filePath = Path.Combine(folderPath, fileName);

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {fileName} does not exist.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);

                for (int y = 0; y < lines.Length; y++)
                {
                    string line = lines[y];
                    string[] tileData = line.Split(' ');

                    int x = 0;
                    foreach (string data in tileData)
                    {
                        string[] parts = data.Split(':');
                        if (parts.Length != 2)
                        {
                            Console.WriteLine($"Invalid format in line {y + 1}: {line}");
                            continue;
                        }

                        if (!int.TryParse(parts[0], out int tileID) || !int.TryParse(parts[1], out int count))
                        {
                            Console.WriteLine($"Invalid integer format in line {y + 1}: {line}");
                            continue;
                        }

                        for (int i = 0; i < count; i++)
                        {
                            if (x < worldSize && y < worldSize)
                            {
                                this.tileID[x, y] = tileID;
                                x++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the file: {ex.Message}");
            }
        }
    }
}
