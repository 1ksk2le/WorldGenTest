using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace WorldGenTest.Content
{
    public class Tile
    {
        public int ID { get; set; }
        public Rectangle Rectangle { get; set; }
        public bool IsWalkable { get; set; }
        public bool IsDestructible { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }

        public static List<Tile> Type { get; private set; }
        public static Dictionary<int, Color> MinimapColors { get; private set; }

        static Tile()
        {
            MinimapColors = new Dictionary<int, Color>
            {
                { 0, Color.Black },
                { 1, new Color(0, 125, 0) },
                { 2, new Color(60, 60, 60) },
                { 3, new Color(125, 125, 255) }
            };

            Type = new List<Tile>
            {
                new Tile(0, Rectangle.Empty, "Unknown", false, false),
                new Tile(1, Rectangle.Empty, "Grass", true, false),
                new Tile(2, Rectangle.Empty, "Stone", true, false),
                new Tile(3, Rectangle.Empty, "Water", false, false)
            };


        }

        public Tile(int id, Rectangle rectangle, string name, bool isWalkable, bool isDestructible)
        {
            ID = id;
            Rectangle = rectangle;
            Name = name;
            IsWalkable = isWalkable;
            IsDestructible = isDestructible;
        }

        public static string GetTileName(int id)
        {
            Tile tile = Type.Find(t => t.ID == id);
            return tile != null ? tile.Name : "Unknown";
        }

        public Tile Clone(Rectangle rectangle)
        {
            return new Tile(ID, rectangle, Name, IsWalkable, IsDestructible);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            spriteBatch.Draw(texture, Rectangle, Color.White);
        }
    }
}
