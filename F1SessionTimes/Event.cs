using System;
using System.Collections.Generic;
using System.Text;

namespace F1SessionTimes
{
    class Event
    {
        public string location { get; set; }
        public string name { get; set; }
        public Dictionary<string, string> sessions { get; set; } // session name - session time

        public Event()
        {
            sessions = new Dictionary<string, string>();
        }
    }
}
