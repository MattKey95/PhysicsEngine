using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This file hold GUI items, currently only buttons.
namespace Game1
{
    public class GUIManager
    {

        public enum GameState
        {
            menu, play
        }

        private static GUIManager guiManager;
        public List<GUI> guiItems = new List<GUI>();
        public int state = (int)GameState.menu;


        private GUIManager()
        {

        }

        public static GUIManager GuiManager
        {
            get
            {
                if(guiManager == null)
                {
                    guiManager = new GUIManager();
                }
                return guiManager;
            }
        }

        public void OnClick(Point pos)
        {
            foreach(Button b in guiItems)
            {
                if (b.area.Contains(pos))
                {
                    GameManager.GM.currentLevel = b.level;
                    GuiManager.state = (int)GUIManager.GameState.play;
                    Level.LoadLevelContent(b.level);
                }
            }
        }
    }


    public class GUI
    {
        public Point pos;

        public Texture2D texture = GameManager.GM.cm.Load<Texture2D>("button");

        public GUI(Point pos)
        {
            this.pos = pos;
            GUIManager.GuiManager.guiItems.Add(this);
        }

        public virtual void OnClick()
        {

        }
    }

    public class Button : GUI
    {

        public Rectangle area;
        public string text;
        public Level.LevelInfo level;

        public Button(Rectangle area, string text, Level.LevelInfo level) : base(new Point(area.Location.X + area.Width / 2, area.Location.Y + area.Height / 2))
        {
            this.area = area;
            this.text = text;
            this.level = level;
        }

        public override void OnClick()
        {
            
        }
    }
}
