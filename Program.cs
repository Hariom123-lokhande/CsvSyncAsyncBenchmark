using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvSyncAsyncBenchmark.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CsvSyncAsyncBenchmark.Data;
using Microsoft.EntityFrameworkCore;

namespace CsvSyncAsyncBenchmark
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Disable all framework/EF Core noise from console
            builder.Logging.ClearProviders();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            builder.Services.AddTransient<CsvService>();

            var app = builder.Build();

            // ─────────────────────────────────────────────
            // STEP 1 — Ensure DB exists & seed if empty
            // ─────────────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();

                if (!db.Users.Any())
                {
                    Console.WriteLine("  [SEED] Database is empty. Seeding from CSV...");
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "data.csv");
                    if (File.Exists(filePath))
                    {
                        var csvService = scope.ServiceProvider.GetRequiredService<CsvService>();
                        var users = csvService.ReadCsvSync(filePath);
                        foreach (var u in users) u.Id = 0;
                        db.Users.AddRange(users);
                        db.SaveChanges();
                        Console.WriteLine($"  [SEED] {users.Count:N0} users inserted into database.\n");
                    }
                    else
                    {
                        Console.WriteLine("  [SEED] WARNING: data.csv not found!\n");
                    }
                }
            }
            //console Benchmark (DB Read)
            
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║     Database Sync vs Async Read Benchmark    ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");
                Console.WriteLine();

                // Synchronous
                Console.WriteLine($"  [SYNC START] {DateTime.Now:HH:mm:ss.fff} | Thread: {Environment.CurrentManagedThreadId}");
                var syncSw = Stopwatch.StartNew();
                var syncData = db.Users.ToList();
                syncSw.Stop();
                Console.WriteLine($"  [SYNC END  ] {DateTime.Now:HH:mm:ss.fff} | Records: {syncData.Count:N0} | Time: {syncSw.ElapsedMilliseconds} ms");
                Console.WriteLine();

                // Asynchronous
                Console.WriteLine($"  [ASYNC START] {DateTime.Now:HH:mm:ss.fff} | Thread: {Environment.CurrentManagedThreadId}");
                var asyncSw = Stopwatch.StartNew();
                var asyncData = await db.Users.ToListAsync();
                asyncSw.Stop();
                Console.WriteLine($"  [ASYNC END  ] {DateTime.Now:HH:mm:ss.fff} | Records: {asyncData.Count:N0} | Time: {asyncSw.ElapsedMilliseconds} ms");
                Console.WriteLine();

                // Results Summary Table
                Console.WriteLine($"  {"Operation",-20} {"Records",-12} {"Time (ms)",-12}");
                Console.WriteLine($"  {"─────────────────────────────────────────────"}");
                Console.WriteLine($"  {"Synchronous",-20} {syncData.Count,-12:N0} {syncSw.ElapsedMilliseconds,-12}");
                Console.WriteLine($"  {"Asynchronous",-20} {asyncData.Count,-12:N0} {asyncSw.ElapsedMilliseconds,-12}");
                Console.WriteLine();
            }


            // ─────────────────────────────────────────────
            // STEP 3 — API Endpoints
            // ─────────────────────────────────────────────
            app.MapGet("/", () => "Welcome! Use /db-sync or /db-async to benchmark database reads.");

            app.MapGet("/db-sync", (AppDbContext db) =>
            {
                Console.WriteLine($"  [DB SYNC START] {DateTime.Now:HH:mm:ss.fff} | Thread: {Environment.CurrentManagedThreadId}");
                var sw = Stopwatch.StartNew();
                var data = db.Users.ToList();
                sw.Stop();
                Console.WriteLine($"  [DB SYNC END  ] {DateTime.Now:HH:mm:ss.fff} | Records: {data.Count:N0} | Time: {sw.ElapsedMilliseconds} ms");

                return new { Count = data.Count, TimeTakenMs = sw.ElapsedMilliseconds };
            });

            app.MapGet("/db-async", async (AppDbContext db) =>
            {
                Console.WriteLine($"  [DB ASYNC START] {DateTime.Now:HH:mm:ss.fff} | Thread: {Environment.CurrentManagedThreadId}");
                var sw = Stopwatch.StartNew();
                var data = await db.Users.ToListAsync();
                sw.Stop();
                Console.WriteLine($"  [DB ASYNC END  ] {DateTime.Now:HH:mm:ss.fff} | Records: {data.Count:N0} | Time: {sw.ElapsedMilliseconds} ms");

                return new { Count = data.Count, TimeTakenMs = sw.ElapsedMilliseconds };
            });

            

            app.Run();
        }
    }
}