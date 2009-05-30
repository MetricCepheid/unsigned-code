using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationEditor
{
    public class FloatReference
    {
        private unsafe float* val;
        public VoidDelegate SetValueDelegate;

        public unsafe FloatReference(float *v)
        {
            val = v;
            SetValueDelegate = null;
        }

        public unsafe void SetValue(float value)
        {
            *val = value;
            if (SetValueDelegate != null)
                SetValueDelegate.Invoke();
        }

        public unsafe float GetValue()
        {
            return *val;
        }
    }
}
