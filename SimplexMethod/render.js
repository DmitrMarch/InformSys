var rstrctns_quantity = 0; //количество ограничений
var vars_quantity = 0; //количество переменных

//динамическое создание полей выбора количества ограничений и переменных
function madeRstrctnsAndVars() {

    let sel_vars = my_form.vars; //поле выбора количества переменных
    let options_vars = ""; //опции количества переменных в виде строки

    let sel_rstrctns = my_form.restrictions; //поле выбора количества ограничений
    let options_rstrctns = ""; //опции количества ограничений в виде строки

    for (let i = 1; i < 13; i++) {

        options_vars += `<option value="${i}">${i}</option>`;
        options_rstrctns += `<option value="${i}">${i}</option>`;
    }

    sel_vars.innerHTML = options_vars;
    sel_rstrctns.innerHTML = options_rstrctns;

    madeFuncAndSystem();
}

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