using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQLibrary.Platforms
{
    public class xinyue : QQPlatformClient, QQWork
    {
        public const string stype = "";
        public const string dtype = "";
        private int wordid = 0;
        private QQPlatformClient p = new QQPlatformClient()
        {
            Name = "心悦",
            ID = -1,
            ServiceType = "tgclub",
            ServiceDepartment = "xinyue",
        };

        private EventMessage EventCallBack { get; set; }
        private QQBaseClient QQEntity { get; set; }
        private string MsgHeader { get; set; }
        bool QQWork.Init(QQBaseClient _QQEntity, EventMessage _EventCallback, string _MsgHeader)
        {
            MsgHeader = _MsgHeader;
            EventCallBack = _EventCallback;
            QQEntity = _QQEntity;
            if (!QQEntity.CheckLogin())
            {
                //登录
                Log("开始登录...");
                bool state = QQEntity.Login();
                //刷新资料
                state &= QQEntity.CheckLogin();
                Log("登录:" + state);
                return state;
            }
            else
                return true;

        }


        void QQWork.Excute()
        {
            //获取worldid
            wordid = GetWorldID();
            if(wordid <= 0)
            {
                Console.WriteLine("Error: 心悦-勇士之路:未绑定");
            }
            InitActivity();
            QQEntity.AMSAuto(p);
        }
        string QQWork.ToString()
        {
            return "";
        }
        private void Log(string str)
        {
            EventCallBack(MsgHeader + str);
        }
        #region 功能

        private string QianDao()
        {
            int ret = -1;
            string resp = QQEntity.AMSRequest(stype, dtype, 116090, 378208);
            JObject jresp = JObject.Parse(resp);
            ret = (int)jresp["ret"];
            resp = (string)jresp["modRet"]["sPackageName"] + ": " + (string)jresp["modRet"]["sMsg"];
            return resp;
        }
        private int GetWorldID()
        {
            int id = 0;

            try
            {
                string str = QQEntity.AMSRequest(stype, dtype, 117729, 383133);
                JObject obj = JObject.Parse(str);
                id = (int)obj["modRet"]["jData"]["data"]["Farea"];
            }catch
            { }

            return id;
        }
        private void 分享到QQ空间()
        {
            string url = "https://pingfore.qq.com/pingd?";
            string get_param = "dm=xinyue.qq.com.hot&url=/act/pc/20170702week/index.html&hottag=dcv_pc.20170720week.index_staytime5_single_%E6%95%B4%E4%B8%AA%E9%A1%B5%E9%9D%A2.adtag_act_top_nav&hotx=9999&hoty=9999&rand=16123";
            QQEntity.GetWebEntity().UploadString(url + get_param,"");
            url = "http://xinyue.qq.com/act/pc/20170702week/index.html?ADTAG=act.top.nav.dayuegu";
            QQEntity.GetWebEntity().DownloadString(url);
        }
        private void InitActivity()
        {
            string[] str = new string[] { "worldid=" + wordid };
            p.ActivityList = new List<QQActivity>()
            {
                
                new QQActivity()
                {
                    Name = "心悦-大悦谷-淬金池",
                    ActiveId = 116090,
                    FlowList = new List<QQActivity>()
                    {
                        new QQActivity() { Name = "签到", ActiveId = 378208, OtherParams = new string[] { "param=620717","source=1","" } },
                        //new QQActivity() {Name = "分享给好友", ActiveId=380706 ,OtherParams = new string[] {"param=620711"}, ActionList = new List<Action>() { 分享到QQ空间 } },
                        //new QQActivity() {Name = "分享到QQ空间",ActiveId = 380706,OtherParams = new string[] { "param=620710" } }
                    }
                },
                new QQActivity()
                {
                    Name = "心悦-勇士币",
                    ActiveId = 110177,
                    FlowList = new List<QQActivity>()
                    {
                        new QQActivity() {Name = "每日抽奖",ActiveId = 359785 },        //抽两次
                        new QQActivity() {Name = "每日抽奖",ActiveId = 359785 }         //抽两次
                    }
                },
                new QQActivity()
                {
                    Name = "心悦-勇士之路",
                    ActiveId = 117729,
                    FlowList = new List<QQActivity>()
                    {
                         new QQActivity() {Name = "每周一测",ActiveId = 383114, OtherParams = str},
                         new QQActivity() {Name = "A级礼包",ActiveId = 383825, OtherParams = str},
                         new QQActivity() {Name = "S级礼包",ActiveId = 383828, OtherParams = str},
                         new QQActivity() {Name = "至强礼包",ActiveId = 383829, OtherParams = str},
                    }
                },
                new QQActivity()
                {
                    Name = "心悦-每日任务",
                    ActiveId = 54842,
                    FlowList = new List<QQActivity>()
                    {
                        new QQActivity() {Name = "通过地下城四次(双倍)",ActiveId = 282373  },
                        new QQActivity() {Name = "通过地下城四次",ActiveId = 281054  },
                        new QQActivity() {Name = "安图日常三次",ActiveId = 319631  },
                        new QQActivity() {Name = "游戏内消耗疲劳值120",ActiveId = 280666  },
                        new QQActivity() {Name = "游戏内PK3次",ActiveId = 280671  },
                        new QQActivity() {Name = "安图日常两次",ActiveId = 319630  },
                        new QQActivity() {Name = "游戏内消耗疲劳值50",ActiveId = 280664  },
                        new QQActivity() {Name = "游戏内在线时长40",ActiveId = 280663  },
                        new QQActivity() {Name = "游戏内PK2次",ActiveId = 280670  },
                        new QQActivity() {Name = "游戏内在线时长30",ActiveId = 280660  },
                        new QQActivity() {Name = "邮箱无未读邮件",ActiveId = 280631  },
                        new QQActivity() {Name = "通关绝望之塔",ActiveId = 327341  },
                        //new QQActivity() {Name = "游戏内消耗疲劳值10",ActiveId = 280499  },
                        //new QQActivity() {Name = "游戏内在线时长15", ActiveId=280463 },
                        //new QQActivity() {Name = "赠送双倍卡", ActiveId = 280743 },
                        //new QQActivity() {Name = "游戏内PK1次",ActiveId = 280505  },
                        //new QQActivity() {Name = "独立kill怪物",ActiveId = 280638  },
                        //new QQActivity() {Name = "每日登录抽奖任务",ActiveId = 280706  },
                        //new QQActivity() {Name = "通关异界两次",ActiveId = 319591  },
                        //new QQActivity() {Name = "通关异界三次",ActiveId = 319617  },
                        //new QQActivity() {Name = "安图日常一次",ActiveId = 319629  },
                        //
                    }
                }
            };
        }
        #endregion

    }
}
