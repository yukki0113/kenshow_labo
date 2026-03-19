let SQL = null;
let db = null;
let currentResultCache = new Map();
let activeTabKey = null;

const STAT_DEFS = [
  { key: "jockey", label: "騎手別", groupExpr: "COALESCE(NULLIF(TRIM(jockey_name), ''), '(不明)' )", orderBy: "firsts DESC, seconds DESC, thirds DESC, starts DESC, key_name ASC" },
  { key: "sire", label: "父別", groupExpr: "COALESCE(NULLIF(TRIM(sire_name), ''), '(不明)' )", orderBy: "firsts DESC, seconds DESC, thirds DESC, starts DESC, key_name ASC" },
  { key: "style", label: "脚質別", groupExpr: "COALESCE(NULLIF(TRIM(running_style), ''), '(不明)' )", orderBy: "firsts DESC, seconds DESC, thirds DESC, starts DESC, key_name ASC" },
  { key: "wakuban", label: "枠番別", groupExpr: "COALESCE(CAST(wakuban AS TEXT), '(不明)')", orderBy: "CAST(key_name AS INTEGER) ASC" },
  { key: "age", label: "年齢別", groupExpr: "COALESCE(CAST(age AS TEXT), '(不明)')", orderBy: "CAST(key_name AS INTEGER) ASC" },
  { key: "sex", label: "性別", groupExpr: "COALESCE(NULLIF(TRIM(sex), ''), '(不明)' )", orderBy: "key_name ASC" },
  { key: "popularity", label: "人気別", groupExpr: "COALESCE(CAST(popularity AS TEXT), '(不明)')", orderBy: "CAST(key_name AS INTEGER) ASC" },
  { key: "weight_carried", label: "斤量別", groupExpr: buildBucketExpr("weight_carried", [48, 50, 52, 54, 56, 58, 60], "kg"), orderBy: "key_name ASC" },
  { key: "horse_weight", label: "馬体重別", groupExpr: buildBucketExpr("horse_weight", [400, 420, 440, 460, 480, 500, 520], "kg"), orderBy: "key_name ASC" },
  { key: "prev_class", label: "前走クラス別", groupExpr: "COALESCE(NULLIF(TRIM(prev_class), ''), '(不明)' )", orderBy: "firsts DESC, seconds DESC, thirds DESC, starts DESC, key_name ASC" },
  { key: "prev_distance", label: "前走距離別", groupExpr: "COALESCE(CAST(prev_distance_m AS TEXT), '(不明)')", orderBy: "CAST(key_name AS INTEGER) ASC" },
  { key: "distance_change", label: "距離変化別", groupExpr: "COALESCE(NULLIF(TRIM(distance_change), ''), '(不明)' )", orderBy: "key_name ASC" }
];

function buildBucketExpr(columnName, thresholds, unitText) {
  const cases = [];
  let previous = null;
  for (const threshold of thresholds) {
    if (previous === null) {
      cases.push(`WHEN ${columnName} < ${threshold} THEN '<${threshold}${unitText}'`);
    } else {
      cases.push(`WHEN ${columnName} >= ${previous} AND ${columnName} < ${threshold} THEN '${previous}-${threshold - 1}${unitText}'`);
    }
    previous = threshold;
  }
  if (previous !== null) {
    cases.push(`WHEN ${columnName} >= ${previous} THEN '${previous}${unitText}+'`);
  }
  return `CASE WHEN ${columnName} IS NULL THEN '(不明)' ${cases.join(" ")} ELSE '(不明)' END`;
}

function byId(id) {
  return document.getElementById(id);
}

function setText(id, value) {
  byId(id).textContent = value;
}

function formatInt(value) {
  return Number(value || 0).toLocaleString("ja-JP");
}

function formatFloat(value) {
  return Number(value || 0).toFixed(1);
}

function normalizeCsv(value) {
  return value
    .split(",")
    .map(function (part) { return part.trim(); })
    .filter(function (part) { return part.length > 0; });
}

async function initializeSqlJs() {
  SQL = await initSqlJs({
    locateFile: function (fileName) {
      return `https://cdn.jsdelivr.net/npm/sql.js@1.10.3/dist/${fileName}`;
    }
  });
}

function execQuery(sql, params) {
  const statement = db.prepare(sql);
  statement.bind(params || []);
  const rows = [];
  while (statement.step()) {
    rows.push(statement.getAsObject());
  }
  statement.free();
  return rows;
}

function getRequiredTableNames() {
  return ["fact_condition_stats_base"];
}

function checkRequiredTables() {
  const sql = "SELECT name FROM sqlite_master WHERE type = 'table' AND name IN (?)";
  const names = getRequiredTableNames();
  if (names.length === 1) {
    const rows = execQuery(sql, [names[0]]);
    return rows.map(function (row) { return row.name; });
  }
  return [];
}

function renderCounts(rows) {
  if (rows.length === 0) {
    byId("counts-output").textContent = "件数確認対象テーブルが見つかりません。";
    return;
  }

  const lines = [];
  for (const row of rows) {
    lines.push(`${row.table_name}: ${formatInt(row.row_count)}件`);
  }
  byId("counts-output").textContent = lines.join("\n");
}

function buildCountSql() {
  return `
    SELECT 'fact_condition_stats_base' AS table_name, COUNT(*) AS row_count
    FROM fact_condition_stats_base
  `;
}

function readFilters() {
  return {
    region: byId("f-region").value,
    dateFrom: byId("f-date-from").value,
    dateTo: byId("f-date-to").value,
    jyoCodes: normalizeCsv(byId("f-jyo").value),
    gradeCd: byId("f-grade").value.trim(),
    surface: byId("f-surface").value,
    distanceFrom: byId("f-distance-from").value,
    distanceTo: byId("f-distance-to").value,
    babaText: byId("f-baba").value.trim(),
    raceNameLike: byId("f-race-name").value.trim(),
    win5Flg: byId("f-win5").value,
    minStarts: Number(byId("f-min-starts").value || 1)
  };
}

function buildWhereClause(filters) {
  const clauses = [];
  const params = [];

  if (filters.region && filters.region !== "ALL") {
    clauses.push("AND region = ?");
    params.push(filters.region);
  }

  if (filters.dateFrom) {
    clauses.push("AND race_date >= ?");
    params.push(filters.dateFrom);
  }

  if (filters.dateTo) {
    clauses.push("AND race_date <= ?");
    params.push(filters.dateTo);
  }

  if (filters.jyoCodes.length > 0) {
    clauses.push(`AND jyo_cd IN (${filters.jyoCodes.map(function () { return "?"; }).join(",")})`);
    params.push.apply(params, filters.jyoCodes);
  }

  if (filters.gradeCd) {
    clauses.push("AND grade_cd = ?");
    params.push(filters.gradeCd);
  }

  if (filters.surface) {
    clauses.push("AND surface = ?");
    params.push(filters.surface);
  }

  if (filters.distanceFrom) {
    clauses.push("AND distance_m >= ?");
    params.push(Number(filters.distanceFrom));
  }

  if (filters.distanceTo) {
    clauses.push("AND distance_m <= ?");
    params.push(Number(filters.distanceTo));
  }

  if (filters.babaText) {
    clauses.push("AND baba_text = ?");
    params.push(filters.babaText);
  }

  if (filters.raceNameLike) {
    clauses.push("AND race_name LIKE ?");
    params.push(`%${filters.raceNameLike}%`);
  }

  if (filters.win5Flg !== "") {
    clauses.push("AND win5_flg = ?");
    params.push(Number(filters.win5Flg));
  }

  return {
    sql: clauses.join("\n  "),
    params: params
  };
}

function buildAppliedFilterText(filters) {
  const pieces = [];
  pieces.push(`地域=${filters.region}`);
  if (filters.dateFrom || filters.dateTo) {
    pieces.push(`期間=${filters.dateFrom || "-"}〜${filters.dateTo || "-"}`);
  }
  if (filters.jyoCodes.length > 0) {
    pieces.push(`場=${filters.jyoCodes.join(",")}`);
  }
  if (filters.gradeCd) {
    pieces.push(`グレード=${filters.gradeCd}`);
  }
  if (filters.surface) {
    pieces.push(`芝ダ=${filters.surface}`);
  }
  if (filters.distanceFrom || filters.distanceTo) {
    pieces.push(`距離=${filters.distanceFrom || "-"}〜${filters.distanceTo || "-"}`);
  }
  if (filters.babaText) {
    pieces.push(`馬場=${filters.babaText}`);
  }
  if (filters.raceNameLike) {
    pieces.push(`レース名=%${filters.raceNameLike}%`);
  }
  if (filters.win5Flg !== "") {
    pieces.push(`WIN5=${filters.win5Flg === "1" ? "はい" : "いいえ"}`);
  }
  pieces.push(`最低出走数=${filters.minStarts}`);
  return pieces.join(" / ");
}

function buildSummarySql(whereSql) {
  return `
    SELECT
      COUNT(*) AS starts,
      COUNT(DISTINCT race_id) AS races,
      COUNT(DISTINCT horse_id) AS horses
    FROM fact_condition_stats_base
    WHERE 1 = 1
      ${whereSql}
  `;
}

function buildStatSql(statDef, whereSql) {
  return `
    SELECT
      ${statDef.groupExpr} AS key_name,
      COUNT(*) AS starts,
      SUM(CASE WHEN finish_pos = 1 THEN 1 ELSE 0 END) AS firsts,
      SUM(CASE WHEN finish_pos = 2 THEN 1 ELSE 0 END) AS seconds,
      SUM(CASE WHEN finish_pos = 3 THEN 1 ELSE 0 END) AS thirds,
      SUM(CASE WHEN IFNULL(finish_pos, 99) >= 4 THEN 1 ELSE 0 END) AS others,
      ROUND(SUM(CASE WHEN finish_pos = 1 THEN 1.0 ELSE 0 END) * 100.0 / COUNT(*), 1) AS win_rate,
      ROUND(SUM(CASE WHEN finish_pos IN (1,2,3) THEN 1.0 ELSE 0 END) * 100.0 / COUNT(*), 1) AS place_rate,
      ROUND(SUM(COALESCE(payout_win_yen, 0)) * 1.0 / COUNT(*), 1) AS win_return_rate,
      ROUND(SUM(COALESCE(payout_place_yen, 0)) * 1.0 / COUNT(*), 1) AS place_return_rate
    FROM fact_condition_stats_base
    WHERE 1 = 1
      ${whereSql}
    GROUP BY ${statDef.groupExpr}
    HAVING COUNT(*) >= ?
    ORDER BY ${statDef.orderBy}
  `;
}

function renderTabs() {
  const container = byId("stat-tabs");
  container.innerHTML = "";

  for (const statDef of STAT_DEFS) {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = statDef.label;
    button.dataset.key = statDef.key;
    if (statDef.key === activeTabKey) {
      button.classList.add("active");
    }
    button.addEventListener("click", function () {
      activeTabKey = statDef.key;
      renderTabs();
      renderActiveTable();
    });
    container.appendChild(button);
  }
}

function renderActiveTable() {
  const target = byId("stat-result");
  if (!activeTabKey || !currentResultCache.has(activeTabKey)) {
    target.innerHTML = "<div class='muted'>集計結果がありません。</div>";
    return;
  }

  const rows = currentResultCache.get(activeTabKey);
  if (rows.length === 0) {
    target.innerHTML = "<div class='muted'>該当データがありません。</div>";
    return;
  }

  const html = [];
  html.push("<table>");
  html.push("<thead><tr>");
  html.push("<th>項目</th>");
  html.push("<th>出走数</th>");
  html.push("<th>1着</th>");
  html.push("<th>2着</th>");
  html.push("<th>3着</th>");
  html.push("<th>着外</th>");
  html.push("<th>勝率</th>");
  html.push("<th>複勝率</th>");
  html.push("<th>単回収率</th>");
  html.push("<th>複回収率</th>");
  html.push("</tr></thead>");
  html.push("<tbody>");

  for (const row of rows) {
    html.push("<tr>");
    html.push(`<td>${escapeHtml(row.key_name)}</td>`);
    html.push(`<td>${formatInt(row.starts)}</td>`);
    html.push(`<td>${formatInt(row.firsts)}</td>`);
    html.push(`<td>${formatInt(row.seconds)}</td>`);
    html.push(`<td>${formatInt(row.thirds)}</td>`);
    html.push(`<td>${formatInt(row.others)}</td>`);
    html.push(`<td>${formatFloat(row.win_rate)}%</td>`);
    html.push(`<td>${formatFloat(row.place_rate)}%</td>`);
    html.push(`<td>${formatFloat(row.win_return_rate)}%</td>`);
    html.push(`<td>${formatFloat(row.place_return_rate)}%</td>`);
    html.push("</tr>");
  }

  html.push("</tbody></table>");
  target.innerHTML = html.join("");
}

function escapeHtml(value) {
  return String(value == null ? "" : value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

async function onLoadDb() {
  const fileInput = byId("sqlite-file");
  const file = fileInput.files[0];
  if (!file) {
    setText("db-status", "SQLiteファイルを選択してください。");
    return;
  }

  setText("db-status", "読み込み中...");
  const arrayBuffer = await file.arrayBuffer();
  const uint8Array = new Uint8Array(arrayBuffer);
  db = new SQL.Database(uint8Array);

  const tables = checkRequiredTables();
  if (tables.length === 0) {
    setText("db-status", "読込完了。ただし fact_condition_stats_base が見つかりません。SQLite生成側が未対応の可能性があります。");
    byId("btn-counts").disabled = true;
    byId("btn-run").disabled = true;
    return;
  }

  setText("db-status", `読込完了: ${file.name}`);
  byId("btn-counts").disabled = false;
  byId("btn-run").disabled = false;
}

function onCountTables() {
  const rows = execQuery(buildCountSql(), []);
  renderCounts(rows);
}

function onRunStats() {
  try {
    const filters = readFilters();
    const where = buildWhereClause(filters);

    const summarySql = buildSummarySql(where.sql);
    const summaryRows = execQuery(summarySql, where.params);
    const summary = summaryRows[0] || { starts: 0, races: 0, horses: 0 };

    setText("summary-starts", formatInt(summary.starts));
    setText("summary-races", formatInt(summary.races));
    setText("summary-horses", formatInt(summary.horses));
    setText("applied-filters", buildAppliedFilterText(filters));

    currentResultCache = new Map();

    for (const statDef of STAT_DEFS) {
      const sql = buildStatSql(statDef, where.sql);
      const params = where.params.slice();
      params.push(filters.minStarts);
      const rows = execQuery(sql, params);
      currentResultCache.set(statDef.key, rows);
    }

    if (!activeTabKey) {
      activeTabKey = STAT_DEFS[0].key;
    }

    renderTabs();
    renderActiveTable();
    setText("run-status", "集計が完了しました。");
  } catch (error) {
    console.error(error);
    byId("stat-result").innerHTML = `<div class="notice">集計中にエラーが発生しました。<br>${escapeHtml(error.message || String(error))}</div>`;
    setText("run-status", "集計エラー");
  }
}

async function main() {
  await initializeSqlJs();

  byId("sqlite-file").addEventListener("change", function () {
    byId("btn-load").disabled = !byId("sqlite-file").files[0];
  });
  byId("btn-load").addEventListener("click", onLoadDb);
  byId("btn-counts").addEventListener("click", onCountTables);
  byId("btn-run").addEventListener("click", onRunStats);

  renderTabs();
}

main().catch(function (error) {
  console.error(error);
  setText("db-status", `初期化エラー: ${error.message || error}`);
});
