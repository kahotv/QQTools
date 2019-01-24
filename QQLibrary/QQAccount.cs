using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public class QQAccount
    {
        public enum QQState
        {
            None,           //普通
            LineOff,        //离线
            LineOn,         //在线
            Error           //错误
        }
        public string QQ { get; private set; }
        public string PWD { get; private set; }

        public QQState State { get; set; }

        public DateTime? UpdateTime { get; private set; }
        private QQAccount() { }
        public QQAccount(string _QQ,string _PWD)
        {
            QQ = _QQ;
            PWD = _PWD;
            State = QQState.None;
        }
        public new string[] ToString ()
        {
            return new string[] { QQ, StateToString(), (UpdateTime == null ? "" : ((DateTime)UpdateTime).ToString("MM-dd HH:mm:ss"))};
        }
        private string StateToString()
        {
            if (State == QQState.None)
                return "";
            if (State == QQState.LineOff)
                return "离线";
            if (State == QQState.LineOn)
                return "在线";
            if (State == QQState.Error)
                return "错误";
            return "异常";
        }
        public void Update()
        {
            UpdateTime = DateTime.Now;
        }
        public void ChangePwd(string pwd)
        {
            PWD = pwd;
            State = QQState.None;
            UpdateTime = null;
        }
    }
}
