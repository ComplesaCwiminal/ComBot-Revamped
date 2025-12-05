using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComBot_Revamped
{
    internal static class StyleUtils
    {
        public static string DefaultTitle;

        public static void resetStyle(bool setTitle = false)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            
            // Title can only be read on windows.
            Console.Title = !System.OperatingSystem.IsWindows() || setTitle ? DefaultTitle : Console.Title;
        }
    }
}
