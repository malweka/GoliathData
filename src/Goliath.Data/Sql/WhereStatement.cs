using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class WhereStatement : Operand
    {
        public ComparisonOperator Operator { get; private set; }
        public Operand LeftOperand { get; private set; }
        public Operand RightOperand { get; private set; }

        public WhereStatement(string column) : base(column)
        {
            LeftOperand = new Operand(column);
        }

        public WhereStatement Equals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.Equals);
            return this;
        }

        public WhereStatement NotEquals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.NotEquals);
            return this;
        }

        public WhereStatement Like(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.Like);
            return this;
        }

        public WhereStatement NotLike(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.NotLike);
            return this;
        }

        public WhereStatement GreaterThan(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.GreaterThan);
            return this;
        }

        public WhereStatement GreaterOrEquals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.GreaterOrEquals);
            return this;
        }

        public WhereStatement LessThan(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.LessThan);
            return this;
        }

        public WhereStatement LessOrEquals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.LessOrEquals);
            return this;
        }

        public WhereStatement In<T>(IEnumerable<T> list)
        {
            Operator = ComparisonOperator.In;
            RightOperand = new InOperand<T>(list);
            return this;
        }

        void SetOperand(string parameterizedValue, ComparisonOperator @operator)
        {
            RightOperand = new Operand(parameterizedValue);
            Operator = @operator;
        }
    }

    class Operand
    {
        string innerValue;
        public Operand(string value)
        {
            innerValue = value;
        }
    }

    class InOperand<T> : Operand
    {
        public InOperand(IEnumerable<T> collection) : base(string.Join(",", collection)) { }
    }
}
