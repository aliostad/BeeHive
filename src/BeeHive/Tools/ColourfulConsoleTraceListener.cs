using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Internal;

namespace BeeHive.Tools
{
    public class ColourfulConsoleTraceListener : TraceListener
    {

        private WhoCalledAMethod _writeCallerFinder = new WhoCalledAMethod("System.Diagnostics.Trace", "Write");
        private WhoCalledAMethod _traceCallerFinder = new WhoCalledAMethod("System.Diagnostics.Trace", "Trace");
        private MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();

        public ColourfulConsoleTraceListener(params ConsoleColor[] colours)
        {
            if(colours==null || colours.Length == 0)
            {
                colours = new[]
                {
                    ConsoleColor.Blue,
                    ConsoleColor.Cyan,
                    ConsoleColor.Gray,
                    ConsoleColor.Green,
                    ConsoleColor.DarkCyan,
                    ConsoleColor.DarkGreen,
                    ConsoleColor.Magenta,
                    ConsoleColor.White,
                    ConsoleColor.Yellow,
                    ConsoleColor.DarkYellow
                };
            }
            _colours = colours;

        }


        private ConsoleColor _originalForeground;
        private ConsoleColor _originalBackground;
        private ConsoleColor[] _colours;


        public override void Write(string message)
        {
            SetColours();
            Console.Write(message);
            ResetColours();            
        }

        public override void WriteLine(string message)
        {
            SetColours();
            Console.WriteLine(message);
            ResetColours();
        }

        private void SetColours()
        {
            _originalForeground = Console.ForegroundColor;
            _originalBackground = Console.BackgroundColor;
            Console.ForegroundColor = GetCallerColour();
        }

        private void ResetColours()
        {
            Console.ForegroundColor = _originalForeground;
            Console.BackgroundColor = _originalBackground;

        }

        private ConsoleColor GetCallerColour()
        {
            var method = _writeCallerFinder.GetTheMethod() ?? _traceCallerFinder.GetTheMethod();
            var hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(method == null ? "XXXX" : method.Name));
            return _colours[hash[hash.Length/2] % _colours.Length];
        }

    }
}
