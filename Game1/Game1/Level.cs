using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    //most of this code is just creating array of objects for each level so it's not very important and a little messy.
    public class Menu
    {
        private static Menu m;
        public List<GUI> MainMenu = new List<GUI>();
        public SpriteFont font = GameManager.GM.cm.Load<SpriteFont>("File");

        public Menu()
        {
            Populate();
        }
        public static Menu M
        {
            get
            {
                if (m == null)
                {
                    m = new Menu();
                }
                return m;
            }
        }

        public void Populate()
        {
            MainMenu.Add(new Button(new Rectangle(new Point(600, 100), new Point(300, 50)), "Level 1", Level.Level1));
            MainMenu.Add(new Button(new Rectangle(new Point(600, 200), new Point(300, 50)), "Level 2", Level.Level2));
        }

        public void Draw(List<GUI> gui)
        {
            foreach(GUI g in gui)
            {
                if(g is Button)
                {
                    Button b = (Button)g;
                    GameManager.GM.sb.Draw(b.texture, b.area, Color.White);
                    GameManager.GM.sb.DrawString(font, b.text, new Vector2(b.area.X + b.area.Width / 2, b.area.Y + b.area.Height / 2), Color.White);
                }
            }
        }
    }

    public static class Level
    {
        public struct LevelInfo
        {
            public int[,] LevelLayout;
            public PhysicsSystem[] LevelPhysics;
            public PhysicsTile[] LevelColliders;
            public int[,] LevelNavMap;
            public Player player;
            public Object[] LevelEntities;
            
            public Dictionary<int, NavTile> nav;


            public LevelInfo(Player player, Object[] LevelEntities, int[,] LevelLayout, PhysicsSystem[] LevelPhysics, PhysicsTile[] LevelColliders, int[,] LevelNavMap)
            {
                this.player = player;
                this.LevelEntities = LevelEntities;
                nav = new Dictionary<int, NavTile>();
                this.LevelLayout = LevelLayout;
                this.LevelPhysics = LevelPhysics;
                this.LevelColliders = LevelColliders;
                this.LevelNavMap = LevelNavMap;
                PopulateNav(this);
                //LoadLevelContent(this);
            }
        }
        public static int tilesize = 70;
        public static float tilescale = 0.5f;

        

        static Dictionary<int, string> tiles = new Dictionary<int, string>()
        {
            { 0, "None" },
            { 1,"floor"},
            { 2,"wall"},
            { 3,"ladder" }
        };

        static int[,] level1Textures =
        {
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,2,2,2,2,2,2,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,2,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,0,0,0,2 },
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }
        };

        static PhysicsSystem[] Level1PhySys =
        {
            new PhysicsSystem(new Rectangle(0, 0, 13*tilesize, 10*tilesize), 9.8f, 1, 2),
            new PhysicsSystem(new Rectangle(13*tilesize, 5*tilesize, 7*tilesize, 5*tilesize), 5f, .5f, 2),
            new PhysicsSystem(new Rectangle(13*tilesize, 0*tilesize, 7*tilesize, 5*tilesize), 9.8f, 1f, 1)
        };

        static PhysicsTile[] level1Colliders =
        {
            new PhysicsTile("wall", new Vector2(10f*tilesize, 0.5f*tilesize), 20*tilesize, tilesize), //top
            new PhysicsTile("wall", new Vector2(0.5f*tilesize, 5f*tilesize), tilesize, 10*tilesize), // left
            new PhysicsTile("wall", new Vector2(19.5f*tilesize, 5f*tilesize), tilesize, 10*tilesize), // right
            new PhysicsTile("floor", new Vector2(10f*tilesize, 9.5f*tilesize), 20*tilesize, tilesize), //bottom
            new PhysicsTile("wall", new Vector2(16f*tilesize, 4.5f*tilesize), 6*tilesize, tilesize),
            new PhysicsTile("wall", new Vector2(13.5f*tilesize, 6.5f*tilesize), tilesize, 3*tilesize),
            new Ladder("ladder", new Vector2(12.5f*tilesize, 6f*tilesize), tilesize, 6*tilesize),
        };

        static int[,] level1nav = 
        {
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0 },
            { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
        };

        static Player level1player = new Player("Player", new Vector2(595, 385), true, "player1");

        static Object[] level1entities =
        {
             new ForceArea("wind", new Vector2(4.5f*tilesize, 8.5f*tilesize), tilesize, tilesize, new Vector2(0, -3f))
        };

        public static LevelInfo Level1 = new LevelInfo(level1player, level1entities, level1Textures, Level1PhySys, level1Colliders, level1nav);

        static int[,] level2Textures =
        {
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
            { 2,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,2 },
            { 2,0,0,3,1,1,1,1,1,1,1,1,1,1,1,3,0,0,0,2 },
            { 2,0,0,3,0,0,0,0,0,0,0,0,0,0,0,3,0,0,0,2 },
            { 2,0,0,3,0,3,0,0,0,0,0,0,0,3,0,3,0,0,0,2 },
            { 2,1,1,1,1,3,0,0,0,0,0,0,0,3,1,1,1,1,1,2 },
            { 2,0,0,0,0,3,0,0,0,0,0,0,0,3,0,0,0,0,0,2 },
            { 2,0,0,0,0,3,0,0,0,0,0,0,0,3,0,0,0,0,0,2 },
            { 2,0,0,0,0,3,0,0,0,0,0,0,0,3,0,0,0,0,0,2 },
            { 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 }
        };

        static PhysicsSystem[] Level2PhySys =
        {
            new PhysicsSystem(new Rectangle(0, 0, 20*tilesize, 10*tilesize), 9.8f, 1, 2)
        };
        new static Point[] level2move = { new Point((int)10f * tilesize, (int)5.5f * tilesize), new Point((int)10f * tilesize, (int)7.5f * tilesize) };
        static PhysicsTile[] level2Colliders =
        {
            new PhysicsTile("wall", new Vector2(10f*tilesize, 0.5f*tilesize), 20*tilesize, tilesize), //top
            new PhysicsTile("wall", new Vector2(0.5f*tilesize, 5f*tilesize), tilesize, 10*tilesize), // left
            new PhysicsTile("wall", new Vector2(19.5f*tilesize, 5f*tilesize), tilesize, 10*tilesize), // right
            new PhysicsTile("floor", new Vector2(10f*tilesize, 9.5f*tilesize), 20*tilesize, tilesize), //bottom
            new Ladder("ladder", new Vector2(13.5f*tilesize, 6.5f*tilesize), tilesize, 5*tilesize),
            new Ladder("ladder", new Vector2(5.5f*tilesize, 6.5f*tilesize), tilesize, 5*tilesize),
            new Ladder("ladder", new Vector2(3.5f*tilesize, 3f*tilesize), tilesize, 4*tilesize),
            new Ladder("ladder", new Vector2(15.5f*tilesize, 3f*tilesize), tilesize, 4*tilesize),
            new PhysicsTile("floor", new Vector2(3f*tilesize, 5.5f*tilesize), 4*tilesize, tilesize),
            new PhysicsTile("floor", new Vector2(16.5f*tilesize, 5.5f*tilesize), 5*tilesize, tilesize),
            new PhysicsTile("floor", new Vector2(9.5f*tilesize, 2.5f*tilesize), 11*tilesize, tilesize),
            new MovingTile("platform", new Vector2(9.5f*tilesize, 6.5f*tilesize), 3*tilesize, tilesize, level2move)
        };

        static int[,] level2nav =
        {
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 },
            { 0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0 },
            { 0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0 },
            { 0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0 },
            { 0,1,1,1,1,1,0,0,0,0,0,0,0,1,1,1,1,1,1,0 },
            { 0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0 },
            { 0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0 },
            { 0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0 },
            { 0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0 },
            { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 }
        };

        //static Player level2player = new Player("Player", new Vector2(595, 385), true, "player");

        static Object[] level2entities =
        {
        };

        public static LevelInfo Level2 = new LevelInfo(level1player, level2entities, level2Textures, Level2PhySys, level2Colliders, level2nav);

        public static void PopulateNav(LevelInfo level)
        {
            int key = 0;

            for(int i = 0; i < level.LevelNavMap.GetLength(0); i++)
            {
                for(int j = 0; j < level.LevelNavMap.GetLength(1); j++)
                {
                    if(level.LevelNavMap[i,j] == 1)
                    {
                        level.nav.Add(key, new NavTile((j * tilesize) + (tilesize / 2), (i * tilesize) + (tilesize / 2)));
                        key++;
                    }
                }
            }

            foreach(KeyValuePair<int,NavTile> t1 in level.nav)
            {
                foreach (KeyValuePair<int, NavTile> t2 in level.nav)
                {
                    if (t2.Key != t1.Key)
                    {
                        if (((Math.Abs(t2.Value.pos.X - t1.Value.pos.X) <= tilesize) && (Math.Abs(t2.Value.pos.Y - t1.Value.pos.Y) <= tilesize))&& (Math.Abs(t2.Value.pos.X - t1.Value.pos.X) + Math.Abs(t2.Value.pos.Y - t1.Value.pos.Y) == tilesize))
                        {
                            if (!t1.Value.children.Contains(t2.Key))
                            {
                                t1.Value.children.Add(t2.Key);
                            }
                            if (!t2.Value.children.Contains(t1.Key))
                            {
                                t2.Value.children.Add(t1.Key);
                            }
                        }
                    }
                }
            }
        }

        public static void LoadLevelContent(LevelInfo level)
        {
            Physics.Phy.physicsSystems.Clear();
            Physics.Phy.physicsObjects.Clear();
            Physics.Phy.ToAdd.Clear();
            GameManager.GM.objects.Clear();
            
            Tile[,] tiles;
            foreach (PhysicsSystem ps in level.LevelPhysics)
            {
                Physics.Phy.physicsSystems.Add(ps.guid, ps);
            }
            //load map tiles
            tiles = new Tile[level.LevelLayout.GetLength(0), level.LevelLayout.GetLength(1)];
            for (int i = 0; i < level.LevelLayout.GetLength(0); i++)
            {
                for (int j = 0; j < level.LevelLayout.GetLength(1); j++)
                {
                    if (level.LevelLayout[i, j] != 0)
                    {
                        tiles[i, j] = new Tile(Level.tiles[level.LevelLayout[i, j]], new Vector2(j * tilesize + 35, i * tilesize + 35));
                        GameManager.GM.objects.Add(tiles[i, j]);
                    }
                }
            }

            foreach (Object o in level.LevelEntities)
            {
                GameManager.GM.objects.Add(o);
                if(o.GetType().IsAssignableFrom(typeof(PhysicsObject)) || o.GetType().IsSubclassOf(typeof(PhysicsObject))){
                    PhysicsObject p = (PhysicsObject)o;
                    GameManager.GM.objects.Add(p);
                    Physics.Phy.ToAdd.Add(p);
                }
            }
            for(int i = 0; i < level.LevelColliders.Length; i++)
            {
                Physics.Phy.physicsObjects.Add(level.LevelColliders[i].guid, level.LevelColliders[i]);
            }

            GameManager.GM.player = level.player;
            GameManager.GM.objects.Add(level.player);
            Physics.Phy.ToAdd.Add(level.player);
        }

        public static int GetClosestNav(Point p, LevelInfo level)
        {
            int c = 0;
            float dist = 10000;
            foreach(KeyValuePair<int, NavTile> t in level.nav)
            {
                if(Math.Abs(p.X - t.Value.pos.X) + Math.Abs(p.Y - t.Value.pos.Y) < dist)
                {
                    dist = Math.Abs(p.X - t.Value.pos.X) + Math.Abs(p.Y - t.Value.pos.Y);
                    c = t.Key;
                }
            }

            return c;
        }
    }

    //This is the same as a tile except its a physics object 
    public class PhysicsTile : Rect
    {
        public PhysicsTile(string name, Vector2 pos, int width, int height) : base(name, pos, false, width, height)
        {
            falls = false;
            this.width = width;
            this.height = height;
            aabb.Width = this.width;
            aabb.Height = this.height;
        }
    }

    public class Tile : Object
    {
        public Tile(string name, Vector2 pos):base(name, pos, name)
        {
            pos = new Vector2(pos.X + tex.Width / 2, pos.X + tex.Height / 2);
        }
    }

    public class NavTile
    {
        public Point pos;
        public float heuristic = 0;
        public float movment = 10;
        public int parent = -1;
        public List<int> children = new List<int>();

        public NavTile(int x, int y)
        {
            pos = new Point(x, y);
        }

    }

    //This is used for moving platforms.
    public class MovingTile : PhysicsTile
    {
        public Point[] moveTo;
        public int lastPoint;
        public int nextPoint = 0;
        public MovingTile(string name, Vector2 pos, int width, int height, Point[] move) : base(name, pos, width, height)
        {
            this.moveTo = move;
            tex = GameManager.GM.cm.Load<Texture2D>("moving");
        }

        public override void Update()
        {
            if (!GameManager.GM.objects.Contains(this))
            {
                GameManager.GM.objects.Add(this);
            }
            Rectangle area = new Rectangle(moveTo[nextPoint] - new Point(1, 1), new Point(2, 2));
            if (pos.X >= area.Left && pos.X < area.Right && pos.Y <= area.Bottom && pos.Y > area.Top)
            {
                lastPoint = nextPoint;
                if (nextPoint == moveTo.Length - 1)
                {
                    nextPoint = 0;
                }
                else
                {
                    nextPoint++;
                }
            }
            Vector2 target = new Vector2(moveTo[nextPoint].X, moveTo[nextPoint].Y) - pos;
            target.Normalize();
            pos += target;
            aabb.Location = new Point((int)pos.X - aabb.Width / 2, (int)pos.Y - aabb.Height / 2);
        }
    }
}
