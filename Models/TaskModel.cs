namespace Tasks.Models;

public class Task
{
    public int Id {get;set;}

    public string UserId {get;set;}

    public string Title {get;set; }

    public string Description {get;set;}

    public DateOnly? Date {get;set;}

    public string Status {get;set;}

    public string Exeuser {get;set;}

    public string Requser {get;set;}

    
}

