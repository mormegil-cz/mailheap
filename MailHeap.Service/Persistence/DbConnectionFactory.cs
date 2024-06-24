using System.Text;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.SqlQuery;
using MailHeap.Service.Persistence.Model;

namespace MailHeap.Service.Persistence;

public class DbConnectionFactory(
    DataOptions dbConnectionOptions
)
{
    public DbConnection Create() => new(dbConnectionOptions);

    public static DataOptions CreateDataOptions(ConfigurationManager configuration) =>
        new DataOptions()
            .UseSQLite(configuration.GetConnectionString("dbConnection") ?? throw new InvalidOperationException("Connection string configuration missing"))
            .UseMappingSchema(CreateMappingSchema());

    private static MappingSchema CreateMappingSchema()
    {
        var mapping = new MappingSchema();
        mapping.AddScalarType(typeof(DateTime), DataType.Double);
        mapping
            .SetConverter<Timestamp, double>(TimestampToSqliteDouble)
            .SetConverter<double, Timestamp>(SqliteDoubleToDateTime)
            .SetConvertExpression<Timestamp, DataParameter>(x => DataParameter.Double(null, TimestampToSqliteDouble(x)))
            .SetConvertExpression<Timestamp?, DataParameter>(x => new DataParameter(null, x.HasValue ? TimestampToSqliteDouble(x.Value) : null, DataType.Double))
            .SetValueToSqlConverter(typeof(Timestamp), ConvertToSql)
            .SetValueToSqlConverter(typeof(Timestamp?), ConvertToSql);
        return mapping;
    }

    private static void ConvertToSql(StringBuilder sb, SqlDataType type, object? value)
    {
        switch (value)
        {
            case null:
                sb.Append("NULL");
                break;
            case Timestamp dt:
                sb.Append(TimestampToSqliteDouble(dt));
                break;
            default:
                throw new ArgumentException("Unexpected type " + value.GetType(), nameof(value));
        }
    }

    private const double OaEpochJulianDate = 2415018.5;

    private static double TimestampToSqliteDouble(Timestamp dt) => dt.Value.ToOADate() + OaEpochJulianDate;

    private static Timestamp SqliteDoubleToDateTime(double d) => new(DateTime.FromOADate(d - OaEpochJulianDate));
}