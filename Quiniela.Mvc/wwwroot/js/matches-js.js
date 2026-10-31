(() => {
    const endpoints = document.getElementById("matches-js-endpoints");
    const filters = document.getElementById("matches-js-filters");
    const grid = document.getElementById("matches-js-grid");
    const detail = document.getElementById("matches-js-detail");
    const status = document.getElementById("matches-js-status");
    const pageLabel = document.getElementById("matches-js-page");
    const previousButton = document.getElementById("matches-js-previous");
    const nextButton = document.getElementById("matches-js-next");
    const pageSizeInput = document.getElementById("matches-js-page-size");
    const tournamentInput = document.getElementById("matches-js-tournament");
    const teamInput = document.getElementById("matches-js-team");

    let pageNumber = 1;
    let totalPages = 0;
    let selectedMatchId = null;
    let matchesRequest;
    let detailRequest;

    const text = (value) => value == null || value === "" ? "—" : String(value);
    const dateTime = (value) => value ? new Date(value).toLocaleString() : "—";

    function cell(row, value) {
        const element = document.createElement("td");
        element.textContent = text(value);
        row.append(element);
    }

    function showGridMessage(message, className = "text-muted") {
        grid.replaceChildren();
        const row = document.createElement("tr");
        const messageCell = document.createElement("td");
        messageCell.colSpan = 7;
        messageCell.className = className;
        messageCell.textContent = message;
        row.append(messageCell);
        grid.append(row);
    }

    function renderMatches(result) {
        grid.replaceChildren();
        if (result.matches.length === 0) {
            showGridMessage("No matches found for these filters.");
            return;
        }

        for (const match of result.matches) {
            const row = document.createElement("tr");
            row.classList.toggle("table-primary", match.matchId === selectedMatchId);
            row.dataset.matchId = match.matchId;
            row.tabIndex = 0;
            row.setAttribute("role", "button");
            row.setAttribute("aria-label", `Show ${match.homeTeam.name} vs ${match.awayTeam.name}`);
            cell(row, `${match.homeTeam.name} vs ${match.awayTeam.name}`);
            cell(row, match.tournament?.name);
            cell(row, match.matchKind);
            cell(row, match.sport);
            cell(row, dateTime(match.playedAt));
            cell(row, match.status);
            cell(row, match.homeScore == null && match.awayScore == null
                ? "—"
                : `${text(match.homeScore)} – ${text(match.awayScore)}`);
            grid.append(row);
        }
    }

    function addDetailLine(container, label, value) {
        const line = document.createElement("p");
        line.className = "mb-1";
        const strong = document.createElement("strong");
        strong.textContent = `${label}: `;
        line.append(strong, document.createTextNode(text(value)));
        container.append(line);
    }

    function addLink(container, label, baseUrl, id, className = "btn btn-sm btn-outline-secondary") {
        const link = document.createElement("a");
        link.className = className;
        link.href = `${baseUrl}/${id}`;
        link.textContent = label;
        container.append(link);
    }

    function renderMatchDetail(match) {
        detail.replaceChildren();
        const columns = document.createElement("div");
        columns.className = "row g-4";
        const infoColumn = document.createElement("div");
        infoColumn.className = "col-lg-4";
        addDetailLine(infoColumn, "Match", `${match.homeTeam.name} vs ${match.awayTeam.name}`);
        addDetailLine(infoColumn, "Tournament", match.tournament?.name);
        addDetailLine(infoColumn, "Kind", match.matchKind);
        addDetailLine(infoColumn, "Sport", match.sport);
        addDetailLine(infoColumn, "Played at", dateTime(match.playedAt));
        addDetailLine(infoColumn, "Status", match.status);
        addDetailLine(infoColumn, "Score",
            match.homeScore == null && match.awayScore == null
                ? "—"
                : `${text(match.homeScore)} – ${text(match.awayScore)}`);

        const links = document.createElement("div");
        links.className = "d-flex flex-wrap gap-2 mt-3";
        addLink(links, "Details", endpoints.dataset.detailsUrl, match.matchId, "btn btn-sm btn-primary");
        addLink(links, "Edit", endpoints.dataset.editUrl, match.matchId);
        addLink(links, "Delete", endpoints.dataset.deleteUrl, match.matchId, "btn btn-sm btn-outline-danger");
        addLink(links, "JSON stats", endpoints.dataset.statsUrl, match.matchId);
        infoColumn.append(links);

        const forecastsColumn = document.createElement("div");
        forecastsColumn.className = "col-lg-8";
        const heading = document.createElement("h3");
        heading.className = "h6";
        heading.textContent = `Forecasts (${match.forecasts.length})`;
        forecastsColumn.append(heading);

        if (match.forecasts.length === 0) {
            const empty = document.createElement("p");
            empty.className = "text-muted";
            empty.textContent = "No forecasts have been submitted.";
            forecastsColumn.append(empty);
        } else {
            const table = document.createElement("table");
            table.className = "table table-sm table-striped";
            const header = document.createElement("thead");
            const headerRow = document.createElement("tr");
            for (const label of ["User id", "Predicted home score", "Predicted away score", "Submitted at"]) {
                const th = document.createElement("th");
                th.scope = "col";
                th.textContent = label;
                headerRow.append(th);
            }
            header.append(headerRow);
            const body = document.createElement("tbody");
            for (const forecast of match.forecasts) {
                const row = document.createElement("tr");
                cell(row, forecast.userId);
                cell(row, forecast.predictedHomeScore);
                cell(row, forecast.predictedAwayScore);
                cell(row, dateTime(forecast.submittedAt));
                body.append(row);
            }
            table.append(header, body);
            forecastsColumn.append(table);
        }

        columns.append(infoColumn, forecastsColumn);
        detail.append(columns);
    }

    async function refreshDetail(matchId) {
        selectedMatchId = matchId;
        detailRequest?.abort();
        detailRequest = new AbortController();
        const { signal } = detailRequest;
        detail.replaceChildren();
        const loading = document.createElement("p");
        loading.className = "text-muted mb-0";
        loading.textContent = "Loading match details…";
        detail.append(loading);

        try {
            const response = await fetch(`${endpoints.dataset.matchUrl}?id=${encodeURIComponent(matchId)}`, { signal });
            if (response.status === 404) {
                throw new Error("This match could not be found.");
            }
            if (!response.ok) {
                throw new Error(`Unable to load match details (HTTP ${response.status}).`);
            }

            renderMatchDetail(await response.json());
            for (const row of grid.querySelectorAll("tr[data-match-id]")) {
                row.classList.toggle("table-primary", Number(row.dataset.matchId) === selectedMatchId);
            }
        } catch (error) {
            if (error.name !== "AbortError") {
                detail.replaceChildren();
                const message = document.createElement("p");
                message.className = "text-danger mb-0";
                message.textContent = error.message || "Unable to load match details.";
                detail.append(message);
            }
        }
    }

    async function refreshMatches() {
        matchesRequest?.abort();
        matchesRequest = new AbortController();
        const { signal } = matchesRequest;
        status.textContent = "Refreshing matches…";
        showGridMessage("Loading matches…");

        const query = new URLSearchParams({
            pageNumber: String(pageNumber),
            pageSize: String(Math.max(1, Math.min(100, Number(pageSizeInput.value) || 10)))
        });
        if (tournamentInput.value) query.set("tournamentId", tournamentInput.value);
        if (teamInput.value) query.set("teamId", teamInput.value);

        try {
            const response = await fetch(`${endpoints.dataset.matchesUrl}?${query}`, { signal });
            if (!response.ok) {
                throw new Error(`Unable to refresh matches (HTTP ${response.status}).`);
            }

            const result = await response.json();
            totalPages = Math.ceil(result.totalCount / result.pageSize);
            pageNumber = result.pageNumber;
            status.textContent = `${result.totalCount} match${result.totalCount === 1 ? "" : "es"} found.`;
            pageLabel.textContent = `Page ${totalPages === 0 ? 0 : pageNumber} of ${totalPages}`;
            previousButton.disabled = pageNumber <= 1;
            nextButton.disabled = pageNumber >= totalPages;
            renderMatches(result);

            if (result.matches.length === 0) {
                selectedMatchId = null;
                detail.replaceChildren();
                const empty = document.createElement("p");
                empty.className = "text-muted mb-0";
                empty.textContent = "Select a match above to see its details and forecasts.";
                detail.append(empty);
            } else if (result.matches.some(match => match.matchId === selectedMatchId)) {
                await refreshDetail(selectedMatchId);
            } else {
                await refreshDetail(result.matches[0].matchId);
            }
        } catch (error) {
            if (error.name !== "AbortError") {
                status.textContent = "";
                showGridMessage(error.message || "Unable to refresh matches.", "text-danger");
            }
        }
    }

    filters.addEventListener("submit", event => {
        event.preventDefault();
        pageNumber = 1;
        refreshMatches();
    });

    document.getElementById("matches-js-refresh").addEventListener("click", refreshMatches);
    document.getElementById("matches-js-clear").addEventListener("click", () => {
        filters.reset();
        pageNumber = 1;
        refreshMatches();
    });

    previousButton.addEventListener("click", () => {
        if (pageNumber > 1) {
            pageNumber -= 1;
            refreshMatches();
        }
    });

    nextButton.addEventListener("click", () => {
        if (pageNumber < totalPages) {
            pageNumber += 1;
            refreshMatches();
        }
    });

    grid.addEventListener("click", event => {
        const row = event.target.closest("tr[data-match-id]");
        if (row) refreshDetail(Number(row.dataset.matchId));
    });

    grid.addEventListener("keydown", event => {
        const row = event.target.closest("tr[data-match-id]");
        if (row && (event.key === "Enter" || event.key === " ")) {
            event.preventDefault();
            refreshDetail(Number(row.dataset.matchId));
        }
    });

    refreshMatches();
})();
