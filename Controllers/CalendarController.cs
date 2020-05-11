using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PlanificatorSali.Data;
using System.Web;
using PlanificatorSali.Models;
using PlanificatorSali.Models.DataAccess;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace PlanificatorSali.Controllers
{
   // [Route("calendar")]
    public class CalendarController : Controller
    {
       
        
       private IConfiguration Configuration;
       private DataAccess db { get; set; }
        private readonly PlanificatorSaliContext _context;
       
       public CalendarController(IConfiguration _configuration, PlanificatorSaliContext context)
       {
            Configuration = _configuration;
            db = new DataAccess(this.Configuration.GetConnectionString("PlanificatorSaliContext"));
            _context = context;
       }
        /*public CalendarController(PlanificatorSaliContext context)
        {
           
            _context = context;
        }*/


        //[Route("")]
        // [Route("~/")]
        [Route("calendar")]
        public IActionResult Calendar()
        {
            List<Sala> roomlist = new List<Sala>();
            roomlist = (from sala in _context.Sala select sala).ToList();
            roomlist.Insert(0, new Sala { salaID = 0, Denumire = "Select" });
            ViewBag.listofitems = roomlist;

            return View();
        }
        public IActionResult Index()
        {
           

            return View();
        }

        [HttpGet]
        public IActionResult GetCalendarEvents(string start, string end)
        {
            List<Evenimente> events = db.GetCalendarEvents(start, end);

            return Json(events);
        }
      
        [HttpPost]
        public IActionResult UpdateEvent([FromBody] Evenimente evt)
        {
            string message = String.Empty;

            message = db.UpdateEvent(evt);

            return Json(new { message });
        }
        [HttpPost]
        public IActionResult AddEvent([FromBody] Evenimente evt)
        {
            string message = String.Empty;
            int eventId = 0;

            message = db.AddEvent(evt, out eventId);

            return Json(new { message, eventId });
        }

        [HttpPost]
        public IActionResult DeleteEvent([FromBody] Evenimente evt)
        {
            string message = String.Empty;

            message = db.DeleteEvent(evt.ID);

            return Json(new { message });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}