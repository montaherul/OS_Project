(function () {
  if (typeof Chart === 'undefined') return;

  Chart.defaults.color = '#8892A8';
  Chart.defaults.borderColor = '#1E2638';
  Chart.defaults.font.family = 'Inter';
  Chart.defaults.plugins.legend.labels.usePointStyle = true;
  Chart.defaults.plugins.tooltip.backgroundColor = '#161B27';
  Chart.defaults.plugins.tooltip.borderColor = '#2A3350';
  Chart.defaults.plugins.tooltip.borderWidth = 1;
  Chart.defaults.plugins.tooltip.titleColor = '#E8EAF0';
  Chart.defaults.plugins.tooltip.bodyColor = '#8892A8';
  Chart.defaults.plugins.tooltip.padding = 12;
  Chart.defaults.plugins.tooltip.cornerRadius = 8;

  window.PROCESS_COLORS = [
    '#7C6EFA','#2DD4A0','#F0A732','#F05252',
    '#38BDF8','#E879F9','#FB923C','#A3E635'
  ];

  window.destroyChart = function (id) {
    var existing = Chart.getChart(id);
    if (existing) existing.destroy();
  };

  window.animateValue = function (el, start, end, duration) {
    duration = duration || 800;
    var range = end - start;
    var startTime = performance.now();
    function update(now) {
      var progress = Math.min((now - startTime) / duration, 1);
      var eased = 1 - Math.pow(1 - progress, 3);
      el.textContent = (start + range * eased).toFixed(1);
      if (progress < 1) requestAnimationFrame(update);
    }
    requestAnimationFrame(update);
  };
})();
