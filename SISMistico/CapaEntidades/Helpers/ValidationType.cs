namespace CapaEntidades.Helpers
{
    using System;
    using System.Data;

    public class ValidationType
    {
        public static SqlDbType ConvertTypeParameter(object obj)
        {
            if (obj is int)
            {
                return SqlDbType.Int;
            }

            if (obj is string)
            {
                return SqlDbType.VarChar;
            }

            if (obj is decimal)
            {
                return SqlDbType.Decimal;
            }

            if (obj is DateTime)
            {
                return SqlDbType.DateTime;
            }

            if (obj is TimeSpan)
            {
                return SqlDbType.Time;
            }

            return SqlDbType.VarChar;
        }
    }
}
