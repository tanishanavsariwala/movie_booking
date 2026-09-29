using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace movie_booking.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Login/Create
        [HttpPost]        

        public ActionResult Create(Models.Login log)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["movie"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd =new SqlCommand("Login_user", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(
                            "@Email_ID",
                            SqlDbType.VarChar, 100
                        ).Value = log.Email_ID;

                        cmd.Parameters.Add(
                            "@User_password",
                            SqlDbType.VarChar, 100
                        ).Value = log.User_password;

                        connection.Open();

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            Session["email"] = log.Email_ID;

                            reader.Close();

                            return RedirectToAction( "Create", "Movie");
                        }
                        else
                        {
                            reader.Close();
                            ViewBag.Error = "Email or Password Invalid.";
                            return View(log);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error =  "Error: " + ex.Message;
                return View(log);
            }
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();

            return RedirectToAction("Create", "Login");
        }
    }
}
