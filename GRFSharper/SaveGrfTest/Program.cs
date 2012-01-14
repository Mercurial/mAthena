using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SaveGrfTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SAIB.SharpGRF.SharpGRF grf = new SAIB.SharpGRF.SharpGRF("test.grf");
            grf.AddNewFile("test.xml", new byte[16854]);
            grf.Save();
        }
    }
}
