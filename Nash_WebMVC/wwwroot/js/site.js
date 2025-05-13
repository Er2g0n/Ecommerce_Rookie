$(document).ready(function () {
    toastr.options = {
        "positionClass": "toast-top-right",
        "progressBar": true,
        "timeOut": "3000"
    };
    var errorMessage = '@TempData["error"]';
    if (errorMessage) {
        toastr.error(errorMessage);
    }

    var successMessage = '@TempData["success"]';
    if (successMessage) {
        toastr.success(successMessage);
    }

});