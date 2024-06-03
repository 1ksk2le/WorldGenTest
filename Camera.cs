using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WorldGenTest.Content;

namespace WorldGenTest
{
    public class Camera
    {
        private Vector2 Position;
        public Vector2 position
        {
            get => Position;
            set => Position = value;
        }

        public Vector2 center
        {
            get
            {
                return position + new Vector2(Main.screenDim.X / (2 * zoom), Main.screenDim.Y / (2 * zoom));
            }
        }

        public float zoom { get; private set; }
        public Matrix TransformMatrix => Matrix.CreateTranslation(new Vector3(-position, 0)) *
                                          Matrix.CreateScale(zoom, zoom, 1);

        private float speed = 3f;
        private int worldWidth;
        private int worldHeight;
        private const float maxZoomOut = 0.5f;
        private const float maxZoomIn = 1.0f;

        public Camera(Vector2 initialPosition, int worldWidth, int worldHeight)
        {
            position = initialPosition;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            zoom = 1.0f;
        }

        public void Update(GameTime gameTime, GameConsole console)
        {
            var input = InputManager.Instance;

            float zoomDelta = input.currentMouseState.ScrollWheelValue - input.previousMouseState.ScrollWheelValue;
            if (zoomDelta != 0 && !input.IsKeyDown(Keys.LeftShift))
            {
                zoom += zoomDelta * 0.00025f;
                zoom = MathHelper.Clamp(zoom, maxZoomOut, maxZoomIn);
            }

            float adjustedSpeed = speed / zoom;

            Vector2 movement = Vector2.Zero;
            if (!console.isVisible)
            {
                if (input.IsKeyDown(Keys.Right) || input.IsKeyDown(Keys.D))
                    movement.X += adjustedSpeed;
                if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.A))
                    movement.X -= adjustedSpeed;
                if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.W))
                    movement.Y -= adjustedSpeed;
                if (input.IsKeyDown(Keys.Down) || input.IsKeyDown(Keys.S))
                    movement.Y += adjustedSpeed;
            }


            position += movement;

            float maxX = worldWidth - (Main.screenDim.X / zoom);
            float maxY = worldHeight - (Main.screenDim.Y / zoom);

            position = new Vector2(MathHelper.Clamp(position.X, 0, maxX), MathHelper.Clamp(position.Y, 0, maxY));

            if (input.IsKeySinglePress(Keys.R))
            {
                zoom = 1f;
            }
        }

        public Rectangle GetVisibleArea(Vector2 screenSize, World world)
        {
            // Calculate the size of the visible area in the world
            float visibleWidth = screenSize.X / zoom;
            float visibleHeight = screenSize.Y / zoom;

            // Calculate the position of the top-left corner of the visible area in the world
            float visibleX = position.X - (visibleWidth / 2);
            float visibleY = position.Y - (visibleHeight / 2);

            // Ensure the visible area does not extend beyond the boundaries of the world
            visibleX = MathHelper.Clamp(visibleX, 0, world.size - visibleWidth);
            visibleY = MathHelper.Clamp(visibleY, 0, world.size - visibleHeight);

            // Create and return the rectangle representing the visible area
            return new Rectangle((int)visibleX, (int)visibleY, (int)visibleWidth, (int)visibleHeight);
        }

    }
}
