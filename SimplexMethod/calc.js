function simplexSolve() {

    const result_div = document.getElementById("result");
    result_div.innerHTML = ""; //очищаем предыдущие результаты

    let xs = [];

    for (let i = 0; i < vars_count; i++) {

        xs.push(parseFloat(document.getElementById(`x${i + 1}`).value));
    }

    let sel_signs = []; //выбранные знаки ограничений 
    let free_terms = []; //свободные члены
    let rstrctns_mtrx = []; //матрица ограничений

    for (let i = 0; i < rstrctns_count; i++) {

        sel_signs.push(document.getElementById(`ss${i}`).value);
        free_terms.push(parseFloat(document.getElementById(`ft${i}`).value));

        let rstrctns_row = [];

        for (let j = 0; j < vars_count; j++) {

            rstrctns_row.push(parseFloat(document.getElementById(`${i}-${j}`).value));
        }

        rstrctns_mtrx.push(rstrctns_row);
    }

    let sel_mode = document.getElementById("sm");
    let mode = sel_mode.options[sel_mode.selectedIndex].value; //способ решения

    if (mode !== "max") {

        alert("Пока не работает");
        return;
    }

    if (sel_signs.every(s => s === "≤" || s === "=") && !free_terms.some(t => t < 0)) {

        let tableau = []; //симплекс таблица
        let slackCount = 0; //количество дополнительных переменных

        for (let i = 0; i < rstrctns_count; i++) {

            let row = rstrctns_mtrx[i].slice();

            if (sel_signs[i] === "≤") {

                for (let j = 0; j < rstrctns_count; j++) {

                    row.push(i === j ? 1 : 0);
                }

                slackCount++;
            } 
            else if (sel_signs[i] === "=") {

                for (let j = 0; j < rstrctns_count; j++) {

                    row.push(i === j ? 1 : 0);
                }

                slackCount++;
            }

            row.push(free_terms[i]);
            tableau.push(row);
        }

        let fRow = xs.concat(new Array(slackCount).fill(0)); //строка дельт

        fRow.push(0);
        fRow = fRow.map(x => -x);
        tableau.push(fRow);

        let step = 0;

        while (true) {

            addTable(tableau, step++, result_div);

            let last_row = tableau[tableau.length - 1];
            let min = Math.min(...last_row.slice(0, -1));

            if (min >= 0) break;

            let pivot_col = last_row.findIndex(v => v === min); //ведущий столбец
            let ratios = tableau.slice(0, -1).map(row => {

                let val = row[pivot_col];
                return val > 0 ? row[row.length - 1] / val : Infinity;
            }); //симплекс-соотношения

            let pivot_row = ratios.indexOf(Math.min(...ratios)); //ведущая строка

            if (pivot_row === -1 || ratios[pivot_row] === Infinity) {

                result_div.innerHTML += "<p>Решение не ограничено</p>";
                return;
            }

            let pivot_val = tableau[pivot_row][pivot_col];

            for (let i = 0; i < tableau[pivot_row].length; i++) {

                tableau[pivot_row][i] /= pivot_val;
            }

            for (let i = 0; i < tableau.length; i++) {

                if (i === pivot_row) continue;
                let factor = tableau[i][pivot_col]; //значение неведущей строки из ведущего столбца

                for (let j = 0; j < tableau[i].length; j++) {

                    tableau[i][j] -= factor * tableau[pivot_row][j];
                }
            }
        }

        addTable(tableau, step, result_div);
        let result_text = "<p><b>Оптимальное решение:</b><br>";

        for (let i = 0; i < vars_count; i++) {

            let col = tableau.map(row => row[i]);
            let one_index = col.findIndex(x => x === 1);

            if (one_index !== -1 && col.every((v, idx) => idx === one_index || v === 0)) {

                result_text += `x${i + 1} = ${tableau[one_index][tableau[0].length - 1]}<br>`;
            } 
            else {

                result_text += `x${i + 1} = 0<br>`;
            }
        }

        result_text += `F = ${tableau[tableau.length - 1][tableau[0].length - 1]}</p>`;
        result_div.innerHTML += result_text;
    } 
    
    else {

        alert("Поддерживаются только ограничения вида ≤ или = и неотрицательные свободные члены");
    }
}

function addTable(tableau, step, parent) {

    let html = `<h4>Шаг ${step}</h4><table border="1" cellpadding="5" style="margin: 10px auto;">`;

    for (let i = 0; i < tableau.length; i++) {

        html += "<tr>";

        for (let j = 0; j < tableau[i].length; j++) {

            html += `<td>${Math.round(tableau[i][j] * 1000) / 1000}</td>`;
        }

        html += "</tr>";
    }

    html += "</table>";
    parent.innerHTML += html;
}
