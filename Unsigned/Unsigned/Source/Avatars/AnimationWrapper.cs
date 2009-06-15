using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Unsigned
{
    public class AnimationWrapper
    {
        private class AnimationRunningInfo
        {
            public bool ShouldDestroy { get; private set; }
            public ARIType type;
            public float time;
            public float speed;
            public float target;
            public int index;

            public AnimationRunningInfo(ARIType tp, int ind, float spd)
            {
                index = ind;
                type = tp;
                time = 0;
                speed = spd;
                ShouldDestroy = false;
            }

            public void Update(SongTime songTime)
            {
                if (type == ARIType.RunOnce || type == ARIType.RunConstantly)
                {
                    time += (float)songTime.ElapsedGameTime.TotalSeconds * speed;
                    if (time > 1)
                    {
                        if (type == ARIType.RunOnce)
                            ShouldDestroy = true;
                        else
                            time -= 1;
                    }
                }
                if (type == ARIType.RunToPoint)
                {
                    time = (((float)songTime.ElapsedGameTime.TotalSeconds * speed) * target) + ((1 - ((float)songTime.ElapsedGameTime.TotalSeconds * speed)) * time);
                }
            }

            internal void Reset(ARIType ariType, float speed)
            {
                type = ariType;
                this.speed = speed;
                time = 0;
                ShouldDestroy = false;
            }

            internal void Reset(ARIType ariType, float speed, float target)
            {
                type = ariType;
                this.speed = speed;
                this.target = target;
                ShouldDestroy = false;
            }
        }

        public enum ARIType
        {
            RunConstantly = 0,
            RunOnce,
            RunToPoint,
        }

        private AnimationInfo AnimInfo;

        private List<AnimationRunningInfo> runningInfos, stoppedInfos;

        private bool[][] EnabledMatrices;

        private float totalTime;

        public int this[String index]
        {
            get
            {
                for (int i = 0; i < AnimInfo.Animations.Count; i++)
                    if (AnimInfo.Animations[i].Name.ToLower() == index.ToLower())
                        return i;
                return -1;
            }
        }

        public Joint RootJoint { get { return AnimInfo.Skeleton.RootJoint; } }
        public Joint InstrRootJoint { get { return AnimInfo.Skeleton.InstrRootJoint; } }

        public AnimationWrapper(AnimationInfo info)
        {
            totalTime = 0;
            AnimInfo = info;
            EnabledMatrices = new bool[AnimInfo.Animations.Count][];
            for (int i = 0; i < EnabledMatrices.Length; i++)
            {
                EnabledMatrices[i] = new bool[AnimInfo.Skeleton.GetMatrixLength()];
                for (int k = 0; k < EnabledMatrices[i].Length; k++)
                    EnabledMatrices[i][k] = true;
            }
            ClearUnchangedMatrices();
            runningInfos = new List<AnimationRunningInfo>();
            stoppedInfos = new List<AnimationRunningInfo>();
            for (int i = 0; i < AnimInfo.Animations.Count; i++)
                stoppedInfos.Add(new AnimationRunningInfo(ARIType.RunConstantly, i, 1.0f));
            RunAnimation(ARIType.RunConstantly, "Idle", 0.25f);
            RunAnimation(ARIType.RunConstantly, "IdleHead", 1/2.5f);
        }

        public bool IsAnimationRunning(String name)
        {
            int an = this[name];
            if (an >= 0)
            {
                for (int i = 0; i < runningInfos.Count; i++)
                    if (runningInfos[i].index == an)
                        return true;
            }
            return false;
        }

        public void RunAnimation(ARIType ariType, string name, float speed)
        {
            if (!IsAnimationRunning(name))
            {
                if (this[name] >= 0)
                {
                    AnimationRunningInfo ari = null;
                    for (int i = 0; i < stoppedInfos.Count; i++)
                    {
                        if (stoppedInfos[i].index == this[name])
                        {
                            ari = stoppedInfos[i];
                            stoppedInfos.RemoveAt(i);
                            break;
                        }
                    }
                    ari.Reset(ariType, speed);
                    runningInfos.Add(ari);
                }
            }
            else
            {
                if (this[name] >= 0)
                {
                    AnimationRunningInfo ari = null;
                    for (int i = 0; i < runningInfos.Count; i++)
                    {
                        if (runningInfos[i].index == this[name])
                        {
                            ari = runningInfos[i];
                            break;
                        }
                    }
                    ari.Reset(ariType, speed);
                }
            }
        }

        public void RunAnimation(ARIType ariType, string name, float speed, float target)
        {
            if (!IsAnimationRunning(name))
            {
                if (this[name] >= 0)
                {
                    AnimationRunningInfo ari = null;
                    for (int i = 0; i < stoppedInfos.Count; i++)
                    {
                        if (stoppedInfos[i].index == this[name])
                        {
                            ari = stoppedInfos[i];
                            stoppedInfos.RemoveAt(i);
                            break;
                        }
                    }
                    ari.Reset(ariType, speed, target);
                    runningInfos.Add(ari);
                }
            }
            else
            {
                if (this[name] >= 0)
                {
                    AnimationRunningInfo ari = null;
                    for (int i = 0; i < runningInfos.Count; i++)
                    {
                        if (runningInfos[i].index == this[name])
                        {
                            ari = runningInfos[i];
                            break;
                        }
                    }
                    ari.Reset(ariType, speed, target);
                }
            }
        }

        private void RemoveAnimation(String name)
        {
            int ai = this[name];
            if (ai >= 0)
            {
                AnimationRunningInfo ari = null;
                for (int i = 0; i < runningInfos.Count; i++)
                    if (runningInfos[i].index == ai)
                    {
                        ari = runningInfos[i];
                        runningInfos.RemoveAt(i);
                        break;
                    }
                if (ari != null)
                    stoppedInfos.Add(ari);
            }
        }

        private void RemoveAnimation(int runningIndex)
        {
            AnimationRunningInfo ari = null;
            ari = runningInfos[runningIndex];
            runningInfos.RemoveAt(runningIndex);
            stoppedInfos.Add(ari);
        }

        public void ClearUnchangedMatrices()
        {
            for (int i = 0; i < AnimInfo.Animations.Count; i++)
            {
                for (int k = 0; k < AnimInfo.Animations[i].Frames[0].Matrices.Length; k++)
                {
                    bool anyChanged = false;
                    for (int j = 0; j < AnimInfo.Animations[i].Frames.Count; j++)
                    {
                        if (!AnimInfo.Animations[i].Frames[j].Matrices[k].Equals(Matrix.Identity))
                        {
                            anyChanged = true;
                        }
                    }
                    EnabledMatrices[i][k] = anyChanged;
                }
                
            }
        }

        public void Update(SongTime songTime)
        {
            totalTime += (float)songTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < runningInfos.Count; i++)
            {
                runningInfos[i].Update(songTime);
                if (runningInfos[i].ShouldDestroy)
                {
                    RemoveAnimation(i);
                    i--;
                }
                else
                {
                    
                }
            }
        }

        internal Vector3 GetOffset()
        {
            Vector3 offset = Vector3.Zero;
            for (int i = 0; i < runningInfos.Count; i++)
            {
                offset += AnimInfo.Animations[runningInfos[i].index].GetOffset((runningInfos[i].time * 100));
            }
            return offset;
        }

        internal Matrix GetJointMatrix(int matrixIndex)
        {
            Matrix offset = Matrix.Identity;
            for (int i = 0; i < runningInfos.Count; i++)
            {
                if(EnabledMatrices[runningInfos[i].index][matrixIndex])
                    offset *= AnimInfo.Animations[runningInfos[i].index].GetJointMatrix((runningInfos[i].time * 100),matrixIndex);
            }
            return offset;
        }
    }
}
