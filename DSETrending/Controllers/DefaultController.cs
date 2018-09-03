using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace DSETrending.Controllers
{
    public class DefaultController : Controller
    {
        public ActionResult Index(string page, string spiked)
        {
            bool sp = !string.IsNullOrEmpty(spiked) && bool.Parse(spiked);
            //var lastDayTrades = AppManager.GetLastDayTrades("DayValue", sp);
            
            int size = 20;
            int p = string.IsNullOrEmpty(page) ? 0 : int.Parse(page) - 1;
            int skip = p * size;
            //var dayTrades = lastDayTrades as dynamic[] ?? lastDayTrades.ToArray();
            

            var result = AppManager.GetStockInfo().OrderByDescending(x=>x.Value);
            //var result = Core.Extensions.Sort(stocks.ToList(), "Value");
            //turn Ok(result);
            ViewData["PageHeader"] = string.Format(@"{0} - {1} out of {2}", skip + 1, skip + size, result.Count());
            return View(result.Skip(skip).Take(size));
        }

        public ActionResult Now(string page, string spiked)
        {
            var codes = new List<string>();
            const Int32 BufferSize = 128;
            using (var fileStream = System.IO.File.OpenRead(HostingEnvironment.MapPath(@"~/App_Data/codes.txt")))
            //using (var fileStream = System.IO.File.OpenRead("codes.txt"))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    codes.Add(line.Trim());
                }
            }
            return View(codes);
        }
    }
}