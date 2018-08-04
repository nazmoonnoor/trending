using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DSETrending.Controllers
{
    public class DefaultController : Controller
    {
        public ActionResult Index(string page, string spiked)
        {
            bool sp = !string.IsNullOrEmpty(spiked) ? bool.Parse(spiked) : false;
            var lastDayTrades = AppManager.GetLastDayTrades("DayValue", sp);
            
            int size = 20;
            int p = string.IsNullOrEmpty(page) ? 0 : int.Parse(page) - 1;
            int skip = p * size;
            ViewData["PageHeader"] = string.Format(@"{0} - {1} out of {2}", skip+1, skip+size, lastDayTrades.Count());

            return View(lastDayTrades.Skip(skip).Take(size));
        }
    }
}