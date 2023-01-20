using System.Collections.Generic;

namespace Tasks.Models.ViewModels
{

    public class TaskEditViewModel
    {

        public List<Tasks.Models.Task> TaskEdit { get; set; }

        public Tasks.Models.Task task { get; set; }

        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateOnly? Date { get; set; }

        public int UserId { get; set; }

    }
}