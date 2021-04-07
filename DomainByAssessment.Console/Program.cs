using System;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DomainByAssessment.EF;
using System.Collections.Concurrent;

namespace DomainByAssessment.Console
{
    class Program
    {
        static void Main()
        {
            var sources = new ConcurrentBag<(string, string)>() { ("INTERFAX", "https://interfax.by/news/feed/"), ("HABR", "https://habr.com/ru/rss/all/all/") };

            var providerUrlFeeds = new ConcurrentBag<(string, string, SyndicationFeed)>();

            // I've got obvious knowledge gaps of AggregateException handling, so skipping it
            sources.AsParallel().ForAll(p =>
            {
                using var reader = XmlReader.Create(p.Item2);
                var feed = SyndicationFeed.Load(reader);
                providerUrlFeeds.Add((p.Item1, p.Item2, feed));
            });

            using var ctx = new RssAggregatorContextFactory().CreateDbContext(Array.Empty<string>());
            ctx.Database.EnsureCreated();

            //ensure feed providers are added to the db
            if (providerUrlFeeds.Select(f => f.Item1).Except(ctx.Feeds.Select(f => f.ProviderName)).Any())
            {
                ctx.Feeds.AddRange(providerUrlFeeds.Select(f => new RssFeed() { ProviderName = f.Item1, Title = f.Item3.Title.Text }));
                ctx.SaveChanges();
            }

            foreach (var feed in providerUrlFeeds)
            {
                //TODO: optimize for not fetching all the items but querying on the db side instead
                var freshNewsItemsSummaries = feed.Item3.Items.Select(i => i.Summary.Text).Except(ctx.NewsItems.Where(i => i.Feed.ProviderName == feed.Item1).Select(i => i.Summary));
                ctx.NewsItems.AddRange(feed.Item3.Items
                    .Where(i => freshNewsItemsSummaries.Contains(i.Summary.Text))
                    .Select(i => new NewsItem() {
                        Feed = ctx.Feeds.Single(f => f.Title == feed.Item3.Title.Text),
                        Guid = Guid.NewGuid(),
                        ExternalUrlId = i.Id,
                        Title = i.Title.Text,
                        Summary = i.Summary.Text
                    }));
                System.Console.WriteLine($"News received count for {feed.Item1}: {feed.Item3.Items.Count()};");
                System.Console.WriteLine($"News saved count for {feed.Item1}: {ctx.SaveChanges()};");
            }
        }
    }
}
