using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CsvSyncAsyncBenchmark.Models;

namespace CsvSyncAsyncBenchmark.Services
{
    public class CsvService
    {
        public List<User> ReadCsvSync(string filePath)
        {
            var data = new List<User>();
            using (var reader = new StreamReader(filePath))
            {
                reader.ReadLine(); // skip header
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out int id))
                    {
                        data.Add(new User
                        {
                            Id = id,
                            Name = parts[1],
                            Email = parts[2]
                        });
                    }
                }
            }
            return data;
        }
    }

    public class CsvServiceAsync
    {
        public async Task<List<User>> ReadCsvAsync(string filePath)
        {
            var data = new List<User>();
            using (var reader = new StreamReader(filePath))
            {
                await reader.ReadLineAsync(); // skip header
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length >= 3 && int.TryParse(parts[0], out int id))
                    {
                        data.Add(new User
                        {
                            Id = id,
                            Name = parts[1],
                            Email = parts[2]
                        });
                    }
                }
            }
            return data;
        }
    }
}
