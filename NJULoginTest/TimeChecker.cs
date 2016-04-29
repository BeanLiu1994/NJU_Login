using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NJULoginTest
{
    public class TimeChecker
    {
        public TimeChecker(TimeSpan _requestspan)
        {
            RequestSpan = _requestspan;
        }
        public TimeSpan RequestSpan;
        private DateTime FirstDownTime = new DateTime();
        private DateTime NewQueryTime = new DateTime();
        public bool Check()
        {
            if (FirstDownTime.Year != DateTime.Today.Year) FirstDownTime = DateTime.Now;
            else NewQueryTime = DateTime.Now;
            if (NewQueryTime.Year == DateTime.Today.Year && (NewQueryTime - FirstDownTime) < RequestSpan)
                return false;
            return true;
        }
    }
}
