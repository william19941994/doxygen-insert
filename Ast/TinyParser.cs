using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoxygenInsert.Ast
{
    class TinyParser
    {
        private string[] Lines = null;
        private Position PreCharPosion= new Position(0, 0);
        private Position NextCharPosion = new Position(0, 0);
        private Position CurCharPosition = new Position(0, 0);
        public TinyParser(string[] lines)
        {
            this.Lines = lines;
            PreCharPosion = new Position(0, 0);
            NextCharPosion = new Position(0, 0);
            CurCharPosition = new Position(0, 0);
        }
        string [] KeyWords=new string[]{
            "int","char","float","main","double","case", "for","if",
            "auto","else","do","while","void","static", "return",
            "break","struct","const","union","switch","typedef","enum"
        };
        /// <summary>
        /// 算法是字母
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        static private bool IsLetter(char c)
        {
            return (((c <= 'z') && (c >= 'a')) || ((c <= 'Z') && (c >= 'A')));
        }

        /// <summary>
        /// 是否是数字
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }
        private char UnGetCharCache = '\0';
        private bool UnGetCharCacheValid = false;
        private bool LineEnded = false;
        private bool FileEnded = false;
        public char GetChar()
        {
            if(UnGetCharCacheValid)
            {
                var back = PreCharPosion;
                PreCharPosion = CurCharPosition;
                CurCharPosition = back; //之前备份在那里了。
                //CurCharPosition = NextCharPosion;
                UnGetCharCacheValid = false;
                return UnGetCharCache;
            }
            LineEnded = false;
            FileEnded = false;

            if (NextCharPosion.Line>=Lines.Length)
            {
                LineEnded = true;
                FileEnded = true;
                return '\0';
            }
            //空行 不能跳过，模拟多行宏不能有行。
            if (Lines[NextCharPosion.Line].Length == 0 && NextCharPosion.Line < Lines.Length)
            {
                LineEnded = true;
                NextCharPosion.Line++;
                NextCharPosion.Column = 0;
                return '\0';
            }
            char c = Lines[NextCharPosion.Line][NextCharPosion.Column];
            PreCharPosion = CurCharPosition;
            CurCharPosition = NextCharPosion;
            NextCharPosion.Column++;
            if (NextCharPosion.Column >= Lines[NextCharPosion.Line].Length )
            {
                NextCharPosion.Line++;
                NextCharPosion.Column = 0;
                //c语言中的 转义字符后面跟着回车，会变成空。用于宏多行变单行，字符串一行写不下等等情况。行注释默认也是有效的。
                if (c!='\\')
                    LineEnded = true;
                else
                {
                    return GetChar(); //本次的转义丢弃，返回下一个
                }
                if (NextCharPosion.Line >= Lines.Length)
                {
                    //End of File
                    //FileEnded = true; //NextIsEnd,this one byte is still ok.
                    //break;
                }
            }
            return c;
        }
        public void UnGetChar(char c)
        {
            UnGetCharCache = c;
            UnGetCharCacheValid = true;
            //NextCharPosion = CurCharPosition;
            var back = CurCharPosition;
            CurCharPosition = PreCharPosion;
            PreCharPosion = back;
        }
        public List<TockenBase> Lex()
        {
            List<TockenBase> AllTocken=new List<TockenBase>();
            Position start, end;
            do
            {
                char ch = GetChar();
                start = this.CurCharPosition;
                end = this.CurCharPosition;
                if (FileEnded )
                    break;
                if (ch == ' ' || ch == '\t' || ch<' ')
                    continue;

                if(IsLetter(ch) || ch == '_')
                {
                    StringBuilder sb = new StringBuilder();
                    do
                    {
                        sb.Append(ch);
                        end = CurCharPosition;
                        ch = GetChar();
                        if (FileEnded || LineEnded) break;
                    } while (IsLetter(ch) || IsDigit(ch) || ch == '_');
                    UnGetChar(ch);
                    AllTocken.Add(new TockenBase()
                    {
                        Start = start,
                        End = end,
                        IsIdentify = true,
                    });
                }
                else if(IsDigit(ch))
                {
                    StringBuilder sb = new StringBuilder();
                    do
                    {
                        sb.Append(ch);
                        end = CurCharPosition;
                        ch = GetChar();
                        if (FileEnded || LineEnded) break;
                    } while (IsLetter(ch) || IsDigit(ch) || ch=='.');
                    UnGetChar(ch);
                    AllTocken.Add(new TockenBase()
                    {
                        Start = start,
                        End = end,
                        IsNumber = true,
                    });
                }
                else
                {
                    end = CurCharPosition;
                    char[] NextAllowed = new char[] { };
                    char[] Next2Allowed = null;
                    switch (ch)
                    {
                        case '+':
                            NextAllowed = new char[] { '+', '=' };
                            break;
                        case '-':
                            NextAllowed = new char[] { '-', '=' };
                            break;
                        case '*': 
                        case '%':
                            NextAllowed = new char[] { '=' };
                            break;
                        case '|':
                            NextAllowed = new char[] {'|', '=' };
                            break;
                        case '&':
                            NextAllowed = new char[] { '&', '=' };
                            break;
                        case '!':
                        case '^':
                        case '~':
                            NextAllowed = new char[] { '=' };
                            break;
                        case '(': break;
                        case ')': break;
                        case '[': break;
                        case ']': break;
                        case ';': break;
                        case '=':
                            NextAllowed = new char[] { '=' };
                            break;
                        case '.': break;
                        case ',': break;
                        case ':':  break;
                        case '{': break;
                        case '}':  break;
                        case '?': break;
                        case '\'': //字符
                        case '\"': //字符串
                            {
                                bool preIsEscape = false;
                                do
                                {
                                    char ch2 = GetChar();
                                    if (FileEnded)
                                        break;
                                    if (ch2 == ch && !preIsEscape )
                                        break;
                                    if(ch2 =='\\')
                                    {
                                        if(preIsEscape==false)
                                            preIsEscape = true;
                                    }
                                    else
                                    {
                                        preIsEscape = false;
                                    }
                                } while (true);
                                end = CurCharPosition;
                            }
                            break;
                        case '\\': //转意字符。
                            {
                                //在getchar中已经处理了 斜杠回车，其它的非字符串和注释中的，暂时不处理了。
                            }
                            break;
                        case '#':   //宏开始
                            {
                                while (true)
                                {//c语言单井号后直接回车，也是允许的
                                    if (LineEnded || FileEnded)
                                        break;
                                    _ = GetChar();
                                }
                                //这里并不展开宏，不做语法分析。
                                end = CurCharPosition;
                            }
                            break;
                        case '>':  //右移，右移等于，大于等于
                            NextAllowed = new char[] { '>', '=' };
                            Next2Allowed = new char[]{'>', '='};//右移等于
                            break;

                        case '<':
                            NextAllowed = new char[] { '<', '=' };
                            Next2Allowed = new char[] { '<', '=' };
                            break;
                        case '/':
                            {
                                ch = GetChar();
                                if (ch == '/')
                                {
                                    while (true)
                                    {
                                        ch = GetChar();
                                        if (LineEnded || FileEnded)
                                            break;
                                    }
                                    end = CurCharPosition;
                                }
                                else if (ch == '*')
                                {
                                    //出现在/*  */
                                    bool preIsStar=false;
                                    do
                                    {
                                        ch = GetChar();
                                        if (FileEnded)
                                            break;
                                        if (preIsStar && ch == '/')
                                            break;
                                        preIsStar = ch == '*';
                                    } while (true);
                                    end = CurCharPosition;
                                }
                                else
                                {
                                    UnGetChar(ch);
                                    NextAllowed = new char[] { '=' }; // 除以等于运算符
                                }
                            }
                            break;
                        //非法字符
                        default:
                            throw new Exception(string.Format("在第{0}行{1}列无法识别的字符{2}",
                                CurCharPosition.Line,
                                CurCharPosition.Column,ch));
                            break;
                    }
                    if(NextAllowed.Length!=0)
                    {
                        ch = GetChar();
                        if (NextAllowed.Contains(ch))
                        {
                            end = CurCharPosition;
                            if (Next2Allowed!=null && Next2Allowed[0]==ch)
                            {
                                ch = GetChar();
                                if(ch==Next2Allowed[1])
                                {
                                    end = CurCharPosition;
                                }
                                else
                                {
                                    UnGetChar(ch);
                                }
                            }
                        }
                        else
                        {
                            UnGetChar(ch);
                        }
                    }
                    var tocken = new TockenBase()
                    {
                        Start = start,
                        End = end,
                        IsOperator = true,
                    };
                    AllTocken.Add(tocken);
                }
            } while (true);
            foreach (var t in AllTocken)
                TockenFill(t);
            return AllTocken;
        }
        private string GetCodeByXy(Position x,Position y)
        {
            if (x.Line == y.Line)
            {
                return Lines[x.Line].Substring(x.Column, y.Column - x.Column + 1);
            }
            else
            {
                string s = string.Empty;
                for (int i = x.Line; i <= y.Line; i++)
                {
                    if (i == Lines.Length)
                        continue;
                    if (i == x.Line)
                        s += Lines[i].Substring(x.Column) + "\r\n";
                    else if (i == y.Line)
                        s += Lines[i].Substring(0, y.Column + 1);
                    else
                        s += Lines[i] + "\r\n";
                }
                return s;
            }
        }
        private void TockenFill(TockenBase tk)
        {
            tk.Value = GetCodeByXy(tk.Start, tk.End);
            tk.IsComment = tk.Value.StartsWith("//") || tk.Value.StartsWith("/*");
            tk.IsString = tk.Value.StartsWith("\"");
            tk.IsPreProcess = tk.Value.StartsWith("#");
        }
        private static void TockenToChildStatement(Statement st)
        {
            if(st.ChildTockens.Count!=0)
            {
                st.BodyStatements.Add(new NormalBlock(st.ChildTockens));
                st.ChildTockens.Clear();
            }
        }
        public Statement Parse(List<TockenBase> tockens, ref int StartIndex, int topLevel ,string ReturnWhenTocken)
        {
            var st = new Statement();
            while(StartIndex < tockens.Count)
            {
                var t = tockens[StartIndex];
                //if(t.Start.Line==110)
                //{
                //    Console.Write("x");//调试陷阱。
                //}
                if(t.IsPreProcess)
                {
                    TockenToChildStatement(st);
                    st.BodyStatements.Add(new PreProcessBlock(t));
                    StartIndex++;
                    continue;
                }
                if(t.IsComment)
                {
                    TockenToChildStatement(st);
                    st.BodyStatements.Add(new CommentBlock(t));
                    StartIndex++;
                    continue;
                }
                if(t.Value==";" )
                {
                    st.ChildTockens.Add(t);
                    TockenToChildStatement(st);
                    StartIndex++;
                    continue;
                    //return st;
                }
                if (t.Value== ReturnWhenTocken)
                {
                    //大括号和其它不一样,startIndex不++; 留到外面去处理
                    if (t.Value==")" || t.Value=="}" || t.Value == "]")
                        return st;
                }
                if (t.Value == "(" || t.Value=="[" || t.Value=="{")
                {
                    TockenToChildStatement(st);

                    string nextTocken = t.Value == "(" ? ")" : t.Value == "[" ? "]" : "}";

                    var pre = t;
                    Statement body = null;
                    StartIndex++;
                    do
                    {
                        if (StartIndex >= tockens.Count)
                        {
                            st.ChildTockens.Add(pre);
                            return st;
                        }
                        body = Parse(tockens, ref StartIndex, topLevel + 1, nextTocken);
                        
                        if (StartIndex >= tockens.Count)
                        {
                            st.ChildTockens.Add(pre);
                            st.BodyStatements.Add(body);
                            return st;
                        }
                        t = tockens[StartIndex];
                    }
                    while (t.Value != nextTocken);

                    st.BodyStatements.Add(new QutoBlock(pre,body,t));
                    st.ChildTockens.Clear();

                    StartIndex++;
                    continue;
                    //if (topLevel != 0)
                    //    return st;
                }

                //其它
                st.ChildTockens.Add(t);
                StartIndex++;
            }
            return st;
        }
        public void FillStatement(Statement st)
        {
            if (st.BodyStatements.Count != 0 && st.ChildTockens.Count != 0 && !(st is QutoBlock))
            {
                TockenToChildStatement(st);
            }
            if(st is QutoBlock)
            {
                var st2 = st as QutoBlock;
                FillStatement(st2.Body);
            }
            if (st.ChildTockens.Count!=0)
            {
                st.Start = st.ChildTockens.First().Start;
                st.End = st.ChildTockens.Last().End;
                st.Contents = GetCodeByXy(st.Start, st.End);
            }
            else if(st.BodyStatements.Count != 0)
            {
                foreach(var s in st.BodyStatements)
                    FillStatement(s);
                st.Start = st.BodyStatements.First().Start;
                st.End = st.BodyStatements.Last().End;
                st.Contents = GetCodeByXy(st.Start, st.End);
            }
        }

        public string[] FindFunction(Statement fileCompile)
        {
            List<string> results = new List<string>();
            for(int i=0;i<fileCompile.BodyStatements.Count-2;i++)
            {
                if(fileCompile.BodyStatements[i] is NormalBlock)
                {
                    if(fileCompile.BodyStatements[i+1] is QutoBlock)
                    {
                        if(fileCompile.BodyStatements[i+2] is QutoBlock)
                        {
                            var x1 = fileCompile.BodyStatements[i] as NormalBlock;
                            var x2 = fileCompile.BodyStatements[i + 1] as QutoBlock;
                            var x3 = fileCompile.BodyStatements[i + 2] as QutoBlock;
                            if(x2.PreBlock.Value=="(" && x3.PreBlock.Value=="{")
                            {
                                string fundeclare = x1.Contents + x2.Contents;
                                results.Add(fundeclare);
                            }
                        }
                    }
                }
                if (fileCompile.BodyStatements[i] is NormalBlock)
                {
                    if (fileCompile.BodyStatements[i + 1] is QutoBlock)
                    {
                        if (fileCompile.BodyStatements[i + 2] is NormalBlock)
                        {
                            var x1 = fileCompile.BodyStatements[i] as NormalBlock;
                            var x2 = fileCompile.BodyStatements[i + 1] as QutoBlock;
                            var x3 = fileCompile.BodyStatements[i + 2] as NormalBlock;
                            if (x2.PreBlock.Value == "(" && x3.Contents == ";")
                            {
                                string fundeclare = x1.Contents + x2.Contents;
                                results.Add(fundeclare);
                            }
                        }
                    }
                }
                if (fileCompile.BodyStatements[i+1] is NormalBlock)
                {
                    if (fileCompile.BodyStatements[i + 2] is QutoBlock)
                    {
                        var x1 = fileCompile.BodyStatements[i+1] as NormalBlock;
                        var x2 = fileCompile.BodyStatements[i + 2] as QutoBlock;
                        if (x1.Contents == "extern \"C\"" && x2.PreBlock.Value == "{")
                        {
                            results.AddRange(FindFunction(x2.Body));
                        }
                    }
                }
            }
            return results.ToArray();
        }
    }
}
