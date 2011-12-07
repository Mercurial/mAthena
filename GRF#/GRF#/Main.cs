using System;
using SAIB.SharpGRF;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;


class MainClass
{
	public static void Main (string[] args)
	{
		//SharpGRF newGRF = new SharpGRF("/media/B06CC97A6CC93BBA/RO/data.grf");
		SharpGRF newGRF = new SharpGRF("/home/mercurial/GRF#/rwc.grf");
		
		newGRF.Open();
		
		List<GRFFile> GRFFiles = new List<GRFFile>();
		foreach(GRFFile file in newGRF.Files)
		{
			if(file.Flags != 3 && file.Flags != 5)
			{
				GRFFiles.Add(file);
			}
		}
		Stopwatch st = new Stopwatch();
		st.Start();
		float percent = 0;
		for(int x=0; x<GRFFiles.Count; x++)
		{
			
		    GRFFiles[x].WriteToDisk("/home/mercurial/tempGRF/");
			percent = (float)(x+1)/(float)GRFFiles.Count*100.0f;
			Console.WriteLine("{0}\t{1}%", GRFFiles[x].Name, percent);
			
		}
		st.Stop();
		Console.WriteLine(st.Elapsed);
		
		
		newGRF.Close();
	}
}