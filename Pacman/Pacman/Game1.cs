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
using System.IO;

namespace Pacman
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // Resources for drawing
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Overlays
        private Texture2D currentOverlay;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;

        // Game state
        Level level;
        int levelIndex = -1;
        const int numberOfLevels = 2;
        bool isInOverlay = false;
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/winOverlay");
            loseOverlay = Content.Load<Texture2D>("Overlays/loseOverlay");

            LoadNextLevel();
        }
        
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            KeyboardState keyboardState = Keyboard.GetState();

            if (isInOverlay)
            {
                if (keyboardState.IsKeyDown(Keys.Space))
                    isInOverlay = false;
                return;
            }

            if (level.LevelEnded == false)
                level.Update(gameTime, keyboardState);
            else if (!level.pacman.IsAlive)
            {
                isInOverlay = true;
                currentOverlay = loseOverlay;
                ReloadLevel();
            }
            else
            {
                isInOverlay = true;
                currentOverlay = winOverlay;
                LoadNextLevel();
            }

            base.Update(gameTime);
        }

        private void ReloadLevel()
        {
            levelIndex--;
            LoadNextLevel();
        }

        private void LoadNextLevel()
        {
            levelIndex = (levelIndex + 1) % numberOfLevels;
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (isInOverlay)
            {
                DrawOverlay();
            }
            else
            {
                level.Draw(gameTime, spriteBatch);                
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawOverlay()
        {
            spriteBatch.Draw(currentOverlay, new Rectangle(0, 0, currentOverlay.Width, currentOverlay.Height), Color.White);
        }
    }
}
