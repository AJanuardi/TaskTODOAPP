using System.ComponentModel.DataAnnotations.Schema;

namespace TODODATABASE.Models
{
    public class ToDo
    {
        public int id {get;set;}
        public string activity{get;set;}
        public string status {get;set;}
        
        [ForeignKey("User")]
        public int UserId {get; set;}
        public User User {get; set;}
    }
}