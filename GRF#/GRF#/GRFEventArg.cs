using System;
using System.Collections.Generic;
using System.Text;

namespace GRFSharp
{
    public class GRFEventArg : EventArgs
    {
        private GRFFile _file;

        public GRFFile File { get { return _file; } }

        public GRFEventArg(GRFFile file)
        {
            _file = file;
        }
    }
}