using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WorldGenTest.Content
{
    public class Tile
    {
        public int TileID { get; set; }
        public Color MiniMapColor { get; set; }
        public Texture2D Texture { get; set; }

        public Tile(int tileID, Color miniMapColor, Texture2D texture)
        {
            TileID = tileID;
            MiniMapColor = miniMapColor;
            Texture = texture;
        }

        // Method to draw the tile
        public virtual void Draw(SpriteBatch spriteBatch, Rectangle rectangle)
        {
            spriteBatch.Draw(Texture, rectangle, Color.White);
        }
    }

    public class Grass : Tile
    {
        public Grass(int tileID, Color miniMapColor, Texture2D texture) : base(tileID, miniMapColor, texture)
        {
            tileID = 0;
            miniMapColor = Color.Green;
        }
    }

    public class Stone : Tile
    {
        public Stone(int tileID, Color miniMapColor, Texture2D texture) : base(tileID, miniMapColor, texture)
        {
            tileID = 1;
            miniMapColor = Color.Gray;
        }
    }
}
