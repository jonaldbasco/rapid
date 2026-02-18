
$(function () {
    $(document).on('click', '#btnTriggerSurge', function (e) {
        e.preventDefault();
        refreshStaffList();
    });
    function refreshStaffList() {
        const staffListUrl = '@Url.Action("StaffListPartial", "Home")';
        $.ajax({
            url: '/Home/StaffListPartial',
            type: 'POST',
            success: function (html) {
                $('#staffFeedScrollContainer').html(html); // 🔥 THIS is what updates the page
                window.location.reload();
                console.log("reload");
                $('#btnTriggerSurge').disabled = true;
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                console.log("not reload");
            }
        });
        console.log("loaded");
    }
});