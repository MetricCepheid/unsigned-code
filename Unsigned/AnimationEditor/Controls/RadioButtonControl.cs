using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace AnimationEditor
{
    public class RadioButtonControl : XNAControl
    {
        public Color CheckedColor { get; set; }
        public Color UncheckedColor { get; set; }
        public Color DisabledColor { get; set; }

        private bool _checked;
        public bool Checked 
        { 
            get { return _checked; } 
            set 
            {
                if (value)
                {
                    myCol.EnableOne(this);
                    _checked = true;
                }
                else
                    _checked = false;
            } 
        }

        public EventHandler CheckedChanged;

        private bool wasPressed;
        private RadioButtonCollection myCol;

        public RadioButtonControl(RadioButtonCollection col, String name)
        {
            myCol = col;
            myCol.Add(this);
            Name = name;
            Enabled = true;
            Checked = false;
            CheckedColor = Color.White;
            UncheckedColor = Color.Black;
            DisabledColor = Color.Gray;
            AltText = name;
        }

        public override void Load(ContentManager Content)
        {
            
        }

        public override void Update(GameTime gameTime, Point mousePoint)
        {
            if (Enabled)
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && !wasPressed)
                    if (Bounds.Contains(mousePoint))
                    {
                        Checked = !Checked;
                        if (CheckedChanged != null)
                            CheckedChanged.Invoke(this, new EventArgs());
                    }
            wasPressed = Mouse.GetState().LeftButton == ButtonState.Pressed;
            base.Update(gameTime, mousePoint);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Global.TexWhite, Bounds, !Enabled ? DisabledColor : Checked ? CheckedColor : UncheckedColor);
            base.Draw(spriteBatch);
        }
    }
}
