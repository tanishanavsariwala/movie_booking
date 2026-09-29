using Microsoft.Ajax.Utilities;
using movie_booking.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Mvc;
using static System.Net.Mime.MediaTypeNames;
using System.Web.UI.WebControls;

namespace movie_booking.Controllers
{
    public class BookingController : Controller
    {
        public List<SelectListItem> Bind_Movie(int Cat_ID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
            List<SelectListItem> list = new List<SelectListItem>();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("Bind_Movie", connection);
            cmd.Parameters.AddWithValue("@Cat_ID", Cat_ID);
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new SelectListItem { Value = reader["Movie_ID"].ToString(), Text = reader["Movie_name"].ToString() + "\t\t|\t\tPrice : " + reader["rate"].ToString() });
            }
            ViewBag.MovieList = list;
            return list;
        }
        public int Calculate_Price(int Movie_ID, int no_tickets)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("Get_Rate", connection);
            cmd.Parameters.AddWithValue("@Movie_id", Movie_ID);
            cmd.CommandType = CommandType.StoredProcedure;
            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            int price = 0;
            if (reader.Read())
            {
                price = Convert.ToInt32(reader["Rate"]);

            }
            int total_price = price * no_tickets;
            ViewBag.TotalPrice = total_price;
            return total_price;
        }
        public int Get_User_id()
        {
            int id = 0;
            if (Session["Email"] != null)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["dbconnection"].ToString();
                SqlConnection connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("Get_User", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                cmd.Parameters.AddWithValue("@Email_id", Session["Email"]);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = Convert.ToInt32(reader["User_id"]);
                }

            }
            return id;
        }
        // GET: Booking
        public ActionResult Index()
        {
            return View();
        }

        // GET: Booking/Details/5
        public ActionResult Details()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("Get_Booking", connection);

            List<booking> bookList = new List<booking>();

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@User_id", Get_User_id());

            connection.Open();

            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                booking book = new booking();

                book.booking_id = Convert.ToInt32(reader["booking_id"]);
                book.Cat_Type = reader["Cat_Type"].ToString();
                book.Movie_name = reader["Movie_name"].ToString();
                book.no_tickets = Convert.ToInt32(reader["No_Tickets"]);
                book.amount = Convert.ToInt32(reader["amount"]);

                bookList.Add(book);
            }

            reader.Close();
            connection.Close();

            return View(bookList);
        }


        // GET: Booking/Create

        public ActionResult Create(int? Cat_ID, int? Movie_ID, int? no_tickets)
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["movie"].ConnectionString;

            List<SelectListItem> list = new List<SelectListItem>();

            try
            {
              
                using (SqlConnection connection =
                       new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd =
                           new SqlCommand("Bind_Category", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        connection.Open();

                        using (SqlDataReader reader =
                               cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new SelectListItem
                                {
                                    Value = reader["Cat_id"].ToString(),
                                    Text = reader["Cat_Type"].ToString()
                                });
                            }
                        }
                    }
                }

                ViewBag.CategoryList = list;
                ViewBag.MovieList = new List<SelectListItem>();

                if (Cat_ID.HasValue)
                {
                    ViewBag.MovieList =
                        Bind_Movie(Cat_ID.Value);
                }
                booking book = new booking();

                if (Cat_ID.HasValue)
                {
                    book.Cat_ID = Cat_ID.Value;
                }

                if (Movie_ID.HasValue)
                {
                    book.Movie_ID = Movie_ID.Value;
                }

                if (no_tickets.HasValue)
                {
                    book.no_tickets = no_tickets.Value;
                }


              
                if (Movie_ID.HasValue && no_tickets.HasValue)
                {
                    book.amount =
                        Calculate_Price(
                            Movie_ID.Value,
                            no_tickets.Value
                        );

                    ViewBag.TotalPrice = book.amount;
                }
                else
                {
                    ViewBag.TotalPrice = 0;
                }

                return View(book);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new booking());
            }
        }


        // POST: Booking/Create
        [HttpPost]
        public ActionResult Create(booking book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int userId = Get_User_id();

                    if (userId == 0)
                    {
                        ViewBag.Error = "User ID not found. Please login again.";
                        return View(book);
                    }

                    // Calculate amount again
                    book.amount = Calculate_Price(
                        book.Movie_ID,
                        book.no_tickets
                    );

                    string connectionString =
                        ConfigurationManager.ConnectionStrings["movie"].ConnectionString;

                    using (SqlConnection connection =
                           new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd =
                               new SqlCommand("Add_Booking", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@User_ID", userId);
                            cmd.Parameters.AddWithValue("@Cat_ID", book.Cat_ID);
                            cmd.Parameters.AddWithValue("@Movie_ID", book.Movie_ID);
                            cmd.Parameters.AddWithValue("@no_tickets", book.no_tickets);
                            cmd.Parameters.AddWithValue("@amount", book.amount);

                            connection.Open();

                            cmd.ExecuteNonQuery();
                        }
                    }

                    ViewBag.Message = "Booking Inserted Successfully";

                    return View(book);
                }

                ViewBag.Error = "Model validation failed.";

                return View(book);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View(book);
            }
        }


        // GET: Booking/Edit/5
        public ActionResult Edit(int id, int? Cat_ID, int? Movie_ID, int? no_tickets)
        {
            booking book = new booking();
            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("Get_Booking_ById", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Booking_id", id);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                book.booking_id = Convert.ToInt32(reader["Booking_id"]);
                book.User_ID = Convert.ToInt32(reader["User_id"]);
                book.Cat_ID = Convert.ToInt32(reader["Cat_id"]);
                book.Movie_ID = Convert.ToInt32(reader["Movie_id"]);
                book.no_tickets = Convert.ToInt32(reader["No_of_Tickets"]);
                book.amount = Convert.ToInt32(reader["Amount"]);
            }

            reader.Close();
            connection.Close();

            List<SelectListItem> list = new List<SelectListItem>();

            connection = new SqlConnection(connectionString);
            cmd = new SqlCommand("Bind_Category", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            connection.Open();
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["Cat_id"].ToString(),
                    Text = reader["Cat_Type"].ToString()
                });
            }

            reader.Close();
            connection.Close();

            ViewBag.CategoryList = list;

            if (Cat_ID != null)
            {
                book.Cat_ID = Cat_ID.Value;
            }

            if (Movie_ID != null)
            {
                book.Movie_ID = Movie_ID.Value;
            }

            if (no_tickets != null)
            {
                book.no_tickets = no_tickets.Value;
            }

            ViewBag.MovieList = new List<SelectListItem>();

            if (book.Cat_ID != 0)
            {
                ViewBag.MovieList = Bind_Movie(book.Cat_ID);
            }

            if (Movie_ID != null && no_tickets != null)
            {
                book.amount = Calculate_Price(Movie_ID.Value, no_tickets.Value);
                ViewBag.TotalPrice = book.amount;
            }
            else
            {
                ViewBag.TotalPrice = book.amount;
            }

            return View(book);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        public ActionResult Edit(booking book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
                    SqlConnection connection = new SqlConnection(connectionString);
                    SqlCommand cmd = new SqlCommand("Update_Booking", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    cmd.Parameters.AddWithValue("@Booking_id", book.booking_id);
                    cmd.Parameters.AddWithValue("@User_id", Get_User_id());
                    cmd.Parameters.AddWithValue("@Cat_id", book.Cat_ID);
                    cmd.Parameters.AddWithValue("@Movie_id", book.Movie_ID);
                    cmd.Parameters.AddWithValue("@no_of_Tickets", book.no_tickets);
                    cmd.Parameters.AddWithValue("@amount", book.amount);
                    int i = cmd.ExecuteNonQuery();
                    connection.Close();
                    if (i > 0)
                    {
                        ViewBag.Message = "Booking Update Successfully";
                        return View(book);
                    }
                }

                return View(book);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex + " Updation Failed";
                return View(book);
            }
        }

        // GET: Booking/Delete/5
        public ActionResult Delete(int id)
        {
            booking book = new booking();
            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand("Get_Booking_ById", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Booking_id", id);

            connection.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                book.booking_id = Convert.ToInt32(reader["Booking_id"]);
                book.User_ID = Convert.ToInt32(reader["User_id"]);
                book.Cat_Type = (reader["Cat_type"]).ToString();
                book.Movie_name = (reader["Movie_name"]).ToString();
                book.no_tickets = Convert.ToInt32(reader["No_of_Tickets"]);
                book.amount = Convert.ToInt32(reader["Amount"]);
            }

            reader.Close();
            connection.Close();

            return View(book);
        }

        // POST: Booking/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, booking book)
        {
            try
            {
                // TODO: Add delete logic here
                string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
                SqlConnection connection = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand("Delete_Booking", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                connection.Open();
                cmd.Parameters.AddWithValue("@Booking_id", id);
                int i = cmd.ExecuteNonQuery();
                connection.Close();
                if (i > 0)
                {
                    ViewBag.Message = "Delete Succssfully";
                }
                return View(book);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex + " Delete Failed";
                return View(book);
            }
        }
    }
}















//public ActionResult Create(int? Cat_ID, int? Movie_ID, int? no_tickets)
//{
//    string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();
//    List<SelectListItem> list = new List<SelectListItem>();
//    SqlConnection connection = new SqlConnection(connectionString);
//    SqlCommand cmd = new SqlCommand("Bind_Category", connection);
//    cmd.CommandType = CommandType.StoredProcedure;
//    connection.Open();
//    SqlDataReader reader = cmd.ExecuteReader();
//    while (reader.Read())
//    {
//        list.Add(new SelectListItem { Value = reader["Cat_id"].ToString(), Text = reader["Cat_Type"].ToString() });
//    }
//    ViewBag.CategoryList = list;
//    ViewBag.MovieList = new List<SelectListItem>();
//    // Bind Movie according to selected Category
//    if (Cat_ID != null)
//    {
//        ViewBag.MovieList = Bind_Movie(Cat_ID.Value);
//    }
//    else
//    {
//        ViewBag.MovieList = new List<SelectListItem>();
//    }

//    booking book = new booking();

//    if (Cat_ID != null)
//    {
//        book.Cat_ID = Cat_ID.Value;
//    }

//    if (Movie_ID != null)
//    {
//        book.Movie_ID = Movie_ID.Value;
//    }

//    if (no_tickets != null)
//    {
//        book.no_tickets = no_tickets.Value;
//    }

//    if (Movie_ID != null && no_tickets != null)
//    {
//        book.amount = Calculate_Price(Movie_ID.Value, no_tickets.Value);
//        ViewBag.TotalPrice = book.amount;
//    }
//    return View(book);
//}





































