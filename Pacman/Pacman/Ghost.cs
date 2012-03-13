using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Pacman
{
    public enum AI
    {
        STUPID,
        DISTANCEBASED,
    }

    class Ghost
    {
        #region Fields and properties
        // Current texture
        Texture2D texture;

        // Closed mouth pacman
        Texture2D texture0;
        // Open mouth pacman
        Texture2D texture1;

        // The direction in which the ghost is currently moving
        public Direction direction;

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

        public Color Colour
        {
            get { return colour; }
        }
        Color colour;

        public Point PositionTile
        {
            get { return new Point((int)position.X / Tile.Width, (int)position.Y / Tile.Width); }
        }

        Random randomizer = new Random();

        public Rectangle Bounds
        {
            get { return new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height); }
        }

        private AI aiType;

        #endregion

        #region Initialization

        public Ghost(Level level, Vector2 position, Color colour, AI aiType)
        {
            this.level = level;
            this.position = position;
            this.speed = 2;
            this.direction = Direction.Right;
            this.colour = colour;
            this.aiType = aiType;

            LoadContent();
        }

        private void LoadContent()
        {
            texture0 = Level.Content.Load<Texture2D>("Sprites/ghost0");
            texture1 = Level.Content.Load<Texture2D>("Sprites/ghost1");
            texture = texture0;
        }

        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Animate(gameTime);
            spriteBatch.Draw(texture, position, colour);
        }

        private void Animate(GameTime gameTime)
        {
            if (gameTime.TotalGameTime.Milliseconds % 250 == 0)
                if (texture == texture1)
                    texture = texture0;
                else texture = texture1;
        }

        #endregion

        //temporary
        //List<Point> path = new List<Point>();        

        //private void UpdatePath()
        //{
        //    if (path.Count == 1) return;

        //    Point destination = path[0];
        //    destination.X *= Tile.Width;
        //    destination.Y *= Tile.Width;
        //    Point currentPosition = new Point();
        //    currentPosition.X = (int)position.X;
        //    currentPosition.Y = (int)position.Y;

        //    if (currentPosition == destination)
        //    {
        //        path.RemoveAt(0);
        //        direction = GetDirection(path[0]);
        //    }
        //}

        public void Update(GameTime gameTime)
        {
            //if (CanMove(direction))
            //    position += MovementVector(direction);

            //else OnStop();

            // If at T-junction, decide
            int numberOfDir = AvailableDirections();
            if (numberOfDir > 2)
                Decide();
            else if (!CanMove(direction))
                Decide();
            else if (numberOfDir == 1)
                direction = OppositeDirection(direction);
            position += MovementVector(direction);
        }
        

        //private Direction GetDirection(Point destination)
        //{
        //    Point currentTile = PositionTile;

        //    if (currentTile.X < destination.X) return Direction.Right;
        //    if (currentTile.X > destination.X) return Direction.Left;
        //    if (currentTile.Y > destination.Y) return Direction.Up;
        //    if (currentTile.Y < destination.Y) return Direction.Down;
        //    return Direction.Right;
        //}

        private void Decide()
        {
            switch (aiType)
            {
                case AI.STUPID: DecideRandomly(); break;
                case AI.DISTANCEBASED: DecideBasedOnDistance(); break;
            }
        }

        private void DecideBasedOnDistance()
        {
            List<Direction> availableDirections = new List<Direction>();
            for (int i = 0; i < 4; i++)
            {
                if (CanMove((Direction)i))
                    availableDirections.Add((Direction)i);
            }

            //int randomDirection = randomizer.Next(availableDirections.Count);
            //direction = availableDirections.ElementAt(randomDirection);

            Point up = new Point(0, -1);
            Point down = new Point(0,  1);
            Point right = new Point(1,  0);
            Point left = new Point(-1, 0);

            float min = 1000;
            Direction bestDir = availableDirections[0];
            foreach (Direction d in availableDirections)
            {
                Point futurePos = new Point();
                switch (d)
                {
                    case Direction.Up:
                        futurePos = Add(PositionTile, up);
                        break;
                    case Direction.Down:
                        futurePos = Add(PositionTile, down);
                        break;
                    case Direction.Left:
                        futurePos = Add(PositionTile, left);
                        break;
                    case Direction.Right:
                        futurePos = Add(PositionTile, right);
                        break;
                }
                
                float distance = Distance(futurePos, level.pacman.PositionTile);
                if (distance < min)
                {
                    min = distance;
                    bestDir = d;
                }
            }

            direction = bestDir;
        }

        private void DecideRandomly()
        {
            List<Direction> availableDirections = new List<Direction>();
            for (int i = 0; i < 4; i++)
            {
                if (CanMove((Direction)i))
                    availableDirections.Add((Direction)i);
            }

            int randomDirection = randomizer.Next(availableDirections.Count);
            direction = availableDirections.ElementAt(randomDirection);
        }

        private int AvailableDirections()
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                Direction dir = (Direction)i;
                if (CanMove(dir))
                    count++;
            }

            return count;
        }

        private Point Add(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        private int ManhattanDistance(Point p1, Point p2)
        {
            int distance = Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
            Debug.WriteLine(string.Format("distance from {0} to {1} is {2}", p1, p2, distance));
            return distance;
        }

        private float Distance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private bool CanMove(Direction direction)
        {
            Vector2 movement = MovementVector(direction);
            Vector2 nextPosition = position + movement;
            Rectangle nextBounds = new Rectangle((int)nextPosition.X, (int)nextPosition.Y, texture0.Width, texture0.Height);

            return !level.CollidesWithWalls(nextBounds);
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

        private Direction OppositeDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.Down: return Direction.Up;
                case Direction.Up: return Direction.Down;
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                default: return Direction.Right;
            }
        }
    }
}
