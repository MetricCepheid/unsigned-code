using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace UnsignedAnimationEditor
{
    public class Animation
    {
        public String Name;
        private List<Frame> Frames;
        public float Length;
        private int NumMatrices;

        public Animation(String name, int numMatrices)
        {
            Name = name;
            Frames = new List<Frame>();
            Frames.Add(new Frame(0,numMatrices));
            NumMatrices = numMatrices;
        }

        public int KeyframeCount { get { return Frames.Count; } }

        internal Matrix GetJointMatrix(int animViewPos, int jointIndex)
        {
            if (Frames.Count <= 0)
                return Matrix.Identity;
            if (Frames.Count == 1)
                return Frames[0].Matrices[jointIndex];
            if (animViewPos < Frames[0].Time)
                return Frames[0].Matrices[jointIndex];
            if (animViewPos > Frames[Frames.Count-1].Time)
                return Frames[Frames.Count - 1].Matrices[jointIndex];
            for (int i = 0; i < Frames.Count; i++)
                if (Frames[i].Time == animViewPos)
                    return Frames[i].Matrices[jointIndex];
            for (int i = 0; i < Frames.Count - 1; i++)
            {
                if (animViewPos > Frames[i].Time && animViewPos < Frames[i + 1].Time)
                {
                    float lerp = (animViewPos - Frames[i].Time) / (float)(Frames[i + 1].Time - Frames[i].Time);
                    return Matrix.Lerp(Frames[i].Matrices[jointIndex], Frames[i + 1].Matrices[jointIndex], lerp);
                }
            }
            throw new Exception();
        }

        public override string ToString()
        {
            return Name;
        }

        internal Frame Keyframe(int i)
        {
            return Frames[i];
        }

        internal void AddKeyframe(int time)
        {
            Frames.Add(new Frame(time, NumMatrices));
            Frames.Sort();
        }
    }

    public class Frame : IComparable<Frame>
    {
        public int Time; // 0-100 (for rounding errors)
        public Matrix[] Matrices;

        public Frame(int time, int numMatrices)
        {
            Time = time;
            Matrices = new Matrix[numMatrices];
            for (int i = 0; i < numMatrices; i++)
                Matrices[i] = Matrix.Identity;
        }

        public int CompareTo(Frame other)
        {
            return Time.CompareTo(other.Time);
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

    public class Skeleton
    {
        public Joint RootJoint;

        public Skeleton()
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

        public int GetMatrixLength()
        {
            return GetMatrixLengthHelper(RootJoint)+1;
        }

        private int GetMatrixLengthHelper(Joint j)
        {
            int r = j.JointMatrix;
            for (int i = 0; i < j.Children.Count; i++)
                r = Math.Max(r, GetMatrixLengthHelper(j.Children[i]));
            return r;
        }

        public String[] GetMatrixNames()
        {
            String[] ret = new String[GetMatrixLength()];
            GetMatrixNamesHelper(RootJoint, ret);
            return ret;
        }

        private void GetMatrixNamesHelper(Joint j, String[] strs)
        {
            strs[j.JointMatrix] = j.Name;
            for (int i = 0; i < j.Children.Count; i++)
                GetMatrixNamesHelper(j.Children[i], strs);
        }
    }

    public class AnimationInfo
    {
        public Skeleton Skeleton;
        public List<Animation> Animations;
        public int AnimationIndex;
        public int CurrentAnimationTimeValue;
        public int SelectedJointIndex;

        public Animation CurrentAnimation { get { return AnimationIndex<0 || AnimationIndex>=Animations.Count ? null : Animations[AnimationIndex]; } }

        public Frame CurrentKeyframe
        {
            get
            {
                Animation ca = CurrentAnimation;
                if (ca != null)
                {
                    for (int i = 0; i < ca.KeyframeCount; i++)
                    {
                        if (ca.Keyframe(i).Time == CurrentAnimationTimeValue)
                        {
                            return ca.Keyframe(i);
                        }
                    }
                }
                return null;
            }
        }

        public AnimationInfo()
        {
            Animations = new List<Animation>();
            AnimationIndex = -1;
            CurrentAnimationTimeValue = 0;
        }
    }
}
