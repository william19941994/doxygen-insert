using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoxygenInsert
{
    class RunningParam
    {
        public string Project { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public int Line { get; set; } = -1;
        public int Position { get; set; } = -1;
        public string Keil { get; set; } = string.Empty;
        public string KeilInclude { get; private set; }
        public string CpuMask { get; set; }
        static public RunningParam Ctor()
        {
            RunningParam result = new RunningParam();

            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                try
                {
                    switch (args[i])
                    {
                        //-Proj #P -File #F -Line ~F -Pos ^F -Keil #X -CPU $M
                        case "-Proj":
                            result.Project = args[i + 1];
                            break;
                        case "-File":
                            result.File = args[i + 1];
                            break;
                        case "-Line":
                            result.Line = Convert.ToInt32(args[i + 1]);
                            break;
                        case "-Pos":
                            result.Position = Convert.ToInt32(args[i + 1]);
                            break;
                        case "-Keil":
                            result.Keil = args[i + 1];
                            if(System.IO.File.Exists(result.Keil))
                            {//keil自带的clang是6.0的，和15.0.2的接口不同。
                                string libclangdllPath = result.Keil;
                                libclangdllPath = Path.GetDirectoryName(libclangdllPath);
                                libclangdllPath = Path.GetDirectoryName(libclangdllPath);

                                result.KeilInclude = Path.Combine(libclangdllPath, @"ARM\ARMCLANG\include");
                                libclangdllPath = Path.Combine(libclangdllPath, @"ARM\ARMCLANG\bin\libclang.dll");
                                if( false &&  System.IO.File.Exists(libclangdllPath))
                                {
                                    libclangdllPath = Path.GetDirectoryName(libclangdllPath);
                                    var oldpath = Environment.GetEnvironmentVariable("PATH") ;
                                    string newPath = oldpath + Path.PathSeparator.ToString() + libclangdllPath;
                                    Environment.SetEnvironmentVariable("PATH", newPath);   // 这种方式只会修改当前进程的环境变量
                                }
                            }
                            break;
                        case "-Cpu":
                            result.CpuMask = args[i + 1];
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("参数解析错误："+ ex.Message+"\r\n"+string.Join("\r\n",args));
                }
            }
            return result;
        }

    }
}
