using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mod.Data.Scripts.RacingTimer
{
    public class TimeDelta
    {
        public TimeDelta(DateTime start)
        {
            Start = start;
        }

        public TimeDelta(DateTime start, DateTime end, BestTime personalBest, BestTime globalBest)
        {
            Start = start;

            Calculate(end, personalBest, globalBest);
        }

        public void Calculate(DateTime end, BestTime personalBest, BestTime globalBest)
        {
            End = end;

            // calculate delta
            Delta = end - Start;

            // is new personal best
            IsPersonalBest = (personalBest == null || Delta < personalBest.Time);

            // delta to personal best lap
            DeltaToPersoanlBest = (personalBest == null ? Delta : personalBest.Time) - Delta;

            // is new global best
            IsGlobalBest = (globalBest == null || Delta < globalBest.Time);

            // delta to global best
            DeltaToGlobalBest = (globalBest == null ? Delta : globalBest.Time) - Delta;
        }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public TimeSpan Delta { get; set; }

        public TimeSpan DeltaToPersoanlBest { get; set; }

        public TimeSpan DeltaToGlobalBest { get; set; }

        public bool IsPersonalBest { get; set; }

        public bool IsGlobalBest { get; set; }

        public string ToString(bool time = true, bool personalDelta = true, bool globalDelta = false)
        {
            var str = "";

            if (time)
                str += string.Format("{0:mm}:{0:ss}.{0:fff}", Delta);

            if (personalDelta)
                str += (str == "" ? "" : " ") + string.Format("({0}{1:ss}.{1:fff})", 
                    (IsPersonalBest ? "-" : "+"), 
                    DeltaToPersoanlBest);

            if (globalDelta)
                str += (str == "" ? "" : " ") + string.Format("({0}{1:ss}.{1:fff})",
                    (IsGlobalBest ? "-" : "+"), 
                    DeltaToGlobalBest);

            return str;
        }
    }
}
