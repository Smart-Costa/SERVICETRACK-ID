document.addEventListener('DOMContentLoaded', () => {
  const selectAllCheckbox = document.getElementById('selectAllRoles');
  const roleCheckboxes = document.querySelectorAll('.role-checkbox');
  const batchDeactivateButton = document.getElementById('batchDeactivateButton');
  const switchToggleSlider = document.querySelector('.switch-toggle-slider');

  // Función para actualizar el estado del botón
  function updateBatchDeactivateButtonState() {
    const anyChecked = Array.from(roleCheckboxes).some(cb => cb.checked);
    batchDeactivateButton.disabled = !anyChecked;
  }
  // Detectar cambio de estado del switch
  batchDeactivateButton.addEventListener('change', () => {
    const state = batchDeactivateButton.checked ? 'activar' : 'desactivar';
    switchToggleSlider.querySelector('.switch-on').setAttribute('data-state', state);
    switchToggleSlider.querySelector('.switch-off').setAttribute('data-state', state);
  });

  // Función para manejar el cambio del checkbox principal
  selectAllCheckbox.addEventListener('change', () => {
    const isChecked = selectAllCheckbox.checked;
    roleCheckboxes.forEach(checkbox => {
      checkbox.checked = isChecked;
    });
    updateBatchDeactivateButtonState();
  });

  // Función para manejar el cambio de los checkboxes individuales
  roleCheckboxes.forEach(checkbox => {
    checkbox.addEventListener('change', () => {
      const allChecked = Array.from(roleCheckboxes).every(cb => cb.checked);
      selectAllCheckbox.checked = allChecked;
      updateBatchDeactivateButtonState();
    });
  });

  // Actualizar el estado del botón de desactivación masiva
  function updateBatchDeactivateButtonState() {
    const anyChecked = Array.from(roleCheckboxes).some(cb => cb.checked);
    batchDeactivateButton.disabled = !anyChecked;
  }

  // Evento para desactivar o activar roles masivamente
  batchDeactivateButton.addEventListener('click', () => {
    const selectedRoleIds = Array.from(roleCheckboxes)
      .filter(cb => cb.checked)
      .map(cb => cb.getAttribute('data-role-id'));

    if (selectedRoleIds.length === 0) return;

    const state = batchDeactivateButton.checked ? 'activar' : 'desactivar';

    console.log("Estado: " + state)

    fetch(activarDesactivarBatchOficinasUrl, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ roles: selectedRoleIds, action: state })
    })
      .then(response => response.json())
      .then(data => {
        if (data.success) {
          showBatchModal(data.roles);
          updateRoleLinks(data.roles);
        } else {
          //alert("Ocurrió un error al actualizar los roles.");
        }
      })
      .catch(error => console.error('Error al actualizar oficinas:', error));
  });
  // Listeners para los checkboxes
  selectAllCheckbox.addEventListener('change', () => {
    const isChecked = selectAllCheckbox.checked;
    roleCheckboxes.forEach(checkbox => {
      checkbox.checked = isChecked;
    });
    updateBatchDeactivateButtonState();
  });

  roleCheckboxes.forEach(checkbox => {
    checkbox.addEventListener('change', () => {
      const allChecked = Array.from(roleCheckboxes).every(cb => cb.checked);
      selectAllCheckbox.checked = allChecked;
      updateBatchDeactivateButtonState();
    });
  });



  // Mostrar modal para varios roles
  function showBatchModal(results) {
    // Crear una lista de los roles con su nombre y estado (activado/desactivado)
    const roleList = results.map(role => {
      // Verifica si el rol fue desactivado o activado
      const state = role.wasDeactivated ? 'Desactivado' : 'Activado';


      return `<li>${role.roleName} (${state})</li>`;
    }).join('');

    // Coloca el mensaje en el modal
    document.getElementById('batchSuccessMessage').innerHTML = `
            <p>Se han hecho cambios en las siguientes oficinas:</p>
            <ul>${roleList}</ul>
        `;

    // Muestra el modal con Bootstrap
    const batchModal = new bootstrap.Modal(document.getElementById('batchSuccessModal'));
    batchModal.show();
  }
});

document.addEventListener('DOMContentLoaded', function () {
  var batchSuccessModal = document.getElementById('batchSuccessModal');


  batchSuccessModal.addEventListener('hidden.bs.modal', function () {
    window.location.href = oficinasRedirectUrl;
  });
});

document.addEventListener('DOMContentLoaded', function () {
  var switchInput = document.getElementById('batchDeactivateButton');
  var batchSuccessModal = document.getElementById('batchSuccessModal');

  // Restaurar estado al cargar la página
  if (localStorage.getItem('switchState') === 'true') {
    switchInput.checked = true;
  } else {
    switchInput.checked = false;
  }

  // Guardar estado del switch en localStorage
  switchInput.addEventListener('change', function () {
    localStorage.setItem('switchState', switchInput.checked);
  });

  // Redirigir al cerrar el modal
  batchSuccessModal.addEventListener('hidden.bs.modal', function () {
    window.location.href = oficinasRedirectUrl;
  });
});



/*Script que modifica ciertas partes del modal, dependiendo de las verificaciones */
document.addEventListener('DOMContentLoaded', function () {
  const deleteModal = document.getElementById('deleteModal');
  const deleteModalLabel = document.getElementById('deleteModalLabel'); // Título del modal
  const deleteModalBody = document.getElementById('deleteModalBody');
  const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

  // Lista de nombres que no se pueden eliminar
  const nonDeletableNames = ["Sin Asignar"];

  // Escuchar los clics en los botones de borrar
  document.querySelectorAll('[data-bs-target="#deleteModal"]').forEach(button => {
    button.addEventListener('click', function () {
      const id = this.getAttribute('data-id-piso');
      const name = this.getAttribute('data-name');
      const assets = parseInt(this.getAttribute('data-assets'));
      console.log(id)

        // Cambiar título y mensaje para confirmación normal
        deleteModalLabel.textContent = "Confirmar Acción";
        deleteModalBody.innerHTML = `
                        <p>¿Está seguro que desea activar/desactivar la oficina <strong>${name}</strong>?</p>`;
        confirmDeleteBtn.classList.remove('d-none'); // Mostrar botón de borrar
      confirmDeleteBtn.setAttribute('href', `${eliminarIndividualOficinasUrl}/${id}`);
      
    });
  });
});


document.getElementById('clearSearch').addEventListener('click', function () {
  // Limpia el contenido del campo de texto
  document.getElementById('searchInput').value = '';

  // Envía el formulario vacío para recargar todos los datos
  document.querySelector('form').submit();
});
