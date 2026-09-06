using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class Player
{
    public int PlayerId { get; set; }
    public string UserName { get; set; }
    public List<Game> Collection { get; set; } = new List<Game>();
}
