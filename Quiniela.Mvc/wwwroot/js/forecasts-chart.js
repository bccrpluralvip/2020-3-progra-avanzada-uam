(() => {
    const root = document.getElementById("forecast-chart-root");
    if (!root) {
        return;
    }

    const sportSelect = document.getElementById("sport");
    const teamSelect = document.getElementById("teamId");
    const matchSelect = document.getElementById("matchId");
    const dateInput = document.getElementById("date");
    const status = document.getElementById("forecast-chart-status");
    const emptyState = document.getElementById("forecast-chart-empty");
    const chartContent = document.getElementById("forecast-chart-content");
    const charts = [
        { resultType: 1, color: "#dc3545" },
        { resultType: 0, color: "#0d6efd" },
        { resultType: -1, color: "#198754" }
    ];
    const svgs = [...root.querySelectorAll(".forecast-chart")];
    let optionRequestController;
    let chartRequestId = 0;
    let connection;
    let connectionStartPromise;
    let subscribedMatchId = null;
    let desiredMatchId = null;
    let subscriptionQueue = Promise.resolve();
    let retryTimer;

    function renderChart(svg, records, resultType, color) {
        const width = 720;
        const height = 300;
        const margin = { top: 16, right: 20, bottom: 36, left: 48 };
        const plotWidth = width - margin.left - margin.right;
        const plotHeight = height - margin.top - margin.bottom;
        const counts = Array(24).fill(0);

        for (const record of records) {
            if (record.resultType === resultType && record.hour >= 0 && record.hour < 24) {
                counts[record.hour] = record.count;
            }
        }

        const maxValue = Math.max(1, ...counts);
        const tickStep = Math.max(1, Math.ceil(maxValue / 4));
        const yMax = tickStep * 4;
        const x = hour => margin.left + hour * plotWidth / 23;
        const y = count => margin.top + plotHeight - count * plotHeight / yMax;

        svg.replaceChildren();

        for (let tick = 0; tick <= yMax; tick += tickStep) {
            const tickY = y(tick);
            svg.append(makeSvgElement("line", {
                x1: margin.left,
                y1: tickY,
                x2: width - margin.right,
                y2: tickY,
                stroke: "#dee2e6"
            }));
            svg.append(makeSvgElement("text", {
                x: margin.left - 10,
                y: tickY + 4,
                "text-anchor": "end",
                fill: "#6c757d",
                "font-size": 12
            }, String(tick)));
        }

        svg.append(makeSvgElement("line", {
            x1: margin.left,
            y1: margin.top,
            x2: margin.left,
            y2: height - margin.bottom,
            stroke: "#6c757d"
        }));
        svg.append(makeSvgElement("line", {
            x1: margin.left,
            y1: height - margin.bottom,
            x2: width - margin.right,
            y2: height - margin.bottom,
            stroke: "#6c757d"
        }));

        [0, 6, 12, 18, 23].forEach(hour => {
            svg.append(makeSvgElement("text", {
                x: x(hour),
                y: height - 10,
                "text-anchor": "middle",
                fill: "#6c757d",
                "font-size": 12
            }, String(hour).padStart(2, "0")));
        });

        const points = counts.map((count, hour) => `${x(hour)},${y(count)}`).join(" ");
        svg.append(makeSvgElement("polyline", {
            points,
            fill: "none",
            stroke: color,
            "stroke-width": 3,
            "stroke-linejoin": "round",
            "stroke-linecap": "round"
        }));
        counts.forEach((count, hour) => {
            svg.append(makeSvgElement("circle", {
                cx: x(hour),
                cy: y(count),
                r: 2.5,
                fill: color
            }));
        });
    }

    function makeSvgElement(name, attributes, text) {
        const element = document.createElementNS("http://www.w3.org/2000/svg", name);
        for (const [attribute, value] of Object.entries(attributes)) {
            element.setAttribute(attribute, value);
        }
        if (text !== undefined) {
            element.textContent = text;
        }
        return element;
    }

    function render(records) {
        charts.forEach((chart, index) =>
            renderChart(svgs[index], records, chart.resultType, chart.color));
    }

    function setOptions(select, placeholder, options) {
        select.replaceChildren(new Option(placeholder, ""));
        for (const option of options) {
            select.add(new Option(option.text, option.value));
        }
    }

    async function getJson(url, signal) {
        const response = await fetch(url, { signal });
        if (!response.ok) {
            throw new Error(`Request failed with HTTP ${response.status}.`);
        }
        return response.json();
    }

    async function loadOptions(url, select, placeholder, enabled) {
        optionRequestController?.abort();
        optionRequestController = new AbortController();
        select.disabled = true;
        try {
            const options = await getJson(url, optionRequestController.signal);
            setOptions(select, placeholder, options);
            select.disabled = !enabled;
        } catch (error) {
            if (error.name === "AbortError") {
                return;
            }
            select.disabled = !enabled;
            status.textContent = "Could not load filter options. Please try again.";
            console.error(error);
        }
    }

    function isLiveSelection() {
        return dateInput.value === new Date().toISOString().slice(0, 10);
    }

    function setChartState(hasMatch) {
        emptyState.classList.toggle("d-none", hasMatch);
        chartContent.classList.toggle("d-none", !hasMatch);
    }

    async function refreshCharts() {
        const matchId = Number(matchSelect.value);
        const requestId = ++chartRequestId;
        if (!matchId) {
            setChartState(false);
            status.textContent = "";
            return;
        }

        setChartState(true);
        status.textContent = "Loading forecasts…";
        const query = new URLSearchParams({ matchId: String(matchId), date: dateInput.value });
        try {
            const records = await getJson(`${root.dataset.dataUrl}?${query}`);
            if (requestId !== chartRequestId) {
                return;
            }
            render(records);
            status.textContent = isLiveSelection()
                ? connection?.state === signalR.HubConnectionState.Connected
                    ? "Live updates connected."
                    : "Live updates connecting…"
                : "Past date: charts are static.";
        } catch (error) {
            if (requestId !== chartRequestId) {
                return;
            }
            status.textContent = "Could not load forecast charts. Please try again.";
            console.error(error);
        }
    }

    function createConnection() {
        if (connection) {
            return connection;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl(root.dataset.hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.on("ForecastsChanged", async changedMatchId => {
            if (Number(changedMatchId) !== Number(matchSelect.value) || !isLiveSelection()) {
                return;
            }

            await refreshCharts();
        });

        connection.onreconnecting(() => {
            if (desiredMatchId !== null) {
                status.textContent = "Live updates disconnected; reconnecting…";
            }
        });

        connection.onreconnected(async () => {
            subscribedMatchId = null;
            await updateSubscription();
            if (desiredMatchId !== null) {
                status.textContent = "Live updates connected.";
            }
        });

        connection.onclose(() => {
            subscribedMatchId = null;
            if (desiredMatchId !== null) {
                status.textContent = "Live updates are disconnected; retrying…";
                scheduleConnectionRetry();
            }
        });

        return connection;
    }

    async function ensureConnection() {
        const activeConnection = createConnection();
        if (activeConnection.state === signalR.HubConnectionState.Connected) {
            return;
        }
        if (connectionStartPromise) {
            await connectionStartPromise;
            return;
        }

        connectionStartPromise = activeConnection.start();
        try {
            await connectionStartPromise;
        } finally {
            connectionStartPromise = undefined;
        }
    }

    function scheduleConnectionRetry() {
        if (retryTimer || desiredMatchId === null) {
            return;
        }

        retryTimer = window.setTimeout(() => {
            retryTimer = undefined;
            void updateSubscription();
        }, 5000);
    }

    function updateSubscription() {
        desiredMatchId = matchSelect.value && isLiveSelection()
            ? Number(matchSelect.value)
            : null;

        subscriptionQueue = subscriptionQueue.then(async () => {
            if (desiredMatchId !== null) {
                try {
                    await ensureConnection();
                } catch (error) {
                    status.textContent = "Live updates unavailable; retrying…";
                    console.error(error);
                    scheduleConnectionRetry();
                    return;
                }
            }

            const activeConnection = connection;
            if (!activeConnection || activeConnection.state !== signalR.HubConnectionState.Connected) {
                return;
            }

            if (subscribedMatchId !== null && subscribedMatchId !== desiredMatchId) {
                await activeConnection.invoke("UnsubscribeFromMatch", subscribedMatchId);
                subscribedMatchId = null;
            }

            const matchId = desiredMatchId;
            if (matchId !== null && subscribedMatchId !== matchId) {
                await activeConnection.invoke("SubscribeToMatch", matchId);
                subscribedMatchId = matchId;
                // Catch changes made before joining the group, including while disconnected.
                if (desiredMatchId === matchId) {
                    await refreshCharts();
                }
            }

            if (desiredMatchId !== null && subscribedMatchId === desiredMatchId) {
                status.textContent = "Live updates connected.";
            }
        }).catch(error => {
            status.textContent = "Could not update live forecast subscriptions.";
            console.error(error);
        });

        return subscriptionQueue;
    }

    sportSelect.addEventListener("change", async () => {
        teamSelect.value = "";
        setOptions(teamSelect, "Select a team", []);
        teamSelect.disabled = true;
        matchSelect.value = "";
        setOptions(matchSelect, "Select a match", []);
        matchSelect.disabled = true;
        void refreshCharts();
        void updateSubscription();
        if (!sportSelect.value) {
            return;
        }

        const query = new URLSearchParams({ sport: sportSelect.value });
        await loadOptions(`${root.dataset.teamsUrl}?${query}`, teamSelect, "Select a team", true);
    });

    teamSelect.addEventListener("change", async () => {
        matchSelect.value = "";
        setOptions(matchSelect, "Select a match", []);
        matchSelect.disabled = !teamSelect.value;
        void refreshCharts();
        void updateSubscription();
        if (!teamSelect.value) {
            return;
        }

        const query = new URLSearchParams({
            sport: sportSelect.value,
            teamId: teamSelect.value
        });
        await loadOptions(`${root.dataset.matchesUrl}?${query}`, matchSelect, "Select a match", true);
    });

    matchSelect.addEventListener("change", () => {
        void refreshCharts();
        void updateSubscription();
    });

    dateInput.addEventListener("change", () => {
        if (!dateInput.value) {
            dateInput.value = new Date().toISOString().slice(0, 10);
        }
        void refreshCharts();
        void updateSubscription();
    });

    const initialData = JSON.parse(document.getElementById("forecast-chart-data").textContent);
    if (matchSelect.value) {
        setChartState(true);
        render(initialData);
        status.textContent = isLiveSelection()
            ? "Live updates connecting…"
            : "Past date: charts are static.";
    } else {
        setChartState(false);
    }
    void updateSubscription();
})();
