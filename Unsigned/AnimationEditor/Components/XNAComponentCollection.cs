using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnimationEditor
{
    public class XNAComponentCollection
    {
        private List<XNAComponent> components;

        public int Count { get { return components.Count; } }

        public XNAComponent this[int index]
        {
            get { return components[index]; }
        }

        public XNAComponent this[String name]
        {
            get 
            {
                for (int i = 0; i < components.Count; i++)
                    if (components[i].Name == name)
                        return components[i];
                return null;
            }
        }

        public XNAComponentCollection()
        {
            components = new List<XNAComponent>();
        }

        public void Add(XNAComponent comp)
        {
            components.Add(comp);
        }

        public void Remove(XNAComponent comp)
        {
            components.Remove(comp);
        }

        public void RemoveAt(int i)
        {
            components.RemoveAt(i);
        }
    }
}
