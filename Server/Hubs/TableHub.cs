using System.Text.Json;
using System.Text.Json.Serialization;
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

        if (allSeats)
        {
            tables.ForEach(t => {
                t.Seats.ForEach(s => { s.Active = false; s.SecondShow = false; });
                t.Note = "";
            });
        }
        else
        {
            tables.ForEach(t => {
                t.Seats.Where(x => !x.SecondShow).ToList().ForEach(s => { s.Active = false; s.SecondShow = false; });
                t.Note = "";
            });
        }
        SetTableLayout(tables);

        await Clients.All.SendAsync("TablesSet", tables);
    }

    public async Task UpdateTableNote(string tablePosition, string note)
    {
        var tables = GetTableLayout();
        var table = tables.FirstOrDefault(x => x.Position == tablePosition);

        if (table != null)
        {
            table.Note = note;

            SetTableLayout(tables);

            await Clients.All.SendAsync("TablesSet", tables);
        }
    }

    protected List<Table> GetTableLayout()
    {
        List<Table>? tables = new List<Table>();

        if (!_memoryCache.Cache.TryGetValue<List<Table>>(CacheKey_TableLayout, out tables) || tables is null)
        {
            // attempt to read from File
            tables = ReadFile();

            if (tables == null || tables.Count == 0)
            {
                tables = new List<Table>
                {
                    new Table("1", 1, 8),
                    new Table("2", 1, 8),
                    new Table("3", 1, 8),
                    new Table("4", 1, 8),
                    new Table("5", 1, 8),
                    new Table("13", 2, 3),
                    new Table("6", 2, 8),
                    new Table("7", 2, 8),
                    new Table("8", 2, 8),
                    new Table("9", 2, 8),
                    new Table("10", 3, 6),
                    new Table("11", 3, 6),
                    new Table("12", 3, 3)
                };
            }

            SetTableLayout(tables);
        }
        return tables;
    }

    protected void SetTableLayout(List<Table> tables)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(8)).SetSize(512);
        
        _memoryCache.Cache.Set(CacheKey_TableLayout, tables, cacheEntryOptions);

        WriteFile(tables);
    }

    private static readonly JsonSerializerOptions _options = 
        new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true };
        
    protected void WriteFile(List<Table> tables)
    {
        var jsonString = JsonSerializer.Serialize(tables, _options);
        File.WriteAllText("tables-data.json", jsonString);
    }

    protected List<Table> ReadFile()
    {
        List<Table> tables = new List<Table>();

        try
        {
            string jsonString = File.ReadAllText("tables-data.json");
            tables = JsonSerializer.Deserialize<List<Table>>(jsonString)!;        
        }
        catch(Exception e)
        {
            Console.WriteLine("No tables found in file or file was missing. " + e.Message);            
        }
        return tables;
        
    }
}