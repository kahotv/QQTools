using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public delegate void EventMessage(string log);
    public interface QQWork
    {
        //初始化
        bool Init(QQBaseClient QQEntity, EventMessage eventCallback, string _MsgHeader);
        //执行工作
        void Excute();
        //获取信息
        string ToString();
    }
}
