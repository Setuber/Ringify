using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Ringify
{
    public class Debugger
    {
        private static string datePatt = "MM/dd/yyyy HH:mm:ss:FF";

        public static void Trace(String i_Message)
        {
            Debug.WriteLine("[{0}] {1}", DateTime.Now.ToString(datePatt), i_Message);
        }

        public static void Trace(String i_Message, params object[] args)
        {
            Debugger.Trace(String.Format(i_Message, args));
        }

        public static void Trace(Exception ex)
        {
            Debugger.Trace(ex.Message);
            Debug.WriteLine("   {0}", ex.Data);
            Debug.WriteLine("{0}", ex.StackTrace);
        }
    }
}
