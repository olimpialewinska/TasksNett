using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tasks.Models;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Data.Sqlite;
using Tasks.Models.ViewModels;
using System.Globalization;
using System.Dynamic;

namespace Tasks.Controllers;

public class TaskController : Controller
{

    static private DateOnly? TestDate(DateOnly? date)
    {
        return date == DateOnly.MinValue ? null : date;

    }
    static private string GetConnectionString()
    {
        return "Data Source=app.db;";
    }
    private readonly ILogger<TaskController> _logger;

    public TaskController(ILogger<TaskController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var taskListViewModel = GetMyTasks();

        return View(taskListViewModel);
    }

    public IActionResult AddTask()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }
        var friendListViewModel = GetMyFriends();
        return View(friendListViewModel);
    }


    public IActionResult AddFriend()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }
        return View();
    }

    [HttpPost]
    public IActionResult AddFriendData(Friend friend)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }
        string user1 = User.Identity.Name;

        if (user1 == friend.User2)
        {
            TempData["Status"] = $"danger";
            TempData["Message"] = $"Nie mozna dodać siebie!";
            return RedirectToAction("AddFriend");
        }

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM AspNetUsers WHERE Email = '{friend.User2}'";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        string connectionString2 = GetConnectionString();
                        using (var connection2 = new SqliteConnection(connectionString2))
                        {
                            connection2.Open();
                            using (var command2 = connection2.CreateCommand())
                            {
                                connection2.Open();
                                command2.CommandText = $"INSERT INTO Friend (User1, User2) VALUES ('{user1}', '{friend.User2}')";

                                command2.ExecuteNonQuery();
                            }
                        }
                        TempData["Status"] = $"success";
                        TempData["Message"] = $"Osoba została dodana!";
                        return RedirectToAction("AddFriend");
                    }
                    else
                    {
                        TempData["Status"] = $"danger";
                        TempData["Message"] = $"Uzytkownik nie istnieje!";
                        return RedirectToAction("AddFriend");
                    }
                }
            }
        }
    }

    public IActionResult Friends()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }
        //wyciagnij wszystkich moich znajomych z tabeli i przekaz je do widoku?

        var friendListViewModel = GetMyFriends();
        return View(friendListViewModel);


    }

    internal FriendListViewModel GetMyFriends()
    {
        var friendListViewModel = new FriendListViewModel();
        friendListViewModel.FriendList = new List<Friend>();
        string user1 = User.Identity.Name;

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT * FROM Friend WHERE User1 = '{user1}'";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var friend = new Friend();
                        friend.Id = reader.GetInt32(0);
                        friend.User1 = reader.GetString(1);
                        friend.User2 = reader.GetString(2);

                        friendListViewModel.FriendList.Add(friend);
                    }
                }
            }
        }
        return friendListViewModel;
    }

    public IActionResult AssignedTask()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var taskListViewModel = GetAssignedTasks();

        return View(taskListViewModel);
    }

    internal AssignedList GetAssignedTasks()
    {
        List<Tasks.Models.Task> taskList = new();
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT Id, Title, Description, Date, requser, exeuser FROM Task WHERE (requser = '{User.Identity.Name}' AND exeuser <> '{User.Identity.Name}') AND Status = '' ORDER BY Date ASC; ";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (DateOnly.TryParse(reader.GetString(3), out DateOnly date))
                            {
                                taskList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = DateOnly.Parse(reader.GetString(3)),
                                        Requser = reader.GetString(4),
                                        Exeuser = reader.GetString(5)
                                    }
                                );
                            }
                            else
                            {
                                taskList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = null,
                                        Requser = reader.GetString(4),
                                        Exeuser = reader.GetString(5)
                                    }
                                );
                            }
                        }
                    }
                    else
                    {
                        return new AssignedList
                        {
                            TaskAssignedList = taskList
                        };
                    }
                };
            }

        }
        return new AssignedList
        {
            TaskAssignedList = taskList
        };
    }

    [HttpPost]
    public JsonResult DeleteTask(int id)
    {
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM Task WHERE Id = '{id}'";

                command.ExecuteNonQuery();
            }
        }
        return Json(new { });
    }

    [HttpPost]
    public JsonResult AddTaskData(string Title,
            string Description,
            string Date,
            string Status,
            string Exeuser,
            string Requser)
    {
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"INSERT INTO Task (Title, Description, Date, Status, UserId, requser, exeuser) VALUES ('{Title}', '{Description}', '{Date}', '{Status}', '{Requser}', '{Requser}', '{Exeuser}')";

                command.ExecuteNonQuery();
            }
        }
        return Json(new { });
    }

    [HttpPost]
    public JsonResult DeleteFriend(int id)
    {
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"DELETE FROM Friend WHERE Id = '{id}'";

                command.ExecuteNonQuery();
            }
        }
        return Json(new { });
    }

    [HttpPost]
    public IActionResult EditTaskUpdate(TaskEditViewModel task)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var date = TestDate(task.Date);

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE Task SET Title = '{task.Title}', Description = '{task.Description}', Date = '{date}' WHERE Id = '{task.Id}'";

                command.ExecuteNonQuery();

            }
        }
        TempData["Status"] = $"success";
        TempData["Message"] = $"Zadanie zostało zaaktualizowane!";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult EditTaskAssignedUpdate(TaskEditViewModel task)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var date = TestDate(task.Date);

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE Task SET Title = '{task.Title}', Description = '{task.Description}', Date = '{date}' WHERE Id = '{task.Id}'";

                command.ExecuteNonQuery();

            }
        }
        TempData["Status"] = $"success";
        TempData["Message"] = $"Zadanie zostało zaaktualizowane!";

        return RedirectToAction("AssignedTask");
    }

    [HttpPost]
    public IActionResult Edit(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var taskEditViewModel = GetMyTasksToEdit(id);

        return View(taskEditViewModel);

    }

    [HttpPost]
    public IActionResult EditAssigned(int id)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }

        var taskEditViewModel = GetMyTasksToEdit(id);

        return View(taskEditViewModel);

    }

    [HttpPost]
    public IActionResult Done(int id)
    {
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE Task SET Status = 'Wykonano' WHERE id = '{id}'";

                command.ExecuteNonQuery();
            }
        }
        TempData["Status"] = $"success";
        TempData["Message"] = $"Zadanie zostało wykonane!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Dismiss(int id)
    {

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE Task SET Status = 'Odrzucono' WHERE id = '{id}'";

                command.ExecuteNonQuery();
            }
        }
        TempData["Status"] = $"danger";
        TempData["Message"] = $"Odrzucono zadanie!";
        return RedirectToAction("Index");
    }
    public IActionResult DismissAssigned(int id)
    {

        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"UPDATE Task SET Status = 'Odrzucono przez nadawce' WHERE id = '{id}'";

                command.ExecuteNonQuery();
            }
        }
        TempData["Status"] = $"danger";
        TempData["Message"] = $"Zadanie zostało usunięte!";
        return RedirectToAction("AssignedTask");
    }

    public IActionResult History()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("~/Identity/Account/Login");
        }
        var taskHistoryListViewModel = GetMyHistory1();

        return View(taskHistoryListViewModel);
    }

    internal TaskEditViewModel GetMyTasksToEdit(int id)
    {

        List<Tasks.Models.Task> taskEdit = new();
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT Id, Title, Description, Date FROM Task WHERE UserId = '{User.Identity.Name}' AND Id='{id}'";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (DateOnly.TryParse(reader.GetString(3), out DateOnly date))
                            {
                                taskEdit.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = DateOnly.Parse(reader.GetString(3))
                                    }
                                );
                            }
                            else
                            {
                                taskEdit.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = null
                                    }
                                );
                            }
                        }
                    }
                    else
                    {
                        return new TaskEditViewModel
                        {
                            TaskEdit = taskEdit
                        };
                    }
                };
            }

        }
        return new TaskEditViewModel
        {
            TaskEdit = taskEdit
        };
    }
    internal TaskViewModel GetMyTasks()
    {

        List<Tasks.Models.Task> taskList = new();
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT Id, Title, Description, Date, requser FROM Task WHERE exeuser = '{User.Identity.Name}' AND Status = '' ORDER BY Date ASC; ";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (DateOnly.TryParse(reader.GetString(3), out DateOnly date))
                            {
                                taskList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = DateOnly.Parse(reader.GetString(3)),
                                        Requser = reader.GetString(4)
                                    }
                                );
                            }
                            else
                            {
                                taskList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = null,
                                        Requser = reader.GetString(4)
                                    }
                                );
                            }
                        }
                    }
                    else
                    {
                        return new TaskViewModel
                        {
                            TaskList = taskList
                        };
                    }
                };
            }

        }
        return new TaskViewModel
        {
            TaskList = taskList
        };
    }

    internal TaskHistoryViewModel GetMyHistory1()
    {
        List<Tasks.Models.Task> taskHistoryList = new List<Tasks.Models.Task>();
        List<Tasks.Models.Task> taskHistoryListFriends = new List<Tasks.Models.Task>();
        string connectionString = GetConnectionString();
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandText = $"SELECT Id, Title, Description, Date, Status, exeuser, requser FROM Task WHERE exeuser = '{User.Identity.Name}' AND Status <> '' ORDER BY Date ASC; ";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (DateOnly.TryParse(reader.GetString(3), out DateOnly date))
                            {
                                taskHistoryList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = DateOnly.Parse(reader.GetString(3)),
                                        Status = reader.GetString(4),
                                        Exeuser = reader.GetString(5),
                                        Requser = reader.GetString(6)
                                    }
                                );
                            }
                            else
                            {
                                taskHistoryList.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = null,
                                        Status = reader.GetString(4),
                                        Exeuser = reader.GetString(5),
                                        Requser = reader.GetString(6)

                                    }
                                );
                            }
                        }
                    }
                }
                command.CommandText = $"SELECT Id, Title, Description, Date, Status, exeuser, requser FROM Task WHERE (requser = '{User.Identity.Name}' AND exeuser <> '{User.Identity.Name}') AND Status <> '' ORDER BY Date ASC; ";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (DateOnly.TryParse(reader.GetString(3), out DateOnly date))
                            {
                                taskHistoryListFriends.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = DateOnly.Parse(reader.GetString(3)),
                                        Status = reader.GetString(4),
                                        Exeuser = reader.GetString(5),
                                        Requser = reader.GetString(6)
                                    }
                                );
                            }
                            else
                            {
                                taskHistoryListFriends.Add(
                                    new Tasks.Models.Task
                                    {
                                        Id = reader.GetInt32(0),
                                        Title = reader.GetString(1),
                                        Description = reader.GetString(2),
                                        Date = null,
                                        Status = reader.GetString(4),
                                        Exeuser = reader.GetString(5),
                                        Requser = reader.GetString(6)

                                    }
    );
                            }
                        }
                    }
                }
            }
        }
        return new TaskHistoryViewModel
        {
            TaskHistoryList = taskHistoryList,
            TaskHistoryListFriends = taskHistoryListFriends
        };
    }




}

