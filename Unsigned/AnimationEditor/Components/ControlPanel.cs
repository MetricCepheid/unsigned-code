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
    class ControlPanel : XNAComponent
    {
        private SpriteBatch spriteBatch;

        private AnimationInfo animInfo;

        private int selectedIndex;

        private float tempVal;

        private EditorViewer _edv;
        public EditorViewer EditorViewer
        {
            get { return _edv; }
            set { _edv = value; _edv.SelectedVertChanged += new VoidDelegate(SelectedVertChanged); }
        }

        private List<XNAControl> Controls;

        public ControlPanel(String name, AnimationInfo info)
        {
            Name = name;
            selectedIndex = -1;
            animInfo = info;
            ResetControls();
        }

        public override void Load(ContentManager Content)
        {
            spriteBatch = new SpriteBatch(Global.Graphics.GraphicsDevice);
        }

        private bool wasPressed;
        public override void Update(GameTime gameTime)
        {
            MouseState st = Mouse.GetState();
            Point mousePoint = new Point(st.X - Bounds.X, st.Y - Bounds.Y);

            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Update(gameTime, mousePoint);

            if (!EditorViewer.IsVertexEditingEnabled())
            {
                if (st.LeftButton == ButtonState.Pressed && Bounds.Contains(mousePoint))
                {
                    if (animInfo.ModifyIndex >= 0 && !wasPressed)
                    {
                        for (int i = 0; i < animInfo.Count; i++)
                        {
                            Rectangle optionButton = new Rectangle(Bounds.X + Bounds.Width - 30, Bounds.Y + 10 + (30 * i), 20, 10);
                            if (optionButton.Contains(mousePoint))
                            {
                                if (animInfo.ModifyIndex == i)
                                    animInfo.ModifyIndex = -1;
                                else
                                {
                                    animInfo.ModifyIndex = i;
                                    animInfo[animInfo.ModifyIndex].Value = (int)(animInfo[animInfo.ModifyIndex].Value + 0.5f);
                                }
                            }
                        }
                    }
                    else if (selectedIndex < 0 && animInfo.ModifyIndex < 0 && !wasPressed)
                    {
                        for (int i = 0; i < animInfo.Count; i++)
                        {
                            Rectangle sliderGrabber = new Rectangle(Bounds.X + 15 + (int)((Bounds.Width - 40) * animInfo[i].Value), Bounds.Y + 25 + (30 * i), 10, 15);
                            if (sliderGrabber.Contains(mousePoint))
                            {
                                selectedIndex = i;
                                tempVal = animInfo[selectedIndex].Value;
                            }
                            else
                            {
                                Rectangle optionButton = new Rectangle(Bounds.X + Bounds.Width - 30, Bounds.Y + 10 + (30 * i), 20, 10);
                                if (optionButton.Contains(mousePoint))
                                {
                                    animInfo.ModifyIndex = i;
                                    animInfo[animInfo.ModifyIndex].Value = (int)(animInfo[animInfo.ModifyIndex].Value + 0.5f);
                                }
                            }
                        }
                    }
                    else if (animInfo.ModifyIndex < 0)
                    {
                        float left = Bounds.X + 20;
                        float right = Bounds.X + 20 + (Bounds.Width - 40);
                        float lerp = (mousePoint.X - left) / (right - left);
                        lerp = Math.Max(0, Math.Min(1, lerp));
                        tempVal = lerp;
                    }
                }
                else
                {
                    if (selectedIndex >= 0)
                    {
                        animInfo[selectedIndex].Value = tempVal;
                        selectedIndex = -1;
                    }
                }
            }
            else
            {
                if (st.LeftButton == ButtonState.Pressed && !wasPressed)
                {
                    int i = animInfo.ModifyIndex;
                    Rectangle optionButton = new Rectangle(Bounds.X + Bounds.Width - 30, Bounds.Y + 10 + (30 * i), 20, 10);
                    if (optionButton.Contains(mousePoint))
                    {
                        animInfo.ModifyIndex = -1;
                    }
                }
            }

            wasPressed = st.LeftButton == ButtonState.Pressed;
        }

        public override void InnerDraw()
        {
            Global.Graphics.GraphicsDevice.Clear(Color.LightGray);
            spriteBatch.Begin();

            for (int i = 0; i < Controls.Count; i++)
                Controls[i].Draw(spriteBatch);

            spriteBatch.End();
        }

        public override void OnResize()
        {
            ResetControls();
            base.OnResize();
        }

        private void SelectedRadioChanged(object owner, EventArgs args)
        {
            int indexOf = Controls.IndexOf((XNAControl)owner);
            if (((RadioButtonControl)owner).Checked)
                animInfo.ModifyIndex = indexOf / 3;
            else
                animInfo.ModifyIndex = -1;
            for (int i = 0; i < animInfo.Count; i++)
            {
                if (i == indexOf / 3)
                    Controls[i * 3].Enabled = true;
                else
                    Controls[i * 3].Enabled = false;
            }
            SelectedVertChanged();
        }

        private void SelectedVertChanged()
        {
            if (EditorViewer.IsVertexEditingEnabled())
            {
                Vector3 vec3 = EditorViewer.GetVertexEditingValue();

                ((SliderControl)Controls[Controls.Count - 2]).Value = vec3.Z;
                Controls[Controls.Count - 2].Enabled = true;
                ((SliderControl)Controls[Controls.Count - 4]).Value = vec3.Y;
                Controls[Controls.Count - 4].Enabled = true;
                ((SliderControl)Controls[Controls.Count - 6]).Value = vec3.X;
                Controls[Controls.Count - 6].Enabled = true;
            }
            else
            {
                ((SliderControl)Controls[Controls.Count - 2]).Value = 0;
                Controls[Controls.Count - 2].Enabled = false;
                ((SliderControl)Controls[Controls.Count - 4]).Value = 0;
                Controls[Controls.Count - 4].Enabled = false;
                ((SliderControl)Controls[Controls.Count - 6]).Value = 0;
                Controls[Controls.Count - 6].Enabled = false;
            }
        }

        private void SelectedVertexValueChanged(object owner, EventArgs e)
        {
            if (EditorViewer.IsVertexEditingEnabled())
            {
                EditorViewer.SetVertexEditingValue(new Vector3(((SliderControl)Controls[Controls.Count - 6]).Value, ((SliderControl)Controls[Controls.Count - 4]).Value, ((SliderControl)Controls[Controls.Count - 2]).Value));
            }
        }

        private void ResetControls()
        {
            Controls = new List<XNAControl>();
            RadioButtonCollection rbCol = new RadioButtonCollection();
            for (int i = 0; i < animInfo.Count; i++)
            {
                {
                    SliderControl s = new SliderControl(animInfo[i].Name + " Slider");
                    s.Bounds = new Rectangle(10, 25 + (30 * i), Bounds.Width - 20, 15);
                    s.UseReferenceValue = true;
                    s.RefValue = animInfo[i].GetReferenceValue();
                    s.Minimum = 0.0f;
                    s.Maximum = 1.0f;
                    s.AltText = "";
                    Controls.Add(s);
                }
                {
                    RadioButtonControl c = new RadioButtonControl(rbCol, animInfo[i].Name + " Edit RadioButton");
                    c.Bounds = new Rectangle(Bounds.Width - 30, 10 + (30 * i), 20, 10);
                    c.Checked = false;
                    c.CheckedColor = Color.Red;
                    c.UncheckedColor = Color.DarkRed;
                    c.AltText = "";
                    c.CheckedChanged += new EventHandler(SelectedRadioChanged);
                    Controls.Add(c);
                }
                {
                    LabelControl l = new LabelControl(animInfo[i].Name+" Label", animInfo[i].Name);
                    l.Bounds = new Rectangle(20, 10 + (30 * i), 1, 1);
                    l.AltText = "";
                    l.Scale = 0.5f;
                    Controls.Add(l);
                }
            }
            {
                int i = 15;
                {
                    SliderControl s = new SliderControl("X-Offset Slider");
                    s.Bounds = new Rectangle(10, 25 + (30 * i), Bounds.Width - 20, 15);
                    s.UseReferenceValue = false;
                    s.Value = 0;
                    s.Minimum = -1.0f;
                    s.Maximum =  1.0f;
                    s.AltText = "";
                    s.Enabled = false;
                    s.ValueChanged += new EventHandler(SelectedVertexValueChanged);
                    Controls.Add(s);
                }
                {
                    LabelControl l = new LabelControl("X-Offset Label", "X-Offset");
                    l.Bounds = new Rectangle(20, 10 + (30 * i), 1, 1);
                    l.AltText = "";
                    l.Scale = 0.5f;
                    Controls.Add(l);
                }
            }
            {
                int i = 16;
                {
                    SliderControl s = new SliderControl("Y-Offset Slider");
                    s.Bounds = new Rectangle(10, 25 + (30 * i), Bounds.Width - 20, 15);
                    s.UseReferenceValue = false;
                    s.Value = 0;
                    s.Minimum = -1.0f;
                    s.Maximum = 1.0f;
                    s.AltText = "";
                    s.Enabled = false;
                    s.ValueChanged += new EventHandler(SelectedVertexValueChanged);
                    Controls.Add(s);
                }
                {
                    LabelControl l = new LabelControl("Y-Offset Label", "Y-Offset");
                    l.Bounds = new Rectangle(20, 10 + (30 * i), 1, 1);
                    l.AltText = "";
                    l.Scale = 0.5f;
                    Controls.Add(l);
                }
            }
            {
                int i = 17;
                {
                    SliderControl s = new SliderControl("Z-Offset Slider");
                    s.Bounds = new Rectangle(10, 25 + (30 * i), Bounds.Width - 20, 15);
                    s.UseReferenceValue = false;
                    s.Value = 0;
                    s.Minimum = -1.0f;
                    s.Maximum = 1.0f;
                    s.AltText = "";
                    s.Enabled = false;
                    s.ValueChanged += new EventHandler(SelectedVertexValueChanged);
                    Controls.Add(s);
                }
                {
                    LabelControl l = new LabelControl("Z-Offset Label", "Z-Offset");
                    l.Bounds = new Rectangle(20, 10 + (30 * i), 1, 1);
                    l.AltText = "";
                    l.Scale = 0.5f;
                    Controls.Add(l);
                }
            }
        }
    }
}
