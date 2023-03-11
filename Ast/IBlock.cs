using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DoxygenInsert.Ast
{

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Position
    {
        public int Line { get; set; }
        public int Column { get; set; }

        public Position(int line, int position)
        {
            this.Line = line;
            this.Column = position;
        }
        public Position(Position position)
        {
            this.Line = position.Line;
            this.Column = position.Column;
        }
        public override string ToString()
        {
            return "Line[" + Line + "].Row["+Column+"]";
        }
    }
    interface ITocken
    {
        Position Start { get; set; }
        Position End { get; set; }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TockenBase : ITocken
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        public bool IsPreProcess { get; set; } = false;
        public bool IsIdentify { get; set; } = false;
        public bool IsKeyword { get; set; } = false;
        public bool IsString { get; set; } = false;
        public bool IsNumber { get; set; } = false;
        public bool IsComment { get; set; } = false;
        public bool IsOperator { get; set; } = false;
        public string Value { get; set; }
        public override string ToString()
        {
            return Value;
        }
    }
    public interface IStatement
    {

    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Statement: IStatement
    {
        public Position Start { get; set; }
        public Position End { get; set; }

        public string Contents { get; set; }
        [TypeConverter(typeof(ListConverter))]
        public List<TockenBase> ChildTockens { get; set; } = new List<TockenBase>();
        [TypeConverter(typeof(ListConverter))]
        public List<Statement> BodyStatements { get; set; } = new List<Statement>();
    }
    internal class PreProcessBlock : Statement
    {
        public PreProcessBlock(TockenBase def)
        {
            this.ChildTockens.Add(def);
        }
    }
    internal class CommentBlock : Statement
    {
        public CommentBlock(TockenBase def)
        {
            this.ChildTockens.Add(def);
        }
    }
    internal class NormalBlock : Statement
    {
        public NormalBlock(List<TockenBase> def)
        {
            this.ChildTockens.AddRange(def);
        }
    }
    internal class QutoBlock : Statement
    {
        public TockenBase PreBlock { get; set; }
        public TockenBase EndBlock { get; set; }
        public Statement Body { get; set; }
        public QutoBlock(TockenBase pre, Statement mid, TockenBase end)
        {           
            this.PreBlock = pre;
            this.EndBlock = end;
            this.Body = mid;

            this.ChildTockens.Add(pre);
            this.ChildTockens.AddRange(mid.ChildTockens);
            this.ChildTockens.Add(end);

            //base.BodyStatements.Add(mid);
        }
    }
}
