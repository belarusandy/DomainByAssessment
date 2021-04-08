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
            var sources = new List<(string, string)>() { ("INTERFAX", "https://interfax.by/news/feed/"), ("HABR", "https://habr.com/ru/rss/all/all/") };

            var provider_Url_Feeds = new ConcurrentBag<(string, string, SyndicationFeed)>();

            // I've got obvious knowledge gaps of AggregateException handling, so skipping it
            sources.AsParallel().ForAll(p =>
            {
                using var reader = XmlReader.Create(p.Item2);
                var feed = SyndicationFeed.Load(reader);
                provider_Url_Feeds.Add((p.Item1, p.Item2, feed));
            });

            using var ctx = new RssAggregatorContextFactory().CreateDbContext(Array.Empty<string>());
            ctx.Database.EnsureCreated();

            //ensure feed providers are added to the db
            if (provider_Url_Feeds.Select(f => f.Item1).Except(ctx.Feeds.Select(f => f.ProviderName)).Any())
            {
                //TODO:fix this bug of adding all the feeds to filter out only missing ones in db. otherwise adding new rss provider will lead to collisions
                ctx.Feeds.AddRange(provider_Url_Feeds.Select(f => new RssFeed() { ProviderName = f.Item1, Title = f.Item3.Title.Text }));
                ctx.SaveChanges();
            }

            foreach (var provider_Url_Feed in provider_Url_Feeds)
            {
                //TODO: optimize to avoid fetching all the items but query on the db side instead. sql profiling required.
                var freshNewsItemsSummaries = provider_Url_Feed.Item3.Items.Select(i => i.Summary.Text).Except(ctx.NewsItems.Where(i => i.Feed.ProviderName == provider_Url_Feed.Item1).Select(i => i.Summary));
                ctx.NewsItems.AddRange(provider_Url_Feed.Item3.Items
                    .Where(i => freshNewsItemsSummaries.Contains(i.Summary.Text))
                    .Select(i => new NewsItem() {
                        Feed = ctx.Feeds.Single(f => f.Title == provider_Url_Feed.Item3.Title.Text),
                        Guid = Guid.NewGuid(),
                        ExternalUrlId = i.Id,
                        Title = i.Title.Text,
                        PublishDateTimeOffset = i.PublishDate,
                        Summary = i.Summary.Text
                    }));
                System.Console.WriteLine($"News received count for {provider_Url_Feed.Item1}: {provider_Url_Feed.Item3.Items.Count()};");
                System.Console.WriteLine($"News saved count for {provider_Url_Feed.Item1}: {ctx.SaveChanges()};");
            }
        }
    }
}
