using System;
using SAIB.SharpGRF;
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
        SharpGRF newGRF = new SharpGRF("C:/RO/rwc.grf");
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
            Console.WriteLine("{0}\t{1}%", GRFFiles[x].Name, percent);

        }
        st.Stop();
        Console.WriteLine(st.Elapsed);


        newGRF.Close();
    }
}