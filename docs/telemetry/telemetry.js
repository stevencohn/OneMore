const CONFIG = {
  region: "us-east-1",
  poolId: "us-east-1:a4f3a55a-e219-49c8-aa6a-a12c6b9098b6",
  roleArn: "arn:aws:iam::855406835171:role/onemore-telemetry-unauth",
  outputBucket: "s3://onemore-telemetry-logs/athena-results/"
};

const REPORT_SECTIONS = [
  { key: "ReportOneMoreVersionCounts", target: "onemore-version" },
  { key: "ReportOneNoteVersionCounts", target: "onenote-version" },
  { key: "ReportOSCultureCounts",      target: "os-culture"      },
  { key: "ReportEventTypeCounts",      target: "event-type"      },
  { key: "ReportMoreCultureCounts",    target: "more-culture"    },
  { key: "ReportCommandCounts",        target: "command",         heatCol: "Count" },
  { key: "ReportOSVersionCounts",      target: "os-version"      },
];

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
  const queryId = manifest["ReportStartTime"];
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
  Promise.all(REPORT_SECTIONS.map(section => loadSection(manifest, section)));
}

async function loadSection(manifest, { key, target, heatCol }) {
  const queryId = manifest[key];
  const el = document.getElementById(target);
  if (!queryId || !el) return;

  const timer = startProgress(el);
  try {
    const named = await athena.getNamedQuery({ NamedQueryId: queryId }).promise();
    const rows = await runQuery(named.NamedQuery.QueryString, named.NamedQuery.Database);
    clearInterval(timer);
    el.innerHTML = renderTable(rows);
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
  let html = "<table><tr>";
  rows[0].Data.forEach(col => html += `<th>${col.VarCharValue}</th>`);
  html += "</tr>";
  rows.slice(1).forEach(row => {
    const isTotal = row.Data[0]?.VarCharValue?.toLowerCase() === "grand total";
    html += isTotal ? '<tr class="grand-total">' : "<tr>";
    row.Data.forEach(cell => html += `<td>${cell.VarCharValue || ""}</td>`);
    html += "</tr>";
  });
  return html + "</table>";
}

function applyHeatmap(tableEl, colName) {
  const headers = Array.from(tableEl.querySelectorAll("th"));
  const colIndex = headers.findIndex(th => th.textContent.trim() === colName);
  if (colIndex === -1) return;

  const dataRows = Array.from(
    tableEl.querySelectorAll("tr:not(:first-child):not(.grand-total)")
  );
  const values = dataRows.map(row =>
    parseFloat(row.cells[colIndex]?.textContent.replace(/,/g, "")) || 0
  );

  const max = Math.max(...values);
  const min = Math.min(...values);
  const range = max - min || 1;

  dataRows.forEach((row, i) => {
    const cell = row.cells[colIndex];
    if (cell) {
      const { bg, text } = heatColor((values[i] - min) / range);
      cell.style.backgroundColor = bg;
      cell.style.color = text;
    }
  });
}

function heatColor(t) {
  // white (low) → blue (mid) → red (high)
  let r, g, b;
  if (t <= 0.5) {
    const s = t * 2;
    r = Math.round(255 + (30 - 255) * s);
    g = Math.round(255 + (78 - 255) * s);
    b = Math.round(255 + (121 - 255) * s);
  } else {
    const s = (t - 0.5) * 2;
    r = Math.round(30 + (200 - 30) * s);
    g = Math.round(78 + (0 - 78) * s);
    b = Math.round(121 + (0 - 121) * s);
  }
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
  return {
    bg: `rgb(${r},${g},${b})`,
    text: luminance < 0.55 ? "#ffffff" : "#000000"
  };
}

function applyEventTypeColors(tableEl) {
  const dataRows = Array.from(tableEl.querySelectorAll("tr:not(:first-child):not(.grand-total)"));
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

  let html = "<table><tr><th>Command</th></tr>";
  unused.forEach(cmd => html += `<tr><td>${cmd}</td></tr>`);
  el.innerHTML = html + "</table>";
}

initAWS();
