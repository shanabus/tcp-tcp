using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using TCP_TCP.Shared.Model;

namespace TCP_TCP.Server.Hubs;

public class TableHub : Hub
{
    private readonly MyMemoryCache _memoryCache = default!;
    
    private string CacheKey_TableLayout = "Table_Layout";
    private string CacheKey_TableData = "Table_Data";

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

    public async Task ResetSeats(bool allSeats)
    {
        var tables = GetTableLayout();
        var saved = SetTableStatistics(tables);

        if (allSeats)
        {
            tables.ForEach(t => t.Seats.ForEach(s => { s.Active = false; s.SecondShow = false; }));
        }
        else
        {
            tables.ForEach(t => t.Seats.Where(x => !x.SecondShow).ToList().ForEach(s => { s.Active = false; s.SecondShow = false; }));
        }
        SetTableLayout(tables);

        await Clients.All.SendAsync("TablesSet", tables);
    }

    public async Task GetTableStatisticData()
    {
        var data = GetTableStatistics();
        await Clients.Caller.SendAsync("TableStatistics", data);
    }

    protected List<Table> GetTableLayout()
    {
        List<Table> tables = new List<Table>();

        if (!_memoryCache.Cache.TryGetValue<List<Table>>(CacheKey_TableLayout, out tables))
        {
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
            tables.Add(new Table("10", 3, 6));
            tables.Add(new Table("11", 3, 6));

            SetTableLayout(tables);
        }
        return tables;
    }

    protected void SetTableLayout(List<Table> tables)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(6)).SetSize(1);
        
        _memoryCache.Cache.Set(CacheKey_TableLayout, tables, cacheEntryOptions);
    }

    protected Dictionary<string, int> GetTableStatistics() {
        Dictionary<string, int> tableData;
        
        if (!_memoryCache.Cache.TryGetValue<Dictionary<string, int>>(CacheKey_TableData, out tableData))
        {
            tableData = new Dictionary<string, int>();            
        }
        return tableData;
    }
    
    protected Dictionary<string, int> SetTableStatistics(List<Table> tables) 
    {
        var existingData = GetTableStatistics();

        foreach(var s in tables.SelectMany(x => x.Seats).Where(x => x.Active || x.SecondShow)){
            var score = s.SecondShow ? 2 : 1;
            if (existingData.ContainsKey(s.Position))
            {                
                var ss = existingData[s.Position] += score;
            }
            else 
            {
                existingData.Add(s.Position, score);
            }
        }

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(7)).SetSize(10);
        
        _memoryCache.Cache.Set(CacheKey_TableData, existingData, cacheEntryOptions);

        return existingData;
    }
}