using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoxygenInsert
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cmdCopy_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void cmdCopy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (txtComment == null)
                e.CanExecute = false;
            else 
                e.CanExecute = txtComment.ToString().Length != 0;
        }

        private void cmdOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void cmdOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private RunningParam KeilFileLine = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string tmp1,tmp2;
            string exe_path =  System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string dir = System.IO.Path.GetDirectoryName(exe_path);
            string file1 = System.IO.Path.Combine(dir, "HeaderTemplate.c");
            if (File.Exists(file1))
                tmp1 = System.IO.File.ReadAllText(file1);
            else
                tmp1 = txtHeaderTemplate.ToTxt();

            txtHeaderTemplate.ColorIt(tmp1,null);

            string file2 = System.IO.Path.Combine(dir, "FunctionTemplate.c");
            if (File.Exists(file2))
                tmp2 = System.IO.File.ReadAllText(file2);
            else
                tmp2 = txtFunctionTemplate.ToTxt();
            txtFunctionTemplate.ColorIt(tmp2, null);

            KeilFileLine = RunningParam.Ctor();
            LoadKeilProject(KeilFileLine.Project);
            LoadFunctions(KeilFileLine.File);

        }
        private void LoadKeilProject(string uvproj)
        {
            lblProjectName.Content = uvproj;
            var subFiles = uvProjxAnalyze.GetFileOfProject(uvproj);
            cmbFile.Items.Clear();
            foreach (var subFile in subFiles)
            {
                cmbFile.Items.Add(subFile);
            }

            if(subFiles.Contains(KeilFileLine.File))
                cmbFile.Text = KeilFileLine.File;
        }
        private void LoadDirectoryProject(string dir)
        {
            lblProjectName.Content = dir;
            var subFiles = uvProjxAnalyze.GetFileOfDirectory(dir);
            cmbFile.Items.Clear();
            foreach (var subFile in subFiles)
            {
                cmbFile.Items.Add(subFile);
            }
        }

        private void LoadFunctions(string cFileName)
        {
            if (!System.IO.File.Exists(cFileName))
                return;
#if false //clang的parser
            var cfg = new CppAst.CppParserOptions()
            {
                ParseAsCpp =false,
                ParseSystemIncludes = false,
                ParseComments = false,                 
                //Defines = uvProjxAnalyze.Define.ToList(),
                //IncludeFolders = uvProjxAnalyze.IncludePath.ToList(), 
                TargetCpu = CppAst.CppTargetCpu.ARM,
                //SystemIncludeFolders = new List<string>() {},                     
            };
            cfg.Defines.AddRange(uvProjxAnalyze.Define);
            cfg.IncludeFolders.AddRange(uvProjxAnalyze.IncludePath);
            cfg.Defines.AddRange(uvProjxAnalyze.Define);

            var cContent = System.IO.File.ReadAllText(cFileName);
            //            cContent = @"
            //enum MyEnum { MyEnum_0, MyEnum_1 };
            //void function0(int a, int b);
            //struct MyStruct { int field0; int field1;};
            //typedef MyStruct* MyStructPtr;
            //";
            var compilation = CppAst.CppParser.Parse(cContent,cfg,cFileName);
            //var compilation = CppAst.CppParser.ParseFile(cFileName);//, cfg);
            cmbFunction.Items.Clear();
            foreach(var fun in compilation.Functions)
            {
                cmbFunction.Items.Add(fun.ToString());
            }
            // Print diagnostic messages
            foreach (var message in compilation.Diagnostics.Messages)
            {
                txtComment.AppendText(message + "\r\n");
            }
            //// Print All enums
            //foreach (var cppEnum in compilation.Enums)
            //    Console.WriteLine(cppEnum);
            //// Print All functions
            //foreach (var cppFunction in compilation.Functions)
            //    Console.WriteLine(cppFunction);
            //// Print All classes, structs
            //foreach (var cppClass in compilation.Classes)
            //    Console.WriteLine(cppClass);
            //// Print All typedefs
            //foreach (var cppTypedef in compilation.Typedefs)
            //    Console.WriteLine(cppTypedef);
#endif
            try
            {
                var lines = System.IO.File.ReadAllLines(cFileName,Encoding.GetEncoding(936));
                var ast = new Ast.TinyParser(lines);
                var CiYu = ast.Lex();
                int index = 0;
                var YuJu = ast.Parse(CiYu, ref index,0,"");
                ast.FillStatement(YuJu);

                var funcs = ast.FindFunction(YuJu);
                cmbFunction.Items.Clear();
                foreach(string fun in funcs)
                    cmbFunction.Items.Add(fun);

                pgDebug.SelectedObject = YuJu;

                GenerateFileComment(cFileName);
            }
            catch (Exception ex)
            {
                txtComment.AppendText(ex.ToString());
            }
        }
        void GenerateFileComment(string FileName)
        {
            string tmp = txtHeaderTemplate.ToTxt();
            tmp = tmp.Replace("%1", System.IO.Path.GetFileName(FileName));
            tmp = tmp.Replace("%4", DateTime.Now.ToString("yyyy-MM-dd"));
            txtComment.ColorIt(tmp, null);
        }
        void GenerateFunctionComment(string FunDecl)
        {
            var info = Ast.TinyParserOld2.Split(FunDecl);
            string[] tmp = txtFunctionTemplate.ToTxt().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < tmp.Length;i++)
            {
                string line = tmp[i];
                if (line.Contains("%1"))
                {
                    line = line.Replace("%1", info.FunctionName);
                    sb.AppendLine(line);
                    continue;
                }

                if (line.Contains("%2"))
                {
                    foreach(var arg in info.ParameterNames)
                    {
                        var line2 = line.Replace("%2", arg);
                        sb.AppendLine(line2);
                    }
                    continue;
                }
                if (line.Contains("%3"))
                {
                    if(info.ReturnType!="void")
                    {
                        var line2 = line.Replace("%3", "");
                        sb.AppendLine(line2);
                    }
                    continue;
                }
                sb.AppendLine(line);
            }
            txtComment.ColorIt(sb.ToString(),null);
        }
        private void cmbFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtFunctionDeclare.Text = cmbFunction.SelectedItem?.ToString();
            GenerateFunctionComment(txtFunctionDeclare.Text);
        }


        private void btnOpenProject_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Keil工程|*.uvproj;*.uvprojx|*.*|*.*";
            if(ofd.ShowDialog() ==  System.Windows.Forms.DialogResult.OK)
            {
                LoadKeilProject(ofd.FileName);
            }
        }

        private void btnOpenDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() ==  System.Windows.Forms.DialogResult.OK)
            {
                LoadDirectoryProject(dlg.SelectedPath);
            }
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "c源码|*.h;*.c|*.*|*.*";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cmbFile.Items.Add(ofd.FileName);
                cmbFile.Text = ofd.FileName;
                LoadFunctions(ofd.FileName);
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            //仅支持文件的拖放
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            //获取拖拽的文件
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0 &&
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            foreach (string file in files)
            {
                cmbFile.Items.Add(file);
            }

            cmbFile.Text = files[0];
            LoadFunctions(files[0]);
        }

        private void cmbFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems!=null && e.AddedItems[0]!=null)
                LoadFunctions(e.AddedItems[0].ToString()) ;
        }

        private void btnGenerateFileHeader_Click(object sender, RoutedEventArgs e)
        {
            GenerateFileComment(cmbFile.Text);
        }

        private void btnGenerateFunction_Click(object sender, RoutedEventArgs e)
        {
            GenerateFunctionComment(txtFunctionDeclare.Text);
        }
    }
}
