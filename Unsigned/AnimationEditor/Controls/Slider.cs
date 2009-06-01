using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using FVProductions.Utility;

namespace AnimationEditor
{
    public class SliderControl : XNAControl
    {
        private static List<int> sliderIndices = new List<int>();
        private int index;

        public float Value { get; set; }
        public FloatReference RefValue { get; set; }
        public bool UseReferenceValue { get; set; }

        public float Minimum { get; set; }
        public float Maximum { get; set; }

        private bool selected, wasPressed;

        public EventHandler ValueChanged;

        public SliderControl()
        {
            Enabled = true;
            index = 0;
            while(true)
            {
                bool indexGood = true;
                for (int i = 0; i < sliderIndices.Count; i++)
                    if (sliderIndices.Contains(index))
                        indexGood = false;
                if (indexGood)
                    break;
                else
                    index++;
            }
            sliderIndices.Add(index);
            Name = "Slider" + index;
        }

        public SliderControl(String name)
        {
            Enabled = true;
            index = 0;
            while (true)
            {
                bool indexGood = true;
                for (int i = 0; i < sliderIndices.Count; i++)
                    if (sliderIndices.Contains(index))
                        indexGood = false;
                if (indexGood)
                    break;
                else
                    index++;
            }
            sliderIndices.Add(index);
            Name = name;
        }

        ~SliderControl()
        {
            sliderIndices.Remove(index);
        }

        public override void Load(ContentManager Content)
        {

        }

        public override void Update(GameTime gameTime, Point mousePoint)
        {
            if (Enabled)
            {
                ButtonState lb = Mouse.GetState().LeftButton;
                if (!selected && lb == ButtonState.Pressed && !wasPressed)
                {
                    float val = ((UseReferenceValue ? RefValue.GetValue() : Value) - Minimum) / (Maximum - Minimum);
                    Rectangle sliderGrabber = new Rectangle(Bounds.X + 5 + (int)((Bounds.Width - 20) * val), Bounds.Y, 10, Bounds.Height);
                    if (sliderGrabber.Contains(mousePoint))
                        selected = true;
                }
                else if (selected)
                {
                    if(lb == ButtonState.Released || !Bounds.Contains(mousePoint))
                        selected = false;
                    else
                    {
                        int left = Bounds.X + 5;
                        int right = Bounds.X + 5 + (Bounds.Width - 20);
                        float val = Math.Max(0, Math.Min(1, (mousePoint.X - left) / (float)(right - left)));
                        float oldValue = UseReferenceValue ? RefValue.GetValue() : Value;
                        if (UseReferenceValue)
                            RefValue.SetValue(Minimum + (val * (Maximum - Minimum)));
                        else
                            Value = (Minimum + (val * (Maximum - Minimum)));
                        if (oldValue != (UseReferenceValue ? RefValue.GetValue() : Value))
                            if (ValueChanged != null)
                                ValueChanged.Invoke(this, new EventArgs());
                    }
                }
                
            }
            wasPressed = Mouse.GetState().LeftButton == ButtonState.Pressed;
            base.Update(gameTime, mousePoint);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            float val = ((UseReferenceValue?RefValue.GetValue():Value)-Minimum)/(Maximum-Minimum);
            spriteBatch.Draw(Global.TexWhite, new Rectangle(Bounds.X, Bounds.Y+5, Bounds.Width, Bounds.Height-10), new Color(0.3f, 0.3f, 0.3f));
            spriteBatch.Draw(Global.TexWhite, new Rectangle(Bounds.X + 5 + (int)((Bounds.Width - 20) * val), Bounds.Y, 10, Bounds.Height), Enabled ? selected ? Color.Red : Color.Black : new Color(0.3f, 0.3f, 0.3f));
            base.Draw(spriteBatch);
        }
    }
}
