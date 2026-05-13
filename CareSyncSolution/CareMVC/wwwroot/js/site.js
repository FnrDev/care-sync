// Auto-dismiss alerts after 5 seconds
$(function () {
    setTimeout(function () {
        $('.alert-dismissible').fadeOut(500, function () {
            $(this).remove();
        });
    }, 5000);
});
