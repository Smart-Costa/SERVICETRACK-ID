console.log('Se cargó')
//document.getElementById('clearSearch2').addEventListener('click', function () {
//  // Limpia el contenido del campo de texto
//  document.getElementById('searchInput2').value = '';
//  console.log('entró')
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
/*

$(document).ready(function () {
  // Evento cuando se hace clic en el botón de eliminación
  $('button[data-bs-toggle="modal"]').on('click', function () {
    var idMarca = $(this).data('id');  // Obtener el id de la marca
    var modeloNombre = $(this).data('name');  // Obtener el nombre del modelo
    console.log('entró')
    console.log(idMarca)

    // Actualizar el mensaje de confirmación en el modal
    $('#deleteModalBody').text('¿Estás seguro de que deseas eliminar el modelo: ' + modeloNombre + '?');

    // Mostrar el botón de confirmación (eliminación)
    $('#confirmDeleteBtn').removeClass('d-none');
    $('#confirmDeleteBtn').data('id', idMarca);  // Guardar el idMarca en el botón
  });

  // Confirmar la eliminación cuando el botón de "Borrar" es clickeado
  $('#confirmDeleteBtn').on('click', function (event) {
    event.preventDefault(); // Prevenir la acción por defecto del enlace
    console.log('entró 2')
    var idMarca = $(this).data('id'); // Obtener el id de la marca
    console.log('Antes de enviar '+idMarca)

    // Realizar la solicitud AJAX para eliminar el modelo
    $.ajax({
      url: '/Activos/EliminarRegistro/' + idMarca,  // Ajusta la URL si es necesario
      method: 'GET',
      success: function (data) {
        // Verifica si la respuesta tiene la propiedad 'message'
        console.log(data);  // Esto te ayudará a ver la respuesta completa en la consola
        console.log('ID MARCA '+idMarca)
        $('#deleteModalBody').text(data.message);  // Mostrar el mensaje de éxito o error
        console.log('entró 3')
        // Cerrar el modal después de la eliminación
        $('#deleteModal').modal('hide');

        // Ocultar el botón de "Borrar" nuevamente
        $('#confirmDeleteBtn').addClass('d-none');
      },
      error: function () {
        $('#deleteModalBody').text('Hubo un error al intentar eliminar el modelo.');
      }
    });

  });
});
*/

// Evento cuando se hace clic en el botón de eliminación (esto será en el archivo JS)
$('button[data-bs-toggle="modal"]').on('click', function () {
  var idMarca = $(this).data('id');  // Obtener el modeloID
  var modeloNombre = $(this).data('name');  // Obtener el nombre del modelo

  // Actualizar el mensaje de confirmación en el modal
  $('#deleteModalBody').text('¿Estás seguro de que deseas eliminar el modelo: ' + modeloNombre + '?');

  // Actualizar el valor de modeloID en el formulario
  $('#modeloID').val(idMarca); // Asignar el valor de modeloID al campo oculto
});








/*

document.addEventListener('DOMContentLoaded', () => {
  const selectAllCheckbox = document.getElementById('select_all');
  const checkboxes = document.querySelectorAll('.checkbox-item');
  const deleteBatchBtn = document.getElementById('deleteBatchBtn');

  // Evento para controlar el estado de los checkboxes secundarios
  selectAllCheckbox.addEventListener('change', function () {
    const isChecked = selectAllCheckbox.checked; // Verificar si el principal está marcado
    checkboxes.forEach(checkbox => {
      checkbox.checked = isChecked; // Cambiar el estado de los secundarios
    });
    toggleDeleteBatchBtn(); // Habilitar/deshabilitar el botón
  });

  // (Opcional) Desmarcar el principal si se desmarca alguno de los secundarios
  checkboxes.forEach(checkbox => {
    checkbox.addEventListener('change', function () {
      if (!checkbox.checked) {
        selectAllCheckbox.checked = false; // Desmarcar el principal
      } else if (Array.from(checkboxes).every(cb => cb.checked)) {
        selectAllCheckbox.checked = true; // Marcar el principal si todos están seleccionados
      }
      toggleDeleteBatchBtn(); // Habilitar/deshabilitar el botón
    });
  });

  // Función para habilitar o deshabilitar el botón de eliminación en batch
  function toggleDeleteBatchBtn() {
    const anyChecked = Array.from(checkboxes).some(cb => cb.checked); // Verifica si al menos uno está marcado
    deleteBatchBtn.disabled = !anyChecked; // Habilitar o deshabilitar el botón
  }

  // Evento para manejar la eliminación en batch
  deleteBatchBtn.addEventListener('click', function () {
    const selectedIds = Array.from(checkboxes)
      .filter(checkbox => checkbox.checked) // Filtrar los seleccionados
      .map(checkbox => checkbox.getAttribute('data-model-id')); // Obtener el modeloID de cada uno

    console.log('Ids ' + selectedIds)

    if (selectedIds.length > 0) {
      // Aquí enviamos los IDs seleccionados al controlador para eliminar en batch
      const url = '/Activos/EliminarRegistroBatch'; // Cambia la URL a la que necesites
      fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ modeloIDs: selectedIds }), // Enviar los IDs seleccionados
      })
        .then(response => response.json())
        .then(data => {
          if (data.success) {
            alert('Modelos eliminados exitosamente.');
            // Aquí puedes agregar lógica para actualizar la vista o eliminar las filas
          } else {
            alert('No se pudieron eliminar los modelos.');
          }
        })
        .catch(error => console.error('Error:', error));
    }
  });
});
*/



document.addEventListener('DOMContentLoaded', () => {
  const selectAllCheckbox = document.getElementById('select_all');
  const checkboxes = document.querySelectorAll('.checkbox-item');
  const deleteBatchBtn = document.getElementById('deleteBatchBtn');
  const deleteBatchForm = document.getElementById('deleteBatchForm');
  const modeloIDsInput = document.getElementById('modeloIDs');

  // Evento para controlar el estado de los checkboxes secundarios
  selectAllCheckbox.addEventListener('change', function () {
    const isChecked = selectAllCheckbox.checked; // Verificar si el principal está marcado
    checkboxes.forEach(checkbox => {
      checkbox.checked = isChecked; // Cambiar el estado de los secundarios
    });
    toggleDeleteBatchBtn(); // Habilitar/deshabilitar el botón
  });

  // (Opcional) Desmarcar el principal si se desmarca alguno de los secundarios
  checkboxes.forEach(checkbox => {
    checkbox.addEventListener('change', function () {
      if (!checkbox.checked) {
        selectAllCheckbox.checked = false; // Desmarcar el principal
      } else if (Array.from(checkboxes).every(cb => cb.checked)) {
        selectAllCheckbox.checked = true; // Marcar el principal si todos están seleccionados
      }
      toggleDeleteBatchBtn(); // Habilitar/deshabilitar el botón
    });
  });

  // Función para habilitar o deshabilitar el botón de eliminación en batch
  function toggleDeleteBatchBtn() {
    const anyChecked = Array.from(checkboxes).some(cb => cb.checked); // Verifica si al menos uno está marcado
    deleteBatchBtn.disabled = !anyChecked; // Habilitar o deshabilitar el botón
  }

  // Evento para manejar la eliminación en batch
  deleteBatchBtn.addEventListener('click', function () {
    const selectedIds = Array.from(checkboxes)
      .filter(checkbox => checkbox.checked) // Filtrar los seleccionados
      .map(checkbox => checkbox.getAttribute('data-model-id')); // Obtener el modeloID de cada uno

    if (selectedIds.length > 0) {
      // Actualizar el campo oculto con los IDs seleccionados
      modeloIDsInput.value = selectedIds.join(','); // Unirlos en una cadena separada por comas

      // Enviar el formulario
      deleteBatchForm.submit(); // Esto enviará los datos al servidor
    }
  });
});

