using System;
using GRFSharp;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;


class MainClass
{
    public static void Main(string[] args)
    {
#if LINUX
		//SharpGRF newGRF = new SharpGRF("/media/B06CC97A6CC93BBA/RO/data.grf");
		SharpGRF newGRF = new SharpGRF("/home/mercurial/GRF#/rwc.grf");
#else
        GRF newGRF = new GRF(@"C:\Documents and Settings\User\My Documents\Visual Studio 2008\Projects\mAthena\mAthena\Example\2011-12-03Example.gpf");
#endif
        newGRF.Open();

        List<GRFFile> GRFFiles = new List<GRFFile>();
        foreach (GRFFile file in newGRF.Files)
        {
            GRFFiles.Add(file);
        }
        Stopwatch st = new Stopwatch();
        st.Start();
        float percent = 0;
        for (int x = 0; x < GRFFiles.Count; x++)
        {

            GRFFiles[x].WriteToDisk("/home/mercurial/tempGRF/");
            percent = (float)(x + 1) / (float)GRFFiles.Count * 100.0f;
            //Console.WriteLine("{0}\t{1}%", GRFFiles[x].Name, percent);

        }
        st.Stop();
        Console.WriteLine(st.Elapsed);


        newGRF.Close();
    }
}