using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Infrastructure.Common
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext context;

        public UnitOfWork(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
