const CONFIG = {
  region: "us-east-1",
  poolId: "us-east-1:a4f3a55a-e219-49c8-aa6a-a12c6b9098b6",
  roleArn: "arn:aws:iam::855406835171:role/onemore-telemetry-unauth",
  outputBucket: "s3://onemore-telemetry-logs/athena-results/"
};

let athena;
let allCommands = [];
let unusedCommandsTimer = null;

function initAWS() {
  AWS.config.region = CONFIG.region;

  const creds = new AWS.CognitoIdentityCredentials({
    IdentityPoolId: CONFIG.poolId,
    RoleArn: CONFIG.roleArn
  });

  creds.clearCachedId();
  creds.get(function (err) {
    if (err) {
      document.getElementById("subtitle").innerHTML = `<b>Error loading AWS credentials</b> ${err}`;
      return;
    }

    AWS.config.credentials = creds;
    athena = new AWS.Athena({ region: CONFIG.region, credentials: creds });
    loadManifest();
  });
}

function loadManifest() {
  Promise.all([
    fetch("/telemetry/manifest.json").then(r => r.json()),
    fetch("/telemetry/commands.json").then(r => r.json())
  ]).then(([manifest, commandsData]) => {
    allCommands = commandsData.commands;
    unusedCommandsTimer = startProgress(document.getElementById("unused-commands"));
    loadSubtitle(manifest);
    loadReport(manifest);
  });
}

async function loadSubtitle(manifest) {
  const queryId = manifest["ReportStartTime"]?.id;
  if (!queryId) return;

  const el = document.getElementById("subtitle");
  const timer = startProgress(el);

  try {
    const named = await athena.getNamedQuery({ NamedQueryId: queryId }).promise();
    const rows = await runQuery(named.NamedQuery.QueryString, named.NamedQuery.Database);

    clearInterval(timer);
    if (rows.length > 1 && rows[1].Data.length > 0) {
      const date = new Date(rows[1].Data[0].VarCharValue);
      const dateStr = date.toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" });
      const timeStr = date.toLocaleTimeString("en-US", { hour: "numeric", minute: "2-digit", hour12: true });
      el.textContent = `Reporting since first event on ${dateStr} at ${timeStr}`;
    } else {
      el.textContent = "";
    }
  } catch (err) {
    clearInterval(timer);
    el.textContent = "";
    console.error("[subtitle] failed:", err);
  }
}

function loadReport(manifest) {
  const sections = Object.entries(manifest)
    .filter(([, v]) => v.target)
    .map(([key, v]) => ({ key, ...v }));
  Promise.all(sections.map(section => loadSection(section)));
}

async function loadSection({ key, id, target, heatCol }) {
  const el = document.getElementById(target);
  if (!id || !el) return;

  const timer = startProgress(el);
  try {
    const named = await athena.getNamedQuery({ NamedQueryId: id }).promise();
    const rows = await runQuery(named.NamedQuery.QueryString, named.NamedQuery.Database);
    clearInterval(timer);
    el.innerHTML = renderTable(rows);
    if (key !== "ReportEventTypeCounts") makeSortable(el.querySelector("table"));
    if (heatCol) applyHeatmap(el.querySelector("table"), heatCol);
    if (key === "ReportEventTypeCounts") applyEventTypeColors(el.querySelector("table"));
    if (key === "ReportCommandCounts") renderUnusedCommands(rows);
  } catch (err) {
    clearInterval(timer);
    el.textContent = "Failed to load.";
    if (key === "ReportCommandCounts") {
      clearInterval(unusedCommandsTimer);
      document.getElementById("unused-commands").textContent = "Failed to load.";
    }
    console.error(`[${key}] failed:`, err);
  }
}

function startProgress(el) {
  const frames = ["○○○", "●○○", "●●○", "●●●"];
  let frame = 0;
  el.textContent = frames[0];
  return setInterval(() => {
    frame = (frame + 1) % frames.length;
    el.textContent = frames[frame];
  }, 350);
}

function runQuery(sql, database) {
  return new Promise((resolve, reject) => {
    athena.startQueryExecution(
      {
        QueryString: sql,
        QueryExecutionContext: { Database: database, Catalog: "AwsDataCatalog" },
        WorkGroup: "primary",
        ResultConfiguration: { OutputLocation: CONFIG.outputBucket }
      },
      function (err, data) {
        if (err) return reject(err);
        pollQueryStatus(data.QueryExecutionId, resolve, reject);
      }
    );
  });
}

function pollQueryStatus(queryId, resolve, reject) {
  athena.getQueryExecution({ QueryExecutionId: queryId }, function (err, execData) {
    if (err) return reject(err);

    const status = execData.QueryExecution.Status;
    if (status.State === "SUCCEEDED") {
      athena.getQueryResults({ QueryExecutionId: queryId }, function (err, results) {
        if (err) return reject(err);
        resolve(results.ResultSet.Rows);
      });
    } else if (status.State === "FAILED" || status.State === "CANCELLED") {
      reject(new Error(status.StateChangeReason || "Query failed"));
    } else {
      setTimeout(() => pollQueryStatus(queryId, resolve, reject), 1000);
    }
  });
}

function renderTable(rows) {
  const headers = rows[0].Data.map(col => col.VarCharValue);
  const pctIndices = new Set(
    headers.map((h, i) => h.trim() === "Pct" ? i : -1).filter(i => i !== -1)
  );

  let html = "<table><thead><tr>";
  headers.forEach(h => html += `<th>${h}</th>`);
  html += "</tr></thead><tbody>";
  rows.slice(1).forEach(row => {
    const isTotal = row.Data[0]?.VarCharValue?.toLowerCase().includes("total");
    html += isTotal ? '<tr class="grand-total">' : "<tr>";
    row.Data.forEach((cell, i) => {
      const val = cell.VarCharValue || "";
      const display = pctIndices.has(i) && val !== ""
        ? `${parseFloat(val).toFixed(1)}%`
        : val;
      html += `<td>${display}</td>`;
    });
    html += "</tr>";
  });
  return html + "</tbody></table>";
}

function makeSortable(tableEl) {
  if (!tableEl) return;
  const headerRow = tableEl.querySelector("thead tr");
  if (!headerRow) return;
  Array.from(headerRow.cells).forEach((th, colIdx) => {
    if (th.textContent.trim() === "Pct") return;
    th.classList.add("sortable");
    th.dataset.sort = "";
    th.addEventListener("click", () => sortTable(tableEl, colIdx, th));
  });
}

function sortTable(tableEl, colIdx, clickedTh) {
  const headerRow = tableEl.querySelector("thead tr");
  const newDir = clickedTh.dataset.sort === "asc" ? "desc" : "asc";

  Array.from(headerRow.cells).forEach(th => { th.dataset.sort = ""; });
  clickedTh.dataset.sort = newDir;

  const tbody = tableEl.querySelector("tbody");
  if (!tbody) return;
  const allRows = Array.from(tbody.querySelectorAll("tr"));
  const dataRows = allRows.filter(r => !r.classList.contains("grand-total"));
  const totalRows = allRows.filter(r => r.classList.contains("grand-total"));

  const sampleVal = dataRows[0]?.cells[colIdx]?.textContent.replace(/[%,]/g, "").trim() || "";
  const isNumeric = sampleVal !== "" && !isNaN(parseFloat(sampleVal));

  dataRows.sort((a, b) => {
    const aRaw = a.cells[colIdx]?.textContent.replace(/[%,]/g, "").trim() || "";
    const bRaw = b.cells[colIdx]?.textContent.replace(/[%,]/g, "").trim() || "";
    if (isNumeric) {
      return newDir === "asc"
        ? (parseFloat(aRaw) || 0) - (parseFloat(bRaw) || 0)
        : (parseFloat(bRaw) || 0) - (parseFloat(aRaw) || 0);
    }
    return newDir === "asc" ? aRaw.localeCompare(bRaw) : bRaw.localeCompare(aRaw);
  });

  [...dataRows, ...totalRows].forEach(r => tbody.appendChild(r));
}

function applyHeatmap(tableEl, colName) {
  const headers = Array.from(tableEl.querySelectorAll("th"));
  const colIndex = headers.findIndex(th => th.textContent.trim() === colName);
  if (colIndex === -1) return;

  const dataRows = Array.from(
    tableEl.querySelectorAll("tbody tr:not(.grand-total)")
  );
  const values = dataRows.map(row =>
    parseFloat(row.cells[colIndex]?.textContent.replace(/,/g, "")) || 0
  );

  const n = dataRows.length;
  const ranked = values.map((v, i) => ({ v, i })).sort((a, b) => b.v - a.v);
  const tByIndex = new Array(n);
  ranked.forEach(({ i }, rank) => {
    tByIndex[i] = n > 1 ? 1 - rank / (n - 1) : 1;
  });

  dataRows.forEach((row, i) => {
    const cell = row.cells[colIndex];
    if (cell) {
      const { bg, text } = heatColor(tByIndex[i]);
      cell.style.backgroundColor = bg;
      cell.style.color = text;
    }
  });
}

function heatColor(t) {
  // white (t=0) → light blue → medium blue → red (t=1)
  // red zone confined to top ~15% of rows (t >= 0.85)
  const stops = [
    { t: 0.00, r: 255, g: 255, b: 255 },
    { t: 0.40, r: 189, g: 215, b: 238 },
    { t: 0.75, r:  68, g: 114, b: 196 },
    { t: 0.85, r:  30, g:  78, b: 121 },
    { t: 1.00, r: 192, g:   0, b:   0 },
  ];

  let lo = stops[0], hi = stops[stops.length - 1];
  for (let i = 0; i < stops.length - 1; i++) {
    if (t <= stops[i + 1].t) { lo = stops[i]; hi = stops[i + 1]; break; }
  }

  const s = hi.t === lo.t ? 0 : (t - lo.t) / (hi.t - lo.t);
  let r = Math.round(lo.r + (hi.r - lo.r) * s);
  let g = Math.round(lo.g + (hi.g - lo.g) * s);
  let b = Math.round(lo.b + (hi.b - lo.b) * s);

  // Increase brightness 75%: blend toward white
  r = Math.round(r + (255 - r) * 0.75);
  g = Math.round(g + (255 - g) * 0.75);
  b = Math.round(b + (255 - b) * 0.75);

  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return {
    bg: `rgb(${r},${g},${b})`,
    text: luminance < 0.55 ? "#ffffff" : "#000000"
  };
}

function applyEventTypeColors(tableEl) {
  const dataRows = Array.from(tableEl.querySelectorAll("tbody tr:not(.grand-total)"));
  dataRows.forEach(row => {
    const label = row.cells[0]?.textContent.trim().toLowerCase() || "";
    const color = label.includes("error") ? "#fde8e8" : label.includes("event") ? "#e8f5e9" : null;
    if (color) Array.from(row.cells).forEach(cell => cell.style.backgroundColor = color);
  });
}

function renderUnusedCommands(commandRows) {
  clearInterval(unusedCommandsTimer);
  const el = document.getElementById("unused-commands");

  const used = new Set(
    commandRows.slice(1).map(row => row.Data[0].VarCharValue.toLowerCase())
  );
  const unused = allCommands
    .filter(cmd => !used.has(cmd.toLowerCase()))
    .sort((a, b) => a.localeCompare(b));

  if (unused.length === 0) {
    el.textContent = "All commands have been used.";
    return;
  }

  el.innerHTML = `<table><tr><th>Command</th></tr><tr><td>${unused.join("<br>")}</td></tr></table>`;
}

initAWS();
