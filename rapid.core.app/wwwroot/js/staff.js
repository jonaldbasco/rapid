
$(function () {
    $(document).on('click', '#btnTriggerSurge', function (e) {
        e.preventDefault();
        refreshStaffList();
    });
    function refreshStaffList() {
        $.ajax({
            url: '@Url.Action("StaffListPartial", "Home")',
            type: 'POST',
            success: function (html) {
                $('#staffFeedScrollContainer').html(html); // 🔥 THIS is what updates the page
            },
            error: function (xhr) {
                console.error(xhr.responseText);
            }
        });
    }
});