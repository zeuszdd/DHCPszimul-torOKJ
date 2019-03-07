using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace DHCP2
{
    class Program
    {
        static List<int> excluded; // Nem kiosztható IP-k (IP)
        static Dictionary<string, int> reserved; // Fenntartott címek (MAC-IP)
        static Dictionary<string, int> dhcp; // Kiosztott címek (MAC-IP)
        static string tartomany = "192.168.10.";
        static int elso = 100;
        static int utolso = 199;
        static void Main(string[] args)
        {
            excluded = new List<int>();
            reserved = new Dictionary<string, int>();
            dhcp = new Dictionary<string, int>();
            foreach (string item in File.ReadAllLines("excluded.csv", Encoding.UTF8))
            {
                excluded.Add(int.Parse(item.Split('.')[3]));
            }
            foreach (string item in File.ReadAllLines("reserved.csv", Encoding.UTF8))
            {
                reserved.Add(item.Split(';')[0], int.Parse(item.Split(';', '.')[4]));
            }
            foreach (string item in File.ReadAllLines("dhcp.csv", Encoding.UTF8))
            {
                dhcp.Add(item.Split(';')[0], int.Parse(item.Split(';', '.')[4]));
            }

            foreach (string item in File.ReadAllLines("test.csv", Encoding.UTF8))
            {
                string muvelet = item.Split(';')[0];
                if (muvelet == "request")
                {
                    string mac = item.Split(';')[1];
                    if (!dhcp.ContainsKey(mac))
                    {
                        if (reserved.ContainsKey(mac))
                        {
                            int ip = reserved[mac];
                            if (!dhcp.ContainsValue(ip))
                            {
                                dhcp.Add(mac, ip);
                            }
                        }
                        else
                        {
                            int ip = elso;
                            // Folyamatábra szerint
                            while (ip <= utolso)
                            {
                                if (!dhcp.ContainsValue(ip))
                                {
                                    if (!excluded.Contains(ip))
                                    {
                                        if (!reserved.ContainsValue(ip))
                                        {
                                            dhcp.Add(mac, ip);
                                            break;
                                        }
                                    }
                                }
                                ip++;
                            }
                            if (ip > utolso)
                                throw (new Exception("Nincs kiadható IP cím!"));
                        }
                    }
                }
                else if (muvelet == "release")
                {
                    int ip = int.Parse(item.Split(';', '.')[4]);
                    if (dhcp.ContainsValue(ip))
                    {
                        dhcp.Remove(dhcp.First(d => d.Value == ip).Key);
                    }
                }
            }
            StreamWriter sw = new StreamWriter("dhcp_kesz.csv", false, Encoding.UTF8);
            foreach (var d in dhcp)
            {
                sw.WriteLine("{0};{1} {2}", d.Key, tartomany, d.Value);
            }
            sw.Close();
            ReadKey();
        }
    }
}

