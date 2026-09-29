using System;

namespace movie_booking.Models
{
    public class Movie
    {
        public string Movie_name { get; set; }
        public DateTime Release_Date { get; set; }
        public int Cat_ID { get; set; }
        public int rate { get; set; }
        public int Movie_ID { get; internal set; }
    //    public IEnumerable&lt;SelectListItem&gt; StateList { get; set; }
    //public IEnumerable&lt;SelectListItem&gt; CityList { get; set; }
    }
}
