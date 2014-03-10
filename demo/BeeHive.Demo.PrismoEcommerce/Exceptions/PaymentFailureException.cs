using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeeHive.Demo.PrismoEcommerce.Exceptions
{
    public class PaymentFailureException : Exception
    {
        private string _info;

        public PaymentFailureException(string info)
        {
            Info = info;
        }

        public string Info
        {
            get { return _info; }
            set { _info = value; }
        }
    }
}
