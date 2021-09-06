using Microsoft.AspNetCore.Authorization;
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

        public static Dictionary<string, List<BookdSlot>> bookedSlots = new Dictionary<string, List<BookdSlot>>();

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
        [HttpPost("getCredentials")]
        public ActionResult GetCredentials([FromBody] LoginModel loginModel)
        {
            if(loginModel.Email == "Moh" && loginModel.Password == "123")
            {
                var tokenAsBytes = Encoding.UTF8.GetBytes("Moh:123");
                return Ok(Convert.ToBase64String(tokenAsBytes));
            }

            return Unauthorized();
        }

        [HttpGet("getBookedSlots/{date}")]
        public ActionResult GetBookedSlots(string date)
        {
            return Ok(new { bookedSlots });
        }

        [HttpGet("getAvailableSlots/{date}")]
        public ActionResult GetAvailableSlots(string date)
        {
            List<object> slots = new List<object>()
            {
                "07:00 - 08:00",
                "08:00 - 09:00",
                "09:00 - 10:00",
                "10:00 - 11:00",
                "11:00 - 12:00",
                "12:00 - 13:00",
                "13:00 - 14:00",
                "14:00 - 15:00",
                "15:00 - 16:00",
                "16:00 - 17:00",
                "17:00 - 18:00",
                "18:00 - 19:00",
            };

            List<BookdSlot> bookedSlotsForThisDate;
            bool isDateExists = bookedSlots.TryGetValue(date, out bookedSlotsForThisDate);

            if (isDateExists)
            {
                for(int i = 0; i < bookedSlotsForThisDate.Count; i++)
                {
                    int indexOfBookedSlot = slots.FindIndex(slot => (string) slot == bookedSlotsForThisDate[i].At);
                    if(indexOfBookedSlot != -1)
                    {
                        slots.RemoveAt(indexOfBookedSlot);
                    }
                }
            }

            return Ok(new { slots, bookedSlots });
        }

        [HttpPost("book")]
        public ActionResult Book([FromBody] BookInformation bookInfo)
        {
            List<BookdSlot> bookedSlotsForThisDate;
            bool isDateExists = bookedSlots.TryGetValue(bookInfo.Date, out bookedSlotsForThisDate);

            if (isDateExists)
            {
                BookdSlot valueAtThisSlot = bookedSlotsForThisDate.Find(book => book.At == bookInfo.Slot );

                if(valueAtThisSlot == null)
                    bookedSlots[bookInfo.Date].Add(new BookdSlot() { At = bookInfo.Slot, UserName = HttpContext.User.Identity.Name });
                else
                    return Ok(new { isBooked = false });
            }
            else
            {
                bookedSlots.Add(bookInfo.Date, new List<BookdSlot>() { new BookdSlot() { At = bookInfo.Slot, UserName = HttpContext.User.Identity.Name } });
            }

            return Ok(new { isBooked = true, bookedAt = bookInfo.Slot });
        }
    }
}
