using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DomainByAssessment.EF
{
    public class RssAggregatorContextFactory : IDesignTimeDbContextFactory<RssAggregatorContext>
    {
        public RssAggregatorContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var dbContextBuilder = new DbContextOptionsBuilder();

            var connectionString = configuration
                        .GetConnectionString("SqlConnectionString");

            dbContextBuilder.UseSqlServer(connectionString);

            return new RssAggregatorContext(dbContextBuilder.Options);
        }
    }
}
