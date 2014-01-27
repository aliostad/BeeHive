using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive
{
    public interface IInterval
    {
        TimeSpan Next();

        TimeSpan Reset();

    }
}
