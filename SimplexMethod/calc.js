var rstrctns_quantity = 0; //количество ограничений
var vars_quantity = 0; //количество переменных

//динамическое создание функции и системы ограничений в виде таблиц с полями ввода 
function madeFuncAndSystem() {

    let sel_rstrctns = my_form.restrictions; //поле выбора количества ограничений
    let rstrctns_indx = sel_rstrctns.selectedIndex; //индекс выбранного количества ограничений

    let sel_vars = my_form.vars; //поле выбора количества переменных
    let vars_indx = sel_vars.selectedIndex; //индекс выбранного количества переменных

    rstrctns_quantity = sel_rstrctns.options[rstrctns_indx].value;
    vars_quantity = sel_vars.options[vars_indx].value;

    let func_tbl = document.getElementById("function_table"); //таблица функции
    let func_tbl_html = ""; //таблица функции в виде строки

    func_tbl_html += "<tr>";

    for (let i = 0; i < vars_quantity; i++) {

        func_tbl_html += `<td><input type="number" id="x${i + 1}"></td><td> x${i + 1}`

        if (i + 1 != vars_quantity) {

            func_tbl_html += " +";
        }

        func_tbl_html += "</td>";
    }

    let sel_mode = '<select id="sm">' + 
    '<option value="max">max</option>' + 
    '<option value="min">min</option>' + 
    '</select>'; //поле выбора способа решения в виде строки

    func_tbl_html += `<td> ➡ ${sel_mode}</td></tr>`;
    func_tbl.innerHTML = func_tbl_html;

    let rstrctns_tbl = document.getElementById("rstrctns_table"); //таблица ограничений как объект DOM
    let rstrctns_tbl_html = ""; //таблица ограничений в виде строки

    for (let i = 0; i < rstrctns_quantity; i++) {

        rstrctns_tbl_html += "<tr>";

        for (let j = 0; j < vars_quantity; j++) {
            
            rstrctns_tbl_html += `<td><input type="number" id="${i}-${j}"></td><td> x${j + 1}`;

            if (j + 1 != vars_quantity) {

                rstrctns_tbl_html += " +";
            }

            rstrctns_tbl_html += "</td>";
        }

        let sel_sign = `<select id="ss${i}">` + 
        '<option value="≤">≤</option>' + 
        '<option value="=">=</option>' + 
        '<option value="≥">≥</option>' + 
        '</select>'; //поле выбора знака в виде строки

        rstrctns_tbl_html += `<td>${sel_sign}</td><td><input type="number" id="ft${i}"></td></tr>`;
    }

    rstrctns_tbl.innerHTML = rstrctns_tbl_html;
}

//решение с помощью симплекс-метода
function simplexSolve() {

    let xs = []; //коэффициенты перед иксами функции

    for (let i = 0; i < vars_quantity; i++) {

        xs.push(parseInt(document.getElementById(`x${i + 1}`).valueAsNumber));
    }

    let sel_signs = []; //выбранные знаки для каждого ограничения
    let free_terms = []; //свободные члены системы ограничений
    let rstrctns_mtrx = []; //матрица коэффициентов системы ограничений

    for (let i = 0; i < rstrctns_quantity; i++) {

        sel_signs.push(document.getElementById(`ss${i}`).value);
        free_terms.push(parseInt(document.getElementById(`ft${i}`).valueAsNumber));

        let rstrctns_row = []; //строка с коэффициентами системы ограничений

        for (let j = 0; j < vars_quantity; j++) {

            rstrctns_row.push(parseInt(document.getElementById(`${i}-${j}`).valueAsNumber));
        }

        rstrctns_mtrx.push(rstrctns_row);
    }

    let sel_mode = document.getElementById("sm"); //поле выбора способа решения
    let mode_indx = sel_mode.selectedIndex; //индекс выбранного способа решения
    let mode = sel_mode.options[mode_indx].value; //способ решения

    if ((mode == "max") && (sel_signs.indexOf("≥") == -1) && 
        (sel_signs.indexOf("=") == -1) && !free_terms.some(term => term < 0)) {

        console.log(structuredClone(xs), sel_signs, free_terms, 
            structuredClone(rstrctns_mtrx), mode);

        for (let i = 0; i < rstrctns_quantity; i++) {

            xs.push(0);

            for (let j = 0; j < rstrctns_quantity; j++) {

                if (i == j) {
                    
                    rstrctns_mtrx[i].push(1);
                }

                else {

                    rstrctns_mtrx[i].push(0);
                }
            }
        }

        let deltas = [];

        for (let i = 0; i < xs.length; i++) {

            deltas.push(xs[i] == 0 ? 0 : -1 * xs[i]);
        }

        let is_optimum = deltas.some(delta => delta < 0);

        console.log(xs, sel_signs, free_terms, rstrctns_mtrx, mode, deltas);

        if (is_optimum) {

            free_terms.forEach(term => {
                
                
            });
        }
    }

    else {

        alert("Ещё в разработке");
    }
}