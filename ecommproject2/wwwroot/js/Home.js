var dataTable;

$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "paging": true,   // Enable pagination
        "ordering": false, // Disable sorting
        "info": true,     // Enable table information
        "searching": true, // Enable search
        "lengthMenu": [6, 8, 10, 12], // Option for page length (e.g., 5, 10, 25, 50 rows per page)
       // "order": [[1, "desc"]], // Default ordering: column index 1, descending
        "columnDefs": [{
            "targets": 0, // Targets the first column (Serial column)
            "orderable": false, // Disable sorting on the serial column
            "searchable": true // Disable searching on the serial column
        }]
    })
}



//        "ajax": {
//            "url": "/Customer/Home/GetAll",
//            "type": "GET",
//            "dataType": "json"
//        },
//        "lengthMenu": [
//            [3, 6, 9, 12], [3, 6, 9, 12] //values, displayValues
//        ],
//        "columns": [
//            {
//                "data": null,
//                "render": function (data, type, row) {             //row=>contains all data eg title,desc,image
//                    return `
//                        <div class="card mb-3" style="max-width: 300px; float: right">
//                            <img src="${row.imageUrl}" class="card-img-top" alt="${row.title}">
//                            <div class="card-body">
//                                <h5 class="card-title">${row.title}</h5>
                                
//                                <p><b>Price:</b> $${row.price.toFixed(2)}</p>
//                                <a href="/Customer/Home/Details/${row.id}" class="btn btn-primary">Details</a>
//                            </div>
//                        </div>
//                    `;
//                },
//                "width": "100%"
           // }
        //]

