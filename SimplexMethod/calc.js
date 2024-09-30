//динамическое создание функции и системы ограничений в виде таблиц с полями ввода 
function madeFuncAndSystem() {

    let sel_rstrctns = my_form.restrictions; //поле выбора количества ограничений
    let rstrctns_indx = sel_rstrctns.selectedIndex; //индекс выбранного количества ограничений
    let rstrctns_quantity = sel_rstrctns.options[rstrctns_indx].value; //количество ограничений

    let sel_vars = my_form.vars; //поле выбора количества переменных
    let vars_indx = sel_vars.selectedIndex; //индекс выбранного количества переменных
    let vars_quantity = sel_vars.options[vars_indx].value; //количество переменных

    let func_tbl = document.getElementById("function_table"); //таблица функции
    let func_tbl_html = ""; //таблица функции в виде строки
    let sel_mode = '<select>' + '<option value="max">max</option>' + 
    '<option value="min">min</option>'; //поле выбора режима в виде строки

    let rstrctns_tbl = document.getElementById("rstrctns_table"); //таблица ограничений как объект DOM
    let rstrctns_tbl_html = ""; //таблица ограничений в виде строки
    let sel_sign = '<select>' + '<option value="≤">≤</option>' + '<option value="=">=</option>' + 
    '<option value="≥">≥</option>' + '</select>'; //поле выбора знака в виде строки

    func_tbl_html += "<tr>";

    for (let i = 0; i < vars_quantity; i++) {

        func_tbl_html += `<td><input type="number" id="x${j + 1}"><td> x${j + 1}`

        if (j + 1 != vars_quantity) {

            func_tbl_html += " + </td></td>";
        }
    }



    for (let i = 0; i < rstrctns_quantity; i++) {

        rstrctns_tbl_html += "<tr>";

        for (let j = 0; j < vars_quantity; j++) {
            
            rstrctns_tbl_html += `<td><input type="number" id="${i}-${j}"><td> x${j + 1}`;

            if (j + 1 != vars_quantity) {

                rstrctns_tbl_html += " + </td></td>";
            }
        }

        rstrctns_tbl_html += `<td>${sel_sign}</td><td><input type="number" id="ft${i}"></td></tr>`;
    }

    rstrctns_tbl.innerHTML = rstrctns_tbl_html;
}

function simplexSolve() {
    alert("В разработке");
}