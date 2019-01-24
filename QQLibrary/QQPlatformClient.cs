using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{

    public  class QQPlatformClient
    {
        //平台名:道聚城
        public string Name { get; set; }
        //平台ID
        public long ID { get; set; }
        //服务类型
        public string ServiceType { get; set; }
        //服务部门
        public string ServiceDepartment { get; set; }
        //活动列表
        public List<QQActivity> ActivityList { get; set; }

    }
}
