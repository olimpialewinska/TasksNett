using Tasks.Models;

namespace Tasks.Models.ViewModels
{

    public class FriendListViewModel
    {
        public List<Tasks.Models.Friend>? FriendList { get; set; }
        
        public Tasks.Models.Friend friend {get;set;}
    }

}