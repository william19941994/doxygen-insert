using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace DoxygenInsert
{
    class uvProjxAnalyze
    {
        static public string[] C_Files = new string[0];
        static public string[] IncludePath = new string[0];
        static public string[] Define = new string[0];
        static public string [] GetFileOfProject(string projx)
        {
            if (!System.IO.File.Exists(projx))
                return new string[0];
            try
            {
                var xml = new XmlDocument();
                xml.Load(projx);
                var Nodes= xml.SelectNodes("/Project/Targets/Target/Groups/Group/Files/File/FilePath");
                List<string> all=new List<string>();
                foreach(XmlNode n in Nodes)
                {
                    all.Add(n.InnerText);
                }
                string path = System.IO.Path.GetDirectoryName(projx);
                C_Files = all.Distinct().ToArray();
                for(int i = 0; i < C_Files.Length;i++)
                {
                    string fn = C_Files[i];
                    if(fn.StartsWith(".\\"))
                        fn = fn.Substring(2);
                    C_Files[i] = System.IO.Path.Combine(path, fn);
                }

                var IncNodes = xml.SelectNodes("/Project/Targets/Target/TargetOption/TargetArmAds/Cads/VariousControls/IncludePath");
                List<string> incList = new List<string>();
                foreach(XmlNode node in IncNodes)
                {
                    incList.AddRange(node.InnerText.Split(';'));
                }
                IncludePath = incList.Select(x=>x.Trim()).Distinct().ToArray();
                for (int i = 0; i < IncludePath.Length; i++)
                {
                    string fn = IncludePath[i];
                    if (fn.StartsWith(".\\"))
                        fn = fn.Substring(2);
                    IncludePath[i] = System.IO.Path.Combine(path, fn);
                }

                var DefNodes = xml.SelectNodes("/Project/Targets/Target/TargetOption/TargetArmAds/Cads/VariousControls/Define");
                List<string> defList = new List<string>();
                foreach (XmlNode node in DefNodes)
                {
                    defList.AddRange(node.InnerText.Split(','));
                }
                Define = defList.Select(x => x.Trim()).Distinct().ToArray();
                for (int i = 0; i < Define.Length; i++)
                {
                }

                return C_Files;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("文件解析失败"
                    + ex.Message + "\r\n" + projx);
            }
            return new string[0];
        }

        static public string [] GetFileOfDirectory(string Dir)
        {
            return System.IO.Directory.GetFiles(Dir, "*.c", System.IO.SearchOption.AllDirectories)
                .Union( System.IO.Directory.GetFiles(Dir,"*.h",System.IO.SearchOption.AllDirectories))
                .ToArray();
        }
    }
}
