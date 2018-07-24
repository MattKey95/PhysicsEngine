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
    public class GameManager
    {
        private static GameManager gm;
        public SpriteBatch sb;
        public List<Object> objects = new List<Object>();
        public ContentManager cm;
        public GraphicsDevice gd;
        public Player player;
        public List<NavTile> navMap = new List<NavTile>();
        public float gameTime = 0;
        public int GlobalUpdateTimer = 20; //time in milliseconds that the game will refresh at ... 50 time per second
        public Random rand = new Random();
        public string currentFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public Level.LevelInfo currentLevel;
        public int score = 0;

        private GameManager()
        {
            
        }

        public static GameManager GM
        {
            get
            {
                if (gm == null)
                {
                    gm = new GameManager();
                }
                return gm;
            }
            
        }

        public void Delete(Object o)
        {
            if (objects.Contains(o))
            {
                objects.Remove(o);
            }
            if((o.GetType().IsAssignableFrom(typeof(PhysicsObject)) || o.GetType().IsSubclassOf(typeof(PhysicsObject))))
            {
                PhysicsObject p = (PhysicsObject)o;
                if (p.phySysGuid != new Guid())
                {
                    Physics.Phy.physicsSystems[p.phySysGuid].phyObjects.Remove(p);
                }
                Physics.Phy.physicsObjects.Remove(p.guid);
            }
            o = null;
        }

        public void Empty()
        {
            objects.Clear();
            Physics.Phy.ToAdd.Clear();
            Physics.Phy.ToRemove.Clear();
            foreach(KeyValuePair<Guid, PhysicsSystem> ps in Physics.Phy.physicsSystems)
            {
                ps.Value.phyObjects.Clear();
            }
            Physics.Phy.physicsSystems.Clear();
        }
    }
}
