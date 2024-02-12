using Dapper;
using System.Data;

namespace Bookify.Infrastructure.Data;

internal sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    /*
     * Dapper does not support DateOnly out of the box. So, we need to 
     * help him to map this value correctly
     */
    public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        //this is invoked when sending a value to the DB or materializing one into Memory
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}