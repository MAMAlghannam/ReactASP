using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReactASP.Model;
using ReactASP.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ReactASP.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        public static Dictionary<string, List<BookdTime>> bookedTimes = new Dictionary<string, List<BookdTime>>();

        public static List<User> Users = new List<User>()
            {
                new User() { Email = "Moh", Password = "123" },
                new User() { Email = "Ahmed", Password = "123" },
                new User() { Email = "Ali", Password = "123" }
            };

        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return HttpContext.User.Identity.Name;
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [AllowAnonymous]
        [HttpPost("getCookies")]
        public ActionResult GetCookies()
        {
            return Ok(new { Cookies = Request.Cookies, HttpContext.User.Identities });
        }


        [AllowAnonymous]
        [HttpPost("getCredentials")]
        public ActionResult GetCredentials([FromBody] LoginModel loginModel)
        {
            User userInfo = Users.Find(user => loginModel.Email == user.Email);

            if (userInfo != null && userInfo.Password == loginModel.Password)
            {
                var tokenAsBytes = Encoding.UTF8.GetBytes(userInfo.Email+":"+userInfo.Password);
                string token = Convert.ToBase64String(tokenAsBytes);
                Response.Cookies.Append("XMyCustomToken", token, new CookieOptions() { Expires = DateTime.Now.AddMinutes(15), HttpOnly = true, SameSite = SameSiteMode.Strict });
                return Ok(new { email = userInfo.Email, token });
            }

            return Unauthorized();
        }

        [HttpGet("getBookedTimes/{date}")]
        public ActionResult GetBookedTimes(string date)
        {
            return Ok(new { bookedTimes });
        }

        [HttpGet("getTimes/{date}")]
        public ActionResult GetTimes(string date)
        {
            string[] yyyyMmDd = date.Split("-");
            DateTime dateQueried = new DateTime(int.Parse(yyyyMmDd[0]), int.Parse(yyyyMmDd[1]), int.Parse(yyyyMmDd[2]));

            // if the dateQueried is passed return an empty array
            if (dateQueried.CompareTo(DateTime.Today) < 0)
                return Ok(new { times = Array.Empty<Time>() });

            List<Time> times = new List<Time>()
            {
                new Time() { At = "07:00 - 08:00" },
                new Time() { At = "08:00 - 09:00" },
                new Time() { At = "09:00 - 10:00" },
                new Time() { At = "10:00 - 11:00" },
                new Time() { At = "11:00 - 12:00" },
                new Time() { At = "12:00 - 13:00" },
                new Time() { At = "13:00 - 14:00" },
                new Time() { At = "14:00 - 15:00" },
                new Time() { At = "15:00 - 16:00" },
                new Time() { At = "16:00 - 17:00" },
                new Time() { At = "17:00 - 18:00" },
                new Time() { At = "18:00 - 19:00" },
            };

            // if the dateQueried is same as this date filter times
            if (dateQueried.Date.CompareTo(DateTime.Now.Date) == 0)
                times = times.FindAll(item => DateTime.Parse(item.At.Split(" - ")[0]).Hour > DateTime.Now.Hour );

            List<BookdTime> bookedTimesForThisDate;
            bool isDateExists = bookedTimes.TryGetValue(date, out bookedTimesForThisDate);

            if (isDateExists)
            {
                for (int i = 0; i < bookedTimesForThisDate.Count; i++)
                {
                    int indexOfBookedTime = times.FindIndex(time => time.At == bookedTimesForThisDate[i].At);
                    if (indexOfBookedTime != -1)
                    {
                        times[indexOfBookedTime].IsBooked = true;
                        times[indexOfBookedTime].UserName = bookedTimesForThisDate[i].UserName;
                    }
                }
            }

            return Ok(new { times });
        }

        [HttpPost("book")]
        public ActionResult Book([FromBody] BookInformation bookInfo)
        {
            string[] yyyyMmDd = bookInfo.Date.Split("-");
            DateTime dateQueried = new DateTime(int.Parse(yyyyMmDd[0]), int.Parse(yyyyMmDd[1]), int.Parse(yyyyMmDd[2]));

            // if the dateQueried is passed it's not allowed to book
            if (dateQueried.CompareTo(DateTime.Today) < 0)
                return Ok(new { isBooked = false, reason = "passed-day" });

            List<BookdTime> bookedTimesForThisDate;
            bool isDateExists = bookedTimes.TryGetValue(bookInfo.Date, out bookedTimesForThisDate);

            if (isDateExists)
            {
                BookdTime valueAtThisTime = bookedTimesForThisDate.Find(book => book.At == bookInfo.Time );

                if(FindListOfBookingsByDate(bookInfo.Date, HttpContext.User.Identity.Name).Count >= 2)
                {
                    return Ok(new { isBooked = false, reason = "exceeded-bookings" });
                }
                else if(valueAtThisTime != null)
                {
                    return Ok(new { isBooked = false, reason = "not-available" });
                }
                else
                {
                    bookedTimes[bookInfo.Date].Add(new BookdTime() { At = bookInfo.Time, UserName = HttpContext.User.Identity.Name });
                }
            }
            else
            {
                bookedTimes.Add(bookInfo.Date, new List<BookdTime>() { new BookdTime() { At = bookInfo.Time, UserName = HttpContext.User.Identity.Name } });
            }

            return Ok(new { isBooked = true, bookedAt = bookInfo.Time });
        }

        [HttpPost("cancelBook")]
        public ActionResult CancelBook([FromBody] BookInformation bookInfo)
        {
            List<BookdTime> bookedTimesForThisDate;
            bool isDateExists = bookedTimes.TryGetValue(bookInfo.Date, out bookedTimesForThisDate);

            if (isDateExists)
            {
                int indexAtThisTime = bookedTimesForThisDate.FindIndex(book => book.At == bookInfo.Time);

                if (indexAtThisTime != -1)
                {
                    bookedTimes[bookInfo.Date].RemoveAt(indexAtThisTime);
                    return Ok(new { isCanceled = true });
                }
                else
                    return BadRequest(new { isCanceled = false });
            }

            return BadRequest(new { isCanceled = false, bookedAt = bookInfo.Time });
        }

        [HttpGet("getMyListOfBookings")]
        public ActionResult GetMyListOfBookings()
        {
            string username = HttpContext.User.Identity.Name;
            List<object> listOfBookings = new List<object>();
            for (int i = 0; i < bookedTimes.Keys.Count; i++)
            {
                string currentDate = bookedTimes.Keys.ToList()[i];
                List<object> itemsFound = FindListOfBookingsByDate(currentDate, username);
                if (itemsFound.Count > 0)
                    itemsFound.ForEach(item => listOfBookings.Add(item));
            }

            return Ok(new { listOfBookings });

        }

        private List<object> FindListOfBookingsByDate(string date, string username)
        {
            List<object> listOfBookings = new List<object>();
            for (int j = 0; j < bookedTimes[date].Count; j++)
            {
                if (bookedTimes[date][j].UserName == username)
                {
                    listOfBookings.Add(new { date = date, time = bookedTimes[date][j].At });
                }
            }

            return listOfBookings;
        }

    }
}
