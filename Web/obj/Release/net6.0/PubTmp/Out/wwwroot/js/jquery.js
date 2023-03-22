function openTab(evt, tabName) {
// Declare all variables
var i, tabcontent, tablinks;

// Get all elements with class="tabcontent" and hide them
tabcontent = document.getElementsByClassName("tabcontent");
for (i = 0; i < tabcontent.length; i++) {
    tabcontent[i].style.display = "none";
}

// Get all elements with class="tablinks" and remove the class "active"
tablinks = document.getElementsByClassName("tablinks");
for (i = 0; i < tablinks.length; i++) {
    tablinks[i].className = tablinks[i].className.replace(" active", "");
}

// Show the current tab, and add an "active" class to the button that opened the tab
document.getElementById(tabName).style.display = "block";
evt.currentTarget.className += " active";
}

function setDefaultTab() {
    // Simulate a click on the Applications tab to display it as the start page
    document.getElementById("defaultOpen").click();
}

// Call setDefaultTab when the page has finished loading
window.onload = setDefaultTab;

$(document).ready(function () {
    $('a').attr('target', '_blank');
});

$(function () {
    $('#flatComment').on('keyup', function () {
        $('#flatComment').attr('value', $('#flatComment').val());
    });

    $('.update-btn').on('click', function () {
        var row = $(this).closest('tr'); // Get the current row
        var itemId = row.find('input[name="itemId"]').val(); // Get the itemId value from the hidden input field in the current row
        var comment = row.find('input[name="comment"]').val(); // Get the comment value from the input field in the current row

        $.ajax({
            type: "POST",
            url: "/Index?handler=UpdateComment",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            data: { itemId: itemId, comment: comment },
            success: function () {
                alert("Комментарий обновлён!");
                row.find('#flatComment').css('background-color', 'transparent').css('border', 'none'); // Use row.find() to update the comment input field in the current row
            },
            error: function (xhr) {
                console.error(xhr.responseText);
            }
        });
    });
});

$('.remove-btn').on('click', function () {
    var row = $(this).closest('tr'); // Get the current row
    var itemId = row.find('input[name="itemId"]').val(); // Get the itemId value from the hidden input field in the current row

    $.ajax({
        type: "POST",
        url: "/Index?handler=RemoveFlat",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        data: { itemId: itemId },
        success: function () {
            alert("Квартира удалена!");
            row.remove(); // Use row.remove() to remove the current row from the table
        },
        error: function (xhr) {
            console.error(xhr.responseText);
        }
    });
});






