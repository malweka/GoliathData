using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Goliath.Data.Sql
{
    class WhereStatement : Operand
    {
        public ComparisonOperator Operator { get;  set; }
        public Sql.SqlOperator PostOperator { get; set; }
        public Operand LeftOperand { get; private set; }
        public Operand RightOperand { get;  set; }

        public WhereStatement(string column)
            : base(column)
        {
            LeftOperand = new StringOperand(column);
        }

        public WhereStatement(Operand leftOperand)
            : base(null)
        {
            if (leftOperand == null)
                throw new ArgumentNullException("leftOperand");

            LeftOperand = leftOperand;
            innerValue = leftOperand.ToString();
        }


        public WhereStatement Equals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.Equal);
            return this;
        }

        public WhereStatement NotEquals(string parameterizedValue)
        {
            SetOperand(parameterizedValue, ComparisonOperator.NotEqual);
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

        public WhereStatement NotNull()
        {
            SetOperand(string.Empty, ComparisonOperator.IsNotNull);
            return this;
        }

        public WhereStatement IsNull()
        {
            SetOperand(string.Empty, ComparisonOperator.IsNull);
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
            RightOperand = new StringOperand(parameterizedValue);
            Operator = @operator;
        }

        public override string ToString()
        {
            string wString = string.Format("{0} {1} {2}", LeftOperand, OperatorToString(Operator), RightOperand);
            return wString.Trim();
        }

        string OperatorToString(ComparisonOperator @operator)
        {
            switch (@operator)
            {
                case ComparisonOperator.Equal:
                    return "=";
                case ComparisonOperator.GreaterOrEquals:
                    return ">=";
                case ComparisonOperator.GreaterThan:
                    return ">";
                case ComparisonOperator.In:
                    return "in({0})";
                case ComparisonOperator.LessOrEquals:
                    return "<=";
                case ComparisonOperator.LessThan:
                    return "<";
                case ComparisonOperator.Like:
                    return "LIKE";
                case ComparisonOperator.NotEqual:
                    return "<>";
                case ComparisonOperator.NotLike:
                    return "NOT LIKE";
                //case ComparisonOperator.And:
                //    return "AND";
                case ComparisonOperator.IsNotNull:
                    return "IS NOT NULL";
                case ComparisonOperator.IsNull:
                    return "IS NULL";
                //case ComparisonOperator.Or:
                //    return "OR";
            }

            return string.Empty;


        }
    }

    abstract class Operand
    {
        protected string innerValue;
        protected Operand(string value)
        {
            innerValue = value;
        }

        public override string ToString()
        {
            return innerValue;
        }
    }

    class StringOperand : Operand
    {
        public StringOperand(string value) : base(value) { }
    }

    class InOperand<T> : Operand
    {
        public InOperand(IEnumerable<T> collection) : base(string.Join(",", collection)) { }
    }
}
