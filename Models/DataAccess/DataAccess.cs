using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using PlanificatorSali.Models.Configuration;

namespace PlanificatorSali.Models.DataAccess
{
    public class DataAccess
    {
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

        public List<Event> GetCalendarEvents(string start, string end)  //Events=events
        {
            List<Event> events = new List<Event>();

            using (SqlConnection conn = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(@"select
                                                             EventId
                                                            ,event_start
                                                            ,event_end
                                                            ,Title
                                                            ,[Description]
                                                            ,all_day
                                                            ,color 
                                                            ,salaID
                                                            ,ApplicationUserId
                                                           

                                                        from
                                                            [Events]
                                                        where
                                                            event_start between @start and @end", conn)
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
                            events.Add(new Event()
                            {
                                EventId = Convert.ToInt32(dr["EventId"]),  
                                Title = Convert.ToString(dr["Title"]), //title
                                Description = Convert.ToString(dr["Description"]),//description
                                Start = Convert.ToString(dr["event_start"]),
                                End = Convert.ToString(dr["event_end"]),
                                AllDay = Convert.ToBoolean(dr["all_day"]),
                                color = Convert.ToString(dr["color"]),//color
                                //participanti = Convert.ToString(dr["participanti"]),
                                salaID = Convert.ToInt32(dr["salaID"]),//roomId
                              
                               ApplicationUserId = Convert.ToString(dr["ApplicationUserId"])
                            });
                        }
                    }
                }
            }

            return events;
        }

        public string UpdateEvent(Event evt)
        {
            string message = "";
            SqlConnection conn = GetConnection();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand(@"update
	                                                [Events]
                                                set
	                                                [Description]=@description
                                                    ,Title=@title
	                                                ,event_start=@event_start
	                                                ,event_end=@end 
	                                                ,all_day=@allDay
                                                    ,color=@color
                                                    ,salaID=@salaID
                                                        
                                                where
	                                                EventId=@eventId", conn, trans)
                {
                    CommandType = CommandType.Text
                };
                cmd.Parameters.Add("@eventId", SqlDbType.Int).Value = evt.EventId;
                cmd.Parameters.Add("@title", SqlDbType.VarChar).Value = evt.Title;
                cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = evt.Description;
                cmd.Parameters.Add("@event_start", SqlDbType.DateTime).Value = evt.Start;
                cmd.Parameters.Add("@end", SqlDbType.DateTime).Value = Helpers.ToDBNullOrDefault(evt.End);
                cmd.Parameters.Add("@allDay", SqlDbType.Bit).Value = evt.AllDay;
                cmd.Parameters.Add("@color", SqlDbType.VarChar).Value = evt.color;
               // cmd.Parameters.Add("@color", SqlDbType.VarChar).Value = evt.participanti;
                cmd.Parameters.Add("@salaID", SqlDbType.Int).Value = evt.salaID;
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
       

        public string DeleteEvent(int eventId)
        {
            string message = "";
            SqlConnection conn = GetConnection();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand(@"delete from 
	                                                [Events]
                                                where
	                                                EventId=@eventId", conn, trans)
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
