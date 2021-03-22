using System;

namespace ac.api.Viewmodels
{
    public class EventViewmodel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int ClientId { get; set; }
        public int ProductId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public bool AllDay { get; set; }
    }
}