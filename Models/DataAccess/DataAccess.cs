using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using PlanificatorSali.Models.Configuration;
using PlanificatorSali.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;


namespace PlanificatorSali.Models.DataAccess
{
    public class DataAccess
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DataAccess(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public string _ConnectionStr { get; set; }
       
        

        public DataAccess(string ConnectionStr)
        {
            _ConnectionStr = ConnectionStr;
        }

        private SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(_ConnectionStr);
            conn.Open();
            return conn;
        }

        private void CloseConnection(SqlConnection conn)
        {
            conn.Close();
        }

        public List<Evenimente> GetCalendarEvents(string start, string end)
        {
            List<Evenimente> events = new List<Evenimente>();

            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(@"select
                                                             ID
                                                            ,start_data
                                                            ,sfarsit_data
                                                            ,titlu
                                                            ,[descriere]
                                                            ,AllDay
                                                            

                                                        from
                                                            [Evenimente]
                                                        where
                                                            start_data between @start and @end", conn)
                {
                    CommandType = CommandType.Text
                })
                {
                    cmd.Parameters.Add("@start", SqlDbType.VarChar).Value = start;
                    cmd.Parameters.Add("@end", SqlDbType.VarChar).Value = end;

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            events.Add(new Evenimente()
                            {
                                ID = Convert.ToInt32(dr["ID"]),
                                Titlu = Convert.ToString(dr["titlu"]),
                                Descriere = Convert.ToString(dr["descriere"]),
                                start_data = Convert.ToString(dr["start_data"]),
                                sfarsit_data = Convert.ToString(dr["sfarsit_data"]),
                                AllDay = Convert.ToBoolean(dr["AllDay"]),
                               // culoare = Convert.ToString(dr["culoare"]),
                               // salaID = Convert.ToInt32(dr["salaID"]),
                                //participanti = Convert.ToString("participanti"),
                               //ApplicationUserId = Convert.ToString(dr["ApplicationUserId"])


                                
                            });
                        }
                    }
                }
            }

            return events;
        }

        public string UpdateEvent(Evenimente evt)
        {
            string message = "";
            SqlConnection conn = GetConnection();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand(@"update
	                                                [Evenimente]
                                                set
	                                                [Descriere]=@descriere
                                                    ,Titlu=@title
	                                                ,start_data=@start
	                                                ,sfarsit_data=@end 
	                                                ,Allday=@allDay
                                                    ,culoare=@color
                                                    ,salaID=@roomID
                                                    ,participanti=@participants
                                                where
	                                                ID=@eventId", conn, trans)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = evt.ID;
                cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = evt.Titlu;
                cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = evt.Descriere;
                cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = evt.start_data;
                cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = Helpers.ToDBNullOrDefault(evt.sfarsit_data);
                cmd.Parameters.Add("@allDay", SqlDbType.Bit).Value = evt.AllDay;
                cmd.Parameters.Add("@color", SqlDbType.VarChar).Value = evt.culoare;
                cmd.Parameters.Add("@salaID", SqlDbType.Int).Value = evt.salaID;
                cmd.Parameters.Add("@participants", SqlDbType.VarChar).Value = evt.participanti;
                cmd.ExecuteNonQuery();

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
        public string AddEvent(Evenimente evt, out int eventId)
        {
            string message = "";
            SqlConnection conn = GetConnection();
            SqlTransaction trans = conn.BeginTransaction();
            eventId = 0;
            




            try
            {
                SqlCommand cmd = new SqlCommand(@"insert into [Evenimente]
                                                (
	                                                Titlu
	                                                ,[Descriere]
	                                                ,start_data
	                                                ,sfarsit_data
	                                                ,AllDay
                                                    ,salaID
                                                   
                                               
                                                  
                                                )
                                                values
                                                (
	                                                @titlu
	                                                ,@description
	                                                ,@start
	                                                ,@end
	                                                ,@allDay
                                                    ,@roomID
                                              
                                                  
                                                   

                                                );
                                                select scope_identity()", conn, trans)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.Add("@titlu", SqlDbType.NVarChar).Value = evt.Titlu;
                cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = evt.Descriere;
                cmd.Parameters.Add("@start", SqlDbType.DateTime).Value = evt.start_data;
                cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = Helpers.ToDBNullOrDefault(evt.sfarsit_data);
                cmd.Parameters.Add("@allDay", SqlDbType.Bit).Value = evt.AllDay;
               // cmd.Parameters.Add("@color", SqlDbType.NVarChar).Value = evt.culoare;
                cmd.Parameters.Add("@roomID", SqlDbType.Int).Value = 
                //cmd.Parameters.Add("@participanti", SqlDbType.NVarChar).Value = evt.participanti;
                //var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                //cmd.Parameters.Add("@ApplicationUserId", SqlDbType.Bit).Value = userId;

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
        public string DeleteEvent(int eventId)
        {
            string message = "";
            SqlConnection conn = GetConnection();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmd = new SqlCommand(@"delete from 
	                                                [Evenimente]
                                                where
	                                                event_id=@eventId", conn, trans)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = eventId;
                cmd.ExecuteNonQuery();

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
    }
}
