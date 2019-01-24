using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQLibrary
{
    public interface ILog
    {
        string GetModuleName();
        void LogBottom(string str);
        void LogList(string str);
    }
    public abstract class LogStatic :ILog
    {
        public static ILog Log { get; private set; }
        static ToolStripStatusLabel TSSL { get;  set; }
        static ListBox LB { get; set; }
        static Form Form { get;  set;}
        public static void SetLogForm(ILog i)
        {
            Log = i;
        }

        public void LogBottom(string str)
        {
            Log.LogBottom(str);
        }
        public void LogList(string str)
        {
            Log.LogList(str);
        }

        public abstract string GetModuleName();
    }
}
