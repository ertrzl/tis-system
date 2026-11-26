namespace Tis.Application.Interfaces.Repositories.Generic;

// Bir neçə repository üzərində "ya hamısı, ya heç biri" addımı. CommitAsync çağırılmadan `await using` blokundan
// çıxılsa, daxildə edilən hər şey geri qaytarılır.
public interface IRepositoryTransaction : IAsyncDisposable
{
    Task CommitAsync();
}
