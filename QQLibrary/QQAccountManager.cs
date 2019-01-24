using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public static class QQAccountManager
    {
        public static List<QQAccount> QQ { get; private set; }

        public static void FormJsonString(string json)
        {
            JArray array = JArray.Parse(json);
            QQ = new List<QQAccount>();
            foreach (var qq in array)
            {
                string qqString = qq["QQ"].ToString();
                if (CheckAccount(qqString))
                {
                    QQ.Add(new QQAccount(qqString, qq["PWD"].ToString()));
                }
            }
        }
        public static int Add(string qq, string pwd)
        {
            if (!CheckAccount(qq))
                return 0;

            int num = 0;
            lock (QQ)
            {
                List<QQAccount> search = QQ.Where(p => p.QQ == qq).ToList();
                if (search.Count > 0)
                {
                    //已存在，修改密码
                    search[0].ChangePwd(pwd);
                    num = -1;
                }
                else
                {
                    QQ.Add(new QQAccount(qq, pwd));
                    num = 1;
                }
            }
            return num;
        }
        public static int RemoveAll(List<string> qq_array)
        {
            int num = 0;
            lock (QQ)
            {
                foreach(var qq in qq_array)
                {
                    if (CheckAccount(qq))
                    {
                        num += QQ.RemoveAll(p => p.QQ == qq);
                    }
                }
            }
            return num;
        }

        //qq号格式验证，返回是否验证成功
        public static bool CheckAccount(string qq)
        {
            bool state = true;

            //1.数字

            try
            {
                if (Convert.ToInt64(qq) <= 0)
                    state = false;
            }
            catch
            {
                try
                {
                    MailAddress mail = new MailAddress(qq);
                    if (string.IsNullOrWhiteSpace(mail.Address))
                        state = false;
                }
                catch
                {
                    state = false;
                }
            }

            return state;
        }
    }
}
