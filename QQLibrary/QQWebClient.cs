using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public class QQWebClient : WebClient
    {
        // Cookie 容器
        private CookieContainer cookieContainer;

        /**/
        /// <summary>
        /// 创建一个新的 WebClient 实例。
        /// </summary>
        public QQWebClient()
        {
            this.cookieContainer = new CookieContainer();
        }

        /**/
        /// <summary>
        /// 创建一个新的 WebClient 实例。
        /// </summary>
        /// <param name="cookie">Cookie 容器</param>
        public QQWebClient(CookieContainer cookies)
        {
            this.cookieContainer = cookies;
        }

        /**/
        /// <summary>
        /// Cookie 容器
        /// </summary>
        public CookieContainer Cookies
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }
        
        /**/
        /// <summary>
        /// 返回带有 Cookie 的 HttpWebRequest。
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                HttpWebRequest httpRequest = request as HttpWebRequest;
                httpRequest.CookieContainer = cookieContainer;
                //添加header
                //Mozilla/5.0 (Windows NT 10.0; WOW64) 
                httpRequest.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_3 like Mac OS X) AppleWebKit/603.3.8 (KHTML, like Gecko) Mobile/14G60;GameHelper";
                //httpRequest.UserAgent = "Chrome/53.0.2785.104 Safari/537.36 Core/1.53.2372.400 QQBrowser/9.5.10801.400 Mozilla/5.0 (iPhone; CPU iPhone OS 10_3_3 like Mac OS X) AppleWebKit/603.3.8 (KHTML, like Gecko) Mobile/14G60;GameHelper";
            }
            return request;
        }

    }
}
