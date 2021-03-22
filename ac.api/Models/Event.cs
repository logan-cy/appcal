using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ac.api.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public Company Company { get; set; }
        public Client Client { get; set; }
        public Product Product { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool AllDay { get; set; }
    }
}