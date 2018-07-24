using LineBatch;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    /// 

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        public bool debug = false;
        public float spawn = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1400;
            graphics.PreferredBackBufferHeight = 700;

            Content.RootDirectory = "Content";

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromMilliseconds(GameManager.GM.GlobalUpdateTimer);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            GameManager.GM.cm = Content;
            GameManager.GM.gd = GraphicsDevice;
            GameManager.GM.sb = spriteBatch;

            this.IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatchEx.GraphicsDevice = GraphicsDevice;
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
           
            
            // TODO: use this.Content to load your game content here

            //new Walker(new Vector2(105,385));
            font = Content.Load<SpriteFont>("File");
            
            //Physics.Phy.AddObjects();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                GUIManager.GuiManager.state = 0;
                GameManager.GM.score = 0;
                GameManager.GM.Empty();
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Delete))
            {
                Exit();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                if (debug)
                {
                    debug = false;
                }
                else
                {
                    debug = true;
                }
            }
            if (GUIManager.GuiManager.state == (int)GUIManager.GameState.menu)
            {
                MouseState ms = Mouse.GetState();
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    GUIManager.GuiManager.OnClick(ms.Position);
                }
            }
            else if (GUIManager.GuiManager.state == (int)GUIManager.GameState.play)
            {
                //update cooldowns 
                Physics.Phy.elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                GameManager.GM.gameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                

                //create list to delete object that are not on screen.
                List<PhysicsObject> offscreen = new List<PhysicsObject>();

                //call physics update to update movment and forces ect.
                Physics.Phy.PhysicsUpdate();

                //collect any objects off screen for deletion and run the update methods for each object
                foreach (KeyValuePair<Guid, PhysicsObject> p in Physics.Phy.physicsObjects)
                {
                    if (p.Value.movable || p.Value is MovingTile)
                    {
                        //p.Update();
                        if (p.Value.pos.X < 0 || p.Value.pos.X > 1400)
                        {
                            offscreen.Add(p.Value);
                        }
                        else if (p.Value.pos.Y < 0 || p.Value.pos.Y > 700)
                        {
                            offscreen.Add(p.Value);
                        }
                        if (p.Value != GameManager.GM.player)//don't run player update as the fire function add objects to the list and can break the loop
                        {
                            p.Value.Update();
                        }
                    }
                }


                //delete objects off screen.
                foreach (PhysicsObject p in offscreen)
                {
                    GameManager.GM.Delete(p);
                }

                //run player update
                if (GameManager.GM.player != null)
                {
                    GameManager.GM.player.Update();
                }



                base.Update(gameTime);
                //spawn enemies
                spawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (spawn >= 7 && GameManager.GM.player != null)
                {
                    Random n = new Random();
                    for (int i = 0; i < 2; i++)
                    {
                        new Flier(new Vector2(n.Next(100, 1300), n.Next(100, 600)));
                        new Walker(new Vector2(n.Next(100, 1300), n.Next(100, 600)));
                    }
                    spawn = 0;
                }
                Physics.Phy.elapsedTime = 0;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);
            spriteBatch.Begin();
            GameManager.GM.sb = spriteBatch;
            // TODO: Add your drawing code here
            //draw menu
            if (GUIManager.GuiManager.state == (int)GUIManager.GameState.menu)
            {
                Menu.M.Draw(Menu.M.MainMenu);
            }
            //draw objects
            else if (GUIManager.GuiManager.state == (int)GUIManager.GameState.play)
            {
                //draw every object
                foreach (Object o in GameManager.GM.objects)
                {
                    spriteBatch.Draw(texture: o.tex, position: o.pos, destinationRectangle: null, sourceRectangle: null, origin: new Vector2(o.tex.Width / 2, o.tex.Height / 2), rotation: o.rot.Z, scale: null, color: null, layerDepth: 0);

                }
                //draw tiles and there reset is debug stuff
                foreach (KeyValuePair<Guid, PhysicsObject> o in Physics.Phy.physicsObjects)
                {

                    //draw aabb
                    if ((o.Value.GetType().IsAssignableFrom(typeof(Rect)) || o.Value.GetType().IsSubclassOf(typeof(Rect))) && debug)
                    {
                        Rect ro = (Rect)o.Value;
                        ro.GetPoints();

                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ro.aabb.Location.X, ro.aabb.Location.Y), new Vector2(ro.aabb.Location.X + ro.aabb.Width, ro.aabb.Location.Y), Color.White);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ro.aabb.Location.X + ro.aabb.Width, ro.aabb.Location.Y), new Vector2(ro.aabb.Location.X + ro.aabb.Width, ro.aabb.Location.Y + ro.aabb.Height), Color.White);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ro.aabb.Location.X + ro.aabb.Width, ro.aabb.Location.Y + ro.aabb.Height), new Vector2(ro.aabb.Location.X, ro.aabb.Location.Y + ro.aabb.Height), Color.White);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ro.aabb.Location.X, ro.aabb.Location.Y + ro.aabb.Height), new Vector2(ro.aabb.Location.X, ro.aabb.Location.Y), Color.White);
                        Vector2[] tpoints = new Vector2[ro.points.Length];
                        for (int i = 0; i < ro.points.Length; i++)
                        {
                            tpoints[i] = ro.GetPointAfterRotation(new Vector2(ro.points[i].X, ro.points[i].Y));
                        }
                        for (int i = 0; i < ro.points.Length; i++)
                        {
                            if (i == ro.points.Length - 1)
                            {
                                SpriteBatchEx.DrawLine(spriteBatch, new Vector2(tpoints[0].X, tpoints[0].Y), new Vector2(tpoints[i].X, tpoints[i].Y), Color.Black);
                            }
                            else
                            {
                                SpriteBatchEx.DrawLine(spriteBatch, new Vector2(tpoints[i + 1].X, tpoints[i + 1].Y), new Vector2(tpoints[i].X, tpoints[i].Y), Color.Black);
                            }
                        }


                    }
                    else if ((o.Value.GetType().IsAssignableFrom(typeof(Circle)) || o.Value.GetType().IsSubclassOf(typeof(Circle))) && debug)
                    {
                        Circle c = (Circle)o.Value;
                        Texture2D circle = SpriteBatchEx.CreateCircle((int)c.radius);
                        spriteBatch.Draw(circle, new Vector2(c.pos.X - circle.Width / 2, c.pos.Y - circle.Height / 2), Color.Red);
                    }
                }
                if (debug)
                {
                    foreach (KeyValuePair<int, NavTile> t in GameManager.GM.currentLevel.nav)
                    {
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(t.Value.pos.X - Level.tilesize / 2, t.Value.pos.Y - Level.tilesize / 2), new Vector2(t.Value.pos.X + Level.tilesize / 2, t.Value.pos.Y - Level.tilesize / 2), Color.Green);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(t.Value.pos.X + Level.tilesize / 2, t.Value.pos.Y - Level.tilesize / 2), new Vector2(t.Value.pos.X + Level.tilesize / 2, t.Value.pos.Y + Level.tilesize / 2), Color.Green);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(t.Value.pos.X + Level.tilesize / 2, t.Value.pos.Y + Level.tilesize / 2), new Vector2(t.Value.pos.X - Level.tilesize / 2, t.Value.pos.Y + Level.tilesize / 2), Color.Green);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(t.Value.pos.X - Level.tilesize / 2, t.Value.pos.Y + Level.tilesize / 2), new Vector2(t.Value.pos.X - Level.tilesize / 2, t.Value.pos.Y - Level.tilesize / 2), Color.Green);
                        spriteBatch.DrawString(font, t.Key.ToString(), new Vector2(t.Value.pos.X, t.Value.pos.Y), Color.Green);
                    }

                    foreach (PhysicsSystem ps in GameManager.GM.currentLevel.LevelPhysics)
                    {
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ps.area.Left, ps.area.Top), new Vector2(ps.area.Right, ps.area.Top), Color.Blue);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ps.area.Right, ps.area.Top), new Vector2(ps.area.Right, ps.area.Bottom), Color.Blue);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ps.area.Right, ps.area.Bottom), new Vector2(ps.area.Left, ps.area.Bottom), Color.Blue);
                        SpriteBatchEx.DrawLine(spriteBatch, new Vector2(ps.area.Left, ps.area.Bottom), new Vector2(ps.area.Left, ps.area.Top), Color.Blue);
                    }
                }
                
                base.Draw(gameTime);
            //draw score
            spriteBatch.DrawString(font, "Score: " + GameManager.GM.score, new Vector2(700, 650), Color.Red);
            }
            spriteBatch.End();
        }
    }
}
