(function () {
  console.log("検証ラボ: app.js 初期化");

  function formatCourse(course) {
    if (!course) {
      return "";
    }

    var track = course.track || "";
    var surface = course.surface || "";
    var distance = course.distance != null ? String(course.distance) : "";
    var turn = course.turn || "";

    var text = track + " " + surface + distance + "m";
    if (turn) {
      text += " " + turn;
    }

    return text;
  }

  function setupSampleButton() {
    var button = document.getElementById("load-sample-button");
    var output = document.getElementById("sample-output");

    if (!button || !output) {
      console.warn("検証ラボ: サンプル表示用の要素が見つかりませんでした。");
      return;
    }

    button.addEventListener("click", function () {
      output.textContent = "読み込み中...";

      fetch("./data/results/results_2024_sample.json")
        .then(function (response) {
          if (!response.ok) {
            throw new Error("HTTP error " + response.status);
          }
          return response.json();
        })
        .then(function (races) {
          if (!Array.isArray(races) || races.length === 0) {
            output.textContent = "データがありません。";
            return;
          }

          // まずは先頭レースを表示（後でセレクト等に拡張できます）
          var race = races[0];

          output.innerHTML = "";

          var header = document.createElement("div");
          var courseText = formatCourse(race.course);
          var gradeText = race.grade != null ? race.grade : "";
          header.innerHTML =
            "<strong>" +
            race.raceName +
            "</strong> (" +
            race.date +
            " / " +
            courseText +
            " / " +
            gradeText +
            ")";
          output.appendChild(header);

          var entries = Array.isArray(race.entries) ? race.entries : [];
          if (entries.length === 0) {
            var empty = document.createElement("div");
            empty.textContent = "出走馬データがありません。";
            output.appendChild(empty);
            return;
          }

          // 着順が数値なら着順順に並べ替え（任意）
          entries.sort(function (a, b) {
            var af = a.finish != null ? Number(a.finish) : 9999;
            var bf = b.finish != null ? Number(b.finish) : 9999;
            return af - bf;
          });

          var list = document.createElement("ul");
          entries.forEach(function (e) {
            var li = document.createElement("li");
            li.textContent =
              e.finish + "着 " + e.horseName + "（オッズ: " + e.odds + "）";
            list.appendChild(li);
          });

          output.appendChild(list);
        })
        .catch(function (error) {
          console.error(error);
          output.textContent = "読み込みエラー: " + error;
        });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    setupSampleButton();
  });
})();
