using System;
using System.Linq;
using System.Globalization;
using MVCFramework.RQL.Parser; 
using MVCFramework.ActiveRecord; 

namespace MVCFramework.RQL.AST2PostgreSQL
{
    
    public class RQLPostgreSQLCompiler : RQLCompiler
    {
        
        private string RQLFilterToSQL(TRQLFilter filter)
        {
            string value;
            
            if (filter.RightValueType == ValueTypeEnum.vtString &&
                !(filter.Token == TokenType.tkContains || filter.Token == TokenType.tkStarts))
            {
                
                value = QuoteString(filter.OpRight, "'");
            }
            else
            {
                value = filter.OpRight;
            }

            
            string dbFieldName = GetDatabaseFieldName(filter.OpLeft, true);
            string sqlField = GetFieldNameForSQL(dbFieldName);

            switch (filter.Token)
            {
                case TokenType.tkEq:
                    if (filter.RightValueType == ValueTypeEnum.vtNull)
                        return $"({sqlField} IS NULL)";
                    else
                        return $"({sqlField} = {value})";

                case TokenType.tkLt:
                    return $"({sqlField} < {value})";

                case TokenType.tkLe:
                    return $"({sqlField} <= {value})";

                case TokenType.tkGt:
                    return $"({sqlField} > {value})";

                case TokenType.tkGe:
                    return $"({sqlField} >= {value})";

                case TokenType.tkNe:
                    if (filter.RightValueType == ValueTypeEnum.vtNull)
                        return $"({sqlField} IS NOT NULL)";
                    else
                        return $"({sqlField} != {value})";

                case TokenType.tkContains:
                    {
                        
                        string likeValue = QuoteString($"%{value.Trim('\'')}%", "'");
                        
                        return $"({sqlField} ILIKE {likeValue.ToLowerInvariant()})";
                    }
                case TokenType.tkStarts:
                    {
                        string likeValue = QuoteString($"{value.Trim('\'')}%", "'");
                        return $"({sqlField} ILIKE {likeValue.ToLowerInvariant()})";
                    }
                case TokenType.tkIn:
                    {
                        if (filter.RightValueType == ValueTypeEnum.vtIntegerArray)
                        {
                            string joined = string.Join(",", filter.OpRightArray);
                            return $"({sqlField} IN ({joined}))";
                        }
                        else if (filter.RightValueType == ValueTypeEnum.vtStringArray)
                        {
                            string[] quotedArray = QuoteStringArray(filter.OpRightArray, "'");
                            string joined = string.Join(",", quotedArray);
                            return $"({sqlField} IN ({joined}))";
                        }
                        else
                        {
                            throw new RQLException("Invalid RightValueType for tkIn");
                        }
                    }
                case TokenType.tkOut:
                    {
                        if (filter.RightValueType == ValueTypeEnum.vtIntegerArray)
                        {
                            string joined = string.Join(",", filter.OpRightArray);
                            return $"({sqlField} NOT IN ({joined}))";
                        }
                        else if (filter.RightValueType == ValueTypeEnum.vtStringArray)
                        {
                            string[] quotedArray = QuoteStringArray(filter.OpRightArray, "'");
                            string joined = string.Join(",", quotedArray);
                            return $"({sqlField} NOT IN ({joined}))";
                        }
                        else
                        {
                            throw new RQLException("Invalid RightValueType for tkOut");
                        }
                    }
                default:
                    throw new RQLException($"Unsupported filter token: {filter.Token}");
            }
        }

        
        private string RQLLimitToSQL(TRQLLimit limit)
        {
            if (limit.Start == 0)
            {
                return $" /*limit*/ LIMIT {limit.Count}";
            }
            else
            {
                return $" /*limit*/ LIMIT {limit.Count} OFFSET {limit.Start}";
            }
        }

        
        private string RQLLogicOperatorToSQL(TRQLLogicOperator logicOp)
        {
            string joinOp = logicOp.Token == TokenType.tkAnd ? " and " :
                            logicOp.Token == TokenType.tkOr ? " or " :
                            throw new RQLException("Invalid token in RQLLogicOperator");

            bool first = true;
            string sql = "";
            foreach (TRQLCustom token in logicOp.FilterAST)
            {
                if (!first)
                {
                    sql += joinOp;
                }
                first = false;
                sql += RQLCustom2SQL(token);
            }
            return "(" + sql + ")";
        }

        
        private string RQLSortToSQL(TRQLSort sort)
        {
            string sql = " /*sort*/ ORDER BY";
            for (int i = 0; i < sort.Fields.Count; i++)
            {
                if (i > 0)
                {
                    sql += ",";
                }
                string field = GetDatabaseFieldName(sort.Fields[i], true);
                string sqlField = GetFieldNameForSQL(field);
                sql += " " + sqlField;
                sql += sort.Signs[i] == "+" ? " ASC" : " DESC";
            }
            return sql;
        }

        
        private string RQLWhereToSQL(TRQLWhere where)
        {
            return " WHERE ";
        }

        
        protected override string RQLCustom2SQL(TRQLCustom rqlCustom)
        {
            if (rqlCustom is TRQLFilter filter)
            {
                return RQLFilterToSQL(filter);
            }
            else if (rqlCustom is TRQLLogicOperator logicOp)
            {
                return RQLLogicOperatorToSQL(logicOp);
            }
            else if (rqlCustom is TRQLSort sort)
            {
                return RQLSortToSQL(sort);
            }
            else if (rqlCustom is TRQLLimit limit)
            {
                return RQLLimitToSQL(limit);
            }
            else if (rqlCustom is TRQLWhere where)
            {
                return RQLWhereToSQL(where);
            }
            else
            {
                throw new RQLException($"Unknown token in compiler: {rqlCustom.GetType().Name}");
            }
        }
    }

    
    public static class RQLPostgreSQLCompilerRegistration
    {
        static RQLPostgreSQLCompilerRegistration()
        {
            
            RQLCompilerRegistry.Instance.RegisterCompiler("postgresql", typeof(RQLPostgreSQLCompiler));
        }

        
        public static void Unregister()
        {
            RQLCompilerRegistry.Instance.UnRegisterCompiler("postgresql");
        }
    }
}
