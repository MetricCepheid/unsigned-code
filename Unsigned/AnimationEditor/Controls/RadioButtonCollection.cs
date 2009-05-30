using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationEditor
{
    public class RadioButtonCollection : List<RadioButtonControl>
    {
        public RadioButtonCollection() : base()
        {

        }

        public void EnableOne(RadioButtonControl c)
        {
            if (Contains(c))
            {
                for (int i = 0; i < Count; i++)
                    if(this[i]!=c)
                    {
                        this[i].Checked = false;
                    }
            }
            else
                throw new IndexOutOfRangeException();
        }
    }
}
