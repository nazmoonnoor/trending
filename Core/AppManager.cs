using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using Core;

namespace Core
{
    public static class AppManager
    {
        public static List<string> GetCompanyCodes()
        {
            //var cc = new List<string>(); //GetCompanyCodes();
            //cc.Add("1JANATAMF");
            //cc.Add("AIL");
            //return cc;

            var codes = new List<string>();
            const Int32 BufferSize = 128;
            //using (var fileStream = System.IO.File.OpenRead(HostingEnvironment.MapPath(@"~/App_Data/codes.txt")))
            using (var fileStream = System.IO.File.OpenRead("codes.txt"))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    codes.Add(line.Trim());
                }
            }
            
            return codes;
        }

        public static void SaveTrades(int counter = 1)
        {
            // "https://www.amarstock.com/data/multiTradeDay/ACI/Minute1/10"
            List<Dictionary<string, object>> trades = new List<Dictionary<string, object>>();
            var codes = GetCompanyCodes();

            for(int i = counter-1; i < codes.Count(); i++)
            {
                string fundamental = string.Format("https://www.amarstock.com/api/feed/fundamental/basic?code={0}", codes[i]);
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    try
                    {
                        var json = wc.DownloadString(fundamental);
                        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        trades.Add(GetTradeData(codes[i], data));
                        var trade = GetTradeData(codes[i], data);
                        AppManager.InsertTrade(trade);
                        ProgressUpdate(codes[i], i, codes.Count);
                        Thread.Sleep(600);

                    }
                    catch
                    {
                        SaveTrades(i);
                    }
                }
            }
        }

        private static void ProgressUpdate(string code, int i, int total)
        {
            Thread.Sleep(600);
            Console.Clear();
            Console.WriteLine(string.Format("{1} out of {2} - {0}", code, i+1, total));
        }

        private static Dictionary<string, object> GetTradeData(string code, dynamic data)
        {

            var trade = new Dictionary<string, object>();
            trade.Add("Code", code);
            trade.Add("LastUpdate", GetLastUpdate(data["LastUpdate"]));
            trade.Add("DayVolumn", Convert.ToInt64(data["Volume"]));
            trade.Add("DayValue", Convert.ToInt64(data["DaysValue"]) * 1000000);
            trade.Add("TotalTrade", data["TotalTrade"]);
            trade.Add("DayRange", data["DayRange"]);
            trade.Add("Week52Range", data["Week52Range"]);
            trade.Add("LTP", data["LastTrade"]);
            trade.Add("YCP", data["YCP"]);
            trade.Add("MarketCategory", data["MarketCategory"]);
            trade.Add("Electronic", data["Electronic"]);
            
            return trade;
        }

        public static string GetLastUpdate(object value)
        {
            if (value == null)
                return DateTime.Today.AddDays(-1).DateTimeFormat();
            var str = value.ToString();
            var split = str.Substring(0, str.IndexOf("-")).Trim().Split('/');
            return DateTime.Parse(string.Format("{0}/{1}/{2}", split[1], split[0], split[2])).DateTimeFormat();
        }

        public static void InsertTrade(Dictionary<string, object> trade)
        {
            var dal = new SqliteDataAccess();

            string query = @"INSERT INTO Tradings_Dump (Code, LastUpdate, DayVolumn, DayValue, TotalTrade, DayRange, Week52Range, LTP, YCP, MarketCategory,Electronic) 
                                       VALUES (@Code,@LastUpdate,@DayVolumn,@DayValue,@TotalTrade,@DayRange,@Week52Range,@LTP,@YCP,@MarketCategory,@Electronic)";
            var connection = new SQLiteConnection(SqliteDataAccess.ConnectionString);
            var command = new SQLiteCommand(query, connection);
            
            command.Parameters.AddWithValue("@Code", trade["Code"]);
            command.Parameters.AddWithValue("@LastUpdate", trade["LastUpdate"]);
            command.Parameters.AddWithValue("@DayVolumn", trade["DayVolumn"]);
            command.Parameters.AddWithValue("@DayValue", trade["DayValue"]);
            command.Parameters.AddWithValue("@TotalTrade", trade["TotalTrade"]);
            command.Parameters.AddWithValue("@DayRange", trade["DayRange"]);
            command.Parameters.AddWithValue("@Week52Range", trade["Week52Range"]);
            command.Parameters.AddWithValue("@LTP", trade["LTP"]);
            command.Parameters.AddWithValue("@YCP", trade["YCP"]);
            command.Parameters.AddWithValue("@MarketCategory", trade["MarketCategory"]);
            command.Parameters.AddWithValue("@Electronic", trade["Electronic"]);

            dal.ExecuteNonQuery(command);
        }

        public static IEnumerable<dynamic> GetTrades(int days = 10)
        {
            var dal = new SqliteDataAccess();
        
            string query = string.Format(@"SELECT Code, LastUpdate, DayVolumn, DayValue, TotalTrade, DayRange, Week52Range, LTP, YCP, MarketCategory,Electronic
                                FROM Tradings
                                WHERE CAST(strftime('%s', LastUpdate)  AS  integer) >= CAST(strftime('%s', '{0}')  AS  integer)", DateTime.Today.AddDays(-days).DateTimeFormat());

            var connection = new SQLiteConnection(SqliteDataAccess.ConnectionString);
            var command = new SQLiteCommand(query, connection);

            var dt = dal.Execute(command);

            return dt.ReadAsDynamicEnumerable();
        }

        public static IEnumerable<dynamic> GetLastDayTrades(string orderby, bool isSpiked = false)
        {
            var lastWorkDay = DateTime.Today.LastWorkDay();
            var dal = new SqliteDataAccess();

            string query = string.Format(@"SELECT Code, LastUpdate, DayVolumn, DayValue, TotalTrade, DayRange, Week52Range, LTP, YCP, MarketCategory,Electronic, 
                    tdv.VolumeSum AS VolumeSum 
                    FROM Tradings
                    INNER JOIN (SELECT Code AS c, SUM(DayVolumn) VolumeSum FROM Tradings WHERE LastUpdate = '{0}' or LastUpdate = '{1}' GROUP BY Code) AS tdv ON tdv.c = Tradings.Code
                    WHERE CAST(strftime('%s', LastUpdate)  AS  integer) = (SELECT MAX(CAST(strftime('%s', LastUpdate)  AS  integer)) FROM Tradings)
                    ORDER BY {2} DESC", lastWorkDay.AddDays(-1).DateTimeFormat(), lastWorkDay.AddDays(-2).DateTimeFormat(), orderby);

            var connection = new SQLiteConnection(SqliteDataAccess.ConnectionString);
            var command = new SQLiteCommand(query, connection);

            var dt = dal.Execute(command);

            var result = dt.ReadAsDynamicEnumerable();

            if (!isSpiked)
            {
                foreach (var item in result)
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in result)
                {
                    if(item.VolumeSum < item.DayVolumn)
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}