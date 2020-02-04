using System.Collections.Generic;

namespace TODODATABASE.Models
{
    public class User
    {
        public int UserId {get; set;}
        public string Nama {get; set;}
        public string Password {get; set;}
        public ICollection<ToDo> Todo { get; set; }
    }
}