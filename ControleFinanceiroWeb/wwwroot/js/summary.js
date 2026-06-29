//#region Listeners

document.addEventListener('DOMContentLoaded', initSummaryCharts);

//#endregion


//#region Functions

/**
 * Builds the financial flow line chart.
 * @param {CanvasRenderingContext2D} ctx - Chart canvas context.
 * @param {Array} labels - Chart X-axis labels.
 * @param {Array} revenues - Revenues dataset.
 * @param {Array} expenses - Expenses dataset.
 */
function renderFlowChart(ctx, labels, revenues, expenses) {
    const greenGradient = ctx.createLinearGradient(0, 0, 0, 260);

    greenGradient.addColorStop(0, 'rgba(25, 135, 84, 0.15)');
    greenGradient.addColorStop(1, 'rgba(25, 135, 84, 0.0)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Receitas',
                    data: revenues,
                    borderColor: '#198754',
                    borderWidth: 3,
                    backgroundColor: greenGradient,
                    fill: true,
                    tension: 0.35,
                    pointBackgroundColor: '#198754',
                    pointHoverRadius: 6
                },
                {
                    label: 'Despesas',
                    data: expenses,
                    borderColor: '#dc3545',
                    borderWidth: 2.5,
                    fill: false,
                    tension: 0.3,
                    pointBackgroundColor: '#dc3545',
                    pointHoverRadius: 5
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 15,
                        font: {
                            family: "'Plus Jakarta Sans', sans-serif",
                            weight: '600'
                        }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false }
                },
                y: {
                    ticks: {
                        callback: function(value) {
                            return 'R$ ' + value.toLocaleString('pt-BR');
                        }
                    }
                }
            }
        }
    });
}

/**
 * Builds the expenses distribution doughnut chart.
 * @param {CanvasRenderingContext2D} ctx - Chart canvas context.
 * @param {Array} labels - Category labels.
 * @param {Array} data - Category expense values.
 * @param {Array} colors - Colors for each category slice.
 */
function renderDoughnutChart(ctx, labels, data, colors) {
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors,
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            cutout: '72%'
        }
    });
}

//#endregion


//#region Init

/**
 * Initializes all charts on page load using configurations.
 */
function initSummaryCharts() {
    const ctxFlowEl = document.getElementById('financialFlowChart');

    if (ctxFlowEl) {
        const ctxFlow = ctxFlowEl.getContext('2d');
        const labels = JSON.parse(ctxFlowEl.dataset.labels || '[]');
        const revenues = JSON.parse(ctxFlowEl.dataset.revenues || '[]');
        const expenses = JSON.parse(ctxFlowEl.dataset.expenses || '[]');

        renderFlowChart(ctxFlow, labels, revenues, expenses);
    }

    const ctxExpensesEl = document.getElementById('expensesDoughnutChart');

    if (ctxExpensesEl) {
        const ctxExpenses = ctxExpensesEl.getContext('2d');
        const labels = JSON.parse(ctxExpensesEl.dataset.labels || '[]');
        const data = JSON.parse(ctxExpensesEl.dataset.values || '[]');
        const colors = JSON.parse(ctxExpensesEl.dataset.colors || '[]');

        renderDoughnutChart(ctxExpenses, labels, data, colors);
    }
}

//#endregion