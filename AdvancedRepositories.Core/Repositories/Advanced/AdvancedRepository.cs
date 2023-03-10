using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using AdvancedRepositories.Core.Repositories.Fluent;
using System.Data;

namespace AdvancedRepositories.Core.Repositories.Advanced;


public abstract class AdvancedRepository : BaseRepository
{
    protected AdvancedRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    protected FluentQueryBuilder<T> Select<T>(params string[] fields) where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromMappedFields(fields)} "));

    protected FluentQueryBuilder<T> SelectDistinct<T>(params string[] fields) where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromMappedFields(fields)} "));

    protected FluentQueryBuilder<T> Select<T>() where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromAttributesOf<T>()} "));

    protected FluentQueryBuilder<T> SelectDistinct<T>() where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromAttributesOf<T>()} "));

    protected InsertCommand InsertInto(string table)
    {
        OpenTransaction();
        return new InsertCommand(CreateCommand($"INSERT INTO {table} "));
    }

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

    protected UpdateCommand UpdateFrom(string table)
    {
        OpenTransaction();
        return new UpdateCommand(CreateCommand($"UPDATE {table} SET "));
    }

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

    protected DeleteCommand DeleteFrom(string table)
    {
        OpenTransaction();
        return new DeleteCommand(CreateCommand($"DELETE {table} "));
    }
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
                using (_cmd)
                {
                    _cmd.ExecuteNonQuery();
                    _cmd.Transaction?.Commit();
                }
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
                using (_cmd)
                {
                    obj = _cmd.ExecuteScalar();
                    _cmd.Transaction?.Commit();
                }
            }
            catch (Exception ex)
            {
                return DbResult.Exception<object>("Could not execute the command.", ex);
            }

            return DbResult.Ok(obj);
        }
    }
}
