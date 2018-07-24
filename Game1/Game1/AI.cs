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

    public class AI
    {
        public PhysicsObject hitBox;
        public string name;
        public int health = 100;

        public AI(string name, Vector2 pos)
        {
            this.name = name;
        }
    }

    class Flier : Circle
    {
        public int health = 100;
        public float speed = 2f;

        public Flier(Vector2 pos) : base("Flier", pos, true, 10, "eye")
        {
            falls = false;
        }
        

        public override void Update()
        {
            if (GameManager.GM.player != null)
            {
                Vector2 target = new Vector2(GameManager.GM.player.pos.X - pos.X, GameManager.GM.player.pos.Y - pos.Y);
                target.Normalize();
                vel = target * speed;
            }
        }

        public void Die()
        {
            GameManager.GM.score++;
            Physics.Phy.ToRemove.Add(this);
            Random rand = new Random();
            if (rand.Next(0, 3) == 1)
            {
                new Drop(pos);
            }
        }
    }

    public class Walker : Rect
    {
        public int health = 100;
        public List<int> path = new List<int>();
        public float pathfindTime = 0;
        public float speed = 2f;
        public bool ladder = false;

        public Walker(Vector2 pos) : base("Walker", pos, true, "ghost")
        {
            falls = true;
            if (GameManager.GM.player != null)
            {
                AStar();
            }
        }

        public override void Update()
        {
            Move();
            base.Update();
            pathfindTime += GameManager.GM.gameTime;
            if (pathfindTime >= 1 && GameManager.GM.player != null)
            {
                AStar();
                pathfindTime = 0;
            }
            ladder = false;
            falls = true;
        }

        public void Die()
        {
            GameManager.GM.score++;
            Physics.Phy.ToRemove.Add(this);
            Random rand = new Random();
            if (rand.Next(0, 3) == 1)
            {
                new Drop(pos);
            }
        }

        public void Move()
        {
            //more towards the next target in the path
            if (path.Count != 0)
            {
                if (new Point((int)pos.X, (int)pos.Y) == GameManager.GM.currentLevel.nav[path[0]].pos)
                {
                    path.Remove(path[0]);
                }
            }
            if (path.Count != 0)
            {
                Vector2 target = new Vector2(GameManager.GM.currentLevel.nav[path[0]].pos.X - pos.X, GameManager.GM.currentLevel.nav[path[0]].pos.Y - pos.Y);
                target.Normalize();
                if (!ladder)
                {
                    vel.X = target.X * speed;
                }
                else
                {
                    vel = target;
                    vel.Y -= 0.5f;
                }
            }
        }

        public override bool ShouldCollide(PhysicsObject p)
        {
            if(p is Walker || p is Flier)
            {
                return false;
            }
            return true;
        }

        public override void OnCollision(PhysicsObject col)
        {
            if(col is Ladder)
            {
                ladder = true;
                falls = false;
            }
        }

        public void AStar()
        {
            Point target = new Point((int)(GameManager.GM.player.pos.X), (int)(GameManager.GM.player.pos.Y));
            Point location = new Point((int)(pos.X), (int)(pos.Y));
            int end = Level.GetClosestNav(target, GameManager.GM.currentLevel);
            int start = Level.GetClosestNav(location, GameManager.GM.currentLevel);

            if(start == end)
            {
                return;
            }

            List<int> closed = new List<int>();
            List<int> open = new List<int>();

            //compute heuristics for all nav tiles in the level. This is just the euclidean distance between the two points.
            foreach (KeyValuePair<int, NavTile> t in GameManager.GM.currentLevel.nav)
            {
                t.Value.heuristic = new Vector2(Math.Abs(GameManager.GM.currentLevel.nav[end].pos.X - t.Value.pos.X), Math.Abs(GameManager.GM.currentLevel.nav[end].pos.Y - t.Value.pos.Y)).Length();
            }

            closed.Add(start);
            foreach(int i in GameManager.GM.currentLevel.nav[start].children)
            {
                if (!closed.Contains(i))
                {
                    open.Add(i);
                    GameManager.GM.currentLevel.nav[i].parent = start;
                }
            }

            int current = start;
            int parent = start;
            while (open.Count != 0)
            {
                //find the neighbouring tile which is closest to the end tile. 
                float fValue = 10000;
                foreach(int i in open)
                {
                    if(GameManager.GM.currentLevel.nav[i].heuristic + GameManager.GM.currentLevel.nav[i].movment < fValue)
                    {
                        fValue = GameManager.GM.currentLevel.nav[i].heuristic + GameManager.GM.currentLevel.nav[i].movment;
                        current = i;
                    }
                }

                GameManager.GM.currentLevel.nav[current].movment += GameManager.GM.currentLevel.nav[parent].movment; //cumulate movment from parent making longer paths worse.
                //Console.WriteLine(GameManager.GM.currentLevel.nav[current].movment);
                if (closed.Contains(current))
                {
                    break;
                }
                parent = current;
                closed.Add(current);
                open.Remove(current);

                //if we are on the end tile get path and reverse it. 
                if (current == end)
                {
                    path.Clear();
                    int i = current;
                    while (i != start)
                    {
                        path.Add(i);
                        i = GameManager.GM.currentLevel.nav[i].parent;
                    }
                    path.Reverse();
                    break;
                }

                foreach (int i in GameManager.GM.currentLevel.nav[current].children)
                {
                    if (!closed.Contains(i))
                    {
                        if (!open.Contains(i))
                        {
                            open.Add(i);
                            GameManager.GM.currentLevel.nav[i].parent = current;
                        }
                        //check if movment from new tile is cheaper than the movment from it's current parent
                        if (GameManager.GM.currentLevel.nav[current].movment + GameManager.GM.currentLevel.nav[i].movment < GameManager.GM.currentLevel.nav[GameManager.GM.currentLevel.nav[i].parent].movment + GameManager.GM.currentLevel.nav[i].movment)
                        {
                            GameManager.GM.currentLevel.nav[i].parent = current;
                        }
                    }
                }
            }
            //Have to reset the navigation tiles moment values as c# does not support pointers.
            foreach(KeyValuePair<int, NavTile> t in GameManager.GM.currentLevel.nav)
            {
                t.Value.movment = 10;
            }
            //failed
        }
    }
}
