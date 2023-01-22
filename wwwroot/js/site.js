// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    var dtToday = new Date();

    var month = dtToday.getMonth() + 1;
    var day = dtToday.getDate();
    var year = dtToday.getFullYear();
    if (month < 10) month = "0" + month.toString();
    if (day < 10) day = "0" + day.toString();

    var maxDate = year + "-" + month + "-" + day;

    $("#txtDate").attr("min", maxDate);
});

function DeleteTask(id){
    $.ajax({
        url: "/Task/DeleteTask",
        type: "POST",
        data: { 
            id: id
        },
        success: function () {
            window.location.reload();
        }
    });
}

function DeleteFriend(id){
    $.ajax({
        url: "/Task/DeleteFriend",
        type: "POST",
        data: { 
            id: id
        },
        success: function () {
            window.location.reload();
        }
    });
}

function AddTask(){
    
    var data = document.getElementById("txtDate").value;
    if (data == '') {
        data="";
    }
    
    $.ajax({
        url: "/Task/AddTaskData",
        type: "POST",
        data: { 
            Title: document.getElementById("txttitle").value,
            Exeuser: document.getElementById("txttaskfor").value,
            Date: data,
            Description: document.getElementById("txtdescription").value,
            Requser: document.getElementById("txtrequser").value,
            Status: document.getElementById("txtstatus").value
        },
        success: function () {  
            window.location.reload();
        }
    });
}

