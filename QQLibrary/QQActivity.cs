using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQLibrary
{

    public class QQActivity
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 活动ID
        /// </summary>
        public long ActiveId { get; set; }

        /// <summary>
        /// 子活动列表
        /// </summary>
        public List<QQActivity> FlowList { get; set;}
        /// <summary>
        /// 额外参数
        /// </summary>
        public string[] OtherParams { get; set; }

        /// <summary>
        /// 额外
        /// </summary>
        public List<Action> ActionList { get; set; }

    }
}
