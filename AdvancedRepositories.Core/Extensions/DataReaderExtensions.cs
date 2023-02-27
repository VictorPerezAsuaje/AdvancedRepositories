﻿using System.Data;


namespace AdvancedRepositories.Core.Extensions;

public static class DataReaderExtensions
{
    public static T GetValueType<T>(this IDataReader reader, string colName)
        => (T)reader[colName];

    public static T? TypeOrNull<T>(this IDataReader reader, string colName)
    {
        if (reader[colName] == DBNull.Value)
            return default;

        return (T)reader[colName];
    }
}