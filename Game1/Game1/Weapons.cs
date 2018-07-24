using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    //base class
    public class Gun
    {
        public int projectilCount = 1;
        public float projectileSpeed = 3;
        public float fireSpeed = 0.5f;
        public float fire = 0;
        public int damage = 1;
        public float projectileSize = 2;


        public Gun()
        {

        }

        public virtual void Fire()
        {

        }

        public virtual void OnCollision()
        {
            
        }
    }

    public class Shotgun : Gun
    {
        public int ammo = 20;
        public Shotgun()
        {
            projectilCount = 5; //how many bullets it fires per click
            projectileSpeed = 8; //how fast do the bullets go?
            damage = 40; //how much damage do they deal
        }

        public override void Fire()
        {
            //if you have no ammo left, give the player a pistol
            if (ammo == 0)
            {
                GameManager.GM.player.weapon = new Pistol();
            }
            //check is the cooldown is back up
            else if (fire > fireSpeed)
            {
                //spawn the bullets
                fire = 0;
                MouseState ms = Mouse.GetState();
                Point p = ms.Position;
                float radians = -0.52f;
                for (int i = 0; i < projectilCount; i++)
                {
                    Bullet bullet = new Bullet("Bullet", GameManager.GM.player.pos);
                    bullet.vel = new Vector2(p.X - GameManager.GM.player.pos.X, p.Y - GameManager.GM.player.pos.Y);
                    Vector2 transformed = Vector2.Transform(bullet.vel, Matrix.CreateRotationZ(radians));
                    bullet.vel = transformed;
                    bullet.vel.Normalize();
                    bullet.vel *= projectileSpeed;
                    radians += 0.26f;
                }
                //take one away from ammo
                ammo--;
            }
        }
    }

    public class MachineGun : Gun
    {
        public int ammo = 40;
        public MachineGun()
        {
            fireSpeed = 0.1f;
            projectilCount = 1;
            projectileSpeed = 9;
            damage = 35;
        }

        public override void Fire()
        {
            if(ammo == 0)
            {
                GameManager.GM.player.weapon = new Pistol();
            }
            else if (fire > fireSpeed)
            {
                fire = 0;
                MouseState ms = Mouse.GetState();
                Point p = ms.Position;
                Bullet bullet = new Bullet("Bullet", GameManager.GM.player.pos);
                bullet.vel = new Vector2(p.X - GameManager.GM.player.pos.X, p.Y - GameManager.GM.player.pos.Y);
                bullet.vel.Normalize();
                bullet.vel *= projectileSpeed;
                ammo -= 1;
            }
        }
    }

    public class Pistol : Gun
    {
        public Pistol()
        {
            fireSpeed = 0.5f;
            projectilCount = 1;
            projectileSpeed = 5;
            damage = 50;
        }

        public override void Fire()
        {
            if (fire > fireSpeed)
            {
                fire = 0;
                MouseState ms = Mouse.GetState();
                Point p = ms.Position;
                Bullet bullet = new Bullet("Bullet", GameManager.GM.player.pos);
                bullet.vel = new Vector2(p.X - GameManager.GM.player.pos.X, p.Y - GameManager.GM.player.pos.Y);
                bullet.vel.Normalize();
                bullet.vel *= projectileSpeed;
            }
        }
    }


    public class Bullet : Circle
    {
        public int damage = 50;
        public Bullet(string name, Vector2 pos) : base(name, pos, true, 2, "bullet")
        {
            this.falls = false;
            this.vel = vel;
            this.resolve = false;
        }

        //if a bullet collides it either has to damage an enemy or be destroyed depending on what it hits.
        public override void OnCollision(PhysicsObject col)
        {
            if (!(col is Player) && !(col is Bullet) && GameManager.GM.player != null)
            {
                if (col is Flier)
                {
                    Flier ai = (Flier)col;
                    ai.health -= GameManager.GM.player.weapon.damage;
                    if(ai.health <= 0)
                    {
                        ai.Die();
                    }
                    Physics.Phy.ToRemove.Add(this);
                }
                else if (col is Walker)
                {
                    Walker ai = (Walker)col;
                    ai.health -= GameManager.GM.player.weapon.damage;
                    if (ai.health <= 0)
                    {
                        ai.Die();
                    }
                    Physics.Phy.ToRemove.Add(this);
                }
                if (col is PhysicsTile && !(col is Ladder))
                {
                    GameManager.GM.player.weapon.OnCollision();
                    Physics.Phy.ToRemove.Add(this);
                }
            }
        }

        public override bool ShouldCollide(PhysicsObject p)
        {
            if(p is Flier || p is Walker)
            {
                return true;
            }
            return false;
        }
    }

    //Create a drop which if the player collides with, will change the gun the player has.
    public class Drop : Rect
    {
        Gun gun;
        float life = 0;
        float lifeSpan = 10;
        public Drop(Vector2 pos) : base("Drop", pos, true, 20, 20)
        {
            falls = true;
            switch (GameManager.GM.rand.Next(0, 3))
            {
                case 0:
                    gun = new Pistol();
                    tex = GameManager.GM.cm.Load<Texture2D>("pistol");
                    break;
                case 1:
                    gun = new Shotgun();
                    tex = GameManager.GM.cm.Load<Texture2D>("shotgun");
                    break;
                case 2:
                    gun = new MachineGun();
                    tex = GameManager.GM.cm.Load<Texture2D>("machinegun");
                    break;
            }
            GameManager.GM.objects.Add(this);
        }

        public override void OnCollision(PhysicsObject col)
        {
            if(col is Player)
            {
                GameManager.GM.player.weapon = gun;
                Physics.Phy.ToRemove.Add(this);
            }
        }

        public override void Update()
        {
            life += Physics.Phy.elapsedTime;
            if(life > lifeSpan)
            {
                Physics.Phy.ToRemove.Add(this);
            }
        }

        public override bool ShouldCollide(PhysicsObject p)
        {
            if(p is Drop || p is Flier || p is Walker)
            {
                return false;
            }
            return true;
        }
    }

}
