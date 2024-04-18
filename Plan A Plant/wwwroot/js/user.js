  
var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/user/getall'
        },
        "columns": [
            { data: "name", "width": "22%", "className": "text-center" },
            { data: "email", "width": "22%", "className": "text-center" },
            { data: "phoneNumber", "width": "22%", "className": "text-center" },
           /* { data: "", "width": "10%", "className": "text-center" },*/
           /* { data: "", "width": "15%", "className": "text-center" },*/
            {
                data: { id: "id", lockoutEnd:"lockoutEnd" },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `<div class="text-center">
                           <a onclick=LockUnlock('${data.id}') class="btn btn-success text-white" style="cursor:pointer;width:100px;"<i class="bi bi-pencil-square"></i> Unblock
                                </a>
                                                            
                           </div>`
                    }
                    else
                    {
                        return `<div class="text-center">
                           <a onclick=LockUnlock('${data.id}') class="btn btn-danger text-white" style="cursor:pointer;width:100px;"<i class="bi bi-pencil-square"></i> Block
                                </a>
                            
                           </div>`


                    }
                },
                "width": "22%",
                "className": "text-center"
            }
        ]
    });
}
function LockUnlock(id)
{
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }

    });

}