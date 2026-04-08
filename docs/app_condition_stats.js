let SQL = null;
let db = null;
let currentResultCache = new Map();
let activeTabKey = null;
const UNKNOWN_LABEL = "(不明)";

const STAT_DEFS = [
  { key: "jockey", label: "騎手別", groupExpr: wrapUnknownTextExpr("jockey_name"), orderBy: buildResultDescOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "sire", label: "父別", groupExpr: wrapUnknownTextExpr("sire_name"), orderBy: buildResultDescOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "style", label: "脚質別", groupExpr: wrapUnknownTextExpr("running_style"), orderBy: buildResultDescOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "wakuban", label: "枠番別", groupExpr: wrapUnknownCastExpr("wakuban"), orderBy: "CASE WHEN key_name = '(不明)' THEN 999 ELSE CAST(key_name AS INTEGER) END ASC", unknownLabel: UNKNOWN_LABEL },
  { key: "age", label: "年齢別", groupExpr: wrapUnknownCastExpr("age"), orderBy: "CASE WHEN key_name = '(不明)' THEN 999 ELSE CAST(key_name AS INTEGER) END ASC", unknownLabel: UNKNOWN_LABEL },
  { key: "sex", label: "性別", groupExpr: wrapUnknownTextExpr("sex"), orderBy: "key_name ASC", unknownLabel: UNKNOWN_LABEL },
  { key: "popularity", label: "人気別", groupExpr: wrapUnknownCastExpr("popularity"), orderBy: "CASE WHEN key_name = '(不明)' THEN 9999 ELSE CAST(key_name AS INTEGER) END ASC", unknownLabel: UNKNOWN_LABEL },
  { key: "weight_carried", label: "斤量別", groupExpr: buildWeightCarriedExpr(), orderBy: buildWeightCarriedOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "horse_weight", label: "馬体重別", groupExpr: buildHorseWeightExpr(), orderBy: buildHorseWeightOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "prev_class", label: "前走クラス別", groupExpr: wrapUnknownTextExpr("prev_class"), orderBy: buildResultDescOrderBy(), unknownLabel: UNKNOWN_LABEL },
  { key: "prev_distance", label: "前走距離別", groupExpr: wrapUnknownCastExpr("prev_distance_m"), orderBy: "CASE WHEN key_name = '(不明)' THEN 99999 ELSE CAST(key_name AS INTEGER) END ASC", unknownLabel: UNKNOWN_LABEL },
  { key: "distance_change", label: "距離変化別", groupExpr: buildDistanceChangeExpr(), orderBy: buildDistanceChangeOrderBy(), unknownLabel: UNKNOWN_LABEL }
];

function wrapUnknownTextExpr(columnName) {
  return `COALESCE(NULLIF(TRIM(${columnName}), ''), '${UNKNOWN_LABEL}')`;
}

function wrapUnknownCastExpr(columnName) {
  return `COALESCE(CAST(${columnName} AS TEXT), '${UNKNOWN_LABEL}')`;
}

function buildResultDescOrderBy() {
  return "firsts DESC, seconds DESC, thirds DESC, starts DESC, key_name ASC";
}

function buildWeightCarriedExpr() {
  return `
    CASE
      WHEN weight_carried IS NULL THEN '${UNKNOWN_LABEL}'
      WHEN weight_carried <= 50.0 THEN '~50'
      WHEN weight_carried >= 60.0 THEN '60~'
      ELSE
        CASE
          WHEN CAST(weight_carried * 10 AS INTEGER) % 10 = 0
            THEN CAST(CAST(weight_carried AS INTEGER) AS TEXT)
          ELSE CAST(weight_carried AS TEXT)
        END
    END
  `.trim();
}

function buildWeightCarriedOrderBy() {
  return `
    CASE
      WHEN key_name = '${UNKNOWN_LABEL}' THEN 9999
      WHEN key_name = '~50' THEN 500
      WHEN key_name = '60~' THEN 600
      WHEN INSTR(key_name, '.') > 0 THEN CAST(REPLACE(key_name, '.', '') AS INTEGER)
      ELSE CAST(key_name AS INTEGER) * 10
    END ASC
  `.trim();
}

function buildHorseWeightExpr() {
  return `
    CASE
      WHEN horse_weight IS NULL THEN '${UNKNOWN_LABEL}'
      WHEN horse_weight <= 399 THEN '~399'
      WHEN horse_weight >= 560 THEN '560~'
      ELSE
        CAST((horse_weight / 20) * 20 AS TEXT) || '~' ||
        CAST(((horse_weight / 20) * 20) + 19 AS TEXT)
    END
  `.trim();
}

function buildHorseWeightOrderBy() {
  return `
    CASE
      WHEN key_name = '${UNKNOWN_LABEL}' THEN 999999
      WHEN key_name = '~399' THEN 399
      WHEN key_name = '560~' THEN 560
      ELSE CAST(SUBSTR(key_name, 1, INSTR(key_name, '~') - 1) AS INTEGER)
    END ASC
  `.trim();
}

function buildDistanceChangeExpr() {
  return wrapUnknownTextExpr("distance_change");
}

function buildDistanceChangeOrderBy() {
  return `
    CASE key_name
      WHEN '短縮' THEN 1
      WHEN '同距離' THEN 2
      WHEN '延長' THEN 3
      ELSE 9
    END ASC
  `.trim();
}

function byId(id) {
  return document.getElementById(id);
}

function setText(id, value) {
  byId(id).textContent = value;
}

function setHtml(id, value) {
  byId(id).innerHTML = value;
}

function formatInt(value) {
  return Number(value || 0).toLocaleString("ja-JP");
}

function formatFloat(value) {
  return Number(value || 0).toFixed(1);
}

function getFieldValue(id) {
  const el = byId(id);
  if (!el) {
    return "";
  }
  return String(el.value || "").trim();
}

function normalizeSingleJyoCode(value) {
  if (!value) {
    return "";
  }

  const first = value.split(",")[0].trim();
  if (first.length === 0) {
    return "";
  }

  return first.length === 1 ? `0${first}` : first;
}

function normalizeMinStarts(value) {
  const parsed = Number(value);
  if (!Number.isFinite(parsed) || parsed < 1) {
    return 1;
  }
  return Math.floor(parsed);
}

function setActionButtonsEnabled(enabled) {
  byId("btn-counts").disabled = !enabled;
  byId("btn-run").disabled = !enabled;
}

function resetSummary() {
  setText("summary-starts", "-");
  setText("summary-races", "-");
  setText("summary-horses", "-");
}

function applySummary(summary) {
  setText("summary-starts", formatInt(summary.starts));
  setText("summary-races", formatInt(summary.races));
  setText("summary-horses", formatInt(summary.horses));
}

function setAppliedFilters(text) {
  setText("applied-filters", text);
}

function setRunStatus(text) {
  setText("run-status", text);
}

function setResultMessage(message) {
  setHtml("stat-result", `<div class="muted">${escapeHtml(message)}</div>`);
}

function setResultError(error) {
  setHtml("stat-result", `<div class="notice">集計中にエラーが発生しました。<br>${escapeHtml(error.message || String(error))}</div>`);
}

function resetCountsOutput() {
  byId("counts-output").textContent = "";
}

function resetResultState(message) {
  currentResultCache = new Map();
  activeTabKey = null;
  renderTabs();
  setResultMessage(message);
}

function ensureActiveTabKey() {
  if (STAT_DEFS.length === 0) {
    activeTabKey = null;
    return null;
  }

  if (!activeTabKey || !currentResultCache.has(activeTabKey)) {
    activeTabKey = STAT_DEFS[0].key;
  }

  return activeTabKey;
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
  const names = getRequiredTableNames();
  if (names.length === 0) {
    return [];
  }

  const placeholders = names.map(function () { return "?"; }).join(", ");
  const sql = `
    SELECT name
    FROM sqlite_master
    WHERE type = 'table'
      AND name IN (${placeholders})
    ORDER BY name
  `;
  const rows = execQuery(sql, names);
  return rows.map(function (row) { return row.name; });
}

function getMissingRequiredTables(foundTableNames) {
  const found = new Set(foundTableNames);
  return getRequiredTableNames().filter(function (name) {
    return !found.has(name);
  });
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
    dateFrom: getFieldValue("f-date-from"),
    dateTo: getFieldValue("f-date-to"),
    jyoCd: normalizeSingleJyoCode(getFieldValue("f-jyo")),
    gradeCd: getFieldValue("f-grade"),
    surface: getFieldValue("f-surface"),
    distanceFrom: getFieldValue("f-distance-from"),
    distanceTo: getFieldValue("f-distance-to"),
    babaText: getFieldValue("f-baba"),
    raceNameLike: getFieldValue("f-race-name"),
    win5Flg: getFieldValue("f-win5"),
    minStarts: normalizeMinStarts(getFieldValue("f-min-starts") || "1")
  };
}

function buildWhereClause(filters) {
  const clauses = [];
  const params = [];

  if (filters.dateFrom) {
    clauses.push("AND race_date >= ?");
    params.push(filters.dateFrom);
  }

  if (filters.dateTo) {
    clauses.push("AND race_date <= ?");
    params.push(filters.dateTo);
  }

  if (filters.jyoCd) {
    clauses.push("AND jyo_cd = ?");
    params.push(filters.jyoCd);
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
  if (filters.dateFrom || filters.dateTo) {
    pieces.push(`期間=${filters.dateFrom || "-"}〜${filters.dateTo || "-"}`);
  }
  if (filters.jyoCd) {
    pieces.push(`場=${filters.jyoCd}`);
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
      COUNT(DISTINCT COALESCE(horse_id, race_id || ':' || CAST(umaban AS TEXT))) AS horses
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
    setResultMessage("集計結果がありません。");
    return;
  }

  const rows = currentResultCache.get(activeTabKey);
  if (rows.length === 0) {
    setResultMessage("該当データがありません。");
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

  try {
    setActionButtonsEnabled(false);
    setText("db-status", "読み込み中...");
    setRunStatus("");
    resetCountsOutput();
    resetSummary();
    setAppliedFilters("条件を指定して集計してください。");
    resetResultState("まだ集計されていません。");

    const arrayBuffer = await file.arrayBuffer();
    const uint8Array = new Uint8Array(arrayBuffer);
    db = new SQL.Database(uint8Array);

    const tables = checkRequiredTables();
    const missingTables = getMissingRequiredTables(tables);
    if (missingTables.length > 0) {
      db = null;
      setText("db-status", `読込完了。ただし必須テーブルが不足しています: ${missingTables.join(", ")}`);
      setActionButtonsEnabled(false);
      resetResultState("必要テーブルが見つかりません。");
      return;
    }

    setText("db-status", `読込完了: ${file.name}`);
    setActionButtonsEnabled(true);
  } catch (error) {
    console.error(error);
    db = null;
    setActionButtonsEnabled(false);
    resetResultState("SQLiteの読込に失敗しました。");
    setText("db-status", `読込エラー: ${error.message || error}`);
  }
}

function onCountTables() {
  if (!db) {
    byId("counts-output").textContent = "先にSQLiteを読み込んでください。";
    return;
  }

  const rows = execQuery(buildCountSql(), []);
  renderCounts(rows);
}

function onSelectSqliteFile() {
  const hasFile = Boolean(byId("sqlite-file").files[0]);
  byId("btn-load").disabled = !hasFile;

  if (hasFile) {
    return;
  }

  db = null;
  setActionButtonsEnabled(false);
  setText("db-status", "SQLiteファイルを選択してください。");
  setRunStatus("");
  resetCountsOutput();
  resetSummary();
  setAppliedFilters("条件を指定して集計してください。");
  resetResultState("まだ集計されていません。");
}

function onRunStats() {
  try {
    if (!db) {
      setRunStatus("SQLite未読込");
      setResultMessage("先にSQLiteを読み込んでください。");
      return;
    }

    setRunStatus("集計中...");
    const filters = readFilters();
    const where = buildWhereClause(filters);

    const summarySql = buildSummarySql(where.sql);
    const summaryRows = execQuery(summarySql, where.params);
    const summary = summaryRows[0] || { starts: 0, races: 0, horses: 0 };

    applySummary(summary);
    setAppliedFilters(buildAppliedFilterText(filters));

    currentResultCache = new Map();

    for (const statDef of STAT_DEFS) {
      const sql = buildStatSql(statDef, where.sql);
      const params = where.params.slice();
      params.push(filters.minStarts);
      const rows = execQuery(sql, params);
      currentResultCache.set(statDef.key, rows);
    }

    ensureActiveTabKey();
    renderTabs();
    renderActiveTable();
    setRunStatus("集計が完了しました。");
  } catch (error) {
    console.error(error);
    setResultError(error);
    setRunStatus("集計エラー");
  }
}

async function main() {
  await initializeSqlJs();

  setActionButtonsEnabled(false);
  setRunStatus("");
  resetCountsOutput();
  resetSummary();
  setAppliedFilters("条件を指定して集計してください。");
  resetResultState("まだ集計されていません。");

  byId("sqlite-file").addEventListener("change", onSelectSqliteFile);
  byId("btn-load").addEventListener("click", onLoadDb);
  byId("btn-counts").addEventListener("click", onCountTables);
  byId("btn-run").addEventListener("click", onRunStats);
}

function bootstrap() {
  main().catch(function (error) {
    console.error(error);
    setText("db-status", `初期化エラー: ${error.message || error}`);
  });
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", bootstrap, { once: true });
} else {
  bootstrap();
}
