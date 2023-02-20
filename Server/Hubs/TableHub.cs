using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using TCP_TCP.Shared.Model;

namespace TCP_TCP.Server.Hubs;

public class TableHub : Hub
{
    private readonly MyMemoryCache _memoryCache = default!;
    
    private string CacheKey_TableLayout = "Table_Layout";

    public TableHub(MyMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task GetTables()
    {
        var tables = GetTableLayout();

        await Clients.Caller.SendAsync("TablesSet", tables);
    }
    
    public async Task SeatedForShow(string seatPosition)
    {        
        var tables = GetTableLayout();

        var target = tables.SelectMany(x => x.Seats).First(x => x.Position == seatPosition);
        if (target != null)
        {
            target.Active = !target.Active;
            if (!target.Active && target.SecondShow)
            {
                Console.WriteLine("false to remove 2nd show");
                target.SecondShow = false;
            }
        }

        SetTableLayout(tables);

        await Clients.All.SendAsync("UpdateSeats", tables, seatPosition);
    }

    public async Task StayingForSecondShow(string seatPosition)
    {
        var tables = GetTableLayout();

        var target = tables.SelectMany(x => x.Seats).First(x => x.Position == seatPosition);
        if (target != null)
        {
            target.Active = true;
            target.SecondShow = true;
        }

        SetTableLayout(tables);

        await Clients.All.SendAsync("UpdateSeats", tables, seatPosition);
    }

    public async Task ResetSeats()
    {
        var tables = GetTableLayout();
        tables.ForEach(t => t.Seats.ForEach(s => { s.Active = false; s.SecondShow = false; }));
        SetTableLayout(tables);

        await Clients.All.SendAsync("TablesSet", tables);
    }

    protected List<Table> GetTableLayout()
    {
        List<Table> tables = new List<Table>();

        if (!_memoryCache.Cache.TryGetValue<List<Table>>(CacheKey_TableLayout, out tables))
        {

            // Console.WriteLine("Cache not found for tables");
            
            tables = new List<Table>();
            tables.Add(new Table("1", 1, 8));
            tables.Add(new Table("2", 1, 8));
            tables.Add(new Table("3", 1, 8));
            tables.Add(new Table("4", 1, 8));        
            tables.Add(new Table("5", 1, 8));

            tables.Add(new Table("12", 2, 3));
            tables.Add(new Table("6", 2, 8));
            tables.Add(new Table("7", 2, 8));
            tables.Add(new Table("8", 2, 8));
            tables.Add(new Table("9", 2, 8));

            
            tables.Add(new Table("13", 3, 3));
            tables.Add(new Table("11", 3, 6));
            tables.Add(new Table("10", 3, 6));

            SetTableLayout(tables);
        } else {
            // Console.WriteLine("Found a table in cache!");
        }

        return tables;
    }

    protected void SetTableLayout(List<Table> tables)
    {
        // Console.WriteLine("Saving changes to Table layout");

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(6)).SetSize(1);
        
        _memoryCache.Cache.Set(CacheKey_TableLayout, tables, cacheEntryOptions);
    }
}