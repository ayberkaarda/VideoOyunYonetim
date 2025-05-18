using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Oyuncu
{
    public int OyuncuId { get; set; }
    public string KullaniciAdi { get; set; }
    public List<Oyun> Koleksiyon { get; set; } = new List<Oyun>();
}
