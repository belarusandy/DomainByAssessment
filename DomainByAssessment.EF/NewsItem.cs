using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainByAssessment.EF
{
    public class NewsItem
    {
        [Key]
        public Guid Guid { get; set; }
        public string ExternalUrlId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public bool SummaryIsHtml { get; set; }
        public RssFeed Feed { get; set; }
    }
}
