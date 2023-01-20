using System.Collections.Generic;

namespace Tasks.Models.ViewModels{

    public class TaskViewModel{

        public List<Tasks.Models.Task> TaskList{get;set;}

        public Tasks.Models.Task task {get;set;}

        public string Title{get;set;}

    }
}