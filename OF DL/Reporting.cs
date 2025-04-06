using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace OF_DL;

internal class Reporting(IEnumerable<Entities.Subscriptions.List> subscribedUsers)
{
    public void GenerateReport(string reportPath)
    {
        using var fileStream = File.Open(reportPath, FileMode.Create);
        using var streamWriter = new StreamWriter(fileStream);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.Context.RegisterClassMap<UserReportViewModelClassMap>();
        csvWriter.WriteRecords(subscribedUsers.Select(user => new UserReportViewModel(user)).OrderByDescending(e => e.category).ThenBy(e => e.currentTotal));
    }
}

internal class UserReportViewModel
{
    private readonly Entities.Subscriptions.List source;

    public const long YoursId = 817758071L;
    public const long MineId = 817758033L;
    public const decimal tax = 0.2m;

    private Lazy<string> _category;
    private Lazy<decimal?> _currentPrice;
    private Lazy<decimal?> _nextPrice;

    public UserReportViewModel(Entities.Subscriptions.List source)
    {
        this.source = source;
        _category = new Lazy<string>(GetCategory);
        _currentPrice = new Lazy<decimal?>(() => decimal.TryParse(source.subscribedByData.price, out var price) ? price : null);
        _nextPrice = new Lazy<decimal?>(() => decimal.TryParse(source.subscribedByData.regularPrice, out var price) ? price : null);
    }

    public string name => source.name;

    public string username => source.username;

    public string category => GetCategory();

    public string status => GetStatus();

    public DateTime? until => source.subscribedByData.expiredAt;

    public decimal? currentPrice => _currentPrice.Value;

    public decimal? currentTax => currentPrice * tax;

    public decimal? currentTotal => currentPrice + currentTax;

    public decimal? renewPrice => _nextPrice.Value;

    public decimal? renewTax => renewPrice * tax;

    public decimal? renewTotal => renewPrice + renewTax;

    public string GetCategory()
    {
        if (source.listsStates.SingleOrDefault(l => l.id is long intId && intId == YoursId && l.hasUser == true) is { } yours)
        {
            return yours.name;
        }
        if (source.listsStates.SingleOrDefault(l => l.id is long intId && intId == MineId && l.hasUser == true) is { } mine)
        {
            return mine.name;
        }
        return "None";
    }

    public string GetStatus()
    {
        if (source.subscribedByData is { } subData)
        {
            if (!string.IsNullOrEmpty(subData.status)) return subData.status;
            return "Subscribed";
        }
        return "API Error";
    }

}

internal class UserReportViewModelClassMap : ClassMap<UserReportViewModel>
{
    public UserReportViewModelClassMap()
    {
        var index = 0;
        Map(m => m.name).Index(index++).Name("Name");
        Map(m => m.username).Index(index++).Name("Username");
        Map(m => m.category).Index(index++).Name("Category");
        Map(m => m.status).Index(index++).Name("Status");
        Map(m => m.until).Index(index++).Name("Until");
        Map(m => m.currentPrice).Index(index++).Name("Price");
        Map(m => m.currentTax).Index(index++).Name("Tax");
        Map(m => m.currentTotal).Index(index++).Name("Total");
        Map(m => m.renewPrice).Index(index++).Name("Price");
        Map(m => m.renewTax).Index(index++).Name("Tax");
        Map(m => m.renewTotal).Index(index++).Name("Total");
    }
}
