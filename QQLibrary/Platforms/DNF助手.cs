using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QQLibrary.Platforms
{
    public class DNF助手 : QQPlatformClient, QQWork
    {
        private EventMessage EventCallBack { get; set; }
        private QQBaseClient QQEntity { get; set; }
        private string MsgHeader { get; set; }
        bool QQWork.Init(QQBaseClient _QQEntity, EventMessage _EventCallback, string _MsgHeader)
        {
            MsgHeader = _MsgHeader;
            EventCallBack = _EventCallback;
            QQEntity = _QQEntity;
            //登录
            Log("检查登陆...");
            bool state = QQEntity.CheckLogin();
            if (!state)
            {
                Log("已过期，重新登陆");
                state = QQEntity.Login();
                //刷新资料
                state = QQEntity.CheckLogin();
            }
            
            Log("登录:" + state);
            return state;
        }
        void QQWork.Excute()
        {
            //签到
            活动_每日签到_天天有惊喜();
        }
        string QQWork.ToString()
        {
            return "";
        }
        private void Log(string str)
        {
            EventCallBack(MsgHeader + str);
        }

        private void 活动_每日签到_天天有惊喜()
        {
            string url = "",req = "",resp = "";
            resp = QQEntity.GetWebEntity().UploadString(url, req);
        }
    }
}