using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapinfoEditor_KOMIKS
{
    internal static class Map
    {
        public const int MAP_MAX_X = 255;
        public const int MAP_MAX_Y = 255;
        public static List<MapRegion> regions = new List<MapRegion>();
        public static int regionDrawSize = 2;
        private static readonly byte[] header = new byte[24];
        
        public static void Init()
        {
            for (var maxValue1 = (int) byte.MaxValue; maxValue1 >= 0; --maxValue1)
            {
                for (var maxValue2 = (int) byte.MaxValue; maxValue2 >= 0; --maxValue2)
                    regions.Add(new MapRegion(maxValue2, maxValue1));
            }
        }

        public static void LoadRefRegion(string path)
        {
            Task.Run(() =>
            {
                foreach (var line in File.ReadAllLines(path, Encoding.Unicode))
                {
                    var data = line.Split('\t');

                    if (data[1] == "0" && data[2] == "0") continue;

                    regions.Find(r => r.X == int.Parse(data[1]) && r.Y == int.Parse(data[2])).State = MapRegion.STATE.ENABLED;
                }
            });
        }

        public static int LoadMFOFile(string path)
        {
            var numArray = File.ReadAllBytes(path);
            var index1 = regions.Count - 1;
            var str1 = "";
            var str2 = "";
            
            for (var index2 = 0; index2 < 7; ++index2)
                str2 += (char) numArray[index2];
            
            if (!str2.Equals("JMXVMFO"))
                return -1;
            
            for (var index3 = 0; index3 < numArray.Length; ++index3)
            {
                if (index3 >= 24)
                    str1 += Convert.ToString(numArray[index3], 2).PadLeft(8, '0');
                else
                    header[index3] = numArray[index3];
            }
            
            for (var index4 = 0; index4 < str1.Length - 1; ++index4)
            {
                regions[index1].State = str1[index4] == '1' ? MapRegion.STATE.ENABLED : MapRegion.STATE.DISABLED;
                --index1;
            }
            
            return 0;
        }
        
        public static int SaveMFOFile(string path)
        {
            var bytes = new byte[8216];
            var index1 = 0;
            var str = "";
            
            foreach (var t in header)
            {
                bytes[index1] = t;
                ++index1;
            }
            
            var num = 0;
            
            for (var index3 = regions.Count - 1; index3 > 0; --index3)
            {
                str += (char) (regions[index3].State == MapRegion.STATE.ENABLED ? 49 : 48);

                if (num == 7)
                {
                    bytes[index1] = Convert.ToByte(str, 2);
                    str = "";
                    ++index1;
                    num = 0;
                }
                else ++num;
            }
            
            File.WriteAllBytes(path, bytes);
            
            return 0;
        }
    }
}
