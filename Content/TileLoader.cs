using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace WorldGenTest.Content
{

    public class TileLoader
    {
        private const string TileIDFilePath = "Content/TileID.csv"; // Assuming fixed file path

        public static string LoadTileName(int tileID)
        {
            Dictionary<int, string> tileNames = new Dictionary<int, string>();

            try
            {
                // Read the TileID.csv file
                string[] lines = File.ReadAllLines(TileIDFilePath);

                // Iterate through each line in the file
                for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
                {
                    string line = lines[i];
                    string[] parts = line.Split(',');

                    // Ensure the line has at least two parts (tile ID and name)
                    if (parts.Length >= 2)
                    {
                        // Parse the tile ID
                        if (int.TryParse(parts[0], out int id))
                        {
                            // Check if the tile ID matches the provided ID
                            if (id == tileID)
                            {
                                return parts[1]; // Return the name of the tile
                            }
                        }
                    }
                }

                return $"Tile with ID {tileID} not found.";
            }
            catch (Exception ex)
            {
                return $"An error occurred while loading tile names: {ex.Message}";
            }
        }

        public static Dictionary<int, Texture2D> LoadTileTextures(string filePath, ContentManager content)
        {
            Dictionary<int, Texture2D> tileTextures = new Dictionary<int, Texture2D>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 2)
                    {
                        int tileID = int.Parse(parts[0]);
                        string texturePath = $"Textures/Tiles/Tile_{parts[0]}";
                        Texture2D texture = content.Load<Texture2D>(texturePath);
                        tileTextures[tileID] = texture;
                    }
                }
            }

            return tileTextures;
        }
        public static Dictionary<int, Color> LoadTileColors(string filePath)
        {
            Dictionary<int, Color> tileColors = new Dictionary<int, Color>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                reader.ReadLine(); // Skip the header line
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length >= 4) // Ensure there are enough parts to extract color information
                    {
                        int tileID = int.Parse(parts[0]);
                        int red = int.Parse(parts[2]);
                        int green = int.Parse(parts[3]);
                        int blue = int.Parse(parts[4]);
                        Color color = new Color(red, green, blue);
                        tileColors[tileID] = color;
                    }
                }
            }

            return tileColors;
        }
    }
}
