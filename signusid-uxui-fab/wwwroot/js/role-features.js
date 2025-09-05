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

    fetch(activarDesactivarBatchAccessUrl, {
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
          alert("Ocurrió un error al actualizar los roles.");
        }
      })
      .catch(error => console.error('Error al actualizar roles:', error));
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
  function updateRoleLinks(roles) {
    roles.forEach(role => {
      console.log(`Buscando roleId: ${role.roleId}`);

      // Selecciona específicamente el enlace con las clases deseadas
      const roleLink = document.querySelector(`a.dropdown-item.btn-toggle-state[data-role-id="${role.roleId}"]`);

      console.log(`Elemento encontrado:`, roleLink);

      if (roleLink) {
        if (role.wasDeactivated) {
          console.log(`Rol desactivado: ${role.roleId}`);
          roleLink.textContent = "Activar";
          roleLink.setAttribute("data-state", "activar");
        } else {
          console.log(`Rol activado: ${role.roleId}`);
          roleLink.textContent = "Desactivar";
          roleLink.setAttribute("data-state", "desactivar");
        }
      }
    });
  }


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
        <p>Se han hecho cambios en los siguientes roles:</p>
        <ul>${roleList}</ul>
    `;

    // Muestra el modal con Bootstrap
    const batchModal = new bootstrap.Modal(document.getElementById('batchSuccessModal'));
    batchModal.show();
  }
});
  




//Limpia el input de buscar y realiza la búsqueda vacía
document.getElementById("clearSearch").addEventListener("click", function () {
  document.getElementById("searchInput").value = ""; // Limpia el campo
});


const form2 = document.getElementById("addRoleForm");

// Escucha el evento 'show.bs.modal' para configurar el modal correctamente
document.getElementById('addRoleModal').addEventListener('show.bs.modal', function (event) {
  const button = event.relatedTarget; // Botón que activó el modal
  const context = button.getAttribute('data-context'); // "new" o "edit"

  const modalTitle = document.querySelector('.role-title');
  const roleNameInput = document.getElementById('RoleName');
  const roleDescriptionInput = document.getElementById('Descripcion');
  const saveButton = document.getElementById('saveRoleButton');

  // Restablece valores por defecto
  roleNameInput.value = '';
  roleDescriptionInput.value = '';
  saveButton.textContent = 'Guardar';

  // Configura el título y los valores en función del contexto
  if (context === 'edit') {
    const roleName = button.getAttribute('data-role-name');
    const roleDescription = button.getAttribute('data-role-description');

    modalTitle.textContent = 'Editar Rol';
    roleNameInput.value = roleName;
    roleDescriptionInput.value = roleDescription;
    saveButton.textContent = 'Actualizar';
  } else {
    modalTitle.textContent = 'Agregar Rol Nuevo';
  }

  // Guarda el contexto en un atributo oculto
  form2.setAttribute('data-context', context);
});

////Acciones que se ejecutan al guardar o editar un rol
//form2.addEventListener("submit", function (e) {
//  e.preventDefault();

//  const roleName = document.getElementById("RoleName").value.trim();
//  const roleDescription = document.getElementById("Descripcion").value.trim();

//  // Limpia mensajes de error previos
//  document.getElementById("roleNameError").textContent = '';
//  document.getElementById("roleDescriptionError").textContent = '';

//  let valid = true;

//  // Expresiones regulares para las validaciones
//  const onlyLettersRegex = /^[a-zA-Z\s]+$/; // Solo letras y espacios
//  const specialCharactersRegex = /^[a-zA-Z0-9\s,.\-()]+$/; // Letras, números, espacios y algunos caracteres especiales

//  // Validar el nombre del rol
//  if (!roleName) {
//    document.getElementById("roleNameError").textContent = "El nombre del rol no puede estar vacío.";
//    document.getElementById("roleName").focus();
//    valid = false;
//  } else if (roleName.length < 3 || roleName.length > 100) {
//    document.getElementById("roleNameError").textContent = "El nombre del rol debe tener entre 3 y 100 caracteres.";
//    document.getElementById("roleName").focus();
//    valid = false;
//  } else if (!onlyLettersRegex.test(roleName)) {
//    document.getElementById("roleNameError").textContent = "El nombre del rol solo puede contener letras y espacios.";
//    document.getElementById("roleName").focus();
//    valid = false;
//  }

//  // Validar la descripción del rol
//  if (!roleDescription) {
//    document.getElementById("roleDescriptionError").textContent = "La descripción no puede estar vacía.";
//    document.getElementById("roleDescription").focus();
//    valid = false;
//  } else if (roleDescription.length < 3 || roleDescription.length > 2500) {
//    document.getElementById("roleDescriptionError").textContent = "La descripción debe tener entre 3 y 2500 caracteres.";
//    document.getElementById("roleDescription").focus();
//    valid = false;
//  } else if (!specialCharactersRegex.test(roleDescription)) {
//    document.getElementById("roleDescriptionError").textContent =
//      'La descripción solo puede contener letras, números y los caracteres: ", . - ( )".';
//    document.getElementById("roleDescription").focus();
//    valid = false;
//  }

//  if (valid) {
//    // Recolectar los permisos seleccionados
//    const permisosSeleccionados = [];
//    const checkboxes = document.querySelectorAll('input[name="PermisosSeleccionados"]:checked');

//    checkboxes.forEach(function (checkbox) {
//      const value = checkbox.value.trim();

//      // Verifica que el valor no esté vacío ni duplicado
//      if (value && !permisosSeleccionados.includes(value)) {
//        permisosSeleccionados.push(value);
//      }
//    });

//    // Crear datos para enviar al servidor
//    const formData = new FormData();
//    formData.append("RoleName", roleName);
//    formData.append("Descripcion", roleDescription);
//    formData.append("PermisosSeleccionados", permisosSeleccionados.join(','));

//    // Determinar la URL de acción según el contexto
//    const context = form2.getAttribute('data-context'); // Obtiene el contexto almacenado
//    const actionUrl = context === 'edit' ? '/Access/UpdateRole' : '/Access/SaveRole';

//    // Enviar el formulario usando fetch
//    fetch(actionUrl, {
//      method: 'POST',
//      body: formData,
//    })
//      .then(response => response.json())
//      .then(data => {
//        if (data.alert) {
//          // Muestra el mensaje devuelto por el servidor
//          const modalTitle = data.alert.Tipo === 'success' ? 'Éxito' : 'Error';
//          const modalMessage = data.alert.Mensaje;

//          // Asumiendo que tienes un modal como en el ejemplo anterior
//          document.getElementById('alertTitle').textContent = modalTitle;
//          document.getElementById('alertMessage').textContent = modalMessage;
//          $('#alertModal').modal('show');

//          // Redirección después de mostrar el mensaje
//          if (data.alert.Tipo === 'success' && data.redirectUrl) {
//            setTimeout(() => {
//              window.location.href = data.redirectUrl;
//            }, 3000); // 3 segundos de espera antes de redirigir
//          }
//        }
//      })
//      .catch(error => {
//        console.error("Error al procesar la solicitud:", error);
//      });
//  } else {
//    console.log("Formulario inválido. Corrige los errores e inténtalo nuevamente.");
//  }
//});


// Configura el botón de cancelar
document.getElementById("cancelButton").addEventListener("click", function () {
  // Limpia los campos
  document.getElementById("RoleName").value = '';
  document.getElementById("Descripcion").value = '';

  // Limpia mensajes de error
  document.getElementById("roleNameError").textContent = '';
  document.getElementById("roleDescriptionError").textContent = '';

  // Cierra el modal (si no funciona el atributo data-bs-dismiss)
  const modalElement = document.querySelector('#addRoleModal');
  if (modalElement) {
    const modalInstance = bootstrap.Modal.getInstance(modalElement);
    if (modalInstance) {
      modalInstance.hide();
    }
  }
});

//Al editar un rol
document.addEventListener('DOMContentLoaded', function () {
  // Lista de roles simulada (esto debe ser cargado desde el backend en un caso real)
  const roles = [
    { RoleId: 1, RoleName: "Administrador", TotalUsers: 4, Descripcion: "Administra todo", Bloqueado: 0, Permisos: ["Gestión de Usuarios", "Gestión de Roles", "Auditoría de Seguridad"] },
    { RoleId: 2, RoleName: "Super Administrador", TotalUsers: 2, Descripcion: "Permisos superiores", Bloqueado: 0, Permisos: ["Gestión de Usuarios", "Administrar Base de Datos"] },
    { RoleId: 3, RoleName: "Editor", TotalUsers: 5, Descripcion: "Edita contenido", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Ver Informes"] },
    { RoleId: 4, RoleName: "Moderador", TotalUsers: 3, Descripcion: "Modera comentarios", Bloqueado: 0, Permisos: ["Monitoreo de Actividades", "Gestionar Contenido"] },
    { RoleId: 5, RoleName: "Usuario Regular", TotalUsers: 8, Descripcion: "Usuario estándar", Bloqueado: 0, Permisos: ["Ver Informes"] },
    { RoleId: 6, RoleName: "Colaborador", TotalUsers: 6, Descripcion: "Contribuye contenido", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Gestión de Roles"] },
    { RoleId: 7, RoleName: "Administrador de Contenido", TotalUsers: 4, Descripcion: "Administra el contenido", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Gestionar Proyectos", "Gestión de Roles"] },
    { RoleId: 8, RoleName: "Desarrollador", TotalUsers: 2, Descripcion: "Desarrollador de software", Bloqueado: 0, Permisos: ["Administrar Base de Datos", "Acceso a la API"] },
    { RoleId: 9, RoleName: "Soporte Técnico", TotalUsers: 3, Descripcion: "Proporciona soporte", Bloqueado: 0, Permisos: ["Monitoreo de Actividades", "Gestión de Roles"] },
    { RoleId: 10, RoleName: "Analista", TotalUsers: 2, Descripcion: "Analiza datos", Bloqueado: 0, Permisos: ["Ver Informes", "Acceso a Datos Sensibles"] },
    { RoleId: 11, RoleName: "Recursos Humanos", TotalUsers: 2, Descripcion: "Gestiona personal", Bloqueado: 0, Permisos: ["Gestión de Roles", "Gestionar Proyectos"] },
    { RoleId: 12, RoleName: "Contador", TotalUsers: 3, Descripcion: "Maneja finanzas", Bloqueado: 0, Permisos: ["Acceso a Datos Sensibles", "Gestionar Configuraciones"] },
    { RoleId: 13, RoleName: "Gerente de Proyectos", TotalUsers: 4, Descripcion: "Gestiona proyectos", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Monitoreo de Actividades"] },
    { RoleId: 14, RoleName: "Coordinador de Marketing", TotalUsers: 3, Descripcion: "Gestiona campañas de marketing", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Administrar Permisos"] },
    { RoleId: 15, RoleName: "Administrador de Sistemas", TotalUsers: 5, Descripcion: "Gestiona servidores y redes", Bloqueado: 0, Permisos: ["Acceso a la API", "Administrar Permisos"] },
    { RoleId: 16, RoleName: "Técnico de Red", TotalUsers: 4, Descripcion: "Gestiona infraestructura de red", Bloqueado: 0, Permisos: ["Gestionar Configuraciones", "Monitoreo de Actividades"] },
    { RoleId: 17, RoleName: "Gestor de Calidad", TotalUsers: 2, Descripcion: "Gestiona procesos de calidad", Bloqueado: 0, Permisos: ["Ver Informes", "Gestión de Roles"] },
    { RoleId: 18, RoleName: "Auditor", TotalUsers: 1, Descripcion: "Realiza auditorías internas", Bloqueado: 0, Permisos: ["Auditoría de Seguridad", "Gestión de Roles"] },
    { RoleId: 19, RoleName: "Operador", TotalUsers: 6, Descripcion: "Opera maquinaria", Bloqueado: 0, Permisos: ["Ver Informes"] },
    { RoleId: 20, RoleName: "Diseñador Gráfico", TotalUsers: 2, Descripcion: "Diseña material visual", Bloqueado: 0, Permisos: ["Gestionar Contenido"] },
    { RoleId: 21, RoleName: "Redactor", TotalUsers: 3, Descripcion: "Escribe contenido", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Ver Informes"] },
    { RoleId: 22, RoleName: "Consultor", TotalUsers: 4, Descripcion: "Asesora en proyectos", Bloqueado: 0, Permisos: ["Gestión de Roles", "Gestión de Proyectos"] },
    { RoleId: 23, RoleName: "Jefe de Ventas", TotalUsers: 3, Descripcion: "Gestiona equipo de ventas", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Gestión de Roles"] },
    { RoleId: 24, RoleName: "Representante de Ventas", TotalUsers: 8, Descripcion: "Vende productos o servicios", Bloqueado: 0, Permisos: ["Ver Informes"] },
    { RoleId: 25, RoleName: "Asistente Administrativo", TotalUsers: 5, Descripcion: "Apoya en tareas administrativas", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Ver Informes"] },
    { RoleId: 26, RoleName: "Supervisor", TotalUsers: 3, Descripcion: "Supervisa personal", Bloqueado: 0, Permisos: ["Monitoreo de Actividades", "Gestión de Roles"] },
    { RoleId: 27, RoleName: "Encargado de Compras", TotalUsers: 2, Descripcion: "Gestiona compras", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Gestión de Roles"] },
    { RoleId: 28, RoleName: "Jefe de Producto", TotalUsers: 3, Descripcion: "Gestiona el desarrollo de productos", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Gestión de Roles"] },
    { RoleId: 29, RoleName: "Desarrollador Frontend", TotalUsers: 4, Descripcion: "Desarrolla interfaces de usuario", Bloqueado: 0, Permisos: ["Administrar Base de Datos", "Acceso a la API"] },
    { RoleId: 30, RoleName: "Desarrollador Backend", TotalUsers: 4, Descripcion: "Desarrolla la lógica del servidor", Bloqueado: 0, Permisos: ["Acceso a la API", "Administrar Base de Datos"] },
    { RoleId: 31, RoleName: "Administrador de Base de Datos", TotalUsers: 2, Descripcion: "Gestiona bases de datos", Bloqueado: 0, Permisos: ["Administrar Base de Datos", "Gestión de Roles"] },
    { RoleId: 32, RoleName: "Administrador de Seguridad", TotalUsers: 3, Descripcion: "Gestiona la seguridad de sistemas", Bloqueado: 0, Permisos: ["Auditoría de Seguridad", "Acceso a Datos Sensibles"] },
    { RoleId: 33, RoleName: "Operador de Soporte", TotalUsers: 5, Descripcion: "Gestiona soporte técnico", Bloqueado: 0, Permisos: ["Monitoreo de Actividades", "Acceso a la API"] },
    { RoleId: 34, RoleName: "Asesor", TotalUsers: 2, Descripcion: "Asesora clientes", Bloqueado: 0, Permisos: ["Gestión de Roles", "Gestión de Proyectos"] },
    { RoleId: 35, RoleName: "Director de TI", TotalUsers: 2, Descripcion: "Gestiona tecnología de la información", Bloqueado: 0, Permisos: ["Administrar Base de Datos", "Acceso a la API"] },
    { RoleId: 36, RoleName: "Responsable de Seguridad", TotalUsers: 1, Descripcion: "Asegura las instalaciones", Bloqueado: 0, Permisos: ["Auditoría de Seguridad", "Gestión de Roles"] },
    { RoleId: 37, RoleName: "Jefe de Logística", TotalUsers: 3, Descripcion: "Gestiona distribución y transporte", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Gestión de Roles"] },
    { RoleId: 38, RoleName: "Gerente de Finanzas", TotalUsers: 2, Descripcion: "Gestiona la parte financiera", Bloqueado: 0, Permisos: ["Gestionar Proyectos", "Gestionar Configuraciones"] },
    { RoleId: 39, RoleName: "Administrador de Marketing", TotalUsers: 3, Descripcion: "Gestiona campañas de marketing", Bloqueado: 0, Permisos: ["Gestionar Contenido", "Gestión de Roles"] },
    { RoleId: 40, RoleName: "Director Ejecutivo", TotalUsers: 1, Descripcion: "Lidera la empresa", Bloqueado: 0, Permisos: ["Gestión de Roles", "Gestión de Proyectos"] }
  ];

  // Escucha los clics en los botones de edición
  document.querySelectorAll('.role-edit-modal').forEach(button => {
    button.addEventListener('click', function () {
      const roleId = parseInt(this.getAttribute('data-role-id'), 10);
      const role = roles.find(r => r.RoleId === roleId);

      if (role) {
        // Rellena los campos del formulario
        document.querySelector('#addRoleModal .role-title').textContent = `Editar Rol: ${role.RoleName}`;
        document.querySelector('#RoleName').value = role.RoleName;
        document.querySelector('#Descripcion').value = role.Descripcion;

        // Marcar los permisos seleccionados
        const permisosSeleccionados = role.Permisos;
        document.querySelectorAll('input[type="checkbox"][name="PermisosSeleccionados"]').forEach(checkbox => {
          if (permisosSeleccionados.includes(checkbox.value)) {
            checkbox.checked = true;
          } else {
            checkbox.checked = false;
          }
        });

        // Actualiza el botón para que refleje la acción de edición
        const saveButton = document.querySelector('#saveRoleButton');
        saveButton.textContent = 'Editar';
        saveButton.setAttribute('data-editing', 'true');
        saveButton.setAttribute('data-role-id', roleId);
      }
    });
  });
});

document.querySelector('.add-new-role').addEventListener('click', function () {
  document.querySelector('#addRoleModal .role-title').textContent = 'Agregar Rol Nuevo';
  document.querySelector('#RoleName').value = '';
  document.querySelector('#Descripcion').value = '';

  // Actualiza el botón para acción de agregar
  const saveButton = document.querySelector('#saveRoleButton');
  saveButton.textContent = 'Guardar';
  saveButton.removeAttribute('data-editing');
  saveButton.removeAttribute('data-role-id');
});

// Desmarcar los checkboxes cuando el modal se cierre
const modal = document.getElementById('addRoleModal');
modal.addEventListener('hidden.bs.modal', function () {
  document.querySelectorAll('input[name="PermisosSeleccionados"]').forEach(checkbox => {
    checkbox.checked = false;
  });
});











