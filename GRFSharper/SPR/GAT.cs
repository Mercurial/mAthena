using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace GRFSharperAddons
{

    public class GAT
    {

        private string m_Magic;
        public string Magic
        {
            get { return m_Magic; }
        }

        private byte m_VerMajor;
        public byte VerMajor
        {
            get { return m_VerMajor; }
        }

        private byte m_VerMinor;
        public byte VerMinor
        {
            get { return m_VerMinor; }
        }

        private int m_Width;
        public int Width
        {
            get { return m_Width; }
        }

        private int m_Height;
        public int v
        {
            get { return m_Height; }
        }

        private GATCell[] m_Cells;
        public GATCell[] Cells
        {
            get { return m_Cells; }
        }

        public GAT(byte[] data)
        {
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(data, 0, data.Length))
                {
                    using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                    {
                        m_Magic = System.Text.Encoding.Default.GetString(br.ReadBytes(4));
                        m_VerMajor = br.ReadByte();
                        m_VerMinor = br.ReadByte();
                        m_Width = br.ReadInt32();
                        m_Height = br.ReadInt32();
                        m_Cells = new GATCell[m_Width * m_Height + 1];
                        for (int i = 0; i <= m_Width * m_Height - 1; i++)
                        {
                            m_Cells[i] = new GATCell(br);
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public Stream GetBitmapStream()
        {
            try
            {
                Bitmap bmp = new Bitmap(m_Width, m_Height);
                Graphics g = Graphics.FromImage(bmp);
                for (int y = 0; y <= m_Height - 1; y++)
                {
                    for (int x = 0; x <= m_Width - 1; x++)
                    {
                        dynamic index = y * m_Height + x;
                        dynamic cell = m_Cells[index];
                        dynamic avg = (cell.TopLeft + cell.TopRight + cell.BottomLeft + cell.BottomRight) / 4;
                        dynamic type = cell.Type;
                        if (avg != 0)
                        {
                        }
                        g.FillRectangle(new SolidBrush(GetColor(type)), x, m_Height - y - 1, 1, 1);
                    }
                }

                MemoryStream s = new MemoryStream();
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                return s;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private Color GetColor(int type)
        {
            switch (type)
            {
                case 0:
                    return Color.FromArgb(255, 25, 25, 25);
                case 1:
                    return Color.FromArgb(255, 153, 153, 153);
                case 5:
                    return Color.FromArgb(255, 70, 70, 70);
                case 8:
                    return Color.FromArgb(255, 66, 97, 255);
                default:
                    Debugger.Break();

                    return Color.Pink;
            }
        }
    }

    public class GATCell
    {

        private float m_TopLeft;
        public float TopLeft
        {
            get { return m_TopLeft; }
        }

        private float m_TopRight;
        public float TopRight
        {
            get { return m_TopRight; }
        }

        private float m_BottomLeft;
        public float BottomLeft
        {
            get { return m_BottomLeft; }
        }

        private float m_BottomRight;
        public float BottomRight
        {
            get { return m_BottomRight; }
        }

        private int m_Type;
        public int Type
        {
            get { return m_Type; }
        }

        public GATCell(System.IO.BinaryReader br)
        {
            try
            {
                m_TopLeft = br.ReadSingle();
                m_TopRight = br.ReadSingle();
                m_BottomLeft = br.ReadSingle();
                m_BottomRight = br.ReadSingle();
                m_Type = br.ReadInt32();
            }
            catch (Exception ex)
            {
            }
        }
    }


}
