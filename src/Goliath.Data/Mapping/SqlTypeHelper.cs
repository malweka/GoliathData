using System;
using System.Data;

namespace Goliath.Data
{
    /// <summary>
    /// 
    /// </summary>
   public static class SqlTypeHelper
   {
       /// <summary>
       /// Gets the type of the SQL.
       /// </summary>
       /// <param name="type">The type.</param>
       /// <param name="length">The length.</param>
       /// <param name="isBlob">if set to <c>true</c> [is BLOB].</param>
       /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
       /// <returns></returns>
      public static string GetSqlType(Type type, int length, bool isBlob, bool isUnicode)
      {
         if (type == typeof(bool))
            return "bit";
         else if (type == typeof(Guid))
            return "uniqueidentifier";
         else if (type == typeof(string))
         {
            string prefix = "";
            if (isUnicode)
               prefix = "n";
            if (isBlob)
               return prefix + "text";
            if (length > 8000 || (isUnicode && length > 4000))
               return prefix + "varchar(max)";
            else
               return prefix + "varchar(" + length + ")";
         }
         else if (type == typeof(char))
         {
            if (isUnicode)
               return "nchar";
            else
               return "char";
         }
         else if (type == typeof(byte))
            return "tinyint";
         else if (type == typeof(short))
            return "smallint";
         else if (type == typeof(int))
            return "int";
         else if (type == typeof(int))
            return "bigint";
         else if (type == typeof(long))
            return "float";
         else if (type == typeof(float))
            return "real";
         else if (type == typeof(DateTime))
            return "datetime";
         else if (type == typeof(decimal))
            return "decimal"; // TODO: Add support for precision and scale
         else if (type == typeof(byte[]))
         {
            if (isBlob)
               return "image";
            else
            {
               if (length > 8000)
                  return "varbinary(max)";
               else
                  return "varbinary(" + length + ")";
            }
         }
         else
            throw new NotImplementedException(string.Format("DbMigrations currently has no mapping of '{0}' for MS-SQL", type));
      }

      //public static Type GetClrType(SqlDbType sqlType)
      //{
      //    Type clrType = null;
      //    switch (sqlType)
      //    {
      //        case SqlDbType.BigInt:
      //            clrType = typeof(long);
      //            break;
      //        case SqlDbType.
      //    }
      //}

      /// <summary>
      /// Gets the type of the CLR.
      /// </summary>
      /// <param name="sqlType">Type of the SQL.</param>
      /// <returns></returns>
      public static Type GetClrType(DbType sqlType, bool isNullable)
      {
         Type clrType = null;
         switch (sqlType)
         {
            case DbType.AnsiStringFixedLength:
            case DbType.StringFixedLength:
            case DbType.String:
            case DbType.AnsiString:
               clrType = typeof(string);
               break;
            case DbType.Binary:
               clrType = typeof(byte[]);
               break;
            case DbType.Boolean:
               if (isNullable)
                  clrType = typeof(bool?);
               else
                  clrType = typeof(bool);
               break;
            case DbType.Byte:
               clrType = typeof(byte);
               break;
            case DbType.Currency:
            case DbType.Double:
               if (isNullable)
                  clrType = typeof(double?);
               else
                  clrType = typeof(double);
               break;
            case DbType.Date:
            case DbType.DateTime:
            case DbType.DateTime2:
            case DbType.Time:
               if (isNullable)
                  clrType = typeof(DateTime?);
               else
                  clrType = typeof(DateTime);
               break;
            case DbType.Decimal:
               if (isNullable)
                  clrType = typeof(decimal?);
               else
                  clrType = typeof(decimal);
               break;
            case DbType.Guid:
               if (isNullable)
                  clrType = typeof(Guid?);
               else
                  clrType = typeof(Guid);
               break;
            case DbType.Int16:
               if (isNullable)
                  clrType = typeof(short?);
               else
                  clrType = typeof(short);
               break;
            case DbType.Int32:
               if (isNullable)
                  clrType = typeof(int?);
               else
                  clrType = typeof(int);
               break;
            case DbType.Int64:
               if (isNullable)
                  clrType = typeof(long?);
               else
                  clrType = typeof(long);
               break;
            case DbType.Object:
               clrType = typeof(object);
               break;
            case DbType.SByte:
               clrType = typeof(sbyte);
               break;
            case DbType.Single:
               if (isNullable)
                  clrType = typeof(double?);
               else
                  clrType = typeof(double);
               break;
            case DbType.UInt16:
               clrType = typeof(UInt16);
               break;
            case DbType.UInt32:
               clrType = typeof(UInt32);
               break;
            case DbType.UInt64:
               clrType = typeof(UInt64);
               break;
             case DbType.DateTimeOffset:
                 clrType = typeof(DateTimeOffset);
                 break;
            default:
               throw new NotImplementedException(string.Format("DbMigrations currently has no mapping of '{0}' for MS-SQL", sqlType));

         }
         return clrType;
      }

      /// <summary>
      /// Gets the type of the SQL.
      /// </summary>
      /// <param name="type">The type.</param>
      /// <returns></returns>
      public static DbType GetSqlType(Type type)
      {
         return GetSqlType(type, false);
      }

      /// <summary>
      /// Gets the type of the SQL.
      /// </summary>
      /// <param name="type">The type.</param>
      /// <param name="isBlob">if set to <c>true</c> [is BLOB].</param>
      /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
      /// <returns></returns>
      public static DbType GetSqlType(Type type, bool isUnicode)
      {
         DbType sqlType = DbType.String;
         if (type == typeof(bool))
            sqlType = DbType.Boolean;

         else if (type == typeof(Guid))
            sqlType = DbType.Guid;

         else if (type == typeof(string))
         {
            if (isUnicode)
            {

               sqlType = DbType.String;

            }
            else
            {
               sqlType = DbType.AnsiString;

            }
         }

         else if (type == typeof(char))
         {
            if (isUnicode)
               sqlType = DbType.StringFixedLength;
            else
               sqlType = DbType.AnsiStringFixedLength;
         }

         else if (type == typeof(byte))
            sqlType = DbType.Byte;

         else if (type == typeof(short))
            sqlType = DbType.Int16;

         else if (type == typeof(int))
            sqlType = DbType.Int32;

         else if (type == typeof(long))
            sqlType = DbType.Int64;

         else if (type == typeof(double))
            sqlType = DbType.Double;

         else if (type == typeof(float))
            sqlType = DbType.Single;

         else if (type == typeof(DateTime))
            sqlType = DbType.DateTime;

         else if (type == typeof(decimal))
            sqlType = DbType.Decimal;

         else if (type == typeof(byte[]))
         {
            sqlType = DbType.Binary;
         }

         return sqlType;
      }
   }
}
