using System;
using System.Collections.Generic;
using System.Text;

namespace XNAP
{
    public class MultipleDeviceException : Exception
    {
        public MultipleDeviceException()
            : base()
        {

        }

        public MultipleDeviceException(String message)
            : base(message)
        {
            
        }

        public MultipleDeviceException(String message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
