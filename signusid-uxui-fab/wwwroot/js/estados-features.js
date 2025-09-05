console.log('Se cargó el script')

/*Script que modifica ciertas partes del modal, dependiendo de las verificaciones */
document.addEventListener('DOMContentLoaded', function () {
  const deleteModal = document.getElementById('deleteModal');
  const deleteModalLabel = document.getElementById('deleteModalLabel'); // Título del modal
  const deleteModalBody = document.getElementById('deleteModalBody');
  const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

  // Lista de nombres que no se pueden eliminar
  const nonDeletableNames = ["Nuevo", "En uso", "Sin uso", "Dañado", "Destruido", "Donado"];

  // Escuchar los clics en los botones de borrar
  document.querySelectorAll('[data-bs-target="#deleteModal"]').forEach(button => {
    button.addEventListener('click', function () {
      const id = this.getAttribute('data-id');
      const name = this.getAttribute('data-name');
      const assets = parseInt(this.getAttribute('data-assets'));


      if (nonDeletableNames.includes(name)) {
        // Cambiar título y mensaje si el nombre no es eliminable
        deleteModalLabel.textContent = "Eliminación no permitida";
        deleteModalBody.innerHTML = `
                        <p>El estado <strong>${name}</strong> no se puede eliminar.</p>`;
        confirmDeleteBtn.classList.add('d-none'); // Ocultar botón de borrar
      } else if (assets >= 1) {
        // Cambiar título y mensaje si hay activos asignados
        deleteModalLabel.textContent = "Eliminación no permitida";
        deleteModalBody.innerHTML = `
                        <p>Este estado tiene <strong>${assets}</strong> activo/s asignado/s, no se puede eliminar.</p>`;
        confirmDeleteBtn.classList.add('d-none'); // Ocultar botón de borrar
      } else {
        // Cambiar título y mensaje para confirmación normal
        deleteModalLabel.textContent = "Confirmar eliminación";
        deleteModalBody.innerHTML = `
                        <p>¿Está seguro que desea borrar el estado <strong>${name}</strong>?</p>`;
        confirmDeleteBtn.classList.remove('d-none'); // Mostrar botón de borrar
        confirmDeleteBtn.setAttribute('href', `${borrarEstadoUrl}/${id}`);
      }
    });
  });
});


/*Script que verifica cuando un estado puede ser o no editado */
/*document.addEventListener('DOMContentLoaded', function () {
  const nonEditableModal = document.getElementById('nonEditableModal');
  const nonEditableModalBody = document.getElementById('nonEditableModalBody');
  const editModal = document.getElementById('addStateModal'); // Modal de edición
  const nonEditableNames = ["Nuevo", "En uso", "Sin uso", "Dañado", "Destruido", "Donado"];

  // Escuchar los clics en los botones de editar
  document.querySelectorAll('.btn-edit').forEach(button => {
    button.addEventListener('click', function (event) {
      const name = this.getAttribute('data-name');
      const description = this.getAttribute('data-description');

      if (nonEditableNames.includes(name)) {
        // Cancelar el evento de edición
        event.preventDefault();

        // Mostrar mensaje en el modal de no editable
        nonEditableModalBody.innerHTML = `
          <p>El estado <strong>${name}</strong> no se puede editar.</p>`;

        // Abrir el modal de no editable
        const modalInstance = new bootstrap.Modal(nonEditableModal);
        modalInstance.show();
      } else {
        console.log('Entró donde debe entrar');
        // Si es editable, configurar el contexto y abrir el modal de edición
        setupEditContext(this, name, description);

        // Abrir el modal manualmente
        const editModalInstance = new bootstrap.Modal(editModal);
        editModalInstance.show();
      }
    });
  });
});
*/



//document.getElementById('clearSearch').addEventListener('click', function () {
//  // Limpia el contenido del campo de texto
//  document.getElementById('searchInput').value = '';

//  // Envía el formulario vacío para recargar todos los datos
//  document.querySelector('form').submit();

//});


/*Script para marcar y desmarcar los checkbox */
document.addEventListener('DOMContentLoaded', () => {

  // Obtener el checkbox principal y los secundarios
  const selectAllCheckbox = document.getElementById('select_all');
  const checkboxes = document.querySelectorAll('.checkbox-item');

  // Evento para controlar el estado de los checkboxes secundarios
  selectAllCheckbox.addEventListener('change', function () {
    const isChecked = selectAllCheckbox.checked; // Verificar si el principal está marcado
    checkboxes.forEach(checkbox => {
      checkbox.checked = isChecked; // Cambiar el estado de los secundarios
    });
  });

  // (Opcional) Desmarcar el principal si se desmarca alguno de los secundarios
  checkboxes.forEach(checkbox => {
    checkbox.addEventListener('change', function () {
      if (!checkbox.checked) {
        selectAllCheckbox.checked = false; // Desmarcar el principal
      } else if (Array.from(checkboxes).every(cb => cb.checked)) {
        selectAllCheckbox.checked = true; // Marcar el principal si todos están seleccionados
      }
    });
  });

});



/*Script para borrar en batch, primero verifica las reglas descritas en el ticket SIGID2-39 si todo está bien, el usuario elige si eliminar o no */
document.addEventListener('DOMContentLoaded', function () {
  const deleteBatchBtn = document.getElementById('deleteBatchBtn'); // Botón de borrar en batch
  const checkboxes = document.querySelectorAll('.checkbox-item');
  const selectAllCheckbox = document.getElementById('select_all');
  const deleteModal = document.getElementById('deleteModal');
  const deleteModalLabel = document.getElementById('deleteModalLabel'); // Título del modal
  const deleteModalBody = document.getElementById('deleteModalBody');
  const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

  // Lista de nombres que no se pueden eliminar
  const nonDeletableNames = ["Nuevo", "En uso", "Sin uso", "Dañado", "Destruido", "Donado"];

  // Función para habilitar/deshabilitar el botón de borrar en batch
  function toggleBatchDeleteButton() {
    const selectedCheckboxes = document.querySelectorAll('.checkbox-item:checked');
    deleteBatchBtn.disabled = selectedCheckboxes.length === 0;
  }

  // Escuchar los cambios en los checkboxes individuales
  checkboxes.forEach(checkbox => {
    checkbox.addEventListener('change', toggleBatchDeleteButton);
  });

  // Escuchar el cambio en el checkbox principal (select-all)
  selectAllCheckbox.addEventListener('change', function () {
    const isChecked = selectAllCheckbox.checked; // Verificar si el principal está marcado
    checkboxes.forEach(checkbox => {
      checkbox.checked = isChecked; // Cambiar el estado de los secundarios
    });
    toggleBatchDeleteButton(); // Verificar si habilitar o deshabilitar el botón
  });

  // Función para verificar si un estado se puede eliminar
  function canDeleteEstado(estado) {
    if (nonDeletableNames.includes(estado.name)) {
      return { allowed: false, message: `El estado <strong>${estado.name}</strong> no se puede eliminar.` };
    } else if (estado.assets >= 1) {
      return { allowed: false, message: `El estado <strong>${estado.name}</strong> tiene <strong>${estado.assets}</strong> activo/s asignado/s, no se puede eliminar.` };
    } else {
      return { allowed: true, message: `¿Está seguro que desea borrar el estado <strong>${estado.name}</strong>?` };
    }
  }

  /*Script para borrar, verifica si tienen activos o si es alguno de los estados por defecto del sistema */
  // Evento para el botón de borrar en batch
  deleteBatchBtn.addEventListener('click', function () {
    const selectedCheckboxes = document.querySelectorAll('.checkbox-item:checked');

    const estadosSeleccionados = Array.from(selectedCheckboxes).map(checkbox => {
      return {
        id: checkbox.getAttribute('data-id-estado'),
        name: checkbox.closest('tr').querySelector('td:nth-child(3)').textContent, // Nombre
        assets: parseInt(checkbox.closest('tr').querySelector('td:nth-child(5)').textContent), // Activos
      };
    });
    console.log("selectedCheckboxes: " + selectedCheckboxes)
    console.log("Estados seleccionados: " + estadosSeleccionados)
    console.log("ppppppppppp" )

    let allCanBeDeleted = true;
    let modalContent = '';
    estadosSeleccionados.forEach(estado => {
      const result = canDeleteEstado(estado);
      if (!result.allowed) {
        allCanBeDeleted = false;
        modalContent += `<p>${result.message}</p>`;
      }
    });

    if (allCanBeDeleted) {
      // Si todos los estados pueden ser eliminados, proceder con la eliminación
      deleteModalLabel.textContent = "Confirmar eliminación en batch";
      deleteModalBody.innerHTML = `
                <p>¿Está seguro que desea borrar los siguientes estados?</p>
                <ul>
                    ${estadosSeleccionados.map(estado => `<li><strong>${estado.name}</strong></li>`).join('')}
                </ul>
            `;
      confirmDeleteBtn.classList.remove('d-none');
      confirmDeleteBtn.setAttribute(
        'href',
        `${borrarEstadoBatchUrl}?${estadosSeleccionados.map(estado => `estadosSeleccionados=${estado.id}`).join('&')}`
      );


    } else {
      // Si no todos pueden eliminarse, mostrar mensaje de error en el modal
      deleteModalLabel.textContent = "Eliminación no permitida";
      deleteModalBody.innerHTML = modalContent;
      confirmDeleteBtn.classList.add('d-none');
    }
  });

  // Mostrar u ocultar el botón de borrar en batch según los checkboxes seleccionados
  checkboxes.forEach(checkbox => {
    checkbox.addEventListener('change', function () {
      toggleBatchDeleteButton();
    });
  });
});


/*Script para el ordenamiento alfanumerico de las colummnas */

//document.addEventListener('DOMContentLoaded', function () {
//  const table = document.querySelector('table.datatables-basic');
//  const headers = table.querySelectorAll('th');
//  const rows = Array.from(table.querySelectorAll('tbody tr'));

//  // Función para ordenar las filas
//  function sortTable(columnIndex, isNumeric = false, ascending = true) {
//    const sortedRows = rows.sort((rowA, rowB) => {
//      const cellA = rowA.children[columnIndex].textContent.trim();
//      const cellB = rowB.children[columnIndex].textContent.trim();

//      if (isNumeric) {
//        const numA = parseFloat(cellA);
//        const numB = parseFloat(cellB);
//        return ascending ? numA - numB : numB - numA;
//      } else {
//        return ascending
//          ? cellA.localeCompare(cellB)
//          : cellB.localeCompare(cellA);
//      }
//    });

//    // Reorganizar las filas en la tabla
//    sortedRows.forEach(row => table.querySelector('tbody').appendChild(row));
//  }

//  // Variables para saber si la columna ya está ordenada
//  let currentSortColumn = null;
//  let ascendingOrder = true;

//  // Event listeners para cada columna
//  headers[2].addEventListener('click', function () {
//    // Si se hizo clic en "Nombre" (columna 2), ordena alfabéticamente
//    if (currentSortColumn === 2) {
//      ascendingOrder = !ascendingOrder; // Cambiar el orden si ya se había hecho clic
//    } else {
//      currentSortColumn = 2;
//      ascendingOrder = true; // Orden ascendente por defecto
//    }
//    sortTable(2, false, ascendingOrder); // false indica que no es numérico
//  });

//  headers[3].addEventListener('click', function () {
//    // Si se hizo clic en "Descripción" (columna 3), ordena alfabéticamente
//    if (currentSortColumn === 3) {
//      ascendingOrder = !ascendingOrder; // Cambiar el orden si ya se había hecho clic
//    } else {
//      currentSortColumn = 3;
//      ascendingOrder = true; // Orden ascendente por defecto
//    }
//    sortTable(3, false, ascendingOrder); // false indica que no es numérico
//  });

//  headers[4].addEventListener('click', function () {
//    // Si se hizo clic en "Activos" (columna 4), ordena numéricamente
//    if (currentSortColumn === 4) {
//      ascendingOrder = !ascendingOrder; // Cambiar el orden si ya se había hecho clic
//    } else {
//      currentSortColumn = 4;
//      ascendingOrder = true; // Orden ascendente por defecto
//    }
//    sortTable(4, true, ascendingOrder); // true indica que es numérico
//  });
//});

