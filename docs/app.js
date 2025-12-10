// 検証ラボ トップ画面用の最初のスクリプト
// まずは「ちゃんと JS が動いている」ことが確認できるレベルの内容にしています。

(function () {
  // ページ読み込み時に簡単なログを出す
  console.log("検証ラボ: app.js 初期化");

  // ダミーデータ（将来はここが JSON 読み込みに置き換わる）
  var sampleRaceResult = {
    raceId: "2023-derby-sample",
    raceName: "第90回 東京優駿（サンプル）",
    date: "2023-05-28",
    course: "東京 芝2400m",
    grade: "GI",
    horses: [
      { horseName: "サンプルホースA", finish: 1, odds: 2.4 },
      { horseName: "サンプルホースB", finish: 2, odds: 5.6 },
      { horseName: "サンプルホースC", finish: 3, odds: 12.1 }
    ]
  };

  // ボタンが押されたらダミーデータを画面に表示する
  function setupSampleButton() {
    var button = document.getElementById("load-sample-button");
    var output = document.getElementById("sample-output");

    if (!button || !output) {
      console.warn("検証ラボ: サンプル表示用の要素が見つかりませんでした。");
      return;
    }

    button.addEventListener("click", function () {
      // 将来は fetch("./data/results/results_2023.json") などで JSON を読み込むイメージ
      output.innerHTML = "";

      var header = document.createElement("div");
      header.innerHTML =
        "<strong>" +
        sampleRaceResult.raceName +
        "</strong> (" +
        sampleRaceResult.date +
        " / " +
        sampleRaceResult.course +
        " / " +
        sampleRaceResult.grade +
        ")";

      var list = document.createElement("ul");
      sampleRaceResult.horses.forEach(function (h) {
        var li = document.createElement("li");
        li.textContent =
          h.finish + "着 " + h.horseName + "（オッズ: " + h.odds + "）";
        list.appendChild(li);
      });

      output.appendChild(header);
      output.appendChild(list);
    });
  }

  // 初期化
  document.addEventListener("DOMContentLoaded", function () {
    setupSampleButton();
  });
})();
