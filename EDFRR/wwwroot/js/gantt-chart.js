function renderGanttChart(canvasId, ganttData) {
    var canvas = document.getElementById(canvasId);
    if (!canvas || !ganttData || ganttData.length === 0) return;

    // Responsive: set canvas width to container width
    var container = canvas.parentElement;
    canvas.width = container.clientWidth || 800;

    var ctx = canvas.getContext('2d');
    var processes = [];
    var colors = {};
    var colorPalette = [
        '#0d6efd', '#198754', '#6f42c1', '#fd7e14', '#20c997',
        '#d63384', '#0dcaf0', '#6610f2', '#ffc107', '#dc3545'
    ];
    var colorIndex = 0;

    // Identify all processes and assign colors
    ganttData.forEach(function(entry) {
        if (entry.ProcessId !== 'IDLE' && !colors[entry.ProcessId]) {
            colors[entry.ProcessId] = colorPalette[colorIndex % colorPalette.length];
            colorIndex++;
        }
    });

    var barHeight = 40;
    var padding = 30;
    var maxTime = Math.max.apply(null, ganttData.map(function(g) { return g.EndTime; }));
    if (maxTime === 0) maxTime = 1;
    var scale = (canvas.width - 2 * padding) / maxTime;

    // Adjust canvas height
    canvas.height = padding * 2 + barHeight + 40;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    var y = padding;

    // Draw time axis
    ctx.font = '11px Segoe UI';
    ctx.fillStyle = '#666';
    ctx.textAlign = 'center';
    for (var t = 0; t <= maxTime; t++) {
        var x = padding + t * scale;
        ctx.fillText(t.toString(), x, y + barHeight + 18);
        ctx.beginPath();
        ctx.strokeStyle = '#e9ecef';
        ctx.lineWidth = 1;
        ctx.moveTo(x, y);
        ctx.lineTo(x, y + barHeight);
        ctx.stroke();
    }

    // Draw Gantt bars
    ganttData.forEach(function(entry) {
        var x = padding + entry.StartTime * scale;
        var width = (entry.EndTime - entry.StartTime) * scale;
        if (width < 1) width = 1;

        if (entry.IsIdle) {
            // Idle: striped pattern
            ctx.fillStyle = '#e9ecef';
            ctx.strokeStyle = '#dee2e6';
        } else {
            ctx.fillStyle = colors[entry.ProcessId] || '#6c757d';
            ctx.strokeStyle = 'white';
        }

        // Draw bar with rounded corners
        var radius = 3;
        ctx.beginPath();
        ctx.moveTo(x + radius, y);
        ctx.lineTo(x + width - radius, y);
        ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
        ctx.lineTo(x + width, y + barHeight - radius);
        ctx.quadraticCurveTo(x + width, y + barHeight, x + width - radius, y + barHeight);
        ctx.lineTo(x + radius, y + barHeight);
        ctx.quadraticCurveTo(x, y + barHeight, x, y + barHeight - radius);
        ctx.lineTo(x, y + radius);
        ctx.quadraticCurveTo(x, y, x + radius, y);
        ctx.closePath();
        ctx.fill();
        ctx.strokeStyle = 'rgba(255,255,255,0.3)';
        ctx.lineWidth = 1;
        ctx.stroke();

        // Process label
        ctx.fillStyle = entry.IsIdle ? '#666' : 'white';
        ctx.font = 'bold 10px Segoe UI';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        var label = entry.ProcessId !== 'IDLE' ? entry.ProcessName : 'Idle';
        if (width > 25) {
            ctx.fillText(label, x + width / 2, y + barHeight / 2 - 6);
            ctx.font = '9px Segoe UI';
            ctx.fillStyle = entry.IsIdle ? '#999' : 'rgba(255,255,255,0.8)';
            ctx.fillText(entry.StartTime + '-' + entry.EndTime, x + width / 2, y + barHeight / 2 + 8);
        }

        // Draw time labels on top
        if (!entry.IsIdle && width > 20) {
            ctx.fillStyle = '#333';
            ctx.font = '9px Segoe UI';
            ctx.fillText(entry.StartTime.toString(), x + 2, y - 5);
        }
    });

    // Bottom line
    ctx.beginPath();
    ctx.strokeStyle = '#dee2e6';
    ctx.lineWidth = 1;
    ctx.moveTo(padding, y + barHeight);
    ctx.lineTo(padding + maxTime * scale, y + barHeight);
    ctx.stroke();

    // Legend
    y += barHeight + 30;
    ctx.font = '10px Segoe UI';
    ctx.textAlign = 'left';
    var legendX = padding;

    Object.keys(colors).forEach(function(pid) {
        if (legendX + 80 > canvas.width) return;
        ctx.fillStyle = colors[pid];
        ctx.beginPath();
        ctx.arc(legendX + 6, y + 6, 5, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = '#333';
        ctx.fillText(pid, legendX + 14, y + 10);
        legendX += 70;
    });

    // Resize handler
    window.addEventListener('resize', function() {
        renderGanttChart(canvasId, ganttData);
    });
}
