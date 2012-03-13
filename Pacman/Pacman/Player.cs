using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Text;
using System.Diagnostics;

namespace Pacman
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left,        
    }

    class Player
    {
        // Current texture 
        Texture2D texture;

        // Closed mouth pacman
        Texture2D texture0;
        // Open mouth pacman
        Texture2D texture1;

        // specifies the direction in which the sprite will be moved
        float rotation;

        // The direction in which the player is currently moving
        public Direction direction;
        // The direction in which the player will move when the path clears
        private Direction nextDirection;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        public int Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        int speed;

        public bool IsAlive
        {
            get; set;
        }

        public Rectangle Bounds
        {
            get { return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height); }
        }

        public Point PositionTile
        {
            get { return new Point((int)position.X / Tile.Width, (int)position.Y / Tile.Width); }
        }

        public Player(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.speed = 2;
            this.direction = Direction.Right;
            this.nextDirection = Direction.Right;
            this.IsAlive = true;

            LoadContent();
        }

        private void LoadContent()
        {
            texture0 = Level.Content.Load<Texture2D>("Sprites/pacman0");
            texture1 = Level.Content.Load<Texture2D>("Sprites/pacman1");
            texture = texture0;
        }

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Animate(gameTime);
            Vector2 drawingPosition = position + new Vector2(texture.Width / 2, texture.Height / 2);
            rotation = GetRotation(direction);
            spriteBatch.Draw(texture, drawingPosition, null, Color.White, rotation,
                new Vector2(texture.Width / 2, texture.Height / 2), 1, SpriteEffects.None, 1);
        }

        private void Animate(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.Milliseconds % 250 == 0)
                if (texture == texture1)
                    texture = texture0;
                else texture = texture1;
        }

        private float GetRotation(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return 0;
                case Direction.Down: return MathHelper.PiOver2;
                case Direction.Left: return 2 * MathHelper.PiOver2;
                case Direction.Up: return 3 * MathHelper.PiOver2;
            }
            return 0;
        }

        #endregion

        #region Update

        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            GetInput(keyboardState);
            UpdatePosition();
        }

        private void GetInput(KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                nextDirection = Direction.Left;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                nextDirection = Direction.Right;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                nextDirection = Direction.Down;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                nextDirection = Direction.Up;
            }
        }

        private Vector2 MovementVector(Direction direction)
        {
            Vector2 movement = new Vector2(0, 0);

            switch (direction)
            {
                case Direction.Right:
                    movement = new Vector2(speed, 0);
                    break;
                case Direction.Left:
                    movement = new Vector2(-speed, 0);
                    break;
                case Direction.Down:
                    movement = new Vector2(0, speed);
                    break;
                case Direction.Up:
                    movement = new Vector2(0, -speed);
                    break;
            }

            return movement;
        }

        private void UpdatePosition()
        {
            if (CanMove(nextDirection))
            {
                direction = nextDirection;
                nextDirection = direction;
            }

            if (CanMove(direction))
            {
                position += MovementVector(direction);
            }
        }

        #endregion

        #region Collision

        private bool CanMove(Direction direction)
        {
            Vector2 movement = MovementVector(direction);
            Vector2 nextPosition = position + movement;
            Rectangle nextBounds = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, texture0.Width, texture0.Height);

            return !level.CollidesWithWalls(nextBounds);
        }

        #endregion
    }
}
