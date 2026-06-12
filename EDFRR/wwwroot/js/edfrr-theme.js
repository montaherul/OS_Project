(function () {
  var html = document.documentElement;
  var saved = localStorage.getItem('edfrr-theme') || 'dark';
  html.setAttribute('data-theme', saved);

  var toggleBtn = document.getElementById('theme-toggle');
  if (toggleBtn) {
    toggleBtn.addEventListener('click', function () {
      var next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
      html.setAttribute('data-theme', next);
      localStorage.setItem('edfrr-theme', next);
      var icon = toggleBtn.querySelector('i');
      if (icon) {
        icon.className = next === 'dark' ? 'fas fa-moon' : 'fas fa-sun';
      }
    });
  }
})();
