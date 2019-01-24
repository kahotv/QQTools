using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QQLibrary;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Forms;

namespace Test
{
    class Program
    {
        static bool run_now = false;
        static bool end = false;
        static object locker = new object();
        static void Main(string[] args)
        {
            Thread work = new Thread(WorkFunc);
            work.Start();
            while (true)
            {

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    lock (locker)
                    {
                        run_now = true;
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    end = true;
                    break;
                }
            }
            work.Join();
            Console.WriteLine("游戏结束");
            Thread.Sleep(500);
        }
        private static DateTime LastDay = DateTime.MinValue;
        public static void WorkFunc()
        {
            Dictionary<string, string> qqlist = new Dictionary<string, string>();
            try
            {
                string filepath = Application.StartupPath + "\\qq.txt";
                JArray jarr = JArray.Parse(File.ReadAllText(filepath));
                for (int i = 0; i < jarr.Count; i++)
                {
                    JArray qqinfo = (JArray)jarr[i];
                    string qq = (string)qqinfo[0];
                    string pwd = (string)qqinfo[1];
                    if (qqlist.ContainsKey(qq))
                        qqlist[qq] = pwd;
                    else
                        qqlist.Add(qq, pwd);
                    Console.WriteLine("add:" + qq);
                }
            }
            catch
            {
                MessageBox.Show("读取qq.txt错误");
                return;
            }
            string file = File.ReadAllText("qq.txt");
            List<QQBaseClient> entitylist = new List<QQBaseClient>();
            List<QQWork> worklist = new List<QQWork>() {
                new QQLibrary.Platforms.DaoJu(), 
                new QQLibrary.Platforms.xinyue(),
                //new QQLibrary.Platforms.DNF助手()
            };
            foreach (var kv in qqlist)
            {
                //新建QQ
                QQBaseClient qq = new QQBaseClient(kv.Key, kv.Value);
                //放入list
                entitylist.Add(qq);
            }
            DateTime? last = null;
            int recount = 1;
            while (true)
            {
                lock (locker)
                {
                    if (end)
                    {
                        Console.WriteLine("正在退出...");
                        break;
                    }
                }

                //每天晚上11:30跑一边

                if((DateTime.Now - LastDay).Hours >= 23 && 
                    DateTime.Now.Hour == 23 && DateTime.Now.Minute >= 30)
                {
                    LastDay = DateTime.Now;
                    run_now = true;
                }else if (last != null && (DateTime.Now - (DateTime)last).Hours < 8)
                {
                    //八小时一次
                    Thread.Sleep(1000);
                    lock (locker)
                    {
                        if (run_now)
                            run_now = false;
                        else
                            continue;
                    }
                }
                last = DateTime.Now;
                Console.WriteLine("=============第" + recount++ + "次执行=============");
                int errcount = 0;
                for (int i = 0; i < entitylist.Count; i++)
                {

                    QQBaseClient qq = entitylist[i];

                    for (int j = 0; j < worklist.Count; j++)
                    {
                        //新建QQ工作接口
                        Type type = worklist[j].GetType();
                        QQWork pt = (QQWork)type.GetConstructor(new Type[0]).Invoke(null);
                        //使用QQ执行功能模块
                        //1.初始化
                        bool state = pt.Init(entitylist[i], Console.WriteLine, "[" + qq.QQ + "]");

                        if (!state)
                        {
                            Console.WriteLine("[" + qq.QQ + "]初始化失败");
                            errcount++;
                            continue;
                        }
                        Console.WriteLine("[" + qq.QQ + "]昵称:" + qq.Nick + " | VIP等级:" + qq.VIPLevel);
                        //2.执行功能模块
                        pt.Excute();
                        //3.取出信息
                        string str = pt.ToString();
                    }

                }
                Console.WriteLine("本次执行结束，共" + entitylist.Count + "个账号，其中" + errcount + "个账号执行失败");
                Console.WriteLine("下次执行时间: " + ((DateTime)last).AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("(回车键=立即执行)");
            }

        }
    }
}
