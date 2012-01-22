using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRFSharp
{
    public class GRFFileExtractEventArg : EventArgs
    {
        private GRFFile _file;

        public GRFFile File { get { return _file; } }

        public GRFFileExtractEventArg(GRFFile file)
        {
            _file = file;
        }
    }
}
