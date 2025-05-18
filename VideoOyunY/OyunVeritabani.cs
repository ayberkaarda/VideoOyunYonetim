using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public static class OyunVeritabani
{
    public static List<Oyun> Oyunlar = new List<Oyun>();

    public static void OyunEkle(Oyun oyun)
    {
        Oyunlar.Add(oyun);
    }

    public static List<Oyun> OyunListele()
    {
        return Oyunlar;
    }

    public static List<Oyun> OneriAl(string tur, string platform)
    {
        return Oyunlar.Where(o => o.Tur == tur && o.Platform == platform).ToList();
    }
}
