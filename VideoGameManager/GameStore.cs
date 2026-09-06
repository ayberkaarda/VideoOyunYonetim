using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public static class GameStore
{
    public static List<Game> Games = new List<Game>();

    public static void Add(Game game)
    {
        Games.Add(game);
    }

    public static List<Game> GetAll()
    {
        return Games;
    }

    public static List<Game> Find(string genre, string platform)
    {
        return Games.Where(g => g.Genre == genre && g.Platform == platform).ToList();
    }
}
