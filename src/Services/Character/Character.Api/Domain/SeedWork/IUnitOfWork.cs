﻿using System.Threading;
using System.Threading.Tasks;

namespace Character.Api.Domain.SeedWork
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync(CancellationToken cancellationToken = default);
    }
}