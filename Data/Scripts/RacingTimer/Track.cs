using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class Track
    {
        public enum Modes
        {
            Hotlap,
            Race_Running,
            Race_Finished
        }

        public Track(string name)
        {
            Race = new Race();
            Name = name;
        }

        public List<Checkpoint> Checkpoints = new List<Checkpoint>();

        public string Name { get; set; }
        public BestTime BestTime { get; set; }
        public Modes Mode { get; set; }
        public Race Race { get; set; }

        public Checkpoint GetCheckpoint(string fullName)
        {
            return Checkpoints.First(chk => chk.FullName == fullName);
        }
    }
}
