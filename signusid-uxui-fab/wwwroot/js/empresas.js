document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('formEmpresa') || document.querySelector('form[asp-action="GuardarEmpresa"]');

  const fields = [
    { id: 'Nombre', type: 'text', required: true },
    { id: 'DireccionEmpresa', type: 'text', required: true },
    { id: 'Email', type: 'email', required: true },
    { id: 'FormaPago', type: 'select', required: true },
    { id: 'Pais', type: 'select', required: true },
  ];

  function showErr(id, msg) {
    const el = document.getElementById('err-' + id);
    if (!el) return;
    el.textContent = msg || 'Obligatorio';
    el.classList.remove('d-none');
  }

  function hideErr(id) {
    const el = document.getElementById('err-' + id);
    if (!el) return;
    el.classList.add('d-none');
  }

  function isEmail(v) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
  }

  function validateField(def) {
    const ctl = document.getElementById(def.id);
    if (!ctl) return true;

    const val = (ctl.value || '').trim();

    if (def.required && val === '') {
      showErr(def.id, 'Obligatorio');
      return false;
    }

    if (def.type === 'email' && val !== '' && !isEmail(val)) {
      showErr(def.id, 'Email inválido');
      return false;
    }

    hideErr(def.id);
    return true;
  }

  if (form) {
    // Submit
    form.addEventListener('submit', function (ev) {
      let ok = true;
      fields.forEach(def => { if (!validateField(def)) ok = false; });
      if (!ok) {
        ev.preventDefault();
        // foco en el primer campo con error
        const firstWithErr = fields.map(f => document.getElementById('err-' + f.id))
          .find(el => el && !el.classList.contains('d-none'));
        if (firstWithErr) {
          const ctlId = firstWithErr.id.replace('err-', '');
          const ctl = document.getElementById(ctlId);
          if (ctl) ctl.focus();
        }
      }
    });

    // Tiempo real
    fields.forEach(def => {
      const ctl = document.getElementById(def.id);
      if (!ctl) return;
      const evt = def.type === 'select' ? 'change' : 'input';
      ctl.addEventListener(evt, () => validateField(def));
      ctl.addEventListener('blur', () => validateField(def));
    });
  }
});



document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('formEmpresa');
  const btnCancel = document.getElementById('btnCancelar');

  function clearSelect(sel, disable = false) {
    if (!sel) return;
    if (sel.options && sel.options.length) sel.selectedIndex = 0;
    sel.value = '';
    if (disable) sel.disabled = true;
    sel.dispatchEvent(new Event('change')); // por si tienes lógica dependiente
  }

  function hideErrors() {
    document.querySelectorAll('[id^="err-"]').forEach(e => e.classList.add('d-none'));
  }

  function resetToInsert() {
    if (!form) return;

    // Reset “duro” (revierte a valores iniciales de la carga)
    form.reset();

    // Limpiar manualmente para NO volver a los valores de edición
    ['Nombre', 'DireccionEmpresa', 'IdentificacionERelacionada', 'DireccionERelacionada',
      'Email', 'Telefono', 'CodigoPostal', 'Identificacion'
    ].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.value = '';
    });

    // Selects
    clearSelect(document.getElementById('EmpresaRelacionadaId'));
    clearSelect(document.getElementById('FormaPago'));
    clearSelect(document.getElementById('CondicionFinanciera'));
    clearSelect(document.getElementById('Pais'));

    // Estado/Ciudad: vaciar y deshabilitar explícitamente
    const estadoSel = document.getElementById('Estado');
    const ciudadSel = document.getElementById('Ciudad');
    if (estadoSel) {
      estadoSel.innerHTML = '<option value="">Seleccione un elemento</option>';
      estadoSel.disabled = true;
    }
    if (ciudadSel) {
      ciudadSel.innerHTML = '<option value="">Seleccione un elemento</option>';
      ciudadSel.disabled = true;
    }

    // Estatus (volver a placeholder)
    clearSelect(document.getElementById('Estatus'));

    // Flags ocultos → Insertar + limpiar IdEmpresa
    const hiddenEstado = form.querySelector('input[name="estadoFomrulario"]');
    const hiddenIdEmp = form.querySelector('input[name="IdEmpresa"]');
    if (hiddenEstado) hiddenEstado.value = 'Insertar';
    if (hiddenIdEmp) hiddenIdEmp.value = '00000000-0000-0000-0000-000000000000';

    // Ocultar mensajes “Obligatorio”
    hideErrors();

    // Foco al primer campo
    const first = document.getElementById('Nombre');
    if (first) first.focus();
  }

  if (btnCancel) btnCancel.addEventListener('click', resetToInsert);
});
