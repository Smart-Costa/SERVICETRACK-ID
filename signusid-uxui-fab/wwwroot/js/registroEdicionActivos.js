
    const baseVerFoto = '@Url.Action("VerFotoPorRuta", "Administration")';

    function cargarFotosActivo() {
        const idActivo = document.getElementById("activoGuid").value;
        if (!idActivo) return;

        const urlObtener = '@Url.Action("ObtenerFotosActivo", "Administration")' + `?idActivo=${idActivo}`;

        fetch(urlObtener)
            .then(res => res.json())
            .then(data => {
                data.forEach(foto => {
                    const div = document.getElementById(`foto${foto.slot}`);
                    if (div) {
                        const urlImagen = `${baseVerFoto}?idActivo=${idActivo}&slot=${foto.slot}`;

                        console.log(`Imagen para slot ${foto.slot}:`, urlImagen); 

                        div.style.backgroundImage = `url('${urlImagen}')`;
                        div.style.backgroundSize = "cover";
                        div.style.backgroundPosition = "center";
                    }
                });
            })
            .catch(err => {
                console.error("Error cargando fotos:", err);
            });
    }

    document.getElementById('modalSubirFoto').addEventListener('hidden.bs.modal', function () {
        // Remover backdrop si queda
        const backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(b => b.remove());

        // Limpiar clase del body que bloquea el scroll
        document.body.classList.remove('modal-open');
        document.body.style = ''; // por si queda overflow: hidden

        // También eliminá el padding-right que Bootstrap a veces agrega
        document.body.style.paddingRight = null;
    });

        document.querySelectorAll('.modal').forEach(function(modal) {
        modal.addEventListener('hidden.bs.modal', function () {
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
            document.body.classList.remove('modal-open');
            document.body.style = '';
        });
    });







    document.addEventListener('DOMContentLoaded', function () {
        const picker = flatpickr("#fechaCompra", {
            dateFormat: "Y-m-d"
        });

        document.getElementById('btnCalendario').addEventListener('click', function () {
            picker.open();
        });
    });

document.addEventListener('DOMContentLoaded', function () {
  const picker = flatpickr("#fechaInicial", {
    dateFormat: "Y-m-d"
  });

  document.getElementById('btnCalendario5').addEventListener('click', function () {
    picker.open();
  });
});

document.addEventListener('DOMContentLoaded', function () {
  const picker = flatpickr("#fechaFinal", {
    dateFormat: "Y-m-d"
  });

  document.getElementById('btnCalendario6').addEventListener('click', function () {
    picker.open();
  });
});

    document.addEventListener('DOMContentLoaded', function () {
        const picker = flatpickr("#fechaCapitalizacion", {
            dateFormat: "Y-m-d"
        });

        document.getElementById('btnCalendario2').addEventListener('click', function () {
            picker.open();
        });
    });

    document.addEventListener('DOMContentLoaded', function () {
        const picker = flatpickr("#fechaGarantia", {
            dateFormat: "Y-m-d"
        });

        document.getElementById('btnCalendario3').addEventListener('click', function () {
            picker.open();
        });
    });

    document.querySelector('form').addEventListener('submit', function (e) {
        const accion = document.activeElement.value;

        if (accion === 'buscar') {
            e.preventDefault(); // ❗ prevenir envío
            // Podés ejecutar tu lógica de búsqueda acá si querés
            return;
        }

        // Validación solo para "guardar"
        let hasError = false;

        const fields = [
            { id: 'numeroActivo', name: 'Número de Activo' },
            { id: 'descripcionLarga', name: 'Descripción Larga' },
            { id: 'fechaCompra', name: 'Fecha de Compra' }
        ];

        fields.forEach(field => {
            const input = document.getElementById(field.id);
            const errorDiv = document.getElementById(field.id + 'Error');

            if (!input.value.trim()) {
                errorDiv.textContent = `Requerido*`;
                hasError = true;
            } else {
                errorDiv.textContent = '';
            }
        });

        if (hasError) e.preventDefault();
    });





    $(document).ready(function () {
        // Cargar categorías desde el backend
        $.getJSON('@Url.Content("~/Administration/ObtenerCategorias")', function (data) {
            data.forEach(function (item) {
                $('#selectCategoria').append(
                    $('<option>', {
                        value: item.id,
                        text: item.nombre,
                        'data-descripcion': item.descripcion
                    })
                );
            });
        });

        // Mostrar descripción al seleccionar
        $('#selectCategoria').on('change', function () {
            var descripcion = $('option:selected', this).data('descripcion') || '';
            $('#descripcionCategoria').val(descripcion);
        });
    });

$(document).ready(function () {
  // Cargar categorías desde el backend para la Toma Fisica
  $.getJSON('@Url.Content("~/Administration/ObtenerCategorias")', function (data) {
    data.forEach(function (item) {
      $('#selectCategoriaTomaFisica').append(
        $('<option>', {
          value: item.id,
          text: item.nombre,
        })
      );
    });
  });
});



    $(document).ready(function () {
        // Cargar estados desde el servidor
        $.getJSON('@Url.Content("~/Administration/ObtenerEstados")', function (data) {
            data.forEach(function (item) {
                $('#selectEstado').append(
                    $('<option>', {
                        value: item.id,
                        text: item.nombre,
                        'data-descripcion': item.descripcion
                    })
                );
            });
        });

        // Mostrar descripción cuando se cambia el estado
        $('#selectEstado').on('change', function () {
            var descripcion = $('option:selected', this).data('descripcion') || '';
            $('#descripcionEstado').val(descripcion);
        });
    });

    document.addEventListener("DOMContentLoaded", function () {
        // Cargar empresas desde el backend
        fetch('@Url.Content("~/Administration/ObtenerEmpresas")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectEmpresa');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.nombre;
                    select.appendChild(option);
                });
            });
    });

    document.addEventListener("DOMContentLoaded", function () {
        // Cargar marcas desde el backend
        fetch('@Url.Content("~/Administration/ObtenerMarcas")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectMarca');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.nombre;
                    select.appendChild(option);
                });
            });
    });

    $(document).ready(function () {
        // Cargar marcas
        $.getJSON('@Url.Content("~/Administration/ObtenerMarcas")', function (data) {
            data.forEach(function (item) {
                $('#selectMarca').append(
                    $('<option>', {
                        value: item.id,
                        text: item.nombre
                    })
                );
            });
        });

        // Cuando se seleccione una marca, cargar los modelos correspondientes
        $('#selectMarca').on('change', function () {
            const marcaId = $(this).val();
            const $selectModelo = $('#selectModelo');

            // Resetear y deshabilitar mientras carga
            $selectModelo.html('<option value="">Seleccione un elemento</option>');
            // $selectModelo.prop('disabled', true); // Descomenta si quieres deshabilitarlo mientras carga

            if (marcaId) {
                $.getJSON(`@Url.Content("~/Administration/ObtenerModelosPorMarca")?idMarca=${marcaId}`, function (data) {
                    data.forEach(function (item) {
                        $selectModelo.append(
                            $('<option>', {
                                value: item.id,
                                text: item.nombre
                            })
                        );
                    });
                    $selectModelo.prop('disabled', false);
                });
            }
        });
    });

    document.addEventListener("DOMContentLoaded", function () {
        fetch('@Url.Content("~/Administration/ObtenerCuentasContables")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectCuentaContableDepreciacion');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.nombre;
                    select.appendChild(option);
                });
            });
    });

    document.addEventListener("DOMContentLoaded", function () {
        fetch('@Url.Content("~/Administration/ObtenerCentrosCostos")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectCentroCosto');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.nombre;
                    select.appendChild(option);
                });
            });
    });

    document.addEventListener("DOMContentLoaded", function () {
        fetch('@Url.Content("~/Administration/ObtenerEmpleados")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectEmpleado');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.nombre;
                    select.appendChild(option);
                });
            });
    });

    //Traer empleados para el Usuario Asignado para la Toma Fisica
document.addEventListener("DOMContentLoaded", function () {
  fetch('@Url.Content("~/Administration/ObtenerEmpleados")')
    .then(response => response.json())
    .then(data => {
      const select = document.getElementById('selectUsusarioAsignadoTomaFisica');
      data.forEach(item => {
        const option = document.createElement('option');
        option.value = item.id;
        option.textContent = item.nombre;
        select.appendChild(option);
      });
    });
});

    document.addEventListener("DOMContentLoaded", function () {
        fetch('@Url.Content("~/Administration/ObtenerUbicacionesA")')
            .then(response => response.json())
            .then(data => {
              const select = document.getElementById('selectUbicacionA');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.texto;
                    select.appendChild(option);
                });
            });
    });

    //Traer Unidad Organizativa para Toma Fisica
document.addEventListener("DOMContentLoaded", function () {
  fetch('@Url.Content("~/Administration/ObtenerUnidadOrganizativa")')
    .then(response => response.json())
    .then(data => {
      const select = document.getElementById('selectUnidadOrganizativaTomaFisica');
      data.forEach(item => {
        const option = document.createElement('option');
        option.value = item.id;
        option.textContent = item.texto;
        select.appendChild(option);
      });
    });
});


    //Traer Ubicacion A para Toma Fisica
document.addEventListener("DOMContentLoaded", function () {
  fetch('@Url.Content("~/Administration/ObtenerUbicacionesA")')
    .then(response => response.json())
    .then(data => {
      const select = document.getElementById('selectUbicacionATomaFisica');
      data.forEach(item => {
        const option = document.createElement('option');
        option.value = item.id;
        option.textContent = item.texto;
        select.appendChild(option);
      });
    });
});



    document.addEventListener("DOMContentLoaded", function () {
        const selectA = document.getElementById('selectUbicacionA');
      const selectA = document.getElementById('selectUnidadOrganizativaTomaFisica');
      const selectA = document.getElementById('selectUbicacionATomaFisica');
        const selectB = document.getElementById('selectUbicacionB');
      const selectB = document.getElementById('selectUbicacionBTomaFisica');

        selectA.addEventListener('change', function () {
            const companyId = this.value;

            // Reset y deshabilita el select B
            selectB.innerHTML = '<option value="">Seleccione un elemento</option>';
            //selectB.disabled = true;

            if (companyId) {
                fetch(`@Url.Content("~/Administration/ObtenerUbicacionesB")?idCompany=${companyId}`)
                    .then(response => response.json())
                    .then(data => {
                        data.forEach(item => {
                            const option = document.createElement('option');
                            option.value = item.id;
                            option.textContent = item.texto;
                            selectB.appendChild(option);
                        });
                        //selectB.disabled = false;
                    });
            }
        });
    });

    document.addEventListener("DOMContentLoaded", function () {
        const selectA = document.getElementById('selectUbicacionA');
      const selectA = document.getElementById('selectUnidadOrganizativaTomaFisica');
      const selectA = document.getElementById('selectUbicacionATomaFisica');
        const selectB = document.getElementById('selectUbicacionB');
      const selectB = document.getElementById('selectUbicacionBTomaFisica');
        const selectC = document.getElementById('selectUbicacionC');
      const selectC = document.getElementById('selectUbicacionCTomaFisica');

        function cargarUbicacionC() {
            const companyId = selectA.value;
            const buildingId = selectB.value;

            // Resetear y desactivar el selectC
            selectC.innerHTML = '<option value="">Seleccione un elemento</option>';
           // selectC.disabled = true;

            if (companyId && buildingId) {
                const url = `@Url.Content("~/Administration/ObtenerUbicacionesC")?idCompany=${companyId}&idBuilding=${buildingId}`;

                fetch(url)
                    .then(response => response.json())
                    .then(data => {
                        data.forEach(item => {
                            const option = document.createElement('option');
                            option.value = item.id;
                            option.textContent = item.texto;
                            selectC.appendChild(option);
                        });
                        //selectC.disabled = false;
                    });
            }
        }

        // Escuchar cambios en A y B para cargar C
        selectA.addEventListener('change', cargarUbicacionC);
        selectB.addEventListener('change', cargarUbicacionC);
    });

    document.addEventListener("DOMContentLoaded", function () {
        const selectA = document.getElementById('selectUbicacionA');
      const selectA = document.getElementById('selectUnidadOrganizativaTomaFisica');
      const selectA = document.getElementById('selectUbicacionATomaFisica');
        const selectB = document.getElementById('selectUbicacionB');
      const selectB = document.getElementById('selectUbicacionBTomaFisica');
      const selectB = document.getElementById('selectUbicacionBTomaFisica');
        const selectC = document.getElementById('selectUbicacionC');
      const selectC = document.getElementById('selectUbicacionCTomaFisica');
        const selectD = document.getElementById('selectUbicacionD');
      const selectD = document.getElementById('selectUbicacionDTomaFisica');

        function cargarUbicacionD() {
            const companyId = selectA.value;
            const buildingId = selectB.value;
            const floorId = selectC.value;

            // Reset y desactivar selectD
            selectD.innerHTML = '<option value="">Seleccione un elemento</option>';
           // selectD.disabled = true;

            if (companyId && buildingId && floorId) {
                const url = `@Url.Content("~/Administration/ObtenerUbicacionesD")?idCompany=${companyId}&idBuilding=${buildingId}&idFloor=${floorId}`;

                fetch(url)
                    .then(response => response.json())
                    .then(data => {
                        data.forEach(item => {
                            const option = document.createElement('option');
                            option.value = item.id;
                            option.textContent = item.texto;
                            selectD.appendChild(option);
                        });
                      //  selectD.disabled = false;
                    });
            }
        }

        // Escuchar cambios en A, B y C
        selectA.addEventListener('change', cargarUbicacionD);
        selectB.addEventListener('change', cargarUbicacionD);
        selectC.addEventListener('change', cargarUbicacionD);
    });

    document.addEventListener("DOMContentLoaded", function () {
        fetch('@Url.Content("~/Administration/ObtenerUbicacionesSecundarias")')
            .then(response => response.json())
            .then(data => {
                const select = document.getElementById('selectUbicacionSecundaria');
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.texto;
                    select.appendChild(option);
                });
            });
    });

    document.addEventListener("DOMContentLoaded", function () {
        const valorResidualInput = document.getElementById("valorResidual");
        const checkDepreciado = document.getElementById("checkDepreciado");
        const depreciadoHidden = document.getElementById("depreciadoValue");

        function actualizarDepreciado() {
            const valor = parseFloat(valorResidualInput.value.replace(',', '.')) || 0;

            if (valor === 0) {
                checkDepreciado.checked = true;
                depreciadoHidden.value = "Depreciado";
            } else {
                checkDepreciado.checked = false;
                depreciadoHidden.value = "No depreciado";
            }
        }

        // Al escribir en valor residual
        valorResidualInput.addEventListener("input", actualizarDepreciado);

        // Al cambiar el check manualmente (por si el usuario insiste)
        checkDepreciado.addEventListener("change", function () {
            depreciadoHidden.value = this.checked ? "Depreciado" : "No depreciado";
        });

        actualizarDepreciado(); // Llamar al iniciar en caso de precarga
    });

    function mostrarModal(tipo, mensaje) {
        const deleteModalLabel = document.getElementById('deleteModalLabel');
        const deleteModalBody = document.getElementById('deleteModalBody');
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');

        deleteModalLabel.textContent = (tipo === "success") ? "Éxito" : "Error";
        deleteModalBody.innerHTML = mensaje;
        confirmDeleteBtn.classList.add('d-none'); // ocultar botón borrar si aplica

        const modal = new bootstrap.Modal(document.getElementById('deleteModal'));
        modal.show();

        document.getElementById('deleteModal').addEventListener('hidden.bs.modal', function () {
            history.replaceState(null, '', window.location.pathname);
        });
    }

    

    function cargarMarcaYModelo(marcaId, modeloId) {
        // Asignar la marca
        $('#selectMarca').val(marcaId).trigger('change');

        // Esperar a que los modelos se carguen (puedes usar setTimeout o mejor, una promesa si adaptas el código)
        setTimeout(function () {
            $('#selectModelo').val(modeloId);
        }, 500); // tiempo suficiente para que cargue, ajustable
    }

    document.querySelector('.btn.btn-orange').addEventListener('click', function () {
        const tipoBusqueda = document.getElementById('selectBusquedaActivo').value;
        const valorBusqueda = document.getElementById('busqueda').value;

        if (!tipoBusqueda || !valorBusqueda.trim()) {
            mostrarModal("error", "Debe seleccionar un tipo de búsqueda y escribir un valor.");
            return;
        }

        const url = '@Url.Content("~/Administration/BuscarActivo")' + `?tipo=${tipoBusqueda}&valor=${encodeURIComponent(valorBusqueda)}`;

        fetch(url)
            .then(response => response.json())
                .then(data => {
        console.log("Datos recibidos:", data);
        if (data && data.idActivo) {
          document.getElementById('EstadoFormulario').value = 'Editar';

          habilitarBotonesDocumentos();

            document.getElementById('activoGuid').value = data.idActivo ?? '';
            document.getElementById('numeroActivo').value = data.numeroActivo ?? '';
            document.getElementById('numeroEtiqueta').value = data.numeroEtiqueta ?? '';
            document.getElementById('descripcionCorta').value = data.descripcionCorta ?? '';
            document.getElementById('descripcionLarga').value = data.descripcionLarga ?? '';
          document.getElementById('selectCategoria').value = data.categoria ?? '';
          document.getElementById('selectCategoriaTomaFisica').value = data.categoria ?? '';
            document.getElementById('selectEstado').value = data.estado ?? '';
            document.getElementById('selectEmpresa').value = data.empresa ?? '';

       
            cargarMarcaYModelo(data.marca, data.modelo);

            document.getElementById('numeroFactura').value = data.numeroFactura ?? '';
            document.getElementById('costo').value = data.costo ?? '';
            document.getElementById('color').value = data.color ?? '';
            document.getElementById('fechaCompra').value = data.fechaCompra ?? '';
            document.getElementById('fechaCapitalizacion').value = data.fechaCapitalizacion ?? '';
            document.getElementById('valorResidual').value = data.valorResidual ?? '';
            document.getElementById('numParteFabricante').value = data.numeroParteFabricante ?? '';
            const checkDepreciado = document.getElementById('checkDepreciado');
    const valorDepreciado = data.depreciado ?? '';

    document.getElementById('depreciadoValue').value = valorDepreciado;
     //cargarFotosActivo();

    if (valorDepreciado === "Depreciado") {
        checkDepreciado.checked = true;
    } else {
        checkDepreciado.checked = false;
    }

            document.getElementById('descripcionDepreciado').value = data.descripcionDepreciado ?? '';
            document.getElementById('anosVidaUtil').value = data.anosVidaUtil ?? '';
            document.getElementById('selectCuentaContableDepreciacion').value = data.cuentaContableDepresiacion ?? '';
            document.getElementById('selectCentroCosto').value = data.centroCostos ?? '';
            document.getElementById('descripcionEstadoUltimoInventario').value = data.descripcionEstadoUltimoInventario ?? '';
            document.getElementById('TagEPC').value = data.tagEPC ?? '';
            document.getElementById('selectEmpleado').value = data.empleado ?? '';
          document.getElementById('selectUsusarioAsignadoTomaFisica').value = data.empleado ?? '';
            document.getElementById('selectUbicacionA').value = data.ubicacionA ?? '';
          document.getElementById('selectUnidadOrganizativaTomaFisica').value = data.ubicacionA ?? '';
          document.getElementById('selectUbicacionATomaFisica').value = data.ubicacionA ?? '';
            document.getElementById('selectUbicacionB').value = data.ubicacionB ?? '';
          document.getElementById('selectUbicacionBTomaFisica').value = data.ubicacionB ?? '';
            document.getElementById('fechaGarantia').value = data.fechaGarantia ?? '';
            document.getElementById('selectUbicacionC').value = data.ubicacionC ?? '';
          document.getElementById('selectUbicacionCTomaFisica').value = data.ubicacionC ?? '';
            document.getElementById('selectUbicacionD').value = data.ubicacionD ?? '';
          document.getElementById('selectUbicacionDTomaFisica').value = data.ubicacionD ?? '';
            document.getElementById('selectUbicacionSecundaria').value = data.ubicacionSecundaria ?? '';
            document.getElementById('numeroSerie').value = data.numeroSerie ?? '';
            document.getElementById('tamanioMedida').value = data.tamanioMedida ?? '';
            document.getElementById('observaciones').value = data.observaciones ?? '';
            document.getElementById('Estado_Activo').value = data.estado_Activo ?? '0';
            actualizarVisual(); 

            cargarUbicaciones(data.ubicacionA, data.ubicacionB, data.ubicacionC, data.ubicacionD);


            $('#selectCategoria').trigger('change');
            $('#selectCategoriaTomaFisica').trigger('change');
            $('#selectEstado').trigger('change');


        } else {
         mostrarModal("error", "Activo no encontrado.");
        }
    })

            .catch(error => {
                console.error('Error al buscar activo:', error);
          mostrarModal("error", "Ocurrió un error al buscar el activo.");
            });
    });



    function actualizarVisual() {
        const botonEstado = document.getElementById("activoInactivo");
        const hiddenEstado = document.getElementById("Estado_Activo");

        const estado = hiddenEstado.value;

        if (estado === "1") {
            botonEstado.textContent = "Activo";
            botonEstado.style.color = "#45bdcb";
        } else {
            botonEstado.textContent = "Inactivo";
            botonEstado.style.color = "#898989";
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        const botonEstado = document.getElementById("activoInactivo");
        const hiddenEstado = document.getElementById("Estado_Activo");

        botonEstado.addEventListener("click", function (e) {
            e.preventDefault();
            hiddenEstado.value = hiddenEstado.value === "0" ? "1" : "0";
            actualizarVisual();
        });

        actualizarVisual(); // Estado inicial
    });

    async function cargarUbicaciones(ubicacionA, ubicacionB, ubicacionC, ubicacionD) {
        const selectA = document.getElementById('selectUbicacionA');
      const selectA = document.getElementById('selectUnidadOrganizativaTomaFisica');
      const selectA = document.getElementById('selectUbicacionATomaFisica');
        const selectB = document.getElementById('selectUbicacionB');
      const selectB = document.getElementById('selectUbicacionBTomaFisica');
        const selectC = document.getElementById('selectUbicacionC');
      const selectC = document.getElementById('selectUbicacionCTomaFisica');
        const selectD = document.getElementById('selectUbicacionD');
      const selectD = document.getElementById('selectUbicacionDTomaFisica');

        // Asignar A y cargar B
        selectA.value = ubicacionA;
        await fetch(`@Url.Content("~/Administration/ObtenerUbicacionesB")?idCompany=${ubicacionA}`)
            .then(response => response.json())
            .then(data => {
                selectB.innerHTML = '<option value="">Seleccione un elemento</option>';
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.texto;
                    selectB.appendChild(option);
                });
                selectB.value = ubicacionB;
            });

        // Cargar C
        await fetch(`@Url.Content("~/Administration/ObtenerUbicacionesC")?idCompany=${ubicacionA}&idBuilding=${ubicacionB}`)
            .then(response => response.json())
            .then(data => {
                selectC.innerHTML = '<option value="">Seleccione un elemento</option>';
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.texto;
                    selectC.appendChild(option);
                });
                selectC.value = ubicacionC;
            });

        // Cargar D
        await fetch(`@Url.Content("~/Administration/ObtenerUbicacionesD")?idCompany=${ubicacionA}&idBuilding=${ubicacionB}&idFloor=${ubicacionC}`)
            .then(response => response.json())
            .then(data => {
                selectD.innerHTML = '<option value="">Seleccione un elemento</option>';
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.id;
                    option.textContent = item.texto;
                    selectD.appendChild(option);
                });
                selectD.value = ubicacionD;
            });
    }

    document.getElementById("btnDocAsoc1").addEventListener("click", function () {
        document.getElementById("tipoDoc").value = "1";
        cargarDocumentoExistente(1);
        new bootstrap.Modal(document.getElementById("modalDocumento")).show();
    });

    document.getElementById("btnDocAsoc2").addEventListener("click", function () {
        document.getElementById("tipoDoc").value = "2";
        cargarDocumentoExistente(2);
        new bootstrap.Modal(document.getElementById("modalDocumento")).show();
    });


    document.getElementById("formDocumento").addEventListener("submit", function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        formData.append("idActivo", document.getElementById("activoGuid").value);
        formData.append("tipoDoc", document.getElementById("tipoDoc").value);

        fetch('@Url.Action("SubirDocumentoActivo", "Administration")', {
            method: "POST",
            body: formData
        })
        .then(resp => resp.json())
        .then(data => {
            // 🔄 Referencia segura al modal abierto
            const modalEl = document.getElementById("modalDocumento");
            const modalInstance = bootstrap.Modal.getInstance(modalEl);

            if (data.success) {
                // ✅ Cerrar modal y limpiar
                modalInstance.hide();
                document.getElementById("archivoDocumento").value = "";
                document.getElementById("visorDocumento").innerHTML = "";
                mostrarModal("success", "Documento guardado.");
            } else {
                modalInstance.hide(); // ✅ cerrar también si hay error

                document.getElementById("archivoDocumento").value = "";
                document.getElementById("visorDocumento").innerHTML = "";
                mostrarModal("error", "Error al guardar el documento.");
            }
        })
        .catch(err => {
            const modalEl = document.getElementById("modalDocumento");
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            modalInstance.hide(); // ✅ cerrar en caso de error inesperado también

                document.getElementById("archivoDocumento").value = "";
                document.getElementById("visorDocumento").innerHTML = "";
            console.error(err);
            mostrarModal("error", "El documento está dañado.");
        });
    });




    const verDocumentoUrl = '@Url.Action("VerDocumento", "Administration")';

    function cargarDocumentoExistente(tipo) {
        const idActivo = document.getElementById("activoGuid").value;

        fetch(`@Url.Action("ObtenerDocumentoActivo", "Administration")?id=${idActivo}&tipo=${tipo}`)
            .then(response => response.json())
            .then(data => {
                if (data.success && data.url) {
                    document.getElementById("visorDocumento").innerHTML =
                        `<iframe src="${verDocumentoUrl}?ruta=${encodeURIComponent(data.url)}" width="100%" height="500px"></iframe>`;
                } else {
                    document.getElementById("visorDocumento").innerHTML = "<p>No hay documento asociado</p>";
                }
            });
    }

    document.addEventListener("DOMContentLoaded", function () {
        const estado = document.getElementById("EstadoFormulario").value;
        if (estado === "Insertar") {
            document.getElementById("btnDocAsoc1").disabled = true;
            document.getElementById("btnDocAsoc2").disabled = true;
        }
    });

    function habilitarBotonesDocumentos() {
        document.getElementById("btnDocAsoc1").disabled = false;
        document.getElementById("btnDocAsoc2").disabled = false;
    }





    document.addEventListener("DOMContentLoaded", function () {
        // Asignar evento a cada div para abrir modal
        document.querySelectorAll(".foto-preview").forEach(function (div) {
            div.addEventListener("click", function () {
                const slot = this.getAttribute("data-slot");
                document.getElementById("slotSeleccionado").value = slot;
                const estado = document.getElementById("EstadoFormulario").value;

                if (estado === "Editar") {
                    const modal = new bootstrap.Modal(document.getElementById("modalSubirFoto"));
                    modal.show();
                } else {
                         mostrarModal("error", "Solo puede subir las fotos en modo edición.");
                }
            });
        });

        // Enviar foto al backend
        document.getElementById("formSubirFoto").addEventListener("submit", function (e) {
            e.preventDefault();

            const idActivo = document.getElementById("activoGuid").value;
            const slot = document.getElementById("slotSeleccionado").value;
            const archivo = document.getElementById("fotoArchivo").files[0];

            if (!idActivo) {
                 mostrarModal("error", "Debe guardar el activo antes de subir fotos.");
                return;
            }

            if (!archivo) {
                 mostrarModal("error", "Debe seleccionar una foto.");
                return;
            }

            const formData = new FormData();
            formData.append("archivoFoto", archivo); // Nombre esperado por el backend
            formData.append("tipoDoc", slot);        // Número del slot
            formData.append("idActivo", idActivo);   // ID del activo

            fetch('@Url.Action("SubirFotoActivo", "Administration")', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
               .then(data => {
                    try {
                        if (data.success) {
                            mostrarModal("success", "Foto subida correctamente.");
                            document.getElementById("fotoArchivo").value = "";


                            const slot = document.getElementById("slotSeleccionado").value;
                            const divFoto = document.getElementById(`foto${slot}`);

                            if (divFoto) {
                                divFoto.style.backgroundImage = `url('/${data.ruta}')`;
                                divFoto.style.backgroundSize = "cover";
                                divFoto.style.backgroundPosition = "center";
                            } else {
                                console.warn(`No se encontró el div foto${slot}`);
                            }
                        } else {
                           mostrarModal("error", "No se pudo subir la foto.");
                           document.getElementById("fotoArchivo").value = "";

                        }
                    } catch (error) {
                        console.error("Error al procesar la respuesta:", error);
                        document.getElementById("fotoArchivo").value = "";

                    }
                })
                .catch(err => {
                    console.error("Error al enviar la solicitud:", err);
                      mostrarModal("error", "Error al subir la foto.");
                      document.getElementById("fotoArchivo").value = "";

                });


            bootstrap.Modal.getInstance(document.getElementById("modalSubirFoto")).hide();
        });
    });

    // Parte Razor procesada en el servidor, al cargar la página
    const baseUrlVerFoto = '@Url.Action("VerFotoExistente", "Administration")';

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".foto-preview").forEach(function (div) {
            div.addEventListener("click", function () {
                const slot = this.getAttribute("data-slot");
                document.getElementById("slotSeleccionado").value = slot;
                const estado = document.getElementById("EstadoFormulario").value;
                const idActivo = document.getElementById("activoGuid").value;

                const preview = document.getElementById("previewFoto");
                const mensaje = document.getElementById("mensajeNoFoto");

                if (estado !== "Editar") {
                    mostrarModal("error", "Solo puede subir fotos en modo edición.");
                    return;
                }

                // URL final armada en JS con parámetros
                const url = `${baseUrlVerFoto}?idActivo=${idActivo}&slot=${slot}`;

                fetch(url)
                    .then(response => {
                        if (!response.ok) throw new Error("No encontrada");
                        return response.blob();
                    })
                    .then(blob => {
                        const urlBlob = URL.createObjectURL(blob);
                        preview.src = urlBlob;
                        preview.style.display = "block";
                        mensaje.style.display = "none";
                    })
                    .catch(() => {
                        preview.src = "";
                        preview.style.display = "none";
                        mensaje.style.display = "block";
                    });

                const modal = new bootstrap.Modal(document.getElementById("modalSubirFoto"));
                modal.show();
            });
        });
    });
