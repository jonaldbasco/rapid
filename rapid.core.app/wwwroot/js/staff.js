
$(function () {
    $(document).on('click', '#btnTriggerSurge', function (e) {
        e.preventDefault();
        refreshStaffList();
    });
    function refreshStaffList() {
        const staffListUrl = '@Url.Action("StaffListPartial", "Home")';
       
    }
});