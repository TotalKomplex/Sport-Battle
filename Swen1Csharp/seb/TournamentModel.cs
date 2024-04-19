using Npgsql.Replication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swen1Csharp.seb
{
    public class TournamentModel
    {
        public int record { get; set; } = 0;
        public string[] leaders { get; set; } = [];
        public string[] losers { get; set; }
        public int[] participants { get; set; }
        public int[] winnerIDs { get; set; }
        public DateTime startTime { get; set; }
        public int  tournamentID { get; set; }
        public bool closed { get; set; } =false;
    }
}
