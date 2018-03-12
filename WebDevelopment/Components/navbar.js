$.ajax({
    url: "Components/navbar.cshtml",
    cache: false,
    success: function (html) {
        $("#nav-placeholder").append(html);
    }
});