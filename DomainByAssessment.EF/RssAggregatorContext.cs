using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainByAssessment.EF
{
    public class RssAggregatorContext:DbContext
    {
        public RssAggregatorContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }

        public DbSet<RssFeed> Feeds { get; set; }
        public DbSet<NewsItem> NewsItems { get; set; }
    }
}
