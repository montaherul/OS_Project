(function ($) {
  if (typeof $ === 'undefined') return;

  $(document).ready(function () {
    // Auto-dismiss alerts
    setTimeout(function () {
      $('.alert').fadeOut('slow');
    }, 5000);

    // Sidebar collapse
    var sidebarToggle = document.getElementById('sidebar-toggle');
    var sidebar = document.getElementById('sidebar');
    if (sidebarToggle && sidebar) {
      sidebarToggle.addEventListener('click', function () {
        sidebar.classList.toggle('collapsed');
      });
    }

    // Drawer open/close
    $(document).on('click', '[data-drawer-open]', function () {
      var target = $(this).data('drawer-open');
      $('#' + target).addClass('open');
      $('.drawer-overlay').addClass('open');
    });

    $(document).on('click', '[data-drawer-close], .drawer-overlay', function () {
      $('.drawer').removeClass('open');
      $('.drawer-overlay').removeClass('open');
    });

    // Pill selector
    $(document).on('click', '.pill-selector__option', function () {
      var parent = $(this).closest('.pill-selector');
      parent.find('.pill-selector__option').removeClass('active');
      $(this).addClass('active');
      var idx = $(this).index();
      var slider = parent.find('.pill-selector__slider');
      var total = parent.find('.pill-selector__option').length;
      var w = 100 / total;
      slider.css({ left: (idx * w) + '%', width: w + '%' });
    });

    // Table sorting
    $(document).on('click', '.edfrr-table thead th[data-sort]', function () {
      var th = $(this);
      var table = th.closest('table');
      var tbody = table.find('tbody');
      var colIdx = th.index();
      var rows = tbody.find('tr').toArray();
      var dir = th.hasClass('sort-asc') ? 'desc' : 'asc';

      th.closest('tr').find('th').removeClass('sort-asc sort-desc');
      th.addClass('sort-' + dir);

      rows.sort(function (a, b) {
        var aVal = $(a).children('td').eq(colIdx).text().trim();
        var bVal = $(b).children('td').eq(colIdx).text().trim();
        var aNum = parseFloat(aVal);
        var bNum = parseFloat(bVal);
        if (!isNaN(aNum) && !isNaN(bNum)) {
          return dir === 'asc' ? aNum - bNum : bNum - aNum;
        }
        return dir === 'asc' ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
      });

      tbody.append(rows);
    });

    // Drop zone
    $(document).on('drag dragstart dragend dragover dragenter dragleave drop', '.drop-zone', function (e) {
      e.preventDefault();
      e.stopPropagation();
    });
    $(document).on('dragover dragenter', '.drop-zone', function () {
      $(this).addClass('drag-over');
    });
    $(document).on('dragleave dragend drop', '.drop-zone', function () {
      $(this).removeClass('drag-over');
    });
    $(document).on('drop', '.drop-zone', function (e) {
      var files = e.originalEvent.dataTransfer.files;
      var inputId = $(this).data('file-input');
      if (inputId && files.length) {
        var input = document.getElementById(inputId);
        if (input) {
          input.files = files;
          $(input).trigger('change');
        }
      }
    });
  });
})(typeof jQuery !== 'undefined' ? jQuery : null);
