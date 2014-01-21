using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace TODO.Custom
{
    /// <summary>
    /// 公用方法类
    /// </summary>
    public class Common
    {
        #region 把日志写入指定文件
        /// <summary>
        /// 把日志写入指定文件
        /// </summary>
        /// <param name="jobLogType"></param>
        /// <param name="msg"></param>
        /// <param name="fileName"></param>       

        public static void WriteFile(String msg, string fileName)
        {


            try
            {
                lock (Lock)
                {
                    logFS = new FileStream(fileName, FileMode.Append);
                    sw = new StreamWriter(logFS, System.Text.ASCIIEncoding.UTF8);
                    sw.WriteLine(DateTime.Now.ToString());
                    sw.WriteLine(msg);
                    sw.Close();
                    logFS.Close();

                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (logFS != null)
                {
                    logFS.Close();
                }
                if (sw != null)
                {
                    sw.Close();
                }
            }

        }
        #endregion

        private static object Lock = new object();
        static FileStream logFS = null;
        static StreamWriter sw = null;

    }
}
