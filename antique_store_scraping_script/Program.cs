﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            int totalShopCount = await GetTotalShopCount();
            // int totalShopCount = 0;
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

        // Match all numbers with a coma
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

        string[] stateCode = {
            "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA",
            "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD",
            "MA", "MI", "MN", "MS", "MO", "MT", "NE", "NV", "NH", "NJ",
            "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC",
            "SD", "TN", "TX", "UT", "VT", "VA", "WA", "WV", "WI", "WY"
        };

        MakeCsvFile("information.csv","State,Shops,Website");
        MakeCsvFile("shops.csv","Name,Phone,Address,Facebook,State");
        using var httpClient = new HttpClient();
        var website = "http://www.alabamaantiquetrail.com/";
        var response = await httpClient.GetStringAsync(website);
        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(response);
        var stateWebsiteList = document.QuerySelector("strong:contains('State Trails')")?.ParentElement?.QuerySelectorAll("a");
        if(stateWebsiteList==null)
        {
            return totalMemberCount;
        }
        var links = stateWebsiteList.Where(x => x.GetAttribute("href").Contains(".com"));
        bool virginia = true;
        int counter = 0;
        foreach (var link in links)
        {   
            var href = link.GetAttribute("href");
            Console.WriteLine($"Getting total member count from {href}");
            response = await httpClient.GetStringAsync(href);
            document = await parser.ParseDocumentAsync(response);
            var stateName = "unavailable";
            if(href!=null)
            {
                stateName = ExtractStateName(href);
            }
            if(stateName=="Virginia")
            {
                if(virginia)
                {
                    stateName="WestVirginia";
                }
                else
                {
                    virginia=true;
                }
            }
            ProcessStateWebsite(document,stateName,stateCode[counter]);
            var memberText = document.QuerySelector("h1:contains('members on')")?.TextContent;
            int memberNumber;
            if(memberText==null){
                Console.WriteLine($"error encounted in {href}");
                memberNumber = 0;
            }else{
                memberNumber = ExtractMemberNumber(memberText);
                Console.WriteLine($"total members present: {memberNumber}");
            }
            if(href!=null)
            {
                WriteToCsv(stateName, memberNumber, href);
            }
            totalMemberCount += memberNumber;
            counter++;
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

    static void MakeCsvFile(string fileName, string headings)
    {
        string csvFilePath = fileName;

        if (File.Exists(csvFilePath))
        {
            File.Delete(csvFilePath);
        }

        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine(headings);
        }
    }

    static void WriteToCsv(string stateName, int number, string website)
    {
        string csvFilePath = "information.csv";

        //writing to csv
        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine($"{stateName},{number},{website}");
        }

        Console.WriteLine($"Data for {stateName} written to CSV file.");
    }

    static void ProcessStateWebsite(IHtmlDocument document,string stateName, string stateCode)
    {
        Console.WriteLine("Processing state website for vendor information...");
        var tablesPre = document.QuerySelectorAll("table tbody tr td div table tbody tr td div div");
        //getting the table content through state address
        var tables = tablesPre.Where(x => x.TextContent.Contains($", {stateCode}"));
        // Console.WriteLine($"Found {tables.Length} tables.");
        int total = 0;
        foreach (var table in tables)
        {
            // Console.WriteLine(table.OuterHtml);
            total++;
            ProcessTable((IHtmlDivElement)table,stateName,stateCode);
            // if(total>0)
            // {
            //     break;
            // }
        }
    }

    static void ProcessTable(IHtmlDivElement table, string stateName, string stateCode)
    {
        var name_to_check = table.ParentElement?.QuerySelectorAll("div a strong")[0].TextContent;
        var name = "unavailable";
        if(name_to_check!=null){
            name = RemoveExtraSpaces(name_to_check);
        }

        var phones = table.ParentElement?.QuerySelectorAll("div a");
        var phone = "not available";
        if(phones!=null)
        {
            foreach(var x in phones)
            {
                if(IsValidPhoneNumberFormat(x.TextContent))
                {
                    phone = x.TextContent;
                }
            }
        }
        

        var addresses = table.ParentElement?.QuerySelectorAll($"div:contains(', {stateCode}')");
        var address = "unavailable";
        if(addresses!=null)
        {
            foreach(var x in addresses)
            {
                var temp_addr_text = x.TextContent.Trim();
                if(temp_addr_text!=null)
                {
                    address = RemoveExtraSpaces(temp_addr_text);    
                }
            }
        }
        

        var socials = table.ParentElement?.QuerySelectorAll("div a img");
        var facebook = "unavailable";
        if(socials!=null)
        {
            facebook = ExtractFacebookLink(socials);
        }
        

        string csvFilePath = "shops.csv";
        //writing to csv
        using (StreamWriter writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine($"{name},{phone},{address},{facebook},{stateName}");
        }
    }

    static bool IsValidPhoneNumberFormat(string phoneNumber)
    {
        string pattern = @"^\d{3}-\d{3}-\d{4}$";
        return Regex.IsMatch(phoneNumber, pattern);
    }

    static string RemoveExtraSpaces(string input)
    {
        string cleanedText = Regex.Replace(input, @"\s+", " ").Trim();
        cleanedText = cleanedText.Replace(", ", " ").Trim();
        return cleanedText;
    }

    static string ExtractFacebookLink(IHtmlCollection<IElement> SocialLinks)
    {
        if(SocialLinks.Any())
        {
            var temp_fb = SocialLinks[0].ParentElement?.GetAttribute("href");
            if(temp_fb!=null)
            {
                return temp_fb;
            }else{
                return "unavailable";
            }
        }
        else
        {
            return "unavailable";
        }
        
    }

    static string ExtractStateName(string stateName)
    {
        string[] stateNames = {
            "Alabama", "Alaska", "Arizona", "Arkansas", "California",
            "Colorado", "Connecticut", "Delaware", "Florida", "Georgia",
            "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa",
            "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland",
            "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri",
            "Montana", "Nebraska", "Nevada", "NewHampshire", "NewJersey",
            "NewMexico", "NewYork", "NorthCarolina", "NorthDakota", "Ohio",
            "Oklahoma", "Oregon", "Pennsylvania", "RhodeIsland", "SouthCarolina",
            "SouthDakota", "Tennessee", "Texas", "Utah", "Vermont",
            "Virginia", "Washington", "WestVirginia", "Wisconsin", "Wyoming"};

        foreach (string state in stateNames)
        {
            if (stateName.Contains(state, StringComparison.OrdinalIgnoreCase))
            {
                return state;
            }
        }
        return "Unavailable";
    }
}
