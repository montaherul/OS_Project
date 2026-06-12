function renderGanttChart(canvasId, ganttData) {
  var canvas = document.getElementById(canvasId);
  if (!canvas || !ganttData || ganttData.length === 0) return;

  var container = canvas.parentElement;
  canvas.width = container.clientWidth || 800;

  var ctx = canvas.getContext('2d');
  var processMap = {};
  var colorIndex = 0;
  var COLORS = window.PROCESS_COLORS || [
    '#7C6EFA','#2DD4A0','#F0A732','#F05252',
    '#38BDF8','#E879F9','#FB923C','#A3E635'
  ];

  ganttData.forEach(function (entry) {
    if (entry.ProcessId !== 'IDLE' && !processMap[entry.ProcessId]) {
      processMap[entry.ProcessId] = COLORS[colorIndex % COLORS.length];
      colorIndex++;
    }
  });

  var barHeight = 36;
  var padding = { top: 8, left: 60, right: 16, bottom: 20 };
  var maxTime = ganttData.reduce(function (m, g) { return Math.max(m, g.EndTime); }, 0);
  if (maxTime === 0) maxTime = 1;
  var scale = (canvas.width - padding.left - padding.right) / maxTime;

  var processIds = Object.keys(processMap);
  var totalHeight = padding.top + (processIds.length + 1) * (barHeight + 4) + padding.bottom;
  canvas.height = Math.max(totalHeight, 100);

  ctx.clearRect(0, 0, canvas.width, canvas.height);

  var y = padding.top;

  // Group entries by PID
  var grouped = {};
  ganttData.forEach(function (entry) {
    var key = entry.ProcessId || 'IDLE';
    if (!grouped[key]) grouped[key] = [];
    grouped[key].push(entry);
  });

  // Sort by start time
  Object.keys(grouped).forEach(function (key) {
    grouped[key].sort(function (a, b) { return a.StartTime - b.StartTime; });
  });

  // Draw rows
  var displayPids = Object.keys(grouped).filter(function (k) { return k !== 'IDLE'; });
  if (grouped['IDLE']) displayPids.push('IDLE');

  displayPids.forEach(function (pid) {
    var label = pid === 'IDLE' ? 'Idle' : pid;
    var entries = grouped[pid];

    // Label
    ctx.fillStyle = 'var(--text-secondary)';
    ctx.font = '11px JetBrains Mono, monospace';
    ctx.textAlign = 'right';
    ctx.textBaseline = 'middle';
    ctx.fillText(label, padding.left - 10, y + barHeight / 2);

    // Background row
    ctx.fillStyle = pid === 'IDLE' ? 'rgba(30,38,56,0.3)' : 'transparent';
    ctx.fillRect(padding.left, y, canvas.width - padding.left - padding.right, barHeight);

    entries.forEach(function (entry) {
      var x = padding.left + entry.StartTime * scale;
      var w = Math.max((entry.EndTime - entry.StartTime) * scale, 1);
      var color;

      if (entry.IsIdle) {
        color = '#1E2638';
      } else if (entry.IsContextSwitch) {
        color = '#4E5A72';
      } else {
        color = processMap[pid] || '#6c757d';
      }

      ctx.fillStyle = color;
      var r = 3;
      ctx.beginPath();
      ctx.moveTo(x + r, y);
      ctx.lineTo(x + w - r, y);
      ctx.quadraticCurveTo(x + w, y, x + w, y + r);
      ctx.lineTo(x + w, y + barHeight - r);
      ctx.quadraticCurveTo(x + w, y + barHeight, x + w - r, y + barHeight);
      ctx.lineTo(x + r, y + barHeight);
      ctx.quadraticCurveTo(x, y + barHeight, x, y + barHeight - r);
      ctx.lineTo(x, y + r);
      ctx.quadraticCurveTo(x, y, x + r, y);
      ctx.closePath();
      ctx.fill();

      if (entry.IsIdle) {
        // Diagonal hatch for idle
        ctx.save();
        ctx.beginPath();
        ctx.rect(x, y, w, barHeight);
        ctx.clip();
        ctx.strokeStyle = '#2A3350';
        ctx.lineWidth = 1;
        for (var s = -barHeight; s < w + barHeight; s += 6) {
          ctx.beginPath();
          ctx.moveTo(x + s, y + barHeight);
          ctx.lineTo(x + s + barHeight, y);
          ctx.stroke();
        }
        ctx.restore();
      }

      if (!entry.IsIdle && w > 20) {
        ctx.fillStyle = '#fff';
        ctx.font = 'bold 10px Inter, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(entry.ProcessName || pid, x + w / 2, y + barHeight / 2);
      }
    });

    y += barHeight + 4;
  });

  // Time axis
  y += 4;
  ctx.strokeStyle = '#1E2638';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(padding.left, y);
  ctx.lineTo(padding.left + maxTime * scale, y);
  ctx.stroke();

  ctx.fillStyle = '#4E5A72';
  ctx.font = '10px JetBrains Mono, monospace';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'top';

  for (var t = 0; t <= maxTime; t++) {
    var tx = padding.left + t * scale;
    ctx.fillText(t.toString(), tx, y + 4);
    ctx.beginPath();
    ctx.strokeStyle = '#1E2638';
    ctx.lineWidth = 1;
    ctx.moveTo(tx, y - 4);
    ctx.lineTo(tx, y + 4);
    ctx.stroke();
  }
}
