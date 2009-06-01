using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

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

    public class Joint
    {
        public String Name;
        public Vector3 preOffset, postOffset;
        public int JointMatrix;
        public List<Joint> Children;

        public Joint(String name, int matIndex)
        {
            Name = name;
            JointMatrix = matIndex;
            Children = new List<Joint>();
        }
    }

    public class AnimationMatrixInfo
    {
        public Joint RootJoint;
        public Matrix[] Matrices;

        public AnimationMatrixInfo()
        {
            
        }

        public void Load(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);

            int count = 0;

            RootJoint = new Joint(sr.ReadLine(), count);
            RootJoint.preOffset = ReadVector3Line(sr);
            RootJoint.postOffset = ReadVector3Line(sr);
            count++;

            while (!sr.EndOfStream)
            {
                Joint jt = new Joint(sr.ReadLine(), count);
                count++;
                Get(sr.ReadLine()).Children.Add(jt);
                jt.preOffset = ReadVector3Line(sr);
                jt.postOffset = ReadVector3Line(sr);
            }

            sr.Close();
        }

        private Vector3 ReadVector3Line(StreamReader sr)
        {
            String line = sr.ReadLine();
            Vector3 ret = Vector3.Zero;
            ret.X = Single.Parse(line.Substring(0, line.IndexOf(',')).Trim());
            line = line.Substring(line.IndexOf(',') + 1);
            ret.Y = Single.Parse(line.Substring(0, line.IndexOf(',')).Trim());
            line = line.Substring(line.IndexOf(',') + 1);
            ret.Z = Single.Parse(line.Trim());
            return ret;
        }

        public Joint Get(String index)
        {
            return GetHelper(RootJoint, index);
        }

        private Joint GetHelper(Joint j, String idx)
        {
            if (j.Name == idx)
                return j;
            for (int i = 0; i < j.Children.Count; i++)
            {
                Joint jj = GetHelper(j.Children[i], idx);
                if (jj != null)
                    return jj;
            }
            return null;
        }
    }
}
