using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{

    public class Physics
    {
        private static Physics phy;
        public float elapsedTime = 0;
        public Dictionary<Guid, PhysicsObject> physicsObjects = new Dictionary<Guid, PhysicsObject>();
        public Dictionary<Guid, PhysicsSystem> physicsSystems = new Dictionary<Guid, PhysicsSystem>();
        public Dictionary<Guid, Guid> IDs = new Dictionary<Guid, Guid>(); // <Physics object id, Physics system id>
        public List<PhysicsObject> ToAdd = new List<PhysicsObject>(); //these are used so that we can keep trakc of what need to be removed and added to the physics systems after the current loop.
        public List<PhysicsObject> ToRemove = new List<PhysicsObject>();

        public static Physics Phy
        {
            get
            {
                if (phy == null)
                {
                    phy = new Physics();
                }
                return phy;
            }
        }

        public void PhysicsUpdate()
        {
            foreach(KeyValuePair<Guid, PhysicsObject> o in physicsObjects)
            {
                FindPhysicsSystem(o.Value);
            }
            foreach(KeyValuePair<Guid, PhysicsSystem> p in physicsSystems)
            {
                p.Value.elapsedTime += elapsedTime/p.Value.time;
                p.Value.PhysicsUpdate();
            }

            List<Guid> tested = new List<Guid>();
            foreach(KeyValuePair<Guid, PhysicsObject> p1 in physicsObjects)
            {
                tested.Add(p1.Key);
                foreach (KeyValuePair<Guid, PhysicsObject> p2 in physicsObjects)
                {
                    if (!tested.Contains(p2.Key))
                    {
                        if (AABB(p1.Value, p2.Value))
                        {
                            //Console.WriteLine("AABB Collision!");
                            if (SAT(p1.Value, p2.Value))
                            {
                                //Console.WriteLine("SAT Collision!");
                                p1.Value.OnCollision(p2.Value);
                                p2.Value.OnCollision(p1.Value);
                                if ((p1.Value.resolve && p2.Value.resolve) && p1.Value.ShouldCollide(p2.Value) && p2.Value.ShouldCollide(p1.Value))
                                {
                                    Resolve(p1.Value, p2.Value);
                                    p1.Value.vel += p1.Value.acc;
                                    p1.Value.pos += p1.Value.vel;
                                }

                            }
                        }
                    }
                }                
            }
            RemoveObjects();
            AddObjects();
        }

        //Simple collision detection
        public bool AABB(PhysicsObject p1, PhysicsObject p2)
        {
            //If the object aren't going to be resolved, why check?
            if (!p1.movable && !p2.movable)
            {
                return false;
            }
            //rect rect collision
            if ((p1.GetType().IsAssignableFrom(typeof(Rect)) || p1.GetType().IsSubclassOf(typeof(Rect))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
            {
                Rect r1 = (Rect)p1;
                Rect r2 = (Rect)p2;
                if (r1.aabb.Center.X + r1.aabb.Width / 2 < r2.aabb.Center.X - r2.aabb.Width / 2 || r1.aabb.Center.X - r1.aabb.Width / 2 > r2.aabb.Center.X + r2.aabb.Width / 2)
                {
                    return false;
                }
                if (r1.aabb.Center.Y - r1.aabb.Height / 2 > r2.aabb.Center.Y + r2.aabb.Height / 2 || r1.aabb.Center.Y + r1.aabb.Height / 2 < r2.aabb.Center.Y - r2.aabb.Height / 2)
                {
                    return false;
                }
                return true;

            }
            //circle circle collision
            else if ((p1.GetType().IsAssignableFrom(typeof(Circle)) || p1.GetType().IsSubclassOf(typeof(Circle))) && (p2.GetType().IsAssignableFrom(typeof(Circle)) || p2.GetType().IsSubclassOf(typeof(Circle))))
            {
                //circle circle
                Circle c1 = (Circle)p1;
                Circle c2 = (Circle)p2;
                if (Math.Abs((c2.pos - c1.pos).Length()) >= c1.radius + c2.radius)
                {
                    return false;
                }
                return true;
            }
            //must be circle rect collision
            else
            {
                //https://yal.cc/rectangle-circle-intersection-test/
                //rect circle
                Rect r;
                Circle c;
                if (p1 is Circle)
                {
                    c = (Circle)p1;
                    r = (Rect)p2;
                }
                else
                {
                    c = (Circle)p2;
                    r = (Rect)p1;
                }

                float deltax = c.pos.X - Math.Max(r.pos.X - r.aabb.Width / 2, Math.Min(c.pos.X, r.pos.X + r.aabb.Width / 2));
                float deltay = c.pos.Y - Math.Max(r.pos.Y - r.aabb.Height / 2, Math.Min(c.pos.Y, r.pos.Y + r.aabb.Height / 2));
                if ((deltax * deltax + deltay * deltay) < (c.radius * c.radius))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        //https://gamedevelopment.tutsplus.com/tutorials/collision-detection-using-the-separating-axis-theorem--gamedev-169
        public bool SAT(PhysicsObject p1, PhysicsObject p2)
        {
            if (!p1.movable && !p2.movable)
            {
                return false;
            }
            if ((p1.GetType().IsAssignableFrom(typeof(Rect)) || p1.GetType().IsSubclassOf(typeof(Rect))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
            {
                //make sure both objects have up to date points.
                p1.GetPoints();
                p2.GetPoints();
                List<Vector2> axis = new List<Vector2>();
                List<Vector2> OriginVector1 = new List<Vector2>();
                List<Vector2> OriginVector2 = new List<Vector2>();
                //get the normal vector for each edge of the shape and the vector from 0,0 to each point.
                for (int i = 0; i < p1.edges.Length; i++)
                {
                    //don't add duplicate axis
                    Vector2 t = p1.GetAxis(p1.edges[i]);
                    if (!axis.Contains(t) || !axis.Contains(-t))
                    {
                        axis.Add(t);
                    }
                    Vector2 v = new Vector2(p1.points[i].X, p1.points[i].Y);
                    OriginVector1.Add(p1.GetPointAfterRotation(v));
                }
                //do the same for shape two
                for (int i = 0; i < p2.edges.Length; i++)
                {
                    axis.Add(p1.GetAxis(p2.edges[i]));
                    Vector2 v = new Vector2(p2.points[i].X, p2.points[i].Y);
                    OriginVector2.Add(p2.GetPointAfterRotation(v));
                }
                //get max point for both shape on each axis
                for (int j = 0; j < axis.Count; j++)
                {
                    float shape1min = Vector2.Dot(OriginVector1[0], axis[j]);
                    float shape1max = shape1min;
                    for (int i = 0; i < OriginVector1.Count; i++)
                    {
                        //dot product of origin vectors with the axis finds the max and min points
                        float current = Vector2.Dot(OriginVector1[i], axis[j]);
                        if (current < shape1min)
                        {
                            shape1min = current;
                        }
                        if (current > shape1max)
                        {
                            shape1max = current;
                        }
                    }

                    float shape2min = Vector2.Dot(OriginVector2[0], axis[j]);
                    float shape2max = shape2min;
                    for (int i = 0; i < OriginVector2.Count; i++)
                    {
                        float current = Vector2.Dot(OriginVector2[i], axis[j]);
                        if (current < shape2min)
                        {
                            shape2min = current;
                        }
                        if (current > shape2max)
                        {
                            shape2max = current;
                        }
                    }

                    //if there is a gap there is no collision so we can cut the loop short
                    if (shape1max < shape2min || shape1min > shape2max)
                    {
                        return false;
                    }
                }
                return true;
            }
            //no objects use circles for aabb only this far so if two circles collide they must be resolved.
            else if ((p1.GetType().IsAssignableFrom(typeof(Circle)) || p1.GetType().IsSubclassOf(typeof(Circle))) && (p2.GetType().IsAssignableFrom(typeof(Circle)) || p2.GetType().IsSubclassOf(typeof(Circle))))
            {
                return true;
            }
            else
            {
                // must be rect circle;
                Rect r;
                Circle c;
                if (p1.GetType().IsAssignableFrom(typeof(Circle)) || p1.GetType().IsSubclassOf(typeof(Circle)))
                {
                    c = (Circle)p1;
                    r = (Rect)p2;
                }
                else
                {
                    c = (Circle)p2;
                    r = (Rect)p1;
                }

                r.GetPoints();
                Vector2 axis = new Vector2(c.pos.X - r.pos.X, c.pos.Y - r.pos.Y);
                Vector2 unityAxis = axis;
                unityAxis.Normalize();
                Vector2[] tpoints = new Vector2[r.points.Length];
                for (int i = 0; i < r.points.Length; i++)
                {
                    tpoints[i] = r.GetPointAfterRotation(new Vector2(r.points[i].X, r.points[i].Y));
                }

                float max = -10000;
                for (int i = 0; i < tpoints.Length; i++)
                {
                    Vector2 corner = new Vector2(tpoints[i].X - r.pos.X, tpoints[i].Y - r.pos.Y);
                    float current = Vector2.Dot(corner, unityAxis);
                    if (max < current)
                    {
                        max = current;
                    }
                }

                if (axis.Length() - max - c.radius > 0 && axis.Length() > 0)
                {
                    return false;
                }

                return true;
            }
        }

        //http://uicvgame.ui.ac.ir/Mathematics%20and%20Physics/Kodicek%20D.,%20Flynt%20J.%20P.,%20Mathematics%20and%20Physics%20for%20Programmers,%202nd%20Edition,%202012.pdf
        //https://gamedev.stackexchange.com/questions/48587/resolving-a-collision-with-forces 
        public void Resolve(PhysicsObject p1, PhysicsObject p2)
        {
            p1.OnCollision();
            p2.OnCollision();
            int elasticity;
            if (p1.phySysGuid != new Guid())
            {
                elasticity = Physics.Phy.physicsSystems[p1.phySysGuid].elasticity;
            }
            else
            {
                elasticity = 2;
            }
            

            Vector2 pen = new Vector2();
            if (!p1.movable && !p2.movable)
            {
                return;
            }
            else if (p1.movable && p2.movable)
            {
                //Find penetration distance and try to move the objects apart
                //only have inelstic conservation and it can move the objects the wrong way sometimes.
                /*if (elasticity == 1) //elastic
                {

                }
                else if (elasticity == 2) //inelastic
                {*/
                    Vector2 finalVel = (p1.mass * p1.vel + p2.mass * p2.vel) / (p1.mass + p2.mass);
                    p1.vel = finalVel;
                    p2.vel = finalVel;
                    finalVel.Normalize();
                    if ((p1.GetType().IsAssignableFrom(typeof(Rect)) || p1.GetType().IsSubclassOf(typeof(Rect))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
                    {
                        Rect r1 = (Rect)p1;
                        Rect r2 = (Rect)p2;
                        pen = new Vector2(r2.aabb.Width / 2 + r1.aabb.Width / 2 - Math.Abs(r2.pos.X - r1.pos.X), r2.aabb.Height / 2 + r1.aabb.Height / 2 - Math.Abs(r2.pos.Y - r1.pos.Y));

                    }
                    else if ((p1.GetType().IsAssignableFrom(typeof(Circle)) || p1.GetType().IsSubclassOf(typeof(Circle))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
                    {
                        Circle c = (Circle)p1;
                        Rect r = (Rect)p2;
                        pen = new Vector2(r.aabb.Width / 2 + c.radius - Math.Abs(r.pos.X - c.pos.X), r.aabb.Height / 2 + c.radius - Math.Abs(r.pos.Y - c.pos.Y));
                    }
                    if (p1.mass > p2.mass)
                    {
                        p2.pos += -finalVel * pen + new Vector2(1, 1);
                    }
                    else if (p1.mass < p2.mass)
                    {
                        p2.pos += -finalVel * pen + new Vector2(1, 1);

                    }
                    else
                    {
                        p2.pos += (-finalVel * pen) * 0.5f + new Vector2(1, 1);
                        p1.pos += (finalVel * pen) * 0.5f + new Vector2(1, 1);
                    }
                /*}
                else if (elasticity == 3) //bounce
                {

                }*/
            }
            //if only one of the objects is moveable this means that we must move the movable object out by the penetration distance.
            //if the physics system has an elasticity value it will change how must energy is lost.
            else if ((p1.movable && !p2.movable) || (!p1.movable && p2.movable))
            {
                if (!p1.movable)
                {
                    PhysicsObject temp = p1;
                    p1 = p2;
                    p2 = temp;
                }
                if ((p1.GetType().IsAssignableFrom(typeof(Rect)) || p1.GetType().IsSubclassOf(typeof(Rect))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
                {
                    Rect r1 = (Rect)p1;
                    Rect r2 = (Rect)p2;
                    pen = new Vector2(r2.aabb.Width / 2 + r1.aabb.Width / 2 - Math.Abs(r2.pos.X - r1.pos.X), r2.aabb.Height / 2 + r1.aabb.Height / 2 - Math.Abs(r2.pos.Y - r1.pos.Y));
                }
                else if ((p1.GetType().IsAssignableFrom(typeof(Circle)) || p1.GetType().IsSubclassOf(typeof(Circle))) && (p2.GetType().IsAssignableFrom(typeof(Rect)) || p2.GetType().IsSubclassOf(typeof(Rect))))
                {
                    Circle c = (Circle)p1;
                    Rect r = (Rect)p2;
                    pen = new Vector2(r.aabb.Width / 2 + c.radius - Math.Abs(r.pos.X - c.pos.X), r.aabb.Height / 2 + c.radius - Math.Abs(r.pos.Y - c.pos.Y));
                }
                if (pen.X < pen.Y)
                {
                    if (p2.pos.X - p1.pos.X > 0)
                    {
                        p1.pos.X -= pen.X + 1;
                        //ApplyForce(p1, -pen.X);

                    }
                    else
                    {
                        p1.pos.X += pen.X + 1;
                        //ApplyForce(p1, new Vector2(p1.vel.X * p1.mass * elapsedTime / 10, 0));

                    }

                    if (elasticity == 1) //lose no energy
                    {
                        p1.vel.X = -p1.vel.X;
                    }
                    else if (elasticity == 2) //lose some energy
                    {
                        p1.vel.X = 0;
                    }
                    else if (elasticity == 3) //use the bounce value of the object to find out how much engery is lost.
                    {
                        p1.vel.X = -p1.vel.X * p1.bounce;
                    }
                }
                else
                {
                    if (p2.pos.Y - p1.pos.Y > 0)
                    {
                        p1.pos.Y -= pen.Y;
                        //ApplyForce(p1, new Vector2(0, -(p1.vel.Y/elapsedTime)*p1.mass));

                    }
                    else
                    {
                        p1.pos.Y += pen.Y + 1;
                        //ApplyForce(p1, new Vector2(0, p1.vel.Y * p1.mass * elapsedTime));


                    }

                    if (elasticity == 1)
                    {
                        p1.vel.Y = -p1.vel.Y;
                    }
                    else if (elasticity == 2)
                    {
                        p1.vel.Y = 0;
                    }
                    else if (elasticity == 3)
                    {
                        p1.vel.Y = -p1.vel.Y * p1.bounce;
                    }
                }
            }
        }

        //not very important just put an object into the right list for physics and rendering.
        public void CreateObject(PhysicsObject o)
        {
            physicsObjects.Add(o.guid, o);
            if (!FindPhysicsSystem(o))
            {
                GameManager.GM.Delete(o);
            }
        }

        //Finds which physics system the object is in from the position
        public bool FindPhysicsSystem(PhysicsObject o)
        {
            IDs.Remove(o.guid);
            foreach (KeyValuePair<Guid, PhysicsSystem> ps in physicsSystems)
            {
                if (ps.Value.phyObjects.Contains(o))
                {
                    ps.Value.phyObjects.Remove(o);
                }
                if (o.pos.X >= ps.Value.area.Left && o.pos.X < ps.Value.area.Right && o.pos.Y <= ps.Value.area.Bottom && o.pos.Y > ps.Value.area.Top)
                {
                    o.phySysGuid = ps.Value.guid;
                    IDs.Add(o.guid, ps.Value.guid);
                    ps.Value.phyObjects.Add(o);
                }
            }
            return true;
        }

        //Adds object to physic list after physic loop, mostly used to spawn bullets after the player has fired
        public void AddObjects()
        {
            foreach(PhysicsObject o in ToAdd)
            {
                CreateObject(o);
            }
            ToAdd.Clear();
        }

        //Deletes object, used for collision and offscreen objects.
        public void RemoveObjects()
        {
            foreach(PhysicsObject o in ToRemove)
            {
                GameManager.GM.Delete(o);
            }
        }
    }

    

    public class PhysicsSystem
    {

        //private static PhysicsSystem phySys;
        public Vector2 gravity { set; get; }
        public List<PhysicsObject> phyObjects = new List<PhysicsObject>();
        public int col = 0;
        public int elasticity = 2; //1 = elastic, 2 = inelastic, 3 = use objects bounciness 
        public float time = 1f;
        public float elapsedTime = 0;
        public Guid guid = Guid.NewGuid();
        public Rectangle area;

        public PhysicsSystem(Rectangle area, float gravity, float time, int elasticity)
        {
            this.area = area;
            this.gravity = new Vector2(0, gravity/GameManager.GM.GlobalUpdateTimer);
            this.time = time;
            this.elasticity = elasticity;
        }

        public void ApplyForce(PhysicsObject o, Vector2 force)
        {
            o.acc += force;
        }

        public void PhysicsUpdate()
        {
            //Keep a list of all the tested objects
            List<PhysicsObject> tested = new List<PhysicsObject>();
            PhysicsObject[,] collisions = new PhysicsObject[phyObjects.Count, phyObjects.Count];
            //update foraces and positions of objects
            foreach (PhysicsObject p1 in phyObjects)
            {
                //tested.Add(p1);
                if (p1.movable)
                {
                    if (p1.falls)
                    {
                        ApplyForce(p1, gravity);
                    }
                    p1.vel += p1.acc * time;
                    ApplyForce(p1, 0.3f*(gravity * p1.mass));
                    p1.pos += p1.vel*time;
                    p1.acc *= 0;
                    if(p1.GetType().IsAssignableFrom(typeof(Rect)) || p1.GetType().IsSubclassOf(typeof(Rect))){
                        Rect r = (Rect)p1;
                        r.aabb.Location = new Point((int)r.pos.X - r.aabb.Width / 2, (int)r.pos.Y - r.aabb.Height / 2);
                    }
                }
                
            }
            elapsedTime = 0;
        }
    }
}
