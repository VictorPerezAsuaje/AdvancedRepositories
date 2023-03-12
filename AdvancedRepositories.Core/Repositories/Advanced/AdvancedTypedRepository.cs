using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using AdvancedRepositories.Core.Repositories.Fluent;
using System.Data;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories.Advanced;

public abstract class AdvancedTypedRepository<T> : BaseRepository where T : class, new()
{
    protected abstract string _TableName { get; }
    protected AdvancedTypedRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    protected QueryReady<T> Select(params string[] fields) 
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromMappedFields(fields)} ")).From(_TableName);

    protected QueryReady<T> SelectTop(int number, params string[] fields)
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromMappedFields(fields)} ")).Top(number).From(_TableName);

    protected QueryReady<T> SelectDistinct(params string[] fields)
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromMappedFields(fields)} ")).From(_TableName);

    protected QueryReady<T> Select() 
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromAttributesOf<T>()} ")).From(_TableName);

    protected QueryReady<T> SelectTop(int number)
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromAttributesOf<T>()} ")).Top(number).From(_TableName);

    protected QueryReady<T> SelectDistinct() 
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromAttributesOf<T>()} ")).From(_TableName);

    protected InsertCommand Insert()
        => new InsertCommand(CreateCommand($"INSERT INTO {_TableName} "));

    public class InsertCommand : AdvancedCommand
    {
        string _Fields = "";
        string _Values = "";
        public InsertCommand(IDbCommand cmd) : base(cmd)
        {
        }

        public AdvancedCommandReady FieldValues(params (string field, object value)[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                _Fields += $"{(i == 0 ? parameters[i].field : $", {parameters[i].field}")}";
                _Values += $"{(i == 0 ? "" : $", ")}@{parameters[i].field}";
                _cmd.AddWithValue(parameters[i].field, parameters[i].value);
            }

            _cmd.CommandText += $" ({_Fields}) VALUES ({_Values})";

            return new AdvancedCommandReady(_cmd);
        }

    }

    protected UpdateCommand Update()
        => new UpdateCommand(CreateCommand($"UPDATE {_TableName} SET "));

    public class UpdateCommand : AdvancedCommand
    {
        string _FieldValues = "";
        public UpdateCommand(IDbCommand cmd) : base(cmd)
        {
        }

        public UpdateCommand FieldValues(params (string field, object value)[] parameters)
        {
            if (!string.IsNullOrWhiteSpace(_FieldValues)) return this;

            for (int i = 0; i < parameters.Length; i++)
            {
                _FieldValues += $"{(i == 0 ? "" : ",")} {parameters[i].field} = @{parameters[i].field}";
                _cmd.AddWithValue(parameters[i].field, parameters[i].value);
            }

            _cmd.CommandText += _FieldValues;

            return this;
        }

        public AdvancedCommandReady Where(Action<QueryFilterBuilder> filterConfig)
        {
            QueryFilterBuilder filter = new(_cmd);
            filterConfig(filter);
            _cmd = filter.GetCommandWithFilter();
            return new AdvancedCommandReady(_cmd);
        }
    }

    protected DeleteCommand Delete()
        => new DeleteCommand(CreateCommand($"DELETE {_TableName} "));
    public class DeleteCommand : AdvancedCommand
    {
        public DeleteCommand(IDbCommand cmd) : base(cmd)
        {
        }

        public AdvancedCommandReady Where(Action<QueryFilterBuilder> filterConfig)
        {
            QueryFilterBuilder filter = new(_cmd);
            filterConfig(filter);
            _cmd = filter.GetCommandWithFilter();
            return new AdvancedCommandReady(_cmd);
        }
    }

    public class AdvancedCommandReady
    {
        IDbCommand _cmd;

        public AdvancedCommandReady(IDbCommand cmd)
        {
            _cmd = cmd;
        }

        public DbResult Execute()
        {
            try
            {
                _cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                return DbResult.Exception("Could not execute the command.", ex);
            }

            return DbResult.Ok();
        }

        public DbResult<object> ExecuteScalar()
        {
            object obj = null;

            _cmd.CommandText = _cmd.CommandText.Replace(") VALUES (", ") OUTPUT Inserted.ID VALUES (");

            try
            {
                obj = _cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                return DbResult.Exception<object>("Could not execute the command.", ex);
            }

            return DbResult.Ok(obj);
        }
    }
}