using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Scheduling
{

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class SimpleAutoPulserDescriptionAttribute : Attribute
    {
        private string _eventType;
        private int _intervalSeconds;

        public SimpleAutoPulserDescriptionAttribute(string eventType, 
            int intervalSeconds)
        {
            _intervalSeconds = intervalSeconds;
            _eventType = eventType;
        }


        public string EventType
        {
            get { return _eventType; }
        }

        public int IntervalSeconds
        {
            get { return _intervalSeconds;  }
        }
    }
}
