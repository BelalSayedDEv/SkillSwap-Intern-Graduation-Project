namespace SkillSwap.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> CompleteAsync();
}
