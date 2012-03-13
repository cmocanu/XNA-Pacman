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
    class Pellet
    {
        Texture2D texture;

        // Represents the point where the texture will be drawn
        Vector2 origin;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        public Level Level
        {
            get { return level; }
            set { level = value; }
        }
        private Level level;

        public Color Colour
        {
            get { return color; }
            set { color = value; }
        }
        Color color;

        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)origin.X, (int)origin.Y, texture.Width, texture.Height);
            }
        }

        /// <summary>
        /// Creates a new pellet object;
        /// </summary>
        /// <param name="level"></param>
        /// <param name="position">
        /// Represents the center of the tile which containts the pellet
        /// </param>
        public Pellet(Level level, Vector2 position)
        {
            this.position = position;
            this.level = level;
            this.color = Color.Teal;

            LoadContent();
        }

        private void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/pellet");

            this.origin = new Vector2(position.X - texture.Width / 2, position.Y - texture.Height / 2);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, origin, color);
        }
    }
}
