using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

        public float zoom { get; private set; }
        public Matrix TransformMatrix => Matrix.CreateTranslation(new Vector3(-position, 0)) *
                                          Matrix.CreateScale(zoom, zoom, 1);

        private float speed = 3f;
        private int worldWidth;
        private int worldHeight;
        private const float maxZoomOut = 0.5f;
        private const float maxZoomIn = 2.0f;

        public Camera(Vector2 initialPosition, int worldWidth, int worldHeight)
        {
            position = initialPosition;
            this.worldWidth = worldWidth;
            this.worldHeight = worldHeight;
            zoom = 1.0f;
        }

        public void Update(GameTime gameTime)
        {
            var input = Input_Manager.Instance;

            float zoomDelta = input.currentMouseState.ScrollWheelValue - input.previousMouseState.ScrollWheelValue;
            if (zoomDelta != 0)
            {
                zoom += zoomDelta * 0.00025f;
                zoom = MathHelper.Clamp(zoom, maxZoomOut, maxZoomIn);
            }

            float adjustedSpeed = speed / zoom;

            Vector2 movement = Vector2.Zero;
            if (input.IsKeyDown(Keys.Right) || input.IsKeyDown(Keys.D))
                movement.X += adjustedSpeed;
            if (input.IsKeyDown(Keys.Left) || input.IsKeyDown(Keys.A))
                movement.X -= adjustedSpeed;
            if (input.IsKeyDown(Keys.Up) || input.IsKeyDown(Keys.W))
                movement.Y -= adjustedSpeed;
            if (input.IsKeyDown(Keys.Down) || input.IsKeyDown(Keys.S))
                movement.Y += adjustedSpeed;

            position += movement;

            float maxX = worldWidth - (Main.screenDimX / zoom);
            float maxY = worldHeight - (Main.screenDimY / zoom);

            position = new Vector2(MathHelper.Clamp(position.X, 0, maxX), MathHelper.Clamp(position.Y, 0, maxY));

            if (input.IsKeySinglePress(Keys.R))
            {
                zoom = 1f;
            }
        }

    }
}
