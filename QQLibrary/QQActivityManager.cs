using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{
    public class QQActivityManager
    {
        public static List<QQPlatform> PlatformList { get; private set; }
        public static void FromJsonString(string strJsonQQActivity)
        {

            JObject PlatformListJosn = JObject.Parse(strJsonQQActivity);
            PlatformList = new List<QQPlatform>();
            foreach (var Platform in PlatformListJosn)
            {
                //载入平台
                PlatformList.Add(new QQPlatform()
                {
                    Name = Platform.Key,
                    ID = long.Parse(Platform.Value["ID"].ToString()),
                    ServiceType = Platform.Value["ServiceType"].ToString(),
                    ServiceDepartment = Platform.Value["ServiceDepartment"].ToString(),
                    ActivityList = new List<QQActivity>()
                    //ActiveId = long.Parse(platform.Value["ID"].ToString()),

                    //FlowList = new List<QQActivity>()
                });

                //载入活动

                JObject ActivityListJosn = (JObject)Platform.Value["ActList"];
                var ActivityList = PlatformList.Last().ActivityList;
                foreach (var ActivityJson in ActivityListJosn)
                {
                    ActivityList.Add(new QQActivity()
                    {
                        Name = ActivityJson.Key,
                        ActiveId = long.Parse(ActivityJson.Value["ID"].ToString()),
                         FlowList = new List<QQActivity>()
                    });

                    //载入flow
                    JObject FlowListJson = (JObject)ActivityJson.Value["FlowList"];
                    var FlowList = ActivityList.Last().FlowList;
                    foreach(var FlowJson in FlowListJson)
                    {
                        FlowList.Add(new QQActivity()
                        {
                            Name = FlowJson.Key,
                             ActiveId = long.Parse(FlowJson.Value["ID"].ToString())
                        });
                        string strEncode = FlowJson.Value["Encode"].ToString();

                        if (string.IsNullOrWhiteSpace(strEncode))
                        {
                            FlowList.Last().Encode = Encoding.UTF8;
                        }
                        else
                        {
                            FlowList.Last().Encode = Encoding.GetEncoding(strEncode);
                        }
                    }
                }
            }
        }
    }
}
