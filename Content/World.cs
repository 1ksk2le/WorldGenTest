using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WorldGenTest.Content
{
    public class World
    {
        public int tileSize { get; } = 32;
        public int size { get; } = 350;
        public Tile[,] tiles { get; private set; }

        public World()
        {
            tiles = new Tile[size, size];
            GenerateWorldGrid();
        }

        private void GenerateWorldGrid()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    var rectangle = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                    tiles[x, y] = Tile.Type[1].Clone(rectangle);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Camera camera, Dictionary<int, Texture2D> tileTextures)
        {
            int startX = (int)MathHelper.Clamp(camera.position.X / tileSize, 0, size - 1);
            int startY = (int)MathHelper.Clamp(camera.position.Y / tileSize, 0, size - 1);

            int endX = (int)MathHelper.Clamp((camera.position.X + spriteBatch.GraphicsDevice.Viewport.Width / camera.zoom) / tileSize + 1, 0, size);
            int endY = (int)MathHelper.Clamp((camera.position.Y + spriteBatch.GraphicsDevice.Viewport.Height / camera.zoom) / tileSize + 1, 0, size);

            Vector2 mouseWorldPosition = InputManager.Instance.mousePosition / camera.zoom + camera.position;
            Point mouseTile = new Point((int)(mouseWorldPosition.X / tileSize), (int)(mouseWorldPosition.Y / tileSize));

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    Texture2D texture = tileTextures[tiles[x, y].ID];
                    tiles[x, y].Draw(spriteBatch, texture);

                    if (x == mouseTile.X && y == mouseTile.Y)
                    {
                        spriteBatch.DrawRectangle(tiles[x, y].Rectangle, Color.White * 0.2f, 0.99f); // Draw a white transparent box on the tile our mouse is hovering
                    }
                }
            }
        }

        public int GetTileID(int x, int y)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                return tiles[x, y].ID;
            }
            return 0;
        }

        public void SetTileID(int x, int y, int tileID)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                tiles[x, y].ID = tileID;
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                return tiles[x, y];
            }
            return null;
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (x >= 0 && x < size && y >= 0 && y < size)
            {
                tiles[x, y] = tile.Clone(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize));
            }
        }

        public void SaveToFile(string fileName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folderPath = Path.Combine(appDataPath, "WhirlingRealms");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < size; y++)
                {
                    int currentTileID = tiles[0, y].ID;
                    int count = 1;

                    for (int x = 1; x < size; x++)
                    {
                        if (tiles[x, y].ID == currentTileID)
                        {
                            count++;
                        }
                        else
                        {
                            writer.Write($"{currentTileID}:{count} ");
                            currentTileID = tiles[x, y].ID;
                            count = 1;
                        }
                    }

                    writer.Write($"{currentTileID}:{count} ");
                    writer.WriteLine();
                }
            }
        }

        public void LoadFromFile(string fileName)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
                            if (x < size && y < size)
                            {
                                var rectangle = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                                tiles[x, y] = Tile.Type[tileID].Clone(rectangle);
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
