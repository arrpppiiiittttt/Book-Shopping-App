var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            //"url": "/Admin/OderManagement/GetAll",
            //"type": "GET",
            //"dataType": "json"
        },
        lengthMenu: [
            [5, 10, 15, 20], [5, 10, 15, 20]           //values, displayValues
        ],
        "columns": [

            {
                "data": "id",
                "render": function (data) {
                    return `

                    
                    `;

                }
            },
            { "data": "name", "width": "50%" },
        ]
    })
}
