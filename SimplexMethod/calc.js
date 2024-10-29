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
    let basis = []; //базисные индексы

    if ((mode == "max") && (sel_signs.indexOf("≥") == -1) && 
        (sel_signs.indexOf("=") == -1) && !free_terms.some(term => term < 0)) {

        console.log(structuredClone(xs), sel_signs, free_terms, 
            structuredClone(rstrctns_mtrx), mode);

        for (let i = 0; i < rstrctns_quantity; i++) {

            xs.push(0);

            for (let j = 0; j < rstrctns_quantity; j++) {

                if (i == j) {
                    
                    basis.push([i, j + rstrctns_quantity * 1]);
                    rstrctns_mtrx[i].push(1);
                }

                else {

                    rstrctns_mtrx[i].push(0);
                }
            }
        }

        xs.push(0);

        let deltas = [];

        for (let i = 0; i < xs.length; i++) {

            deltas.push(xs[i] == 0 ? 0 : -1 * xs[i]);
        }

        let is_optimum = !deltas.some(delta => delta < 0);

        console.log(xs, sel_signs, free_terms, rstrctns_mtrx, mode);
        console.log(deltas, basis);

        let answer = [];

        if (is_optimum) {

            for (let i = 0; i < vars_quantity; i++) {

                if (basis[i].indexOf(i) == -1) {

                    answer.push(0);
                }

                else {

                    answer.push(free_terms[i]);
                }
            }
        }

        else {


        }

        console.log(answer);
    }

    else {

        alert("Ещё в разработке");
    }
}