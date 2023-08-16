using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            int totalShopCount = await GetTotalShopCount();
            Console.WriteLine($"Total shop count from main site: {totalShopCount}");

            int totalMemberCount = await CalculateTotalMemberCount();
            Console.WriteLine($"Total member count from state sites: {totalMemberCount}");

            if (totalShopCount == totalMemberCount)
            {
                Console.WriteLine("Total shop count matches total member count.");
                Console.WriteLine($"Total shop count: {totalShopCount}");
                Console.WriteLine($"Total member count: {totalMemberCount}");
            }
            else
            {
                Console.WriteLine("Total shop count does not match total member count.");
                Console.WriteLine($"Total shop count: {totalShopCount}");
                Console.WriteLine($"Total member count: {totalMemberCount}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static async Task<int> GetTotalShopCount()
    {
        Console.WriteLine("Getting total shop count from main site...");
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync("https://www.antiquetrail.com");
        
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(response);

        var shopNumberText = document.QuerySelector("p:contains('antique and vintage shops')")?.TextContent;
        int shopNumber;
        if(shopNumberText==null){
            Console.WriteLine("text is not available on website");
            shopNumber = 0;
        }else{
            shopNumber = ExtractShopNumber(shopNumberText);
        }

        return shopNumber;
    }

    static int ExtractShopNumber(string input)
    {
        int maxNumber = 0;

        // Match all numbers
        MatchCollection matches = Regex.Matches(input, @"[\d,]+");
        foreach (Match match in matches)
        {
            string valueWithoutCommas = match.Value.Replace(",", "");
            int number;
            if (int.TryParse(valueWithoutCommas, out number))
            {
                maxNumber = Math.Max(maxNumber, number);
            }
        }

        return maxNumber;
    }

    static async Task<int> CalculateTotalMemberCount()
    {
        int totalMemberCount = 0;
        
    //     string[] stateNames = {
    // "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia",
    // "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland",
    // "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "NewHampshire", "NewJersey",
    // "NewMexico", "NewYork", "NorthCarolina", "NorthDakota", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "RhodeIsland", "SouthCarolina",
    // "SouthDakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "WestVirginia", "Wisconsin", "Wyoming"
    // };
        string[] stateNames = {"Alabama"};

        MakeCsvFile();

        foreach (string stateName in stateNames)
        {
            Console.WriteLine($"Getting total member count from {stateName} site...");
            using var httpClient = new HttpClient();
            var website = $"http://{stateName.ToLower()}antiquetrail.com";
            var response = await httpClient.GetStringAsync(website);
            
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(response);
            ProcessStateWebsite(document);
            var memberText = document.QuerySelector("h1:contains('members on')")?.TextContent;
            int memberNumber;
            if(memberText==null){
                Console.WriteLine($"error encounted in {stateName}");
                memberNumber = 0;
            }else{
                memberNumber = ExtractMemberNumber(memberText);
                Console.WriteLine($"total members present: {memberNumber}");
            }
            WriteToCsv(stateName, memberNumber, website);
            totalMemberCount += memberNumber;
        }

        return totalMemberCount;
    }

    static int ExtractMemberNumber(string input)
    {
        Match match = Regex.Match(input, @"\d+");
        int number = 0;
        if(int.TryParse(match.Value, out number))
        {
            return number;
        }
        return number;
    }

    static void MakeCsvFile()
    {
        string csvFilePath = "output.csv";

        if (File.Exists(csvFilePath))
        {
            File.Delete(csvFilePath);
            Console.WriteLine("CSV file cleared.");
        }

        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine("State,MemberCount,Website");
        }

        Console.WriteLine("Headings written to CSV file.");
    }

    static void WriteToCsv(string stateName, int number, string website)
    {
        string csvFilePath = "output.csv";

        //writing to csv
        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine($"{stateName},{number},{website}");
        }

        Console.WriteLine("Data written to CSV file.");
    }

    static void ProcessStateWebsite(IHtmlDocument document)
    {
        Console.WriteLine("Processing state website...");
        var tablesPre = document.QuerySelectorAll("table tbody tr td div table tbody tr td div div");
        var tables = tablesPre.Where(x => x.TextContent.Contains(", AL"));
        // Console.WriteLine($"Found {tables.Length} tables.");
        int total = 0;
        foreach (var table in tables)
        {
            // Console.WriteLine(table.OuterHtml);
            total++;
            Console.WriteLine(table.ParentElement.OuterHtml);
            if(total>0)
            {
                break;
            }
            // ProcessTable((IHtmlTableElement)table);
        }
        Console.WriteLine($"Found {total} tables.");
    }

    static void ProcessTable(IHtmlTableElement table)
    {
        Console.WriteLine("Processing table...");
        Console.WriteLine(table.OuterHtml);
        Console.WriteLine();
    }
}
