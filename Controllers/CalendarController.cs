using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PlanificatorSali.Data;
using PlanificatorSali.Models;
using PlanificatorSali.Models.DataAccess;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using PlanificatorSali.Models.Configuration;
using MailKit.Search;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlanificatorSali.Controllers
{
    // [Route("calendar")]
    public class CalendarController : Controller
    {
        private IConfiguration Configuration;
        private DataAccess _DA { get; set; }
        private readonly PlanificatorSaliContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(IConfiguration _configuration, PlanificatorSaliContext context, UserManager<ApplicationUser> userManager)
        {

            Configuration = _configuration;
            _DA = new DataAccess(this.Configuration.GetConnectionString("PlanificatorSaliContext"));
            _context = context;
            _userManager = userManager;

        }

        [Authorize]
        [Route("calendar")]
        public IActionResult Calendar()
        {
            List<Models.Sala> roomlist = new List<Models.Sala>();
            roomlist = (from sala in _context.Sala select sala).ToList();
            roomlist.Insert(0, new Models.Sala { salaID = 0, Denumire = "Selectează sala" });
            ViewBag.listofitems = roomlist;

            List<Models.ApplicationUser> userlist = new List<Models.ApplicationUser>();
            userlist = (from users in _context.ApplicationUser select users).ToList();
            userlist.Insert(0, new Models.ApplicationUser { Id = "0", Email = "Selectează participanții" });
            ViewBag.listofusers = userlist;
            return View();
        }
        

        [HttpGet]
        public IActionResult GetCalendarEvents(string start, string end)
        {
            List<Event> events = _DA.GetCalendarEvents(start, end);
            return Json(events);
        }

        [HttpPost]
        public IActionResult UpdateEvent([FromBody] Event evt)
        {
            string message = String.Empty;
            var userIdlogged = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string UserId = _context.Events.Where(u => u.EventId == evt.EventId).Select(u => u.ApplicationUserId).SingleOrDefault();
            if (userIdlogged != UserId)
            {
                if (User.IsInRole("Administrator"))
                {
                    message = _DA.UpdateEvent(evt);
                }
                else
                {
                    message = "Evenimentul nu va apartine nu îl puteți actualiza";
                    return Json(new { message });
                }
            }
            message = _DA.UpdateEvent(evt);

            return Json(new { message });
        }

        private SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(this.Configuration.GetConnectionString("PlanificatorSaliContext"));
            conn.Open();
            return conn;
        }
        private void CloseConnection(SqlConnection conn)
        {
            conn.Close();
        }

        [HttpPost]
        public IActionResult AddEvent([FromBody]Event evt, [FromBody]  Participants part)
        {
            string message = String.Empty;
            int eventId = 0;
            message = AddEvent(evt,part, out eventId);
            string AddEvent(Event evt,Participants part, out int eventId)
            {
                SqlConnection conn = GetConnection();
                SqlTransaction trans = conn.BeginTransaction();
                eventId = 0;
                var userIdlogged = User.FindFirstValue(ClaimTypes.NameIdentifier);
                try
                {
                    SqlCommand cmd = new SqlCommand(@"
                                                 insert into [Events]
                                                (
	                                                title
	                                                ,[description]
	                                                ,event_start
	                                                ,event_end
	                                                ,all_day
                                                    ,color
                                                    ,salaID
                                                    ,ApplicationUserId
                                                )
                                                values
                                                (
	                                                @title
	                                                ,@description
	                                                ,@start
	                                                ,@end
	                                                ,@allDay
                                                    ,@color
                                                    ,@salaID
                                                    ,@ApplicationUserID
                                                );
                                                select scope_identity()", conn, trans)
                    {
                        CommandType = CommandType.Text
                    };
                    cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = evt.Title;
                    cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = evt.Description;
                    cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = evt.Start;
                    cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = Helpers.ToDBNullOrDefault(evt.End);
                    cmd.Parameters.Add("@allDay", SqlDbType.Bit).Value = evt.AllDay;
                    cmd.Parameters.Add("@color", SqlDbType.VarChar).Value = evt.color;
                    cmd.Parameters.Add("@salaID", SqlDbType.Int).Value = evt.salaID;
                    cmd.Parameters.Add("@ApplicationUserId", SqlDbType.VarChar).Value = userIdlogged;
                    //cmd.Parameters.Add(new SqlParameter("@UserId", Participants.UserId));
                    //cmd.Parameters.Add("@eventid", SqlDbType.Int).Value =eventId;
                    eventId = Convert.ToInt32(cmd.ExecuteScalar());
                    trans.Commit();
                }
                catch (Exception exp)
                {
                    trans.Rollback();
                    message = exp.Message;
                }
                finally
                {
                    CloseConnection(conn);
                }
                return message;
            }
            return Json(new { message, eventId });
        }
       /* [HttpPost]
        public IActionResult Create(string UserId,int eventId)
        {
            var participants = new Participants
            {
                UserId = UserId,
                EventId = eventId
            };
            _context.Participants.Add(participants);
            _context.SaveChanges();
            return RedirectToAction("AddEvent");
        }*/

        [HttpPost]
        public IActionResult DeleteEvent([FromBody] Event evt)
        {
            var userIdlogged = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string message = String.Empty;
           
            string UserId = _context.Events.Where(u => u.EventId == evt.EventId).Select(u => u.ApplicationUserId).SingleOrDefault();
            if (userIdlogged != UserId)
            {if(User.IsInRole("Administrator"))
                {
                    message = _DA.DeleteEvent(evt.EventId);
                }
                else
                {
                    message = "Evenimentul nu va apartine nu îl puteți șterge";
                    return Json(new { message });
                }
               
            }
            return Json(new { message });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }


        public IActionResult GetUserEvents()
        {
             var userIdlogged = User.FindFirstValue(ClaimTypes.NameIdentifier);
             //var users = _context.Events.Where(x => x.ApplicationUserId == userIdlogged);
            List<Event> events = new List<Event>();
            List<Sala> sala = new List<Sala>();

             using (SqlConnection conn = GetConnection())
             {
                 using (SqlCommand cmd = new SqlCommand(@"select
                                                             event_start
                                                             ,event_end
                                                             ,Title
                                                             ,[Description]
                                                             ,salaID
                                                         from
                                                             [Events]
                                                        
                                                         where
                                                             ApplicationUserId=@userIdlogged", conn)
                 {
                     CommandType = CommandType.Text
                 })
                 {
                     cmd.Parameters.Add("@userIdlogged", SqlDbType.VarChar).Value = userIdlogged;

                     using (SqlDataReader dr = cmd.ExecuteReader())
                     {
                         while (dr.Read())
                         {
                             events.Add(new Event()
                             {

                                 Title = Convert.ToString(dr["Title"]), //title
                                 Description = Convert.ToString(dr["Description"]),//description
                                 Start = Convert.ToString(dr["event_start"]),
                                 End = Convert.ToString(dr["event_end"]),
                                 salaID=Convert.ToInt32(dr["salaId"])
                             });
                            
                         }
                     }

                 }
             }

            List<Sala> roomlist = _context.Sala.ToList();
            List<Event> eventsala = events.ToList();
            
            ViewData["EventSala"] = from e in eventsala
                                    join s in roomlist on e.salaID equals s.salaID into table1 
                                    from s in table1.DefaultIfEmpty() 
                                    select new EventSala { eventsala = e, roomlist = s  }  ;
            return View(ViewData["EventSala"]);
        }

    }
}