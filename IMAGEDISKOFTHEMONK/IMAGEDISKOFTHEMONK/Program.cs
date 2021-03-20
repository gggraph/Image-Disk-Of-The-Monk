using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO; 

namespace IMAGEDISKOFTHEMONK
{
    class Program
    {

        public static Bitmap bmp;

        static void Main(string[] args)
        {

            byte[] data = File.ReadAllBytes(@"C:\Users\Gaël\PROJET ART\ESAD RECHERCHE 1\PI\test.mp3");
            byte[] reduceddata = new byte[2953];
            for (int i = 0; i < 2953; i++)
            {
                reduceddata[i] = data[i];
            }
            DataToMonkCypher(reduceddata);
            Console.ReadKey();
        }
        
            public class Point
            {

                public float X { get; set; }
                public float Y { get; set; }

                public Point(float x = 0f, float y = 0f)
                {
                    this.X = x;
                    this.Y = y;
                }
            }
            private static byte ConvertBoolArrayToByte(bool[] source)
            {
                byte result = 0;
                // This assumes the array never contains more than 8 elements!
                int index = 8 - source.Length;

                // Loop through the array
                foreach (bool b in source)
                {
                    // if the element is 'true' set the bit at that position
                    if (b)
                        result |= (byte)(1 << (7 - index));

                    index++;
                }

                return result;
            }

            public static void DataToMonkCypher(byte[] data)
            {
                //byte[] data = BlockToBytes(b);

                if (BitConverter.IsLittleEndian)
                {
                    Console.WriteLine("petit endian ! ");
                }
                BitArray bits = new BitArray(data);
                List<UInt16> b13 = new List<UInt16>();
                // pack by 13 bits .... 

                long bitoffset = 0;
                int counter = 0;
                while (bitoffset < bits.Length - 13)
                {
                    bool[] shortedA = new bool[8];
                    bool[] shortedB = new bool[8];
                    for (int i = 0; i < 8; i++)
                    {
                        shortedA[i] = bits[(int)bitoffset + i];

                    }
                    for (int i = 8; i < 13; i++)
                    {
                        shortedB[7 - (i - 8)] = bits[(int)bitoffset + i];

                    }

                    byte[] u16 = new byte[2];
                    u16[0] = ConvertBoolArrayToByte(shortedA);
                    u16[1] = ConvertBoolArrayToByte(shortedB);

                    UInt16 valueout = BitConverter.ToUInt16(u16, 0);
                   // Console.WriteLine(valueout.ToString());
                    b13.Add(valueout);

                    bitoffset += 13;
                    counter++;
                }

                // just next to the padding distance ( 13 - 
                // do the truncate block 

                

                int dist = bits.Length - (int)bitoffset;
                bool[] alonebits = new bool[dist];
                for (int i = 0; i < dist; i++)
                {
                    alonebits[i] = bits[(int)bitoffset + i];
                }
                // forcement inferieur a 13 
                bool[] sA = new bool[8];
                bool[] sB = new bool[8];
                if (alonebits.Length >= 8)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        sA[i] = bits[(int)bitoffset + i];

                    }
                    for (int i = 8; i < 8 + (alonebits.Length - 8); i++)
                    {
                        sB[7 - (i - 8)] = bits[(int)bitoffset + i];

                    }
                    byte[] u16 = new byte[2];
                    u16[0] = ConvertBoolArrayToByte(sA);
                    u16[1] = ConvertBoolArrayToByte(sB);

                    UInt16 valueout = BitConverter.ToUInt16(u16, 0);
                    Console.WriteLine(valueout.ToString());
                    b13.Add(valueout);
                    counter++;
                }
                else
                {
                    for (int i = 0; i < alonebits.Length; i++)
                    {
                        sA[i] = bits[(int)bitoffset + i];

                    }
                    byte[] u16 = new byte[2];
                    u16[0] = ConvertBoolArrayToByte(sA);
                    u16[1] = ConvertBoolArrayToByte(sB);

                    UInt16 valueout = BitConverter.ToUInt16(u16, 0);
                    Console.WriteLine(valueout.ToString());
                    b13.Add(valueout);
                    counter++;

                }
                 Console.WriteLine("number of symboles : " + counter);
                double sqrcnt = Math.Sqrt((double)counter);
            Console.WriteLine("sqrt : " + sqrcnt);
            int recsize = (int)Math.Ceiling(sqrcnt);
            Console.WriteLine("++ : " + recsize);
            int squaresize = 10;
            int offX = 0;

                int offY = 0;
                // ctr sign
                int ctr = 0;



            bmp = new Bitmap(recsize * squaresize, recsize * squaresize);
            
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            Pen blackPen = new Pen(Color.Black, 3);

            foreach (short u in b13)
                {
                    // transcrire u en cistercien
                    string s = u.ToString();
                    int zeropadding = 4 - s.Length;
                    for (int i = 0; i < zeropadding; i++)
                    {
                        s = "0" + s;
                    }

                   
                    List<Point> unit = DigitToCistercianFragment((byte)byte.Parse(s[3].ToString()), squaresize);
                    List<Point> digit = DigitToCistercianFragment((byte)byte.Parse(s[2].ToString()), squaresize);
                    List<Point> cent = DigitToCistercianFragment((byte)byte.Parse(s[1].ToString()), squaresize);
                    List<Point> thsd = DigitToCistercianFragment((byte)byte.Parse(s[0].ToString()), squaresize);

                    List<List<Point>> CistercianSymbol = AssembleCistercian(unit, digit, cent, thsd, squaresize);
                    foreach ( List<Point> lp in CistercianSymbol)
                    {
                        for (int i = 1; i < lp.Count; i++)
                        {
                         g.DrawLine(blackPen, lp[i-1].X + offX, lp[i - 1].Y + offY, lp[i].X + offX, lp[i].Y + offY); 
                        }
                    }
                    offX += squaresize * 2;
                    ctr++;
                    if (ctr >= recsize)
                    {
                        ctr = 0;
                        offX = 0;
                        offY += squaresize * 2;
                    }

                }

            // save the bmp somewhere
                string fileName = @"C:\Users\Gaël\PROJET ART\ESAD RECHERCHE 1\PI\monkcipher.bmp";
                bmp.Save(fileName, ImageFormat.Jpeg);
                Console.WriteLine("nem bmp saved from : " + fileName);

            }

            public static List<List<Point>> AssembleCistercian(List<Point> unit, List<Point> digit, List<Point> cent, List<Point> thsd, int squaresize = 10)
            {
                List<List<Point>> result = new List<List<Point>>();

                List<Point> tempList = new List<Point>();
                // va faire 20 20 sur 20 20 ...
                // dessiner la barre vertical
                tempList.Add(new Point(squaresize, 0)); // move to 
                tempList.Add(new Point(squaresize, squaresize * 2)); // line to 
                result.Add(tempList);

                if (unit.Count > 0)
                {
                    // do the unit side // OK [ INVERTED : NO ]  [ OFFSET : 10 0 ]
                    tempList = new List<Point>();
                    foreach (Point p in unit)
                    {
                        tempList.Add(new Point(p.X + squaresize, p.Y)); // x + squaresize
                    }
                    result.Add(tempList);
                }

                if (digit.Count > 0)
                {
                    // do the tenth side // OK [ INVERTED : X ]  [ OFFSET : 0 0 ]
                    tempList = new List<Point>();
                    foreach (Point p in digit)
                    {
                        tempList.Add(new Point(squaresize - p.X, p.Y)); // x inverted
                    }
                    result.Add(tempList);
                }


                if (cent.Count > 0)
                {
                    // do the cent side // OK [ INVERTED : Y ]  [ OFFSET : 10 10 ]
                    tempList = new List<Point>();
                    foreach (Point p in cent)
                    {
                        tempList.Add(new Point(squaresize + p.X, squaresize + (squaresize - p.Y))); // y sont  inversé,  y + squaresize , x + squaresize
                    }
                    result.Add(tempList);
                }


                if (thsd.Count > 0)
                {
                    // do the thsd side // OK [ INVERTED :  X Y ]  [ OFFSET : 0 10 ]
                    tempList = new List<Point>();
                    foreach (Point p in thsd)
                    {
                        tempList.Add(new Point(squaresize - p.X, squaresize + (squaresize - p.Y))); // x  et y sont  inversé,  y + squaresize 
                    }
                    result.Add(tempList);
                }


                return result;
            }
            public static List<Point> DigitToCistercianFragment(byte b, int squaresize = 10)
            {
                List<Point> val = new List<Point>();
                switch (b)
                {
                    case 1:
                        val.Add(new Point(0, 0)); // move to 
                        val.Add(new Point(squaresize, 0));  // line to 
                        break;
                    case 2:
                        val.Add(new Point(0, squaresize)); // move to 
                        val.Add(new Point(squaresize, squaresize)); // line to 
                        break;
                    case 3:
                        val.Add(new Point(0, 0)); // move to 
                        val.Add(new Point(squaresize, squaresize));// line to
                        break;
                    case 4:
                        val.Add(new Point(0, squaresize)); // move to 
                        val.Add(new Point(squaresize, 0));// line to
                        break;
                    case 5:
                        val.Add(new Point(0, 0)); // move to
                        val.Add(new Point(squaresize, 0));// line to
                        val.Add(new Point(0, squaresize));// line to
                        break;
                    case 6:
                        val.Add(new Point(squaresize, 0)); // move to
                        val.Add(new Point(squaresize, squaresize));// line to
                        break;
                    case 7:
                        val.Add(new Point(0, 0)); // move to
                        val.Add(new Point(squaresize, 0)); // line to
                        val.Add(new Point(squaresize, squaresize));// line to
                        break;
                    case 8:
                        val.Add(new Point(squaresize, 0)); // move to
                        val.Add(new Point(squaresize, squaresize)); // line to
                        val.Add(new Point(0, squaresize));// line to
                        break;
                    case 9:
                        val.Add(new Point(0, 0)); // move to
                        val.Add(new Point(squaresize, 0)); // line to
                        val.Add(new Point(squaresize, squaresize)); // line to
                        val.Add(new Point(0, squaresize));// line to
                        break;
                }

                return val;
            }

    }
}
