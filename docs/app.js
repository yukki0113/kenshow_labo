(function () {
  console.log("検証ラボ: app.js 初期化");

  function setupSampleButton() {
    var button = document.getElementById("load-sample-button");
    var output = document.getElementById("sample-output");

    if (!button || !output) {
      console.warn("検証ラボ: サンプル表示用の要素が見つかりませんでした。");
      return;
    }

    button.addEventListener("click", function () {
      output.textContent = "読み込み中...";

      // ここで JSON を読み込む
      fetch("./data/results/results_2023_derby_sample.json")
        .then(function (response) {
          if (!response.ok) {
            throw new Error("HTTP error " + response.status);
          }
          return response.json();
        })
        .then(function (rows) {
          // rows は 1レコード=1頭 の配列
          if (!Array.isArray(rows) || rows.length === 0) {
            output.textContent = "データがありません。";
            return;
          }

          output.innerHTML = "";

          // 先頭行からレース情報を取る（同じレースと仮定）
          var first = rows[0];

          var header = document.createElement("div");
          header.innerHTML =
            "<strong>" +
            first.raceName +
            "</strong> (" +
            first.date +
            " / " +
            first.course +
            " / " +
            first.grade +
            ")";
          output.appendChild(header);

          var list = document.createElement("ul");
          rows.forEach(function (r) {
            var li = document.createElement("li");
            li.textContent =
              r.finish +
              "着 " +
              r.horseName +
              "（オッズ: " +
              r.odds +
              "）";
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
