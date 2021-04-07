using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainByAssessment.EF
{
    public class RssFeed
    {
        public int Id { get; set; }
        public string ProviderName { get; set; }
        public string Title { get; set; }
        public ICollection<NewsItem> NewsItems { get; set; }
    }
}
