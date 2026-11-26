using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Tis.Persistence.Extensions;

public static class DbUpdateExceptionExtensions
{
    // Unikal indeks və ya məhdudiyyət sətri qəbul etmədi (SQL Server xətası 2601 / 2627): başqası daha tez yazıb.
    public static bool IsUniqueViolation(this DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
