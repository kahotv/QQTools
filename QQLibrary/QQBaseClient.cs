using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQLibrary
{
    /*
        1.QQ的基本登陆,只支持无验证码、普通验证码两种类型
    */
    public class QQBaseInfo
    {
        public static string appid = "636014201";
        public static string js_ver = "10228";
        public static string js_type = "1";
        public static string u1 = "http://www.qq.com/qq2012/loginSuccess.htm";
        public static string pt_uistyle = "40";
        public static string pt_jstoken = "3597461747";

        public static string ptlang = "2052";
        public static string login_base_action = "1-25-1503167858533";
        public static string dev_id = "CEC8975B-14A2-4268-9AB1-7C3A05C2D89F";
    }
    public class QQBaseClient
    {
        //基础属性
        public string QQ { get; private set; }
        public string QQHexString { get; private set; }
        public string Password { get; private set; }
        public string PTVFSession { get; private set; }

        public string Nick { get; private set; }
        public string VIPLevel { get; private set; }
        private string VCode { get; set; }
        public long G_TK { get; private set; }
        public string VSIG { get; private set; }
        public DateTime? UpdateTime { get; private set; }
        //工具
        protected QQWebClient Web = new QQWebClient();
        protected MJScript Script = new MJScript(File.ReadAllText(Application.StartupPath + "\\c_login_2_Encription.js"));

        public QQBaseClient(string qq, string pwd)
        {
            QQ = qq;
            QQHexString = 
            Password = pwd;
            QQActivity z = new QQActivity();
        }

        public void WebHeaderAddPost()
        {
            Web.Headers.Add("Content-type: application/x-www-form-urlencoded");
        }
        public void WebEncodingUTF8()
        {
            Web.Encoding = Encoding.UTF8;
        }
        public void WebEncodingGB2312()
        {
            Web.Encoding = Encoding.GetEncoding("gb2312");
        }
        public WebClient GetWebEntity()
        {
            return Web;
        }
        public string ExcuteJS(string js)
        {
            return Script.Excute(js);
        }
        /// <summary>
        /// 检查是否已经登录，通过获取QQ信息实现
        /// </summary>
        /// <returns></returns>
        public bool CheckLogin()
        {
            return UpdateQQInfo();
        }
        private bool UpdateQQInfo()
        {
            bool err = true;
            try
            {
                var cookies = Web.Cookies.GetCookies(new Uri("http://ptlogin2.qq.com"));
                string skey = cookies["skey"].Value;
                //刷新G_TK
                G_TK = Get_g_tk(skey);
                /*  检查是否已经登录
                 *  Req:GET
                 *      Url:http://qfwd.qq.com/
                 *      Params:
                 *          uin=792272773
                 *          skey=@63fqnhk9A
                 *          func=loginAll
                 *          refresh=0
                 *          callback=loginAll
                 *  Resp:
                 *      loginAll({"result":"0","nick":"Yui","Vip":"8","Face":"http://thirdqq.qlogo.cn/g?b=sdk&k=5ib7NuINKMmpQsTmkpu52Fg&s=40&t=1502645806","info":{"QZone":"","Friend":{"totalnum":"5","passivenum":"","initnum":"","aboutnum":"5"},"QMail":"","Mblog":"","Article":""}});
                 */

                string url = "http://qfwd.qq.com/?";
                string req = string.Format("uin={0}&skey={1}&func=loginAll&refresh=0&callback=loginAll", QQ, skey);
                string resp = Web.DownloadString(url + req);
                if (string.IsNullOrWhiteSpace(resp) || !resp.StartsWith("loginAll"))
                    throw new Exception("未登录");

                string json = resp.Replace("loginAll(", "").Replace(");", "");
                //分析Json，得到QQ信息
                JObject j_info = JObject.Parse(json);
                
                Nick = (string)j_info["nick"];
                VIPLevel = (string)j_info["Vip"];
                if (string.IsNullOrWhiteSpace(VIPLevel))
                    VIPLevel = "0";
                err = false;
            }
            catch { }

            return !err;
        }
        public bool Login()
        {
            WebEncodingUTF8();

            bool err = true;
            try
            {
                /*  打开主页登陆框
                 *  Req:GET
                 *      Url:http://xui.ptlogin2.qq.com/cgi-bin/xlogin
                 *      Params:
                 *          appid = QQBaseInfo.appid
                 *  Resp:
                 *      Set - Cookie: pt_user_id = 16582471178313794042; EXPIRES = Tue, 17 - Aug - 2027 18:39:28 GMT; PATH =/; DOMAIN = ui.ptlogin2.qq.com;
                 *      Set - Cookie: pt_login_sig = zZBaVCeHiH78MKJcZ9lt5YyyDAg0Q4V7eIL2boSXgvzSW6BKl - bgkCoiDhwjtc5J; PATH =/; DOMAIN = ptlogin2.qq.com;
                 *      Set - Cookie: pt_guid_sig = efad36621940155454db520bc2d1516923e4d69bf152f78d9b12bd12184f3ca9; EXPIRES=Mon, 18-Sep-2017 19:56:46 GMT; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *      Set - Cookie: pt_clientip = 60c6abd488a1416c; PATH =/; DOMAIN = ptlogin2.qq.com;
                 *      Set - Cookie: pt_serverip = 9d120abf0662c1dd; PATH =/; DOMAIN = ptlogin2.qq.com;
                 *      Set - Cookie: uikey = c769b6dbea08911979ee778c0636ec609103b26768351c6ac1004a059ecb0ebc; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *      Set - Cookie: ptui_identifier = 000DCAE38BCA4D194AEA1CC2D61F95011AC34D1D7A04429243D562AE27; PATH =/; DOMAIN = ui.ptlogin2.qq.com;
                */
                string resp = Web.DownloadString("http://xui.ptlogin2.qq.com/cgi-bin/xlogin?hide_title_bar=0&low_login=0&qlogin_auto_login=1&no_verifyimg=1&link_target=blank&appid=" + QQBaseInfo.appid + "&target=self&s_url=" + WebUtility.HtmlEncode("http://www.qq.com/qq2012/loginSuccess.htm"));
                var cookies = Web.Cookies.GetCookies(new Uri("http://ptlogin2.qq.com"));

                /*  获取验证码
                 *  Req:GET
                 *      Url:http://check.ptlogin2.qq.com/check
                 *      Params:
                 *          uin=792272773
                 *          appid=636014201
                 *          js_ver=10228
                 *          js_type=1
                 *          login_sig=zZBaVCeHiH78MKJcZ9lt5YyyDAg0Q4V7eIL2boSXgvzSW6BKl-bgkCoiDhwjtc5J
                 *          u1=http%3A%2F%2Fwww.qq.com%2Fqq2012%2FloginSuccess.htm
                 *          r=0.10681139230384162
                 *          pt_uistyle=40
                 *          pt_jstoken=3597461747
                 *  Resp:
                 *      Params:
                 *                            验证码       QQ号Hex                          ptvfsession
                 *          ptui_checkVC('0','!JGB','\x00\x00\x00\x00\x2f\x39\x1f\x85','a1b9df50cb4d14f9f035eb0fd5b46e2240eb3e5b71544663d0e46574665a7efc679776ef7510cdbb0a9e13c8ff8c0d701e290f52694264ca','2');
                 *      Cookie:
                 *          Set-Cookie: confirmuin=0; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: ptvfsession=a1b9df50cb4d14f9f035eb0fd5b46e2240eb3e5b71544663d0e46574665a7efc679776ef7510cdbb0a9e13c8ff8c0d701e290f52694264ca; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: ptdrvs=Wmwp2BC1zaTv*dwFi6nvdHxDpVtJ*ov8buHYD740wAI_; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: ptisp=ctc; PATH=/; DOMAIN=qq.com;
                 *      Content-Type: application/x-javascript; charset=utf-8
                 *  
                */
                string url = "http://check.ptlogin2.qq.com/check?";
                string req = "regmaster=&pt_tea=2&pt_vcode=1";
                req += "&uin=" + QQ;
                req += "&appid=" + QQBaseInfo.appid;
                req += "&js_ver=" + QQBaseInfo.js_ver;
                req += "&js_type=" + QQBaseInfo.js_type;
                req += "&login_sig=" + cookies["pt_login_sig"].Value;
                req += "&u1=http%3A%2F%2Fwww.qq.com%2Fqq2012%2FloginSuccess.htm";
                req += "&r=0." + Guid.NewGuid().GetHashCode();
                req += "&pt_uistyle=" + QQBaseInfo.pt_uistyle;
                req += "&pt_jstoken=" + QQBaseInfo.pt_jstoken;
                resp = Web.DownloadString(url + req);
                if (!resp.StartsWith("ptui_checkVC"))
                    throw new Exception("基础登录:获取验证码失败");
                //Console.WriteLine(resp);
                //处理验证码的问题，并得到QQ号hex、验证码、pt_verifysession
                bool need_vcode = ProcessLoginData(resp);
                if(string.IsNullOrWhiteSpace(PTVFSession))
                    throw new Exception("基础登录:PTVFSession为空");
                /*  登录
                 *  Req:GET
                 *      Url:http://ptlogin2.qq.com/login
                 *      Params:
                 *          u	792272773
                 *          verifycode	!JGB
                 *          pt_vcode_v1	0
                 *          pt_verifysession_v1	a1b9df50cb4d14f9f035eb0fd5b46e2240eb3e5b71544663d0e46574665a7efc679776ef7510cdbb0a9e13c8ff8c0d701e290f52694264ca
                 *          p	gfUI4u40pf6qKQtKXgjEQ3-gwJocDFndTjOFERVpQiy4gx4vLZFOoK-fJjRGPKeZT3PjV8Vdf9urGytmLXoRbLgJMXyRNjjumhG1Rp2f-x0N7l6-n3Ci4-*Wd8k0qY2xCXvLbtEuupBKAmOzzijcaAx6fU5NR7h9MH1S4sS1P4T8i*DZzfvQMmIp8boT92nBmi40W2gRCZ3Mv8uXNFF*sTpmwHHrEdui5MIrxtyp1JIhSfoyw3QBWyojpsIXDZR1k6wLPPPlsDIcTD6kJvFQkvJaKkJS3OjRkyp9NhtRHVwIcYwXCKbRp4pwg-cHHy6LVoLxns1jc7IcUQJ1HvQ8LQ__
                 *          pt_randsalt	2
                 *          pt_jstoken	3597461747
                 *          u1	http://www.qq.com/qq2012/loginSuccess.htm
                 *          ptredirect	0
                 *          h	1
                 *          t	1
                 *          g	1
                 *          from_ui	1
                 *          ptlang	2052
                 *          action	1-25-1503167858533
                 *          js_ver	10228
                 *          js_type	1
                 *          login_sig	zZBaVCeHiH78MKJcZ9lt5YyyDAg0Q4V7eIL2boSXgvzSW6BKl-bgkCoiDhwjtc5J
                 *          pt_uistyle	40
                 *          aid	636014201
                 *          pt_guid_sig	516706DD579BA01B4A20B31F56E70BFB6AB2E9A0A698C4B79100E7BE686ABBDBD42C2D6ABEF18897
                 * Resp:
                 *      Cookies:
                 *          Set-Cookie: pt_recent_uins=72298591a589c1a3e067ec9d47a0ef09d52c843a4cbcc10814e55e1ba2a89c8042c08ce81148ddb7a2fb6bd2695dd6d759e79c76d84156f4; EXPIRES=Mon, 18-Sep-2017 18:39:36 GMT; PATH=/; DOMAIN=ptlogin2.qq.com; HttpOnly
                 *          Set-Cookie: pt2gguin=o0792272773; EXPIRES=Fri, 02-Jan-2020 00:00:00 GMT; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: uin=o0792272773; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: skey=@63fqnhk9A; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: ETK=; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: superuin=o0792272773; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: superkey=erb3GkiKw3iupMO6GX2PfstqhNuR*YICDJ1rSsVTTYw_; PATH=/; DOMAIN=ptlogin2.qq.com; HttpOnly
                 *          Set-Cookie: supertoken=1949129067; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: ptisp=ctc; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: RK=2SEPqEMPRj; EXPIRES=Tue, 17-Aug-2027 18:39:36 GMT; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: ptnick_792272773=597569; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: ptcz=361841a2b357cd1b15fdb67bef7e8c1c5c3cdca2cd943cc1582814a6bf656e4f; EXPIRES=Fri, 02-Jan-2020 00:00:00 GMT; PATH=/; DOMAIN=qq.com;
                 *          Set-Cookie: ptcz=; EXPIRES=Fri, 02-Jan-1970 00:00:00 GMT; PATH=/; DOMAIN=ptlogin2.qq.com;
                 *          Set-Cookie: airkey=; EXPIRES=Fri, 02-Jan-1970 00:00:00 GMT; PATH=/; DOMAIN=qq.com;
                 */

                //使用验证码来加密密码
                string func_encrypt_pwd = string.Format("$.Encryption.getEncryption('{0}',$.str.uin2hex('{1}'),'{2}',undefined)", Password, QQ, VCode);
                string enc_pwd = Script.Excute(func_encrypt_pwd);
                string pt_guid_sig = cookies["pt_guid_sig"].Value;
                url = "http://ptlogin2.qq.com/login?";
                req = "u=" + QQ;
                req += "&verifycode=" + VCode;
                req += "&pt_vcode_v1=" + (need_vcode ? 1 : 0);
                req += "&pt_verifysession_v1=" + PTVFSession;
                req += "&p=" + enc_pwd;
                req += "&pt_randsalt=2";
                req += "&pt_jstoken=" + QQBaseInfo.pt_jstoken;
                req += "&u1=http://www.qq.com/qq2012/loginSuccess.htm";
                req += "&ptredirect=0";
                req += "&h=1&t=1&g=1";
                req += "&from_ui=1";
                req += "&ptlang=" + QQBaseInfo.ptlang;
                req += "&action=" + QQBaseInfo.login_base_action;
                req += "&js_ver=" + QQBaseInfo.js_ver;
                req += "&js_type=" + QQBaseInfo.js_type;
                req += "&login_sig=" + cookies["pt_login_sig"].Value;
                req += "&pt_uistyle=" + QQBaseInfo.pt_uistyle;
                req += "&aid=" + QQBaseInfo.appid;
                req += "&pt_guid_sig=" + pt_guid_sig;

                resp = Web.DownloadString(url + req);
                if (!resp.Contains("登录成功"))
                    throw new Exception("基础登录:登录失败");
                if (resp.Contains("mibao_vry"))
                {
                    throw new Exception("基础登录:失败,需要密保");
                }
                err = false;
                /*
acaacd	OD6q9t0AraWJf+dtq0j8VjD7hxC7fEEIl1sNPiI5trM2gV+IXZmJpGjGUep2ppDVOu0cwlGKro583iKSwR7Qx/EtWHfgQqTJAZiFlL4laI8XKGi/GDtxkSZGEmZQ4mYtdxR2RoEtFM8Icq3WSSzN/IbAy4jFJAWp7ZXglf/k3wz8mO3MOZZ7343OdAL2IVXzn+hHdtedPQ85aKai4Y2iqYO7kzTlm/ZP/4IBeSAaqCcnr6pqXnRH8s2bPMs/w2IL0RTELFJ6uYa5V5+IjtG/ILgjVdS36HD8qKLf1Bi9Eze04B082IyAmgvWinhIf9aiL9bKaMfceWLebcBWPUZGLRpjwcfX03+PKiGoTBjZbQi4hmckUfjggHje9937ap8aoLwaBgxCVCJAbYtjoJtkomoTdt6CBOyst16ivYV79ScrQFUcuoayTjvL4wYRW8wHDYYjlbz4+2ghT6blec1FQAIlyv9xUlG5X42NbMhmgoNYVRzr2EzTd+bfc0V4ROH4TpYd7IZcnFn3jvvxP7ylFLj9qzAk4qoZfnGl8UqE2mDZly4/Igca2RoTRphMwUQFCeSHnbJ6zoY79JiJS3ebfrE914LchqHqV6aHKUv85igjyKI9nnx1ZNFxpxF5FvmdF4AFPFU4p+Qh7ZyCc4OYw0qY4BW2O1vf5RWIx4O3DSjnYQcsutkvSCwynDAtQ+J2NmlNzZsDT2TjN92eroLQ+baaUkCicAWXNXaSS3S9z4emqYQCDJKl46OWw1SiYYeJqbKOTt/bEzxuO7UfudsMR3xqq7I84hDXXqTngU3WUVj3krLkSUkXseSaWPuIW3m5ClIzKdPbsghNWlhdqZa8Ex4sOY0BP9tuLsvtoRaAjvwYakS61ZaU/d/l/spvwyUMkozFfgx521mrExkliyo/ExGiNXYD2TlTLZzjZsEK0eIMPXJiyZfBPIdKVw/cTsS1tv7hCv7102uNdH4qATXsTuICMo9PCMJOgNOY5xPHaQgMQJHO0lrn5DrzcTBDaGkrrEouB9DKo4UZnrpVZGHs2xAmkxiLlT8G7II9dyeedOQebrkRJnuv9eLI/mnzoG9i6RDMxkgqMnHjuNn2TOyUayLNmKH2rrtXaRjk5Y9vLH7rtDfMoKaZuU+M0ahBCc0jLKLUOhTHSZoMSP+lRBbVeyy2elgXpkWnVgQrVUd9FZjBLwYY2c5amUUbJFrhX2+u4YNMnPihCcnQHkTimBcN2xqELNVjnje4oZxU+tRZO6zon0H0Xh9waOkbgm090RAXp9HkFqAO8oU5Ysxz2LRS++KpunKL09W56fLCzgRT5s6sWIAqYC9qfWf5JjPMXEMXYEmh2FjU+bLGTtqr0LBG7SkM7WnY13yjh1vZtUHOCZZUnbHSGfHPOOQ0UlNTg9jYADmO5uYf2RISL/PHtCG+LxMP+NyLVjPDzPpeldiaclmGoHqLb9GaCp1Do6B36SUlRJ/m6YQ+PeGZMEksnwd5jFRGxggoYkD0Du+Rxkg8AU/4efLzCUxU2skkAfsXnURBHj+uw+KLVHyfZ9y1Fw7UjfM4cblrcN9cC72RZak6HeJRf+wC/lVdJZZ4yjEOcVhG
websig	ea3d6ee471dc0467410f73b5704e471d77539293d57c2042697648e6a7c291f459bcf5cf87ebf1343d4cc365c578d88cee971c110bf82e7e6458b7885eb95bfa
                 */
            }
            catch (Exception e){ Console.WriteLine(e.Message); }
            return !err;
        }

        private string AMSUrl(string sType, string sDepartment, long actId)
        {
            string url = "http://comm.ams.game.qq.com/ams/ame/ame.php";
            //string url = "http://apps.game.qq.com/ams/ame/ame.php";
            string param = "ameVersion=0.3";
            param += "&sServiceType=" + sType;
            param += "&iActivityId=" + actId;
            param += "&sServiceDepartment=" + sDepartment;
            param += "&set_info=" + sDepartment;

            return url + "?" + param;
        }
        private string AMSData(string sType, string sDepartment, long actId, long flowId, long g_tk)
        {
            string data = "";
            data += "sServiceType=" + sType;
            data += "&sServiceDepartment=" + sDepartment;
            data += "&iActivityId=" + actId;
            data += "&iFlowId=" + flowId;
            data += "&g_tk=" + g_tk;

            return data;
        }

        public string AMSRequest(string sType, string sDepartment, long actId, long flowId)
        {
            return AMSRequest("",sType ,sDepartment, actId, flowId);
        }
        public string AMSRequest(string sdid,string sType, string sDepartment, long actId, long flowId,params string[] others)
        {

            string url = AMSUrl(sType, sDepartment, actId);
            if(!string.IsNullOrWhiteSpace(sdid))
                url += "&sSDID=" + sdid;
            string data = AMSData(sType, sDepartment, actId, flowId, G_TK);
            if(others != null)
            {
                foreach (var other in others)
                {
                    data += "&" + other;
                }
            }

            Cookie ck = new Cookie("UserAgent", "UserAgent=Vendor:Apple|appVersion:59|OSVersion:10.3.3|DeviceName:iPhone|DeviceType:iPhone|ScreenHeight:1136|OS:iOS|ScreenWidth:640|NetworkType:WiFi|");
            Web.Cookies.Add(new Uri("http://apps.game.qq.com"),ck);
            Web.Headers.Add("Content-type: application/x-www-form-urlencoded");
            Web.Encoding = Encoding.UTF8;
            string result = Web.UploadString(url, data);
            result = Regex.Unescape(result);

            return result;
        }
        private long Get_g_tk(string skey)
        {
            //从cookie获取skey

            long hash = 5381;
            for (int i = 0; i < skey.Length; i++)
            {
                hash += (hash << 5) + skey[i];
            }
            long g_tk = hash & 0x7fffffff;

            return g_tk;
        }
        private bool ProcessLoginData(string data)
        {
            bool need_vcode = false;
            if (!data.Contains("ptui_checkVC"))
                return need_vcode;
            try
            {
                string resp = data.Replace("ptui_checkVC(", "").Replace(");", "").Replace(")","");
                string[] arr_resp = resp.Split(new char[] { ',','\''}, StringSplitOptions.RemoveEmptyEntries);
                if (arr_resp.Length != 5 && arr_resp.Length != 4)
                    throw new Exception("获取验证码信息失败");
                //解析出数据
                int type = int.Parse(arr_resp[0]);
                //type,验证码,QQ号hex,pt_verifysession,'2'
                if (type == 0)
                {
                    //不需要验证码
                    //ptui_checkVC('0','!DCK','\x00\x00\x00\x00\x2f\x39\x1f\x85','5966d7b911a32b216f0279563b2e33341ea9fef89ffdad67401810bc3482f5729108902da455e041c08f23880d90b9082e99793144190df2','2');
                    VCode = arr_resp[1];
                    QQHexString = arr_resp[2];
                    PTVFSession = arr_resp[3];
                    need_vcode = false;
                }
                else if(type == 1)
                {
                    //需要验证码
                    //ptui_checkVC('1','uV1OTkYgX6MkAX_jwRLCYH3v5IhiSWiPJthHuYw_kpaJh5g1MV_ieQ**','\x00\x00\x00\x00\x03\x07\x2a\x79','','2');
                    QQHexString = arr_resp[2];
                    PTVFSession = "";
                    int max_err = 3;    //尝试三次
                    while(max_err-- > 0)
                    {
                        VCode = GetVCode(arr_resp[1]);
                        if (!string.IsNullOrWhiteSpace(VCode))
                            break;
                    }
                    need_vcode = true;
                }

                //判断类型
            } catch
            {

            }

            return need_vcode;
        }

        private string GetVCode(string vcode_string)
        {
            string vcode = "";
            string url = "", req = "", resp = "";

            try
            {
                //载入验证码
                ///1.生成验证码
                url = "http://captcha.qq.com/cap_union_prehandle?";
                req = "aid=" + QQBaseInfo.appid;
                //req += "&asig=";
                //req += "&captype=";
                req += "&protocol=http";
                req += "&clientype=2";
                //req += "&disturblevel=";
                req += "&apptype=2";
                req += "&curenv=inner";
                req += "&uid=" + QQ;
                req += "&cap_cd=" + vcode_string;
                req += "&lang=" + QQBaseInfo.ptlang;
                req += "&callback=_aq_69469";
                resp = Web.DownloadString(url + req);

                if (!resp.StartsWith("_aq_69469"))
                    throw new Exception("关联验证码失败");
                resp = resp.Replace("_aq_69469(", "").Replace(")", "");
                JObject jresp = JObject.Parse(resp);
                if((string)jresp["state"] != "1")
                    throw new Exception("关联验证码失败");
                string sess = (string)jresp["sess"];
                ///2.生成验证码图片
                url = "http://captcha.qq.com/cap_union_new_show?";
                req = "aid=" + QQBaseInfo.appid;
                //req += "&asig=";
                //req += "&captype=";
                req += "&protocol=http";
                req += "&clientype=2";
                //req += "&disturblevel=";
                req += "&apptype=2";
                req += "&curenv=inner";
                req += "&sess=" + sess;
                //req += "&theme=";
                req += "&noBorder=noborder";
                req += "&fb=1";
                req += "&showtype=embed";
                req += "&uid=" + QQ;
                req += "&cap_cd=" + vcode_string;
                req += "&lang=" + QQBaseInfo.ptlang;
                req += "&rnd=199179";
                resp = Web.DownloadString(url + req);
                int s = resp.IndexOf("),_=\"");
                if (s <= 0)
                    throw new Exception("获取vsig失败");
                int e = resp.IndexOf("\"",s + 5);
                VSIG = resp.Substring(s + 5, e - s - 5);
                string key = "websig:\"";
                s = resp.IndexOf(key);
                e = resp.IndexOf("\"", s + key.Length);
                string WebSig = resp.Substring(s + key.Length, e - s - key.Length);
                ///2.获取验证码图片
                url = "http://captcha.qq.com/cap_union_new_getcapbysig?";
                req = "aid=" + QQBaseInfo.appid;
                //req += "&asig=";
                //req += "&captype=";
                req += "&protocol=http";
                req += "&clientype=2";
                //req += "&disturblevel=";
                req += "&apptype=2";
                req += "&curenv=inner";
                req += "&sess=" + sess;
                //req += "&theme=";
                req += "&noBorder=noborder";
                req += "&fb=1";
                req += "&showtype=embed";
                req += "&uid=" + QQ;
                req += "&cap_cd=" + vcode_string;
                req += "&lang=" + QQBaseInfo.ptlang;
                req += "&rnd=199179";
                req += "&vsig=" + VSIG;
                req += "&ischartype=1";
                byte[] data = Web.DownloadData(url + req);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    Bitmap bmp = (Bitmap)Bitmap.FromStream(ms);
                    FormVCode f = new FormVCode(bmp);
                    f.ShowDialog();
                    vcode = f.GetCode();
                }
                //使用图片验证码得到验证码句柄
                url = "http://captcha.qq.com/cap_union_new_verify";
                req = "aid=" + QQBaseInfo.appid;
                req += "&asig=";
                req += "&captype=";
                req += "&protocol=http";
                req += "&clientype=2";
                req += "&disturblevel=";
                req += "&apptype=2";
                req += "&curenv=inner";
                req += "&sess=" + sess;
                req += "&theme=";
                req += "&noBorder=noborder";
                req += "&fb=1";
                req += "&showtype=embed";
                req += "&uid="  + QQ;
                req += "&cap_cd=" + vcode_string;
                req += "&lang=" + QQBaseInfo.ptlang;
                req += "&rnd=199179";
                req += "&subcapclass=0";
                req += "&vsig=" + VSIG;
                req += "&cdata=43";
                req += "&acaacd=";
                req += "&websig=" + WebSig;
                req += "&ans=" + vcode;
                req += "&tlg=1";
                resp = Web.UploadString(url,req);
                //resp = "{"errorCode":"0" , "randstr" : "@WV9" , "ticket" : "t02MWu-ocSTDLwlcMkEx9jp_6G9BF7adq7wfS1mr-3H35nO7q_A5CCUbT-09gPVfv0DYjoSutSbO2ljG4BpsD_kgJs-fuWU1cA2rTwwKU_15K4*" , "errMessage":"OK","sess":""}"
                jresp = JObject.Parse(resp);
                if ((string)jresp["errorCode"] != "0")
                {
                    throw new Exception("验证码错误");
                }
                PTVFSession = (string)jresp["ticket"];
                vcode = (string)jresp["randstr"];
            }
            catch { vcode = ""; }
            return vcode;
        }

        public string GetSDID(long actid)
        {
            string sdid = "";
            string  resp, url;

            try
            {
                url = "http://apps.game.qq.com/comm-htdocs/js/ams/v0.2R02/act/" + actid + "/act.desc.js";
                resp = Web.DownloadString(url);
                int s = resp.IndexOf("\r\nvar ams_actdesc=");
                resp = "[" + resp.Substring(s + "\r\nvar ams_actdesc=".Length) ;
                resp = resp.Replace("var ams_actdesc_" + actid+"=", ",") + "]";
                JArray jarr = JArray.Parse(resp);
                sdid = (string)jarr[0]["sSDID"];
            }
            catch { }
            return sdid;
        }
        public void AMSAuto(QQPlatformClient p)
        {
            Console.WriteLine("平台:" + p.Name + "| ID=" + p.ID);
            Console.WriteLine("活动个数:" + p.ActivityList.Count);
            foreach(var act in p.ActivityList)
            {
                Console.WriteLine("执行活动:" + act.Name + "| actID=" + act.ActiveId);
               Random r = new Random();
                foreach (var sub in act.FlowList)
                {
                    string resp;
                    if (sub.ActionList != null)
                    {
                        foreach (var act_call in sub.ActionList)
                        {
                            try
                            {
                                act_call();
                            }
                            catch
                            { }
                        }
                    }


                    try
                    {
                        string sdid = GetSDID(act.ActiveId);
                        resp = AMSRequest(sdid,p.ServiceType, p.ServiceDepartment, act.ActiveId, sub.ActiveId,sub.OtherParams);
                        resp = resp.Replace("null", "\"\"");
                        resp = resp.Replace("\"{", "{");
                        resp = resp.Replace("}\"", "}");
                        JObject jresp = JObject.Parse(resp);
                        int ret = (int)jresp["ret"];
                        string msg = (string)jresp["msg"];
                        string msg2 = "";
                        if(ret == 700)
                        {

                        }

                        if (jresp.Property("modRet") != null && jresp["modRet"].Type == JTokenType.Object)
                        {
                            msg2 = (string)jresp["modRet"]["sMsg"];
                        }else if(jresp.Property("flowRet") != null && jresp["flowRet"].Type == JTokenType.Object)
                            msg2 = (string)jresp["flowRet"]["sMsg"];

                        if(!string.IsNullOrWhiteSpace(msg2) && msg2.Length > 3)
                            Console.WriteLine(sub.Name + " ==> " + msg2);
                        else
                            Console.WriteLine(sub.Name + " ==> " + msg);
                    }
                    catch { Console.WriteLine("异常:" + sub.Name); }
                    Thread.Sleep(r.Next(3000,4500));
                }
            }
        }
    }
}
