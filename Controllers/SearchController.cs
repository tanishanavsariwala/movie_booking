using movie_booking.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace movie_booking.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }


        // GET: Search/Details/5
        public ActionResult Details(int? cat_id)
        {
            List<Movie> movlist = new List<Movie>();

            string connectionString = ConfigurationManager.ConnectionStrings["movie"].ToString();

            List<SelectListItem> list = new List<SelectListItem>();

            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            // Bind Category
            SqlCommand cmd1 = new SqlCommand("Add_Movie_Category", connection);
            cmd1.CommandType = System.Data.CommandType.StoredProcedure;

            SqlDataReader reader = cmd1.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new SelectListItem
                {
                    Value = reader["Cat_ID"].ToString(),
                    Text = reader["Cat_Type"].ToString()
                });
            }

            reader.Close();
            connection.Close();

            ViewBag.CategoryList = list;


            // Bind Movie
            if (cat_id != null)
            {
                connection.Open();

                SqlCommand cmd = new SqlCommand("Add_Movie", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Cat_id", cat_id);

                SqlDataReader reader1 = cmd.ExecuteReader();

                while (reader1.Read())
                {
                    Movie mov = new Movie();

                    mov.Movie_ID = Convert.ToInt32(reader1["Movie_id"]);
                    mov.Movie_name = reader1["Movie_name"].ToString();
                    mov.Cat_ID = Convert.ToInt32(reader1["Cat_id"]);
                    mov.Release_Date = Convert.ToDateTime(reader1["Release_Date"]);
                    mov.rate = Convert.ToInt32(reader1["Rate"]);

                    movlist.Add(mov);
                }
                ViewBag.SelectedCatId = cat_id;
                reader1.Close();
                connection.Close();
            }

            return View(movlist);
        }

        // GET: Search/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Search/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Search/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Search/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Search/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Search/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}