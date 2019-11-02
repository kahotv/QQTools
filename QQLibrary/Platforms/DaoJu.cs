using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QQLibrary.Platforms
{
    public class DaoJu : QQPlatformClient,QQWork
    {

        private EventMessage EventCallBack { get; set; }
        private QQBaseClient QQEntity { get; set; }
        private string MsgHeader { get; set; }
        private QQPlatformClient p = new QQPlatformClient()
        {
            Name = "道聚城",
            ID = -1,
            ServiceType = "dj",
            ServiceDepartment = "djc",
        };

        bool QQWork.Init(QQBaseClient _QQEntity, EventMessage _EventCallback,string _MsgHeader)
        {
            MsgHeader = _MsgHeader;
            EventCallBack = _EventCallback;
            QQEntity = _QQEntity;
            //登录
            if(!QQEntity.CheckLogin())
            {
                Log("开始登录...");
                bool state = QQEntity.Login();
                //刷新资料
                state &= QQEntity.CheckLogin();
                Log("登录:" + state);
                return state;
            }else
            {
                return true;
            }

        }
        void QQWork.Excute()
        {
            //获取聚豆
            Log("当前聚豆:" + GetJuDou());
            //每日签到
            Log("签到:" + QianDao());
            //领取签到奖励
            Log("签到奖励:" + QianDaoJiangLi());
            //领取累计签到奖励
            Log("累计签到奖励:" + QianDaoLeiJiJiangLi());
            //每日动态
            Log("每日动态:" + 发布动态and领取奖励());
            //自动点赞
            Log("自动点赞:\n" + 自动点赞10次and领取奖励() + "个聚豆");
            Log("访问好友10次:\n" + 访问关注好友and领取奖励() + "个聚豆");

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
        private int GetJuDou()
        {
            //获取聚豆
            string jd_url = "http://apps.game.qq.com/daoju/appmarket/daoju_promotion/cloud_ticket/QueryCloudTicket.php?acctid=A100078&id=28";
            string jd_result = QQEntity.GetWebEntity().DownloadString(jd_url);
            QQEntity.ExcuteJS(jd_result);
            int jd = 0;
            int.TryParse(QQEntity.ExcuteJS("sCLOUDJF_RES.data.ticket"), out jd);
            return jd;
        }
        private string QianDao()
        {
            int ret = -1;
            string msg = "";
            try
            {
                string resp = QQEntity.AMSRequest("dj", "djc", 11117, 96939);
                //resp = "{\"flowRet\":{\"iRet\":\"0\",\"sMsg\":\"MODULE OK\",\"iAlertSerial\":\"0\",\"sLogSerialNum\":\"AMS-DJ-0820052933-YAi9lh-11117-96939\"},\"modRet\":{\"msg\":\"签到成功\",\"sMsg\":\"签到成功\",\"action_id\":\"7932\",\"ret\":\"0\",\"iRet\":\"0\"},\"ret\":\"0\",\"msg\":\"\"}";
                JObject j_resp = JObject.Parse(resp);
                ret = (int)j_resp["ret"];
                msg = (string)j_resp["msg"];
                if(ret == 0)
                {
                    //获取活动执行状态 签到结果
                    ret = (int)j_resp["modRet"]["iRet"];
                    msg = (string)j_resp["modRet"]["sMsg"];
                }
            }
            catch { }

            return msg;
        }
        private string QianDaoJiangLi()
        {
            int ret = -1;
            string msg = "";
            try
            {
                string resp = QQEntity.AMSRequest("dj", "djc", 11117, 96910);
                JObject j_resp = JObject.Parse(resp);

                ret = (int)j_resp["ret"];
                msg = (string)j_resp["msg"];
                if (ret == 0)
                {
                    //获取活动执行状态 领取签到奖励
                    ret = (int)j_resp["modRet"]["iRet"];
                    msg = (string)j_resp["modRet"]["sMsg"];
                }
            }
            catch { }
            return msg;
        }
        private string QianDaoLeiJiJiangLi()
        {
            int count_judou = 0;
            Dictionary<string, int> mapLeiJi = new Dictionary<string, int>()
            {
                { "累计5天奖励",322021 },
                { "累计7天奖励",322036 },
                { "累计10天奖励",322037 },
                { "累计15天奖励",322038 },
                { "累计20天奖励",322039 },
                { "累计25天奖励",322040 }
            };
            string url = "", req = "", resp = "";
            req = "appSource=ios";
            req += "&appSourceDetail=ios10.3.3";
            req += "&appVersion=59";
            req += "&sDeviceID=" + QQBaseInfo.dev_id;
            req += "&p_tk=" + QQEntity.G_TK;

            #region 一些测试数据
            /*
            //尝试获取会员信息
            url = "https://apps.game.qq.com/daoju/v3/api/we/member/Member.php?";
            req = "optype=get_baseinfo";
            req += "&appid=1001";
            req += "&output_format=json";
            resp = QQEntity.GetWebEntity().DownloadString(url + req);
            */
            //登陆test1
            //url = "https://djcapp.game.qq.com/daoju/v3/api/message/IMLogin.php?";
            //resp = QQEntity.GetWebEntity().DownloadString(url + req);
            //注册规则
            url = "https://djcapp.game.qq.com/daoju/v3/api/app/SignRewardRules.php?";
            resp = QQEntity.GetWebEntity().DownloadString(url + req);

            int recount = 3;
            while (recount-- > 0)
            {
                //登陆test2
                url = "https://djcapp.game.qq.com/daoju/v3/api/app/e_app/add_jf_firstlogin.php?";
                resp = QQEntity.GetWebEntity().DownloadString(url + req);
                JObject jresp = JObject.Parse(resp);
                if ((string)jresp["firstlogin"] == "yes")
                {
                    Console.WriteLine("未登录APP，重试中..." + recount);
                    Thread.Sleep(1000);
                }
                else
                    break;
            }

            #endregion
            //登陆test3 设置APP登陆状态
            //url = "https://djcapp.game.qq.com/daoju/v3/api/moment/uvs/UvsSign.php?";
            //resp = QQEntity.GetWebEntity().DownloadString(url + req);


            foreach (var kv in mapLeiJi)
            {
                count_judou += QianDaoLeiJiJiangLi(kv.Key, kv.Value);
            }

            return "共领取" + count_judou + "个聚豆";
        }
        private int QianDaoLeiJiJiangLi(string name = "", int id = 0)
        {
            int num_jd = 0;
            int ret = -1;

            Dictionary<string, int> mapLeiJiNum = new Dictionary<string, int>()
            {
                { "累计5天奖励",5 },
                { "累计7天奖励",15 },
                { "累计10天奖励",20 },
                { "累计15天奖励",25 },
                { "累计20天奖励",30 },
                { "累计25天奖励",50 }
            };

            try
            {
                //遍历奖励列表，依次尝试领取
                string resp = QQEntity.AMSRequest("dj", "djc", 11117, id);
                //modRet.jData字段需要特殊处理
                if (resp.Contains("\"jData\""))
                {
                    resp = resp.Replace("\"jData\":\"", "\"jData\":").Replace("null}\"}", "null}}");
                }
                JObject j_resp = JObject.Parse(resp);

                ret = (int)j_resp["ret"];
                if (ret == 0)
                {
                    //获取活动执行状态 领取签到奖励
                    ret = (int)j_resp["modRet"]["jData"]["result"];
                    string rlist = (string)j_resp["modRet"]["jData"]["list"];
                    //分割rlist得到本次奖励
                    string[] list_arr = rlist.Split(new char[] {'|',' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //num_jd = int.Parse(list_arr[1]);
                    if (list_arr.Length ==4)
                        num_jd = mapLeiJiNum[name];
                }else if(ret == 700)
                {
                    string msg = (string)j_resp["flowRet"]["sMsg"];
                    Console.WriteLine(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return num_jd;
        }

        private int 发布动态and领取奖励()
        {
            int bean = 0;
            string url = "", req = "", resp = "";
            try
            {
                //发布动态
                url = "https://djcapp.game.qq.com/daoju/djcapp/v5/moment/PostCreate.php?";
                req = "_output_fmt=json";
                req += "&sTalk=" + Guid.NewGuid().GetHashCode();    //动态内容
                req += "&sTopic=";
                req += "&iType=3";
                req += "&sPhotos=";
                req += "&inform=";
                req += "&sBizCode=dnf";
                req += "&sRoleName=";
                req += "&iZoneId=";
                req += "&iReward=";
                req += "&appSource=ios";
                req += "&appSourceDetail=ios10.3.3";
                req += "&appVersion=64";
                req += "&sDeviceID=" + QQBaseInfo.dev_id;
                req += "&p_tk=" + QQEntity.G_TK;
                resp = QQEntity.GetWebEntity().DownloadString(url + req);
                JObject jresp = JObject.Parse(resp);
                int ret = (int)jresp["ret"];
                if (ret == 0)
                {
                    //发布成功，领取奖励
                    bean = (int)jresp["data"]["bean"];
                }
            }
            catch { }
            return bean;
        }
        private void 道具许愿and领取奖励()
        {

        }
        private int 自动点赞10次and领取奖励()
        {
            int jd = 0;
            int ret = 0;
            string req = "", resp = "", url = "";
            try
            {
                //动态首页
                url = "https://djcapp.game.qq.com/daoju/djcapp/v5/action/home_post_list.php?";
                req = "page_size=10";
                req += "&op_type=2";
                req += "&cursor=0";
                req += "&biz=dnf";
                req += "&appSource=ios";
                req += "&appSourceDetail=ios10.3.3";
                req += "&appVersion=64";
                req += "&sDeviceID=" + QQBaseInfo.dev_id;
                req += "&p_tk=" + QQEntity.G_TK;
                resp = QQEntity.GetWebEntity().DownloadString(url + req);
                JObject jobj = JObject.Parse(resp);
                ///取出顶项
                int bottom_cursor = int.Parse((string)jobj["data"]["bottom_cursor"]);
                int server_time = int.Parse((string)jobj["serverTime"]);
                //点赞十个（最多尝试30次，每次间隔500~1000毫秒）
                Random r = new Random(server_time);
                int succ_count = 0;
                int i = 0;
                for(; succ_count < 10 && i < 30; i++)
                {
                    try
                    {
                        url = "https://djcapp.game.qq.com/daoju/djcapp/v5/moment/PostPraise.php?";
                        req = "lPostId=" + (bottom_cursor - i);
                        req += "&iPraise=0";
                        req += "&_output_fmt=json";
                        req += "&appSource=ios";
                        req += "&appSourceDetail=ios10.3.3";
                        req += "&appVersion=64";
                        req += "&sDeviceID=" + QQBaseInfo.dev_id;
                        req += "&p_tk=" + QQEntity.G_TK;
                        resp = QQEntity.GetWebEntity().DownloadString(url + req);
                        jobj = JObject.Parse(resp);
                        ret = (int)jobj["ret"];
                        if (ret == 0)
                        {
                            succ_count++;
                            Console.Write("成功 ");
                        }
                        else
                        {
                            Console.Write("失败 ");
                        }
                        Thread.Sleep(r.Next(300, 500));
                    } catch { }
                }
                if(succ_count >= 10)
                {
                    Console.WriteLine("共尝试" + i + "次，成功点赞" + succ_count + "个");
                }
                //不管超没超过十个，都尝试领取奖励
                url = "https://apps.game.qq.com/daoju/v3/api/we/usertaskv2/Usertask.php?";
                req = "optype=receive_usertask";
                req += "&appid=1001";
                req += "&output_format=json";
                req += "&iruleId=100003";
                req += "&bizcode=";
                req += "&appSource=ios";
                req += "&appSourceDetail=ios10.3.3";
                req += "&appVersion=64";
                req += "&sDeviceID=" + QQBaseInfo.dev_id;
                req += "&p_tk=" + QQEntity.G_TK;
                resp = QQEntity.GetWebEntity().DownloadString(url + req);
                jobj = JObject.Parse(resp);
                ret = (int)jobj["ret"];
                string msg = (string)jobj["msg"];
                //显示得到的数据
                JArray jitems = (JArray)jobj["data"]["ams_data"]["modRet"]["all_item_list"];
                jd += ShowItemList(jitems);
                if (ret != 0)
                {
                    throw new Exception(msg);
                }

            }
            catch (Exception e){ Console.WriteLine("领取点赞奖励异常:" + e.Message); }
            return jd;
        }
        private int ShowItemList(JArray jarr)
        {
            int jd = 0;
            for (int i = 0; i < jarr.Count; i++)
            {
                int num = (int)jarr[i]["iItemCount"];
                string name = (string)jarr[i]["sItemName"];
                Console.WriteLine("奖励:" + name + "+" + num);
                if (name.Contains("聚豆"))
                    jd += num;
            }
            return jd;
        }
        private int 访问关注好友and领取奖励()
        {
            int jd = 0;
            JObject j = null;
            string req = "", url = "", resp = "";
            try
            {
                //获取关注列表
                url = "https://djcapp.game.qq.com/daoju/djcapp/v5/relation/RelationApp.php?";
                req = "s_type=attentionList";
                req += "&userid=" + QQEntity.QQ;
                req += "&page=1";
                req += "&appSource=ios";
                req += "&appSourceDetail=ios10.3.3";
                req += "&appVersion=64";
                req += "&sDeviceID=" + QQBaseInfo.dev_id;
                req += "&p_tk=" + QQEntity.G_TK;
                resp = QQEntity.GetWebEntity().DownloadString(url + req);
                j = JObject.Parse(resp);
                int ret = (int)j["ret"];
                if (ret != 0)
                    throw new Exception((string)j["msg"]);
                JArray jarr = (JArray)j["data"]["attention"];
                //访问10个
                if (jarr.Count < 10)
                    throw new Exception("好友数量不足10个:" + jarr.Count);

                int err_max = 5;

                url = "https://djcapp.game.qq.com/daoju/djcapp/v5/solo/query_member.php?";
                req = "appSource=ios";
                req += "&appSourceDetail=ios10.3.3";
                req += "&appVersion=64";
                req += "&sDeviceID=" + QQBaseInfo.dev_id;
                req += "&p_tk=" + QQEntity.G_TK;
                for (int i = 0;i < jarr.Count && i < 10 && err_max > 0; i++)
                {
                    j = (JObject)jarr[i];
                    string lUin = (string)j["lUin"];

                    resp = QQEntity.GetWebEntity().DownloadString(url + req + "&uin=" + lUin);
                    j = JObject.Parse(resp);
                    if (ret != 0)
                    {
                        Console.WriteLine("访问" + lUin + "失败重试(" + err_max + ")");
                        err_max--;
                        i--;
                        continue;
                    }
                    Console.WriteLine("访问成功:" + (string)j["intimate"]["add_msg"]);
                }

                //领取奖励
                url = "https://apps.game.qq.com/daoju/v3/api/we/usertaskv2/Usertask.php?";
                string req2 = "&output_format=json&optype=receive_usertask&appid=1001&iruleId=100004";

                resp = QQEntity.GetWebEntity().DownloadString(url + req + req2);
                j = JObject.Parse(resp);
                ret = (int)j["ret"];
                if(ret == 0)
                {
                    //遍历奖励列表
                    jarr = (JArray)j["data"]["ams_data"]["modRet"]["all_item_list"];
                    jd+=ShowItemList(jarr);
                }
                else
                {
                    //打印错误信息
                    Console.WriteLine((string)j["msg"]);
                }
            } catch(Exception e)
            {
                Console.WriteLine("访问好友异常:" + e.Message);
            }

            return jd;
        }

        #endregion
        private void InitActivity()
        {
            p.ActivityList = new List<QQActivity>()
            {

                new QQActivity()
                {
                    Name = "道聚城-周年庆",
                    ActiveId = 121569,
                    FlowList = new List<QQActivity>()
                    {

                        new QQActivity() { Name = "获取资格", ActiveId = 395651},
                        new QQActivity() { Name = "获取资格2", ActiveId = 398390},
                        new QQActivity() { Name = "签到", ActiveId = 395618},
                    }
                }
       
          
            };
        }
    }
}
