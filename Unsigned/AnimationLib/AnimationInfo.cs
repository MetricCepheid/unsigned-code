using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    public class Animation
    {
        public String Name;
        public List<Frame> Frames;
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

        public Matrix GetJointMatrix(int animViewPos, int jointIndex)
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

        public Vector3 GetOffset(int animViewPos)
        {
            if (Frames.Count <= 0)
                return Vector3.Zero;
            if (Frames.Count == 1)
                return Frames[0].Offset;
            if (animViewPos < Frames[0].Time)
                return Frames[0].Offset;
            if (animViewPos > Frames[Frames.Count - 1].Time)
                return Frames[Frames.Count - 1].Offset;
            for (int i = 0; i < Frames.Count; i++)
                if (Frames[i].Time == animViewPos)
                    return Frames[i].Offset;
            for (int i = 0; i < Frames.Count - 1; i++)
            {
                if (animViewPos > Frames[i].Time && animViewPos < Frames[i + 1].Time)
                {
                    float lerp = (animViewPos - Frames[i].Time) / (float)(Frames[i + 1].Time - Frames[i].Time);
                    return Vector3.Lerp(Frames[i].Offset, Frames[i + 1].Offset, lerp);
                }
            }
            throw new Exception();
        }

        public override string ToString()
        {
            return Name;
        }

        public Frame Keyframe(int i)
        {
            return Frames[i];
        }

        public void AddKeyframe(int time)
        {
            Frames.Add(new Frame(time, NumMatrices));
            Frames.Sort();
        }
    }

    public class Frame : IComparable<Frame>
    {
        public int Time; // 0-100 (for rounding errors)
        public Matrix[] Matrices;
        public Vector3 Offset;

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
        public Joint Parent;
        public String Name;
        public Vector3 preOffset, postOffset;
        public int JointMatrix;
        public List<Joint> Children;

        public Joint(Joint Parent, String name, int matIndex)
        {
            this.Parent = Parent;
            Name = name;
            JointMatrix = matIndex;
            Children = new List<Joint>();
        }

        public Joint Root
        {
            get
            {
                if (Parent == null)
                    return this;
                else
                    return Parent.Root;
            }
        }
    }

    public class Skeleton
    {
        public Joint RootJoint;
        public Joint InstrRootJoint;

        public Skeleton()
        {
            
        }

        public void Load(Stream stream)
        {
            StreamReader sr = new StreamReader(stream);

            int count = 0;

            RootJoint = new Joint(null, "Root", count);
            RootJoint.preOffset = Vector3.Zero;
            RootJoint.postOffset = Vector3.Zero;
            count++;
            InstrRootJoint = new Joint(null, "InstrRoot", count);
            InstrRootJoint.preOffset = Vector3.Zero;
            InstrRootJoint.postOffset = Vector3.Zero;
            count++;

            while (!sr.EndOfStream)
            {
                String name = sr.ReadLine();
                String parentName = sr.ReadLine();
                Joint parent = GetRoot(parentName);
                if(parent==null)
                    parent = GetInstr(parentName);
                Joint jt = new Joint(parent, name, count);
                parent.Children.Add(jt);
                jt.preOffset = ReadVector3Line(sr);
                jt.postOffset = ReadVector3Line(sr);
                count++;
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

        public Joint GetRoot(String index)
        {
            return GetHelper(RootJoint, index);
        }

        public Joint GetInstr(String index)
        {
            return GetHelper(InstrRootJoint, index);
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
            return Math.Max(GetMatrixLengthHelper(RootJoint)+1,GetMatrixLengthHelper(InstrRootJoint)+1);
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
            GetMatrixNamesHelper(InstrRootJoint, ret);
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

        public static AnimationInfo Load(String skeletonFilename, String animFilename)
        {
            AnimationInfo AnimInfo = new AnimationInfo();
            AnimInfo.Skeleton = new Skeleton();
            AnimInfo.Skeleton.Load(System.IO.File.OpenRead(skeletonFilename));
            AnimInfo.LoadAnimation(animFilename);
            return AnimInfo;
        }

        public void LoadAnimation(String animFilename)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(animFilename));
            Animations = new List<Animation>();
            uint numAnims = br.ReadUInt32();
            for (int i = 0; i < numAnims; i++)
            {
                Animation anim = new Animation(br.ReadString(), Skeleton.GetMatrixLength());
                anim.Frames.Clear();
                anim.Length = br.ReadSingle();
                uint numKeyframes = br.ReadUInt32();
                for (int k = 0; k < numKeyframes; k++)
                {
                    Frame f = new Frame(br.ReadInt32(), (int)br.ReadUInt32());
                    f.Offset = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    if (f.Matrices.Length != Skeleton.GetMatrixLength())
                    {
                        throw new Exception("Error: Skeleton does not match animation");
                    }
                    for (int j = 0; j < f.Matrices.Length; j++)
                    {
                        f.Matrices[j] = new Microsoft.Xna.Framework.Matrix(
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                            br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                    }
                    anim.Frames.Add(f);
                }
                Animations.Add(anim);
            }
            br.Close();
        }

        public void SaveAnimations(string animationFilename)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(animationFilename));
            bw.Write((uint)Animations.Count);
            for (int i = 0; i < Animations.Count; i++)
            {
                bw.Write(Animations[i].Name);
                bw.Write(Animations[i].Length);
                bw.Write((uint)Animations[i].KeyframeCount);
                for (int k = 0; k < Animations[i].KeyframeCount; k++)
                {
                    Frame f = Animations[i].Keyframe(k);
                    bw.Write(f.Time);
                    bw.Write((uint)f.Matrices.Length);
                    bw.Write(f.Offset.X);
                    bw.Write(f.Offset.Y);
                    bw.Write(f.Offset.Z);
                    for (int j = 0; j < f.Matrices.Length; j++)
                    {
                        bw.Write(f.Matrices[j].M11);
                        bw.Write(f.Matrices[j].M12);
                        bw.Write(f.Matrices[j].M13);
                        bw.Write(f.Matrices[j].M14);

                        bw.Write(f.Matrices[j].M21);
                        bw.Write(f.Matrices[j].M22);
                        bw.Write(f.Matrices[j].M23);
                        bw.Write(f.Matrices[j].M24);

                        bw.Write(f.Matrices[j].M31);
                        bw.Write(f.Matrices[j].M32);
                        bw.Write(f.Matrices[j].M33);
                        bw.Write(f.Matrices[j].M34);

                        bw.Write(f.Matrices[j].M41);
                        bw.Write(f.Matrices[j].M42);
                        bw.Write(f.Matrices[j].M43);
                        bw.Write(f.Matrices[j].M44);
                    }
                }
            }
            bw.Close();
        }
    }
}
