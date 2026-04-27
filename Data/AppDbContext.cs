using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using CsvSyncAsyncBenchmark.Models;

namespace CsvSyncAsyncBenchmark.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}