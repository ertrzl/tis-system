using System.Linq.Expressions;
using Tis.Domain.Common;

namespace Tis.Application.Interfaces.Repositories.Generic;

// Hər cədvəl üçün ortaq oxuma/yazma əməliyyatları. Universitet (tenant) süzgəci DbContext-də olduğu üçün
// burada ayrıca yazmağa ehtiyac yoxdur: istifadəçi yalnız öz universitetinin qeydlərini görür.
public interface IRepository<T> where T : BaseEntity, new()
{
    IQueryable<T> GetAll(
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool isDescending = false,
        bool asNoTracking = false,
        int page = 0,
        int take = 0,
        params string[]? includes
    );

    Task<T?> GetByIdAsync(int id, params string[] includes);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);

    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);

    Task SaveChangesAsync();

    // Paylaşılan context üzərində tranzaksiya başladır: CommitAsync-ə qədər bütün repository-lərlə edilən saxlamalar
    // bir addım sayılır. Artıq tranzaksiya daxilində işləyən kodda istifadə olunmur (iç-içə olmur).
    Task<IRepositoryTransaction> BeginTransactionAsync();
}
