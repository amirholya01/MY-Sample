// fill page for paging
function fillPage(id) {
    $("#Page").val(id);
    $("#filter-form").submit();
}

// submit form with filter-search id on change radio buttons
$('#filter-form input[type=radio]').change(function () {
    $("#Page").val(1);
    $("#filter-form").submit();
});
