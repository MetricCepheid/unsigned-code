using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using FVProductions.Utility;

namespace AnimationEditor
{
    public class SimpleCheckBoxControl : XNAControl
    {
        public Color CheckedColor { get; set; }
        public Color UncheckedColor { get; set; }
        public Color DisabledColor { get; set; }

        public bool Checked { get; set; }

        public EventHandler CheckedChanged;

        private bool wasPressed;

        public SimpleCheckBoxControl(String name)
        {
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
