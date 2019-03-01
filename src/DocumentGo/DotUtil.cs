using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DocumentGo
{
    public static class DotUtil
    {
        public static void Exec(string dotExe,string dotFolder)
        {
            var dotFiles = Directory.EnumerateFiles(dotFolder, "*.dot").ToList();

            foreach (var dot in dotFiles)
            {
                ConvertDot2Png(dotExe, dot);
            }

            Console.WriteLine("ER图绘制完成");
        }

        private static void ConvertDot2Png(string dotExe, string dotFile)
        {
            var cmdStr = string.Format(@"""{1}"" ""{0}.dot"" -T png -o ""{0}.png""", dotFile.Remove(dotFile.Length - 4, 4), dotExe);

            try
            {
                using (var myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = true;
                    myPro.Start();
                    //如果调用程序路径中有空格时，cmd命令执行失败，可以用双引号括起来 ，在这里两个引号表示一个引号（转义）
                    var str = $"{cmdStr} {"&exit"}";

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    myPro.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}