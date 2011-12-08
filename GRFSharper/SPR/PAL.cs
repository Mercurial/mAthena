using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace GRFSharperAddons
{
    public class PAL
    {

        public System.Drawing.Color[] Palette = new System.Drawing.Color[256];

        public void Load(byte[] data)
        {
            try
            {
                System.IO.MemoryStream ms = null;
                System.IO.BinaryReader br = null;
                ms = new System.IO.MemoryStream(@data, 0, @data.Length);
                br = new System.IO.BinaryReader(ms);

                for (int i = 0; i <= 255; i++)
                {
                    byte red = br.ReadByte();
                    byte green = br.ReadByte();
                    byte blue = br.ReadByte();
                    byte res = br.ReadByte();
                    Palette[i] = System.Drawing.Color.FromArgb(red, green, blue);
                }

            }
            catch (Exception ex)
            {
            }
        }

        public Stream GetPalette()
        {
            int size = 16;
            int spacing = 2;
            Bitmap bm = new Bitmap(16 * size + spacing * size, 16 * size + spacing * size);

            Graphics g = Graphics.FromImage(bm);
            for (int y = 0; y< 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int posX = x * size + x * spacing;
                    int posY = y * size + y * spacing;
                    int index = x + y * 16;
                    //g.DrawRectangle(new Pen(Palette[index]), posX, posY, size, size);
                    g.FillRectangle(new SolidBrush(Palette[index]), posX, posY, size, size);
                }
            }

            MemoryStream s = new MemoryStream();
            bm.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            return s;
        }
    }
}
