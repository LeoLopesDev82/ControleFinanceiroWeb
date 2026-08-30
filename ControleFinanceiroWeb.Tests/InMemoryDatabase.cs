using System;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Data;

namespace ControleFinanceiroWeb.Tests
{
    internal static class InMemoryDatabase
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }
    }
}
