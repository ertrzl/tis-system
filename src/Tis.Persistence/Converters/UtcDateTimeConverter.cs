using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Tis.Persistence.Converters;

// SQL Server-in datetime2 tipində saat qurşağı yoxdur, ona görə EF dəyəri Kind=Unspecified kimi oxuyur və API onu
// "Z" olmadan göndərir. Oxuyanda UTC işarələmək JSON-a brauzerin ehtiyac duyduğu qurşağı əlavə edir.
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
