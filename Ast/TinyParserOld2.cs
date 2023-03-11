using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoxygenInsert.Ast
{
    class TinyParserOld2
    {
        public class FunctionInfo
        {
            public string FunctionName { get; set; }
            public string ReturnType { get; set; }
            public List<string> ParameterNames { get; set; }= new List<string>();
        }
        static public FunctionInfo Split(string decl)
        {
            FunctionInfo result = new FunctionInfo();
            var ss = decl.Split(new char[] { '(', ')', '）', '（' });
            if (ss.Length < 2)
                return result;
            ss[0] = ss[0].Replace("static ", "").Replace("const ", "").Replace("extern ", "").Trim();
            var part1 = SplitTypeName(ss[0].Trim(),false);
            result.ReturnType = part1[0];
            result.FunctionName = part1[1];

            var part2 = ss[1].Trim().Split(',');
            foreach(var arg in part2)
            {
                var argInfo = SplitTypeName(arg.Trim(), false);
                if(argInfo[0]!="void")
                    result.ParameterNames.Add(argInfo[1]);
            }
            return result;
        }
        static private string[] SplitTypeName(string s,bool DefaultIsInt)
        {
            int index = s.LastIndexOfAny(new char[] { '*', ' ' });
            if(index ==-1 || index==0 && index==s.Length-1)
            {
                if(DefaultIsInt)
                    return new string[] { "int", s };
                else
                    return new string[]{"void", s};
            }
            return new string[] { s.Substring(0, index), s.Substring(index+1) };
        }
    }
}
