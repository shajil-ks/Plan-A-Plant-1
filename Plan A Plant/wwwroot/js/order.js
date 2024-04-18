var dataTable;
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else {
        if (url.includes("completed")) {
            loadDataTable("completed");
        }
        else {
            if (url.includes("pending")) {
                loadDataTable("pending");
            }
            else {
                if (url.includes("approved")) {
                    loadDataTable("approved");
                }
                else {
                    loadDataTable("all");
                }
            }
        }

    }
    
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/order/getall?status='+status
        },
        "columns": [
            { data: "id", "width": "5%", "className": "text-center" },
            { data: "applicationUser.name", "width": "10%", "className": "text-center" },
            { data: "applicationUser.phoneNumber", "width": "9%", "className": "text-center" },
            { data: "applicationUser.email", "width": "10%", "className": "text-center" },
            { data: "orderStatus", "width": "10%", "className": "text-center" },
            { data: "orderTotal", "width": "10%", "className": "text-center" },
            {
                data: "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                           <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Details
                                </a>
                                 
                    </div>`
                },
                "width": "10%",
                "className": "text-center"
            }
        ]
    });
}
