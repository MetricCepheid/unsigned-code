using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationEditor
{
    public class AnimationInfo
    {
        public class AnimationValue
        {
            public AnimationInfo Owner;
            public String Name;
            private float _value;
            public float Value
            {
                get { return _value; }
                set
                {
                    _value = value;
                    ValueChanged();
                }
            }

            public AnimationValue(String name, float val)
            {
                Name = name;
                Value = val;
            }

            private void ValueChanged()
            {
                if (Owner != null)
                    Owner.Dirty = true;
            }

            public FloatReference GetReferenceValue()
            {
                unsafe
                {
                    fixed (float* f = &_value)
                    {
                        FloatReference fr = new FloatReference(f);
                        fr.SetValueDelegate = new VoidDelegate(ValueChanged);
                        return fr;
                    }
                }
            }
        }

        public bool Dirty;

        private List<AnimationValue> animValues;

        public int Count { get { return animValues.Count; } }

        public int ModifyIndex;

        public AnimationValue this[int index] { get { return animValues[index]; } }

        public AnimationValue this[String index] 
        { 
            get 
            { 
                for(int i=0;i<animValues.Count;i++)
                    if(animValues[i].Name == index)
                        return animValues[i];
                return null;
            } 
        }

        public AnimationInfo()
        {
            animValues = new List<AnimationValue>();
            Dirty = false;
            ModifyIndex = -1;
        }

        public void Add(AnimationValue val)
        {
            val.Owner = this;
            animValues.Add(val);
            Dirty = true;
        }

        public void Remove(AnimationValue val)
        {
            animValues.Remove(val);
        }

        public void RemoveAt(int index)
        {
            animValues.RemoveAt(index);
        }
    }
}
