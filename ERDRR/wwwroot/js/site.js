$(document).ready(function () {
    // Dark mode toggle
    var theme = localStorage.getItem('theme') || 'light';
    if (theme === 'dark') {
        $('body').attr('data-theme', 'dark');
        $('#themeToggle i').removeClass('fa-moon').addClass('fa-sun');
    }

    $('#themeToggle').click(function () {
        if ($('body').attr('data-theme') === 'dark') {
            $('body').removeAttr('data-theme');
            localStorage.setItem('theme', 'light');
            $(this).find('i').removeClass('fa-sun').addClass('fa-moon');
        } else {
            $('body').attr('data-theme', 'dark');
            localStorage.setItem('theme', 'dark');
            $(this).find('i').removeClass('fa-moon').addClass('fa-sun');
        }
    });

    // Auto-dismiss alerts
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);
});
