using System;

namespace ac.api.Models
{
    public class Event
    {
        public int Id { get; set; }
        public Company Company { get; set; }
        public Client Client { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}