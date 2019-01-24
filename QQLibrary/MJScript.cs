using MSScriptControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public class MJScript
    {
        private ScriptControl m_sc = new MSScriptControl.ScriptControl();
        public MJScript(string js)
        {
            m_sc.UseSafeSubset = true;
            m_sc.Language = "JScript";
            m_sc.AddCode(js);
        }

        public string Excute(string js)
        {
            string str = "";
            try
            {
                str = m_sc.Eval(js).ToString();
                return str;
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            return str;
        }

        public bool AddCode(string js)
        {
            try
            {
                m_sc.AddCode(js);
            }
            catch { return false; }

            return true;
        }
    }
}
