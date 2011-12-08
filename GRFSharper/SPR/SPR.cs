using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace GRFSharperAddons
{
    public class SPR
    {

        private int _magic;
        public int Magic
        {
            get { return _magic; }
            set { _magic = value; }
        }

        private int _version;
        public int Version
        {
            get { return _version; }
            set { _version = value; }
        }

        private RGBImage[] m_RGBImage;
        public RGBImage[] RGBImage
        {
            get { return m_RGBImage; }
        }

        private PaletteImage[] m_PaletteImage;
        public PaletteImage[] PaletteImage
        {
            get { return m_PaletteImage; }
        }

        private bool m_Compressed;
        public bool Compressed
        {
            get { return m_Compressed; }
        }


        public System.Drawing.Color[] Palette = new System.Drawing.Color[256];
        public System.Drawing.Color[] OriginalPalette = new System.Drawing.Color[256];
        private int _pixelcount;
        private int _x;
        private int _y;

        private System.Drawing.Bitmap _bm;
        public int Load(string Filename)
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(Filename);
                if (fi.Exists)
                {
                    System.IO.MemoryStream ms = null;
                    System.IO.BinaryReader br = null;
                    System.IO.FileStream fs = new System.IO.FileStream(fi.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    byte[] @by = new byte[Convert.ToInt32(fs.Length) + 1];
                    fs.Read(@by, 0, Convert.ToInt32(fs.Length));
                    ms = new System.IO.MemoryStream(@by, 0, @by.Length);
                    br = new System.IO.BinaryReader(ms);
                    fs.Close();
                    fs.Dispose();

                    Magic = br.ReadInt16();
                    m_Compressed = br.ReadBoolean();
                    Version = br.ReadByte();

                    int palcnt = br.ReadInt16();
                    int rgbcnt = br.ReadInt16();

                    m_PaletteImage = new PaletteImage[palcnt];
                    for (int i = 1; i <= palcnt; i++)
                    {
                        PaletteImage palImage = new PaletteImage(br, m_Compressed);
                        m_PaletteImage[i - 1] = palImage;
                    }

                    m_RGBImage = new RGBImage[rgbcnt];
                    for (int i = 1; i <= rgbcnt; i++)
                    {
                        RGBImage rgbImage = new RGBImage(br);
                        m_RGBImage[i - 1] = rgbImage;
                    }

                    for (int i = 0; i <= 255; i++)
                    {
                        byte red = br.ReadByte();
                        byte green = br.ReadByte();
                        byte blue = br.ReadByte();
                        byte res = br.ReadByte();
                        Palette[i] = System.Drawing.Color.FromArgb(red, green, blue);
                    }

                    OriginalPalette = Palette;

                    br.Close();
                    ms.Close();
                    ms.Dispose();
                }
                else
                {
                    return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return -2;
            }
        }

        public int Load(byte[] data)
        {
            try
            {
                System.IO.MemoryStream ms = null;
                System.IO.BinaryReader br = null;
                ms = new System.IO.MemoryStream(@data, 0, @data.Length);
                br = new System.IO.BinaryReader(ms);

                Magic = br.ReadInt16();
                m_Compressed = br.ReadBoolean();
                Version = br.ReadByte();

                int palcnt = br.ReadInt16();
                int rgbcnt = br.ReadInt16();

                m_PaletteImage = new PaletteImage[palcnt];
                for (int i = 1; i <= palcnt; i++)
                {
                    PaletteImage palImage = new PaletteImage(br, m_Compressed);
                    m_PaletteImage[i - 1] = palImage;
                }

                m_RGBImage = new RGBImage[rgbcnt];
                for (int i = 1; i <= rgbcnt; i++)
                {
                    RGBImage rgbImage = new RGBImage(br);
                    m_RGBImage[i - 1] = rgbImage;
                }

                for (int i = 0; i <= 255; i++)
                {
                    byte red = br.ReadByte();
                    byte green = br.ReadByte();
                    byte blue = br.ReadByte();
                    byte res = br.ReadByte();
                    Palette[i] = System.Drawing.Color.FromArgb(red, green, blue);
                }

                OriginalPalette = Palette;

                br.Close();
                ms.Close();
                ms.Dispose();
                return 0;
            }
            catch (Exception ex)
            {
                return -2;
            }
        }

        public void LoadPalette(string file)
        {
            try
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(file);
                if (fi.Exists)
                {
                    System.IO.MemoryStream ms = null;
                    System.IO.BinaryReader br = null;
                    System.IO.FileStream fs = new System.IO.FileStream(fi.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    //If Filename = "D:\Games\items\°í°í.spr" Then Stop
                    byte[] @by = new byte[Convert.ToInt32(fs.Length) + 1];
                    fs.Read(@by, 0, Convert.ToInt32(fs.Length));
                    ms = new System.IO.MemoryStream(@by, 0, @by.Length);
                    br = new System.IO.BinaryReader(ms);
                    fs.Close();
                    fs.Dispose();

                    for (int i = 0; i <= 255; i++)
                    {
                        byte red = br.ReadByte();
                        byte green = br.ReadByte();
                        byte blue = br.ReadByte();
                        byte res = br.ReadByte();
                        Palette[i] = System.Drawing.Color.FromArgb(red, green, blue);
                    }
                }

            }
            catch (Exception ex)
            {
            }
        }

        public System.Drawing.Bitmap GetPaletteImage(int index)
        {
            try
            {
                int i = 0;
                if (m_PaletteImage.Length < index)
                    return null;

                _x = 0;
                _y = 0;
                _pixelcount = 0;

                System.IO.MemoryStream ms = null;
                System.IO.BinaryReader br = null;

                SpriteImage si = (SpriteImage)m_PaletteImage[index];
                _bm = new System.Drawing.Bitmap(si.Width, si.Height);

                ms = new System.IO.MemoryStream(si.Data, 0, si.Data.Length);
                br = new System.IO.BinaryReader(ms);
                do
                {
                    if (Compressed)
                    {
                        byte b = br.ReadByte();
                        if (b == 0)
                        {
                            b = br.ReadByte();
                            for (i = 0; i <= b - 1; i++)
                            {
                                SetPixel(Palette[0], si, true);
                                _pixelcount = _pixelcount + 1;
                            }
                        }
                        else
                        {
                            SetPixel(Palette[b], si, false);
                            _pixelcount = _pixelcount + 1;
                        }
                    }
                    else
                    {
                        byte b = br.ReadByte();
                        if (b == 0)
                        {
                            SetPixel(Palette[b], si, true);
                        }
                        else
                        {
                            SetPixel(Palette[b], si, false);
                        }
                        _pixelcount = _pixelcount + 1;
                    }
                    if (ms.Position == ms.Length)
                        break;
                } while (true);
                return _bm;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public System.IO.Stream GetPaletteImageStream(int index)
        {
            try
            {
                int i = 0;
                if (m_PaletteImage.Length < index)
                    return null;

                _x = 0;
                _y = 0;
                _pixelcount = 0;

                System.IO.MemoryStream ms = null;
                System.IO.BinaryReader br = null;

                SpriteImage si = (SpriteImage)m_PaletteImage[index];
                _bm = new System.Drawing.Bitmap(si.Width, si.Height);

                ms = new System.IO.MemoryStream(si.Data, 0, si.Data.Length);
                br = new System.IO.BinaryReader(ms);
                do
                {
                    if (Compressed)
                    {
                        byte b = br.ReadByte();
                        if (b == 0)
                        {
                            b = br.ReadByte();
                            for (i = 0; i <= b - 1; i++)
                            {
                                SetPixel(Palette[0], si, true);
                                _pixelcount = _pixelcount + 1;
                            }
                        }
                        else
                        {
                            SetPixel(Palette[b], si, false);
                            _pixelcount = _pixelcount + 1;
                        }
                    }
                    else
                    {
                        byte b = br.ReadByte();
                        if (b == 0)
                        {
                            SetPixel(Palette[b], si, true);
                        }
                        else
                        {
                            SetPixel(Palette[b], si, false);
                        }
                        _pixelcount = _pixelcount + 1;
                    }
                    if (ms.Position == ms.Length)
                        break;
                } while (true);
                MemoryStream s = new MemoryStream();
                _bm.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                return s;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public System.Drawing.Bitmap GetRGBImage(int index)
        {
            try
            {
                if (m_RGBImage.Length < index)
                    return null;

                _x = 0;
                _y = 0;
                _pixelcount = 0;

                System.IO.MemoryStream ms = null;
                System.IO.BinaryReader br = null;

                SpriteImage si = (SpriteImage)m_RGBImage[index];
                _bm = new System.Drawing.Bitmap(si.Width, si.Height);

                ms = new System.IO.MemoryStream(si.Data, 0, si.Data.Length);
                br = new System.IO.BinaryReader(ms);
                _y = si.Height - 1;
                do
                {
                    byte a = br.ReadByte();
                    byte b = br.ReadByte();
                    byte g = br.ReadByte();
                    byte r = br.ReadByte();
                    //Debug.Print(String.Format("A={0}, R={1}, G={2}, B={3}", a, r, g, b))
                    SetPixelInvers(System.Drawing.Color.FromArgb(a, r, g, b), si);
                    _pixelcount = _pixelcount + 1;
                    if (ms.Position == ms.Length)
                        break; // TODO: might not be correct. Was : Exit Do
                } while (true);
                return _bm;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int SaveBitmap(string filename)
        {
            try
            {
                _bm.Save(filename);
                return 0;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public void SetPixel(System.Drawing.Color color, SpriteImage si, bool trans)
        {
            try
            {
                if (trans == false)
                {
                    _bm.SetPixel(_x, _y, color);
                }
                _x = _x + 1;
                if (_x + 1 > si.Width)
                {
                    _x = 0;
                    _y = _y + 1;
                }

            }
            catch (Exception ex)
            {
            }
        }

        public void SetPixelInvers(System.Drawing.Color color, SpriteImage si)
        {
            try
            {
                _bm.SetPixel(_x, _y, color);
                _x = _x + 1;
                if (_x + 1 > si.Width)
                {
                    _x = 0;
                    _y = _y - 1;
                }
            }
            catch (Exception ex)
            {
            }
        }

    }

    public class SpriteImage
    {

        private Int16 _width;
        public Int16 Width
        {
            get { return _width; }
            set { _width = value; }
        }

        private Int16 _height;
        public Int16 Height
        {
            get { return _height; }
            set { _height = value; }
        }

        private int _size;
        public int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        private byte[] _data;
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        //Dumpt ein Bytearray als Hex-String
        [DebuggerStepThrough()]
        public string ByteArrayToHex(byte[] data)
        {
            byte b = 0;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b_loopVariable in data)
            {
                b = b_loopVariable;
                sb.Append(b.ToString("X2"));
                sb.Append(' ');
            }
            return sb.ToString();
        }

    }

    public class PaletteImage : SpriteImage
    {

        public PaletteImage(System.IO.BinaryReader br, bool compressed)
        {
            try
            {
                this.Width = br.ReadInt16();
                this.Height = br.ReadInt16();
                this.Size = Convert.ToInt32(this.Width) * Convert.ToInt32(this.Height);
                if (compressed)
                {
                    this.Size = br.ReadUInt16();
                }
                this.Data = br.ReadBytes(this.Size);
            }
            catch (Exception ex)
            {
            }
        }
    }

    public class RGBImage : SpriteImage
    {

        public RGBImage(System.IO.BinaryReader br)
        {
            try
            {
                this.Width = br.ReadInt16();
                this.Height = br.ReadInt16();
                this.Size = Convert.ToInt32(this.Width) * Convert.ToInt32(this.Height) * 4;
                this.Data = br.ReadBytes(this.Size);
            }
            catch (Exception ex)
            {
            }
        }
    }



}
