using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace DoxygenInsert
{
    static class RichEditDoxyKeywordColor
    {
        static string[] DoxygenKeywords = "file,brief,details,author,date,version,param,par,return,see".Split(',');
        static public void ColorIt(this RichTextBox edt, string txt, string[] KeyWords)
        {
            if (KeyWords == null)
                KeyWords = DoxygenKeywords;
            var para = new Paragraph();
            edt.Document.Blocks.Clear();
            edt.Document.Blocks.Add(para);
            para.Inlines.Add(new Run(txt));
            foreach (var w in KeyWords)
            {
                ChangeColor(edt,Colors.SandyBrown,  "@"+w);
            }
        }

        static public void ChangeColor(RichTextBox richBox, Color l, string keyword)
        {
            //设置文字指针为Document初始位置           
            //richBox.Document.FlowDirection           
            TextPointer position = richBox.Document.ContentStart;
            while (position != null)
            {
                //向前搜索,需要内容为Text       
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    //拿出Run的Text        
                    string text = position.GetTextInRun(LogicalDirection.Forward);
                    //可能包含多个keyword,做遍历查找           
                    int index = 0;
                    index = text.IndexOf(keyword, 0);
                    if (index != -1)
                    {
                        TextPointer start = position.GetPositionAtOffset(index);
                        TextPointer end = start.GetPositionAtOffset(keyword.Length);
                        position = selecta( richBox,l, keyword.Length, start, end);
                    }

                }
                //文字指针向前偏移   
                position = position.GetNextContextPosition(LogicalDirection.Forward);

            }
        }
        static public TextPointer selecta( RichTextBox richTextBox1, Color l,int selectLength, TextPointer tpStart, TextPointer tpEnd)
        {
            TextRange range = richTextBox1.Selection;
            range.Select(tpStart, tpEnd);
            //高亮选择         

            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(l));
            range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

            return tpEnd.GetNextContextPosition(LogicalDirection.Forward);
        }
        static public string ToTxt(this RichTextBox rtb)
        {
            TextRange textRange = new TextRange(
                // TextPointer to the start of content in the RichTextBox.
                rtb.Document.ContentStart,
                // TextPointer to the end of content in the RichTextBox.
                rtb.Document.ContentEnd
            );

            // The Text property on a TextRange object returns a string
            // representing the plain text content of the TextRange.
            return textRange.Text;
        }
    }
}
