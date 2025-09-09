// wwwroot/js/control-trafico.js

function parseLocalDate(val) {
  if (!val) return null;

  let y, m, d;

  if (val.includes('-')) {
    // Esperado: YYYY-MM-DD
    const parts = val.split('-').map(Number);
    if (parts.length !== 3) return null;
    [y, m, d] = parts;
  } else if (val.includes('/')) {
    // Soporte a mm/dd/yyyy ó dd/mm/yyyy
    const [a, b, c] = val.split('/');
    const n1 = Number(a), n2 = Number(b), n3 = Number(c);
    if (!n3) return null;
    if (n1 > 12) { d = n1; m = n2; y = n3; }        // dd/mm/yyyy
    else if (n2 > 12) { m = n1; d = n2; y = n3; }   // mm/dd/yyyy
    else { m = n1; d = n2; y = n3; }                // por defecto mm/dd/yyyy
  } else {
    return null;
  }

  const dt = new Date(y, (m || 1) - 1, d || 1); // LOCAL
  dt.setHours(0, 0, 0, 0);
  return isNaN(dt) ? null : dt;
}

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('frmControlTrafico');

  // --- Campos y sus contenedores de error ---
  const fields = {
    solicitante: { el: document.getElementById('selectSolicitante'), err: 'solicitanteError', required: true, type: 'select' },
    empresa: { el: document.getElementById('selectEmpresa'), err: 'empresaError', required: true, type: 'select' },
    telefono: { el: document.getElementById('TelefonoServicio'), err: 'telefonoEmailError', required: true, type: 'text' },
    email: { el: document.getElementById('EmailServicio'), err: 'telefonoEmailError2', required: true, type: 'email' },
    asignado: { el: document.getElementById('selectAsignado'), err: 'asignadoError', required: true, type: 'select' },
    razon: { el: document.getElementById('selectRazonServicio'), err: 'razonError', required: true, type: 'select' },
    direccion: { el: document.getElementById('DireccionServicio'), err: 'direccionError', required: true, type: 'text' },
    fecha: { el: document.getElementById('FechaProximoServicio'), err: 'fechaError', required: true, type: 'date' },
    hora: { el: document.getElementById('HoraServicio'), err: 'horaError', required: true, type: 'select' },
    descripcion: { el: document.getElementById('DescripcionIncidente'), err: 'descripcionError', required: true, type: 'text' }
  };

  // --- IDs de errores de grupos ---
  const canalesErrId = 'canalesError';
  const lugarErrId = 'lugarServicioError';
  const proveedorErrId = 'proveedorError'; // <-- nuevo

  // --- Helpers de error ---
  const clearError = id => { const c = document.getElementById(id); if (c) c.innerHTML = ''; };
  const showError = (id, msg) => { const c = document.getElementById(id); if (c) c.innerHTML = `<span class="error-text">${msg}</span>`; };

  // Si por alguna razón no existe parseLocalDate, usa un fallback simple (no pisa tu implementación si ya existe)
  if (typeof window.parseLocalDate !== 'function') {
    window.parseLocalDate = (v) => {
      // espera "YYYY-MM-DD"
      if (!v) return null;
      const [y, m, d] = v.split('-').map(Number);
      if (!y || !m || !d) return null;
      const dt = new Date(y, m - 1, d, 0, 0, 0, 0);
      return isNaN(dt) ? null : dt;
    };
  }

  function validate() {
    let ok = true;

    // Canales: al menos uno
    clearError(canalesErrId);
    const canalesSel = document.querySelectorAll('#channelGroup .chx-canal:checked').length;
    if (canalesSel === 0) { showError(canalesErrId, 'Obligatorio'); ok = false; }

    // Lugar de servicio: debe haber radio seleccionado
    clearError(lugarErrId);
    if (!document.querySelector('input[name="LugarServicio"]:checked')) {
      showError(lugarErrId, 'Obligatorio'); ok = false;
    }

    // Proveedor (GD / SC / S-ID): al menos uno
    clearError(proveedorErrId);
    const proveedorSel = ['flagGD', 'flagSC', 'flagSID']
      .map(id => document.getElementById(id))
      .filter(el => el && el.checked).length;
    if (proveedorSel === 0) {
      showError(proveedorErrId, 'Obligatorio'); ok = false;
    }

    // Campos individuales
    for (const k in fields) {
      const f = fields[k];
      if (!f.required) { clearError(f.err); continue; }

      const val = (f.el?.value ?? '').trim();

      // Si está vacío
      if (!val) {
        const msg =
          f.el?.id === 'EmailServicio' ? 'Email obligatorio' :
            f.el?.id === 'TelefonoServicio' ? 'Teléfono obligatorio' :
              'Obligatorio';
        showError(f.err, msg);
        ok = false;
        continue;
      }

      // Email formato
      if (f.type === 'email') {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!re.test(val)) { showError(f.err, 'Email inválido'); ok = false; }
        else { clearError(f.err); }

        // Teléfono formato
      } else if (f.el?.id === 'TelefonoServicio') {
        // 7-20 caracteres: dígitos, espacios y + ( ) -
        const phoneRe = /^[0-9\s()+-]{7,20}$/;
        if (!phoneRe.test(val)) { showError(f.err, 'Teléfono inválido'); ok = false; }
        else { clearError(f.err); }

      } else {
        clearError(f.err);
      }
    }

    // Fecha no menor a hoy (comparación local)
    {
      const el = fields.fecha.el;
      if (el && el.value) {
        const today = new Date(); today.setHours(0, 0, 0, 0);
        const d = parseLocalDate(el.value);
        if (!d || d < today) {
          showError(fields.fecha.err, 'Debe ser hoy o posterior');
          ok = false;
        } else {
          clearError(fields.fecha.err);
        }
      }
    }

    if (!ok) {
      const firstMsg = document.querySelector('.error-text');
      if (firstMsg) firstMsg.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
    return ok;
  }

  // --- Submit ---
  form?.addEventListener('submit', (e) => {
    if (!validate()) e.preventDefault();
  });

  // --- Limpiar mensajes al cambiar/escribir ---
  Object.values(fields).forEach(f => {
    const evt = (f.type === 'text' || f.type === 'email') ? 'input' : 'change';
    f.el?.addEventListener(evt, () => clearError(f.err));
  });

  // Canales
  document.querySelectorAll('#channelGroup .chx-canal').forEach(ch =>
    ch.addEventListener('change', () => clearError(canalesErrId))
  );

  // Lugar
  document.querySelectorAll('input[name="LugarServicio"]').forEach(r =>
    r.addEventListener('change', () => clearError(lugarErrId))
  );

  // Proveedor (GD/SC/S-ID): limpia error al tocar cualquiera
  document.querySelectorAll('#flagGD, #flagSC, #flagSID').forEach(ch =>
    ch.addEventListener('change', () => clearError(proveedorErrId))
  );

  // (Opcional) Forzar que sólo uno de GD/SC/S-ID pueda estar marcado
  const provChecks = Array.from(document.querySelectorAll('#flagGD, #flagSC, #flagSID'));
  provChecks.forEach(ch => {
    ch.addEventListener('change', () => {
      if (ch.checked) provChecks.forEach(o => { if (o !== ch) o.checked = false; });
      clearError(proveedorErrId);
    });
  });

  // --- Borrar: limpiar todos los mensajes ---
  document.getElementById('btnBorrar')?.addEventListener('click', () => {
    Object.values(fields).forEach(f => clearError(f.err));
    clearError(canalesErrId);
    clearError(lugarErrId);
    clearError(proveedorErrId);
  });
});





document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('frmControlTrafico');
  const estadoInput = document.getElementById('EstadoFormulario');
  const ticketHidden = form?.querySelector('input[name="ticket"]');
  const btnGuardar = form?.querySelector('.form-actions .btn-primary-ux');

  // Helper para selects
  function setSelectValue(selectId, value) {
    const sel = document.getElementById(selectId);
    if (!sel) return;
    // Si la opción aún no existe (raro, pero por si hay filtros), la agregamos on the fly.
    if (value && !Array.from(sel.options).some(o => o.value === value)) {
      const opt = new Option(value, value, true, true);
      sel.add(opt);
    } else {
      sel.value = value || "";
    }
    sel.dispatchEvent(new Event('change'));
  }

  // Helper para flatpickr
  function setDateValue(inputId, value) {
    const inp = document.getElementById(inputId);
    if (!inp) return;
    const fp = inp._flatpickr;
    if (fp) fp.setDate(value || null, true);
    else inp.value = value || '';
  }

  // Helper para hora (select)
  function setTimeValue(selectId, value) {
    const sel = document.getElementById(selectId);
    if (!sel) return;
    if (value && !Array.from(sel.options).some(o => o.value === value)) {
      sel.add(new Option(value, value, true, true));
    }
    sel.value = value || "";
    sel.dispatchEvent(new Event('change'));
  }

  // Rellenar campos con la respuesta
  function fillFormWithData(d) {
    if (!form) return;

    // Estado & ticket
    if (estadoInput) estadoInput.value = 'Editar';
    if (ticketHidden) ticketHidden.value = d.ticket ?? '';

    if (btnGuardar) btnGuardar.textContent = 'Actualizar';

    // Canales (checkboxes)
    form.querySelector('input[name="CanalEmail"]').checked = !!d.canalEmail;
    form.querySelector('input[name="CanalWeb"]').checked = !!d.canalWeb;
    form.querySelector('input[name="CanalPresencial"]').checked = !!d.canalPresencial;
    form.querySelector('input[name="CanalTelefono"]').checked = !!d.canalTelefono;
    form.querySelector('input[name="CanalChatbot"]').checked = !!d.canalChatbot;

    // Selects (GUIDs)
    setSelectValue('selectSolicitante', d.solicitanteId);
    setSelectValue('selectContrato', d.contratoId);   // puede ser null (no obligatorio)
    setSelectValue('selectEmpresa', d.empresaId);
    setSelectValue('selectAsignado', d.asignadoAId);
    setSelectValue('selectRazonServicio', d.razonServicioId);

    // Inputs
    document.getElementById('TelefonoServicio').value = d.telefonoServicio || '';
    document.getElementById('EmailServicio').value = d.emailServicio || '';
    document.getElementById('DireccionServicio').value = d.direccionServicio || '';
    document.getElementById('DescripcionIncidente').value = d.descripcionIncidente || '';

    // Radio LugarServicio
    const radio = form.querySelector(`input[name="LugarServicio"][value="${d.lugarServicio ?? 0}"]`);
    if (radio) radio.checked = true;

    // Fecha / Hora
    setDateValue('FechaProximoServicio', d.fechaProximoServicio);
    setTimeValue('HoraServicio', d.horaServicio);

    // Limpia estados de error y sube al form
    form.querySelectorAll('.traffic-field.invalid').forEach(f => f.classList.remove('invalid'));
    form.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  // Delegación para el botón Editar
  //document.addEventListener('click', function (e) {
  //  const btn = e.target.closest('.btn-edit');
  //  if (!btn) return;

  //  e.preventDefault();

  //  const ticket = btn.dataset.ticket;
  //  if (!ticket) return;

  //  fetch(`/GestionServicio/ObtenerControlTrafico?ticket=${encodeURIComponent(ticket)}`, {
  //    headers: { 'Accept': 'application/json' }
  //  })
  //    .then(r => { if (!r.ok) throw new Error('No encontrado'); return r.json(); })
  //    .then(d => {
  //      if (!d.ok) throw new Error(d.message || 'Error');
  //      fillFormWithData(d);
  //    })
  //    .catch(err => {
  //      console.error(err);
  //      //alert(`No se pudo cargar el ticket #${ticket}`);
  //    });
  //});

  // (ya tienes cableado el botón Borrar para volver a "Insertar")
});


document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('frmControlTrafico');
  const btnBorrar = document.getElementById('btnBorrar');
  const btnGuardar = form?.querySelector('.form-actions .btn-primary-ux');
  const ticketHidden = form?.querySelector('input[name="ticket"]');
  let fpFecha = document.getElementById('FechaProximoServicio')?._flatpickr || null;

  btnBorrar?.addEventListener('click', function () {
    if (!form) return;

    // --- lo que ya tenías ---
    form.reset();
    ['selectSolicitante', 'selectContrato', 'selectEmpresa', 'selectAsignado', 'selectRazonServicio', 'HoraServicio']
      .forEach(id => { const el = document.getElementById(id); if (el) el.selectedIndex = 0; });

    const remoto = form.querySelector('input[name="LugarServicio"][value="0"]');
    if (remoto) remoto.checked = true;

    const inpFecha = document.getElementById('FechaProximoServicio');
    if (inpFecha) { const fp = inpFecha._flatpickr || fpFecha; if (fp) fp.clear(); else inpFecha.value = ''; }

    const estado = document.getElementById('EstadoFormulario');
    if (estado) estado.value = 'Insertar';
    if (ticketHidden) ticketHidden.value = '';
    if (btnGuardar) btnGuardar.textContent = 'Guardar';
    form.querySelectorAll('.traffic-field.invalid').forEach(f => f.classList.remove('invalid'));

    // --- NUEVO: habilitar y poner modo "libre" los canales ---
    const group = document.getElementById('channelGroup');
    const checks = group ? group.querySelectorAll('.chx-canal') : [];
    checks.forEach(ch => {
      ch.disabled = false;                 // quita disabled
      ch.removeAttribute('disabled');      // por si quedaba en el DOM
      ch.style.pointerEvents = '';         // re-activa clicks si había CSS
      ch.closest('label')?.classList.remove('is-disabled');
    });
    group?.classList.remove('channels-locked');

    ['flagGD', 'flagSC', 'flagSID'].forEach(id => {
      const el = document.getElementById(id);
      if (el) el.checked = false;
    });


    // si tienes el script de exclusividad, apaga el modo exclusivo
    if (window.setChannelExclusive) window.setChannelExclusive(false);
  });
});


document.addEventListener('DOMContentLoaded', function () {
  const sel = document.getElementById('HoraServicio');
  if (!sel) return;

  const step = 15; // minutos -> cambia a 5, 10 o 30 si quieres
  for (let h = 0; h < 24; h++) {
    for (let m = 0; m < 60; m += step) {
      const hh = String(h).padStart(2, '0');
      const mm = String(m).padStart(2, '0');
      const opt = document.createElement('option');
      opt.value = `${hh}:${mm}`;
      opt.text = `${hh}:${mm}`;
      sel.add(opt);
    }
  }

  // Preselección (edición/reenvío); usa data-selected o ViewBag
  const preset = sel.getAttribute('data-selected');
  if (preset) sel.value = preset;
});


document.addEventListener('DOMContentLoaded', function () {
  const contratoEl = document.getElementById('trafDelContratoText');
  const ticketEl = document.getElementById('trafDelTicketText');
  const ticketInp = document.getElementById('trafDelTicketInput');
  const btnSubmit = document.getElementById('trafConfirmDeleteBtn');

  // Rellena el modal al abrirse desde cualquier botón que apunte a #trafficDeleteModal
  document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-bs-target="#trafficDeleteModal"]');
    if (!btn) return;

    const contrato = btn.getAttribute('data-contrato') || '—';
    const ticket = btn.getAttribute('data-ticket') || '';

    if (contratoEl) contratoEl.textContent = contrato;
    if (ticketEl) ticketEl.textContent = ticket;
    if (ticketInp) ticketInp.value = ticket;
  }, true);

  // Evitar doble submit
  document.addEventListener('submit', function (e) {
    const form = e.target;
    if (form.matches('form[asp-action="EliminarControlTrafico"]')) {
      if (btnSubmit) { btnSubmit.disabled = true; btnSubmit.textContent = 'Borrando...'; }
    }
  }, true);
});


document.addEventListener('submit', function (e) {
  const form = e.target;
  if (form.matches('form[asp-action="EliminarControlTrafico"]')) {
    const btn = form.querySelector('button[type="submit"]');
    if (btn) { btn.disabled = true; btn.textContent = 'Borrando...'; }
  }
}, true);


document.addEventListener('DOMContentLoaded', function () {
  const group = document.getElementById('channelGroup');
  if (!group) return;

  const checks = group.querySelectorAll('.chx-canal');

  // Enforce single-selection
  checks.forEach(ch => {
    ch.addEventListener('change', () => {
      if (ch.checked) {
        checks.forEach(other => {
          if (other !== ch) other.checked = false;
        });
      }
    });
  });
});



document.addEventListener('DOMContentLoaded', function () {
  const input = document.getElementById('FechaProximoServicio');
  if (!input) return;

  let fp = input._flatpickr;
  if (fp) {
    fp.set('minDate', 'today'); // bloquear selección futura de fechas pasadas (solo para usuario)
  } else {
    fp = flatpickr(input, {
      dateFormat: 'Y-m-d',
      minDate: 'today',
      allowInput: false,
      disableMobile: true
    });
  }

  // Botón calendario
  const btn = document.getElementById('btnCalendarioServicio');
  if (btn) btn.addEventListener('click', () => fp.open());

  // Limpieza inicial: usa el valor seleccionado por flatpickr (evita new Date('YYYY-MM-DD'))
  const today = new Date(); today.setHours(0, 0, 0, 0);
  const selected = fp.selectedDates?.[0] || null;
  if (selected && selected < today) fp.clear();

  // No limpies cuando el cambio es programático (setDate dispara change)
  input.addEventListener('change', (ev) => {
    if (!ev.isTrusted) return; // cambios desde JS (setDate) no se validan aquí
    const sel = fp.selectedDates?.[0] || null;
    if (!sel || sel < today) fp.clear();
  });
});


document.addEventListener('DOMContentLoaded', () => {
  const selHora = document.getElementById('HoraServicio');
  const inpFecha = document.getElementById('FechaProximoServicio');
  if (!selHora || !inpFecha) return;

  // Usa tu parseLocalDate si ya la tienes definida
  const toLocalDate = (str) => {
    if (!str) return null;
    if (typeof parseLocalDate === 'function') return parseLocalDate(str);
    const d = new Date(str);
    return isNaN(d) ? null : d;
  };

  const two = (n) => String(n).padStart(2, '0');
  const sameDay = (a, b) =>
    a && b &&
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate();

  function rebuildHoraOptions() {
    // guarda lo que había seleccionado para respetarlo cuando NO sea hoy
    const presetAttr = selHora.getAttribute('data-selected');
    const presetPrev = selHora.value || (presetAttr || '');

    // limpia
    selHora.innerHTML = '';
    selHora.add(new Option('hh:mm', ''));

    // ¿la fecha es HOY?
    const today = new Date(); today.setHours(0, 0, 0, 0);
    const selDate = toLocalDate(inpFecha.value);
    const isToday = sameDay(selDate, today);

    // si es HOY, agregamos "Ahora (HH:MM)" y lo AUTOSELECCIONAMOS
    let nowValue = null;
    if (isToday) {
      const now = new Date();
      nowValue = `${two(now.getHours())}:${two(now.getMinutes())}`;
      const optNow = new Option(`${nowValue}`, nowValue);
      selHora.add(optNow); // quedará como segunda opción
    }

    // Intervalos regulares (15 min)
    const step = 15; // ajusta si quieres
    for (let h = 0; h < 24; h++) {
      for (let m = 0; m < 60; m += step) {
        const v = `${two(h)}:${two(m)}`;
        if (v === nowValue) continue; // evita duplicar la opción "Ahora"
        selHora.add(new Option(v, v));
      }
    }

    // Selección final:
    if (isToday && nowValue) {
      selHora.value = nowValue;               // ← autoselecciona la hora actual
    } else if (presetPrev) {
      selHora.value = presetPrev;             // respeta valor previo si NO es hoy
    } else {
      selHora.value = '';                     // deja "hh:mm"
    }
  }

  // reconstruye al cargar (por si ya viene fecha)
  rebuildHoraOptions();

  // reconstruye cuando cambie la fecha
  inpFecha.addEventListener('change', rebuildHoraOptions);

  // si usas flatpickr, engancha onChange también
  if (inpFecha._flatpickr && Array.isArray(inpFecha._flatpickr.config.onChange)) {
    inpFecha._flatpickr.config.onChange.push(() => rebuildHoraOptions());
  }

  // por si quieres llamarlo manualmente tras setear la fecha en tu flujo de edición:
  window.rebuildHoraOptions = rebuildHoraOptions;
});
