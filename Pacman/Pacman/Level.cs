using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pacman
{
    class Level
    {
        // Physical structure
        Tile[,] tiles;

        // Entities
        public Player pacman;
        private List<Ghost> ghosts = new List<Ghost>();
        private List<Pellet> pellets = new List<Pellet>();

        // Level Content
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        public bool LevelEnded
        {
            get;
            set;
        }


        public Level(IServiceProvider serviceProvider, Stream fileStream)
        {
            content = new ContentManager(serviceProvider, "Content");
            ReadTiles(fileStream);
        }

        #region Loading

        /// <summary>
        /// Loads appearance and behavior of each tile in the level.
        /// </summary>
        /// <param name="fileStream"></param>
        private void ReadTiles(Stream fileStream)
        {
            // Load the level; check that all lines have the same length
            int width;
            List<string> lines = new List<string>();

            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(string.Format("The length of line {0} is different from the preceding lines", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid
            tiles = new Tile[width, lines.Count];

            // Allocate each tile
            for (int y = 0; y < lines.Count; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }
        }

        /// <summary>
        /// Load appearance and behavior for an individual tile.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the level file.
        /// </param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank tile
                case ' ':
                    return LoadPellet(x, y);

                // Wall
                case 'X':
                    return LoadTile("Wall", TileCollision.Impassable);

                // Pellet
                case '*':
                    return LoadPellet(x, y);

                // Pacman
                case 'P':
                    return LoadPacman(x, y);

                // Ghost #1
                case '1':
                    return LoadGhost(x, y, Color.Red, AI.DISTANCEBASED);

                // Ghost #2
                case '2':
                    return LoadGhost(x, y, Color.Blue, AI.STUPID);

                // Ghost #3
                case '3':
                    return LoadGhost(x, y, Color.Orange, AI.STUPID);

                // Ghost #4
                case '4':
                    return LoadGhost(x, y, Color.Pink, AI.STUPID);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadPellet(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            pellets.Add(new Pellet(this, new Vector2(position.X, position.Y)));

            return LoadTile("Empty", TileCollision.Passable);
        }

        private Tile LoadPacman(int x, int y)
        {
            pacman = new Player(this, new Vector2(Tile.Width * x, Tile.Width * y));
            return LoadTile("Empty", TileCollision.Passable);
        }

        private Tile LoadGhost(int x, int y, Color colour, AI aiType)
        {
            ghosts.Add(new Ghost(this, GetTileCoord(x, y), colour, aiType));
            return LoadTile("Empty", TileCollision.Passable);
        }

        #endregion
        
        #region Collisions

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Returns the coordinates of the upper left point of a tile
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public Vector2 GetTileCoord(int x, int y)
        {
            return new Vector2(x * Tile.Width, y * Tile.Width);
        }

        public Point GetTileAtPoint(int x, int y)
        {
            return new Point(x / Tile.Width, y / Tile.Width);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        /// <summary>
        /// Checks to see if a given rectangle collides with any Impassable tile.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public bool CollidesWithWalls(Rectangle rect)
        {
            Point upperLeft = GetTileAtPoint(rect.Left, rect.Top);
            Point lowerRight = GetTileAtPoint(rect.Right, rect.Bottom);

            for (int x = upperLeft.X; x < lowerRight.X + 1; x++)
            {
                for (int y = upperLeft.Y; y < lowerRight.Y + 1; y++)
                {
                    if (tiles[x, y].Collision == TileCollision.Passable)
                        continue;

                    Rectangle tileRect = new Rectangle(x * (int)Tile.Size.X, y * (int)Tile.Size.Y, Tile.Width, Tile.Width);
                    if (rect.Intersects(tileRect))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Makes player eat any pellets he touches
        /// </summary>
        private void PelletsCollisions()
        {
            for (int i = 0; i < pellets.Count; i++)
            {
                if (pacman.Bounds.Intersects(pellets[i].Bounds))
                {
                    pellets.RemoveAt(i);
                    return;
                }
            }
        }

        private void GhostCollisions()
        {
            foreach (Ghost ghost in ghosts)
                if (ghost.Bounds.Intersects(pacman.Bounds))
                {
                    pacman.IsAlive = false;
                    LevelEnded = true;
                }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates each entity in the level and tests for collisions
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyboardState"></param>
        public void Update(GameTime gameTime, KeyboardState keyboardState)
        {
            // Update entities
            pacman.Update(gameTime, keyboardState);
            foreach (Ghost ghost in ghosts)
                ghost.Update(gameTime);

            // Handle collisions
            PelletsCollisions();
            GhostCollisions();

            // Win level if there are no pellets left
            if (pellets.Count == 0)
                LevelEnded = true;
        }       

        #endregion

        #region Draw

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            DrawTiles(spriteBatch);
            pacman.Draw(gameTime, spriteBatch);
            DrawPellets(gameTime, spriteBatch);
            DrawGhosts(gameTime, spriteBatch);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void DrawPellets(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Pellet pellet in pellets)
            {
                pellet.Draw(gameTime, spriteBatch);
            }
        }

        private void DrawGhosts(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (Ghost ghost in ghosts)
                ghost.Draw(gameTime, spriteBatch);
        }

        #endregion Draw
    }
}
