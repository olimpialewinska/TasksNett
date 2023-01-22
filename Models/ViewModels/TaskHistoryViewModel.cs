using System.Collections.Generic;

namespace Tasks.Models.ViewModels{

    public class TaskHistoryViewModel{

        public List<Tasks.Models.Task> TaskHistoryList {get;set;}

        public Tasks.Models.Task task {get;set;}

        public List<Tasks.Models.Task> TaskHistoryListFriends {get;set;}
    }
}