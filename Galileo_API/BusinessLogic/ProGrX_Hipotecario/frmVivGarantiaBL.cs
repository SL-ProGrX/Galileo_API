using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivGarantiaBL
    {
        private readonly FrmVivGarantiaDB _db;
        private readonly string pValidaGarantia = "Debe indicar una garantía válida.";

        public FrmVivGarantiaBL(IConfiguration config)
            => _db = new FrmVivGarantiaDB(config);

        #region Principal

        public ErrorDto<FrmVivGarantiaPrincipalResponse> Viv_GarantiaPrincipal_Cargar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
         {
 
            var respOperacion = _db.Viv_GarantiaOperacion_Obtener(codEmpresa, request);
            if (respOperacion.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respOperacion.Code,
                    Description = respOperacion.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            var respProvincias = _db.Viv_GarantiaProvincias_Obtener(codEmpresa);
            if (respProvincias.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respProvincias.Code,
                    Description = respProvincias.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            var respZonas = _db.Viv_GarantiaZonas_Obtener(codEmpresa);
            if (respZonas.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respZonas.Code,
                    Description = respZonas.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            var operacion = respOperacion.Result ?? new FrmVivGarantiaOperacionResponse();
            var cantidadGarantias = 0;

            if (operacion.id_solicitud > 0)
            {
                var respCantidad = _db.Viv_GarantiaCantidadGarantias_Obtener(
                    codEmpresa,
                    operacion.id_solicitud);

                if (respCantidad.Code < 0)
                {
                    return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                    {
                        Code = respCantidad.Code,
                        Description = respCantidad.Description,
                        Result = new FrmVivGarantiaPrincipalResponse()
                    };
                }

                cantidadGarantias = respCantidad.Result;
            }

            bool habilitaAvaluoPosterior =
                (operacion.estadosol ?? string.Empty).Trim() == "F"
                && cantidadGarantias == 0;

            return new ErrorDto<FrmVivGarantiaPrincipalResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaPrincipalResponse
                {
                    operacion = operacion,
                    grados_hipoteca = ObtenerGradosHipoteca(),
                    tipos_poliza = ObtenerTiposPoliza(),
                    provincias = respProvincias.Result ?? [],
                    zonas = respZonas.Result ?? [],
                    habilita_avaluo_posterior = habilitaAvaluoPosterior,
                }
            };
        }

        public ErrorDto<FrmVivGarantiaGuardarResponse> Viv_GarantiaGuardar(
    int codEmpresa,
    FrmVivGarantiaGuardarRequest request)
        {
            var validacion = ValidarGarantiaGuardar(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            validacion = ValidarOperacionPermiteMovimiento(codEmpresa, request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            validacion = ValidarDetalleGradoSiEdita(codEmpresa, request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            validacion = ValidarAvaluoPosteriorSiAplica(codEmpresa, request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var resp = _db.Viv_GarantiaGuardar(codEmpresa, request);
            if (resp.Code < 0)
            {
                return CrearErrorGuardar(resp.Description);
            }

            validacion = GuardarAvaluoPosteriorSiAplica(codEmpresa, request, resp.Result?.id_garantia ?? 0);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            return new ErrorDto<FrmVivGarantiaGuardarResponse>
            {
                Code = 0,
                Description = "Información de garantía guardada correctamente.",
                Result = resp.Result ?? new FrmVivGarantiaGuardarResponse()
            };
        }



        #endregion

        #region General

        public ErrorDto<List<FrmVivGarantiaGeneralItem>> Viv_GarantiaGeneral_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _db.Viv_GarantiaGeneral_Listar(codEmpresa, request);
        }

        #endregion

        #region Garantia

        public ErrorDto<FrmVivGarantiaDetalleResponse> Viv_GarantiaDetalle_Obtener(
    int codEmpresa,
    FrmVivGarantiaDetalleRequest request)
        {
            

            return _db.Viv_GarantiaDetalle_Obtener(codEmpresa, request);
        }


        public ErrorDto<List<DropDownListaGenericaModel>> Viv_GarantiaCantones_Obtener(
    int codEmpresa,
    string provincia)
        {
            if (string.IsNullOrEmpty(provincia))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe indicar una provincia válida.",
                    Result = []
                };
            }

            return _db.Viv_GarantiaCantones_Obtener(codEmpresa, provincia);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Viv_GarantiaDistritos_Obtener(
    int codEmpresa,
    string provincia,
    string canton)
        {
            if (string.IsNullOrEmpty(provincia) || string.IsNullOrEmpty(canton))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe indicar una provincia y cantón válidos.",
                    Result = []
                };
            }

            return _db.Viv_GarantiaDistritos_Obtener(codEmpresa, provincia, canton);
        }

        public ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse> Viv_GarantiaProfesionales_Buscar(
    int codEmpresa,
    FrmVivGarantiaProfesionalesBuscarRequest request)
        {
            string tipo = request.tipo_profesional.Trim().ToUpperInvariant();

            if (tipo != "I" && tipo != "A")
            {
                return new ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un tipo de profesional válido.",
                    Result = new FrmVivGarantiaProfesionalesBuscarResponse()
                };
            }

            var resp = _db.Viv_GarantiaProfesionales_Buscar(
                codEmpresa,
                new FrmVivGarantiaProfesionalesBuscarRequest
                {
                    filtro = request.filtro,
                    tipo_profesional = tipo,
                    first = request.first,
                    rows = request.rows
                });

            if (resp.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = new FrmVivGarantiaProfesionalesBuscarResponse()
                };
            }

            var lista = resp.Result ?? [];

            return new ErrorDto<FrmVivGarantiaProfesionalesBuscarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaProfesionalesBuscarResponse
                {
                    value = lista,
                    total = lista.FirstOrDefault()?.total ?? 0
                }
            };
        }


        #endregion

        #region Derechos

        public ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>> Viv_GarantiaDerechos_Listar(
            int codEmpresa,
            FrmVivGarantiaIdGarantiaRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>>
                {
                    Code = -1,
                    Description = pValidaGarantia,
                    Result = []
                };
            }

            return _db.Viv_GarantiaDerechos_Listar(codEmpresa, request);
        }

        public ErrorDto<FrmVivGarantiaSocioItem> Viv_GarantiaSocio_Obtener(
    int codEmpresa,
    FrmVivGarantiaSocioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return new ErrorDto<FrmVivGarantiaSocioItem>
                {
                    Code = -1,
                    Description = "Debe indicar una cédula válida.",
                    Result = new FrmVivGarantiaSocioItem()
                };
            }

            return _db.Viv_GarantiaSocio_Obtener(codEmpresa, request);
        }

        public ErrorDto<FrmVivGarantiaSociosBuscarResponse> Viv_GarantiaSocios_Buscar(
    int codEmpresa,
    FrmVivGarantiaSociosBuscarRequest request)
        {
            var resp = _db.Viv_GarantiaSocios_Buscar(codEmpresa, request);
            if (resp.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaSociosBuscarResponse>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = new FrmVivGarantiaSociosBuscarResponse()
                };
            }

            var lista = resp.Result ?? [];

            return new ErrorDto<FrmVivGarantiaSociosBuscarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaSociosBuscarResponse
                {
                    value = lista,
                    total = lista.FirstOrDefault()?.total ?? 0
                }
            };
        }

        public ErrorDto Viv_GarantiaDerecho_Guardar(
    int codEmpresa,
    FrmVivGarantiaDerechoGuardarRequest request)
        {
            var validacion = ValidarDerechoGuardar(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var estadoOperacion = _db.Viv_GarantiaEstadoOperacion_Obtener(codEmpresa, request.numero_operacion);
            if (estadoOperacion.Code < 0)
            {
                return new ErrorDto() {
                    Code = -1,
                    Description = $"No se pudo validar el estado de la operación: {estadoOperacion.Description}"
                };
            }

            return _db.Viv_GarantiaDerecho_Guardar(codEmpresa, request);
        }

        public ErrorDto Viv_GarantiaDerecho_Borrar(
            int codEmpresa,
            FrmVivGarantiaDerechoBorrarRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return DbHelper.ErrorResponse(pValidaGarantia);
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse("Debe indicar una cédula válida.");
            }

            var estadoOperacion = _db.Viv_GarantiaEstadoOperacion_Obtener(codEmpresa, request.numero_operacion);
            if (estadoOperacion.Code < 0)
            {
                return new ErrorDto
                {
                     Code = -1,
                     Description = $"No se pudo validar el estado de la operación: {estadoOperacion.Description}"
                } ;
            }

            if ((estadoOperacion.Result ?? string.Empty).Trim() == "F")
            {
                return DbHelper.ErrorResponse("No es posible realizar movimientos para un número de operación en estado FORMALIZADA.");
            }

            return _db.Viv_GarantiaDerecho_Borrar(codEmpresa, request);
        }

        private static ErrorDto ValidarDerechoGuardar(FrmVivGarantiaDerechoGuardarRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return DbHelper.ErrorResponse("Debe indicar una garantía válida.");
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return DbHelper.ErrorResponse("Debe ingresar un número de cédula válido.");
            }

            if (string.IsNullOrWhiteSpace(request.nombre))
            {
                return DbHelper.ErrorResponse("Debe ingresar un nombre de dueño.");
            }

            if (string.IsNullOrWhiteSpace(request.provincia))
            {
                return DbHelper.ErrorResponse("Debe seleccionar una provincia.");
            }

            if (string.IsNullOrWhiteSpace(request.canton))
            {
                return DbHelper.ErrorResponse("Debe seleccionar un cantón.");
            }

            if (request.actualiza != -1 && request.actualiza != 1)
            {
                return DbHelper.ErrorResponse("Debe indicar una acción válida para el dueño.");
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
        }

        #endregion

        #region Historial del Tramite

        public ErrorDto<FrmVivGarantiaHistorialResponse> Viv_GarantiaHistorial_Obtener(
    int codEmpresa,
    FrmVivGarantiaIdGarantiaRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaHistorialResponse>
                {
                    Code = -1,
                    Description = pValidaGarantia,
                    Result = new FrmVivGarantiaHistorialResponse()
                };
            }

            var respIngeniero = _db.Viv_GarantiaHistorial_ObtenerPorTipo(codEmpresa, request.id_garantia, "I");
            if (respIngeniero.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaHistorialResponse>
                {
                    Code = respIngeniero.Code,
                    Description = respIngeniero.Description,
                    Result = new FrmVivGarantiaHistorialResponse()
                };
            }

            var respAbogado = _db.Viv_GarantiaHistorial_ObtenerPorTipo(codEmpresa, request.id_garantia, "A");
            if (respAbogado.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaHistorialResponse>
                {
                    Code = respAbogado.Code,
                    Description = respAbogado.Description,
                    Result = new FrmVivGarantiaHistorialResponse()
                };
            }

            var ingeniero = respIngeniero.Result ?? new FrmVivGarantiaHistorialRawItem();
            var abogado = respAbogado.Result ?? new FrmVivGarantiaHistorialRawItem();

            return new ErrorDto<FrmVivGarantiaHistorialResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaHistorialResponse
                {
                    resumen = new FrmVivGarantiaHistorialResumenResponse
                    {
                        fecha_registro = ingeniero.RegistroFecha,
                        usuario_registro = ingeniero.RegistroUsuario,
                        estado_actual = ingeniero.GEstado
                    },
                    ingeniero = MapearHistorialProfesional(ingeniero, false),
                    abogado = MapearHistorialProfesional(abogado, true)
                }
            };
        }

        private static FrmVivGarantiaHistorialProfesionalResponse MapearHistorialProfesional(
            FrmVivGarantiaHistorialRawItem item,
            bool incluirFirmas)
        {
            return new FrmVivGarantiaHistorialProfesionalResponse
            {
                nombre = item.Nombre,
                estado = item.EstadoProf,
                asignacion_fecha = item.AsignacionFecha,
                asignacion_usuario = item.AsignacionUsuario,
                entrega_fecha = item.EntregaFecha,
                entrega_usuario = item.EntregaUsuario,
                recepcion_fecha = incluirFirmas ? null : item.RecepcionFecha,
                recepcion_usuario = incluirFirmas ? string.Empty : item.RecepcionUsuario,
                firmas_fecha = incluirFirmas ? item.FirmasFecha : null,
                firmas_usuario = incluirFirmas ? item.FirmasUsuario : string.Empty,
                registro_fecha = item.RegistroFechaProf,
                registro_usuario = item.RegistroUsuarioProf
            };
        }

        #endregion

        #region Fincas

        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> Viv_GarantiaFincasAsociadas_Listar(
    int codEmpresa,
    FrmVivGarantiaCargaRequest request)
        {
            if (request.operacion <= 0 && string.IsNullOrWhiteSpace(request.expediente))
            {
                return new ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>>
                {
                    Code = -1,
                    Description = "Debe indicar un número de operación o expediente válido.",
                    Result = []
                };
            }

            return _db.Viv_GarantiaFincasAsociadas_Listar(codEmpresa, request);
        }

        #endregion

        #region Notas

        public ErrorDto<List<FrmVivGarantiaNotaTramiteItem>> Viv_GarantiaNotas_Listar(
    int codEmpresa,
    FrmVivGarantiaNotasRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<List<FrmVivGarantiaNotaTramiteItem>>
                {
                    Code = -1,
                    Description = pValidaGarantia,
                    Result = []
                };
            }

            string tipo = request.tipo.Trim().ToUpperInvariant();
            if (tipo != "A" && tipo != "I")
            {
                return new ErrorDto<List<FrmVivGarantiaNotaTramiteItem>>
                {
                    Code = -1,
                    Description = "Debe indicar un tipo de profesional válido.",
                    Result = []
                };
            }

            var resp = _db.Viv_GarantiaNotas_Listar(
                codEmpresa,
                new FrmVivGarantiaNotasRequest
                {
                    id_garantia = request.id_garantia,
                    tipo = tipo
                });

            if (resp.Code < 0)
            {
                return new ErrorDto<List<FrmVivGarantiaNotaTramiteItem>>
                {
                    Code = resp.Code,
                    Description = resp.Description,
                    Result = []
                };
            }

            return new ErrorDto<List<FrmVivGarantiaNotaTramiteItem>>
            {
                Code = 0,
                Description = string.Empty,
                Result = (resp.Result ?? []).Select(x => new FrmVivGarantiaNotaTramiteItem
                {
                    id_nota = x.IdNota,
                    tipo = x.Tipo,
                    identificacion = x.Identificacion,
                    nombre = x.Nombre,
                    estado = x.Estado,
                    nota = x.Nota,
                    usuario = x.Usuario,
                    fecha = x.Fecha
                }).ToList()
            };
        }

        #endregion

        #region Helpers

        private static List<DropDownListaGenericaModel> ObtenerGradosHipoteca()
        {
            return
            [
                new DropDownListaGenericaModel
                {
                    item = "P",
                    descripcion = "Primer Grado"
                },
                new DropDownListaGenericaModel
                {
                    item = "S",
                    descripcion = "Segundo Grado"
                },
                new DropDownListaGenericaModel
                {
                    item = "T",
                    descripcion = "Tercer Grado"
                }
            ];
        }

        private static List<DropDownListaGenericaModel> ObtenerTiposPoliza()
        {
            return
            [
                new DropDownListaGenericaModel
                {
                    item = "C",
                    descripcion = "Comercial"
                },
                new DropDownListaGenericaModel
                {
                    item = "P",
                    descripcion = "Personal"
                }
            ];
        }

        private static ErrorDto<FrmVivGarantiaGuardarResponse> ValidarGarantiaGuardar(
            FrmVivGarantiaGuardarRequest request)
        {
            if (request.id_garantia <= 0
                    && request.numero_operacion <= 0
                    && string.IsNullOrWhiteSpace(request.expediente))
            {
                return CrearErrorGuardar("Debe indicar una operación o expediente válido.");
            }

            if (string.IsNullOrWhiteSpace(request.numero_finca))
            {
                return CrearErrorGuardar("Debe ingresar un número de finca válido.");
            }

            if (string.IsNullOrWhiteSpace(request.tipo_derecho))
            {
                return CrearErrorGuardar("Debe ingresar un tipo de derecho.");
            }

            if (string.IsNullOrWhiteSpace(request.num_plano_catastro))
            {
                return CrearErrorGuardar("Debe ingresar un número de plano catastro.");
            }

            if (request.area_finca <= 0)
            {
                return CrearErrorGuardar("Debe ingresar el área en metros cuadrados.");
            }

            if (string.IsNullOrWhiteSpace(request.ubicacion_provincia))
            {
                return CrearErrorGuardar("Debe seleccionar una provincia.");
            }

            if (string.IsNullOrWhiteSpace(request.ubicacion_canton))
            {
                return CrearErrorGuardar("Debe seleccionar un cantón.");
            }

            if (!request.id_zona.HasValue || request.id_zona.Value <= 0)
            {
                return CrearErrorGuardar("Debe seleccionar una zona.");
            }

            string grado = request.grado_hipoteca.Trim().ToUpperInvariant();
            if (grado != "P" && grado != "S" && grado != "T")
            {
                return CrearErrorGuardar("Debe seleccionar un grado de hipoteca válido.");
            }

            string tipoPoliza = request.tipo_poliza.Trim().ToUpperInvariant();
            if (tipoPoliza != "P" && tipoPoliza != "C")
            {
                return CrearErrorGuardar("Debe seleccionar un tipo de póliza válido.");
            }

            return new ErrorDto<FrmVivGarantiaGuardarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaGuardarResponse()
            };
        }

        private static ErrorDto<FrmVivGarantiaGuardarResponse> CrearErrorGuardar(string mensaje)
        {
            return new ErrorDto<FrmVivGarantiaGuardarResponse>
            {
                Code = -1,
                Description = mensaje,
                Result = new FrmVivGarantiaGuardarResponse()
            };
        }

        private static ErrorDto<FrmVivGarantiaGuardarResponse> ValidarAvaluoPosterior(
    FrmVivGarantiaAvaluoPosteriorRequest? request)
        {
            if (request is null)
            {
                return CrearErrorGuardar("Debe indicar la información del avalúo posterior.");
            }

            if (request.id_ingeniero <= 0)
            {
                return CrearErrorGuardar("Información de avalúo: el ingeniero no puede estar en blanco.");
            }

            if (request.id_abogado <= 0)
            {
                return CrearErrorGuardar("Información de avalúo: el abogado no puede estar en blanco.");
            }

            if (!request.fecha_inspeccion.HasValue)
            {
                return CrearErrorGuardar("Información de avalúo: debe indicar la fecha de inspección.");
            }

            string tipoPoliza = request.tipo_poliza.Trim().ToUpperInvariant();
            if (tipoPoliza != "P" && tipoPoliza != "C")
            {
                return CrearErrorGuardar("Información de avalúo: debe indicar un tipo de póliza válido.");
            }

            return new ErrorDto<FrmVivGarantiaGuardarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaGuardarResponse()
            };
        }

        private ErrorDto<FrmVivGarantiaGuardarResponse> ValidarOperacionPermiteMovimiento(
    int codEmpresa,
    FrmVivGarantiaGuardarRequest request)
        {
            var estadoOperacion = _db.Viv_GarantiaEstadoOperacion_Obtener(
                codEmpresa,
                request.numero_operacion);

            if (estadoOperacion.Code < 0)
            {
                return CrearErrorGuardar(estadoOperacion.Description);
            }

            string estado = (estadoOperacion.Result ?? string.Empty).Trim();

            if (request.guardar_avaluo_posterior)
            {
                return estado == "F"
                    ? CrearOkGuardar()
                    : CrearErrorGuardar("El avalúo posterior solo aplica para operaciones en estado FORMALIZADA.");
            }

            if (estado == "F")
            {
                return CrearErrorGuardar("No es posible realizar movimientos para un número de operación en estado FORMALIZADA.");
            }

            return CrearOkGuardar();
        }

        private ErrorDto<FrmVivGarantiaGuardarResponse> ValidarDetalleGradoSiEdita(
            int codEmpresa,
            FrmVivGarantiaGuardarRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return CrearOkGuardar();
            }

            var validaDetalle = _db.Viv_GarantiaDetalleGrado_Validar(
                codEmpresa,
                request.id_garantia,
                request.grado_hipoteca.Trim().ToUpperInvariant());

            if (validaDetalle.Code < 0)
            {
                return CrearErrorGuardar(validaDetalle.Description);
            }

            return validaDetalle.Result
                ? CrearOkGuardar()
                : CrearErrorGuardar("Antes de modificar el grado de la garantía, debe revisar el detalle de acreedores.");
        }

        private ErrorDto<FrmVivGarantiaGuardarResponse> ValidarAvaluoPosteriorSiAplica(
            int codEmpresa,
            FrmVivGarantiaGuardarRequest request)
        {
            if (!request.guardar_avaluo_posterior)
            {
                return CrearOkGuardar();
            }

            var validacion = ValidarAvaluoPosterior(request.avaluo_posterior);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            var cantidadGarantias = _db.Viv_GarantiaCantidadGarantias_Obtener(
                codEmpresa,
                request.numero_operacion);

            if (cantidadGarantias.Code < 0)
            {
                return CrearErrorGuardar(cantidadGarantias.Description);
            }

            if (request.id_garantia > 0 || cantidadGarantias.Result > 0)
            {
                return CrearErrorGuardar("El avalúo posterior solo aplica al agregar la primera garantía de una operación formalizada.");
            }

            return ValidarContactosAvaluoPosterior(codEmpresa, request.avaluo_posterior);
        }

        private ErrorDto<FrmVivGarantiaGuardarResponse> ValidarContactosAvaluoPosterior(
            int codEmpresa,
            FrmVivGarantiaAvaluoPosteriorRequest request)
        {
            var existeIngeniero = _db.Viv_GarantiaContacto_Existe(codEmpresa, request.id_ingeniero, "I");
            if (existeIngeniero.Code < 0)
            {
                return CrearErrorGuardar(existeIngeniero.Description);
            }

            if (!existeIngeniero.Result)
            {
                return CrearErrorGuardar("Información de avalúo: el ingeniero no existe.");
            }

            var existeAbogado = _db.Viv_GarantiaContacto_Existe(codEmpresa, request.id_abogado, "A");
            if (existeAbogado.Code < 0)
            {
                return CrearErrorGuardar(existeAbogado.Description);
            }

            return existeAbogado.Result
                ? CrearOkGuardar()
                : CrearErrorGuardar("Información de avalúo: el abogado no existe.");
        }

        private ErrorDto<FrmVivGarantiaGuardarResponse> GuardarAvaluoPosteriorSiAplica(
            int codEmpresa,
            FrmVivGarantiaGuardarRequest request,
            long idGarantia)
        {
            if (!request.guardar_avaluo_posterior || request.avaluo_posterior is null)
            {
                return CrearOkGuardar();
            }

            request.avaluo_posterior.id_garantia = idGarantia;

            var respAvaluo = _db.Viv_GarantiaAvaluoPosterior_Guardar(
                codEmpresa,
                request.avaluo_posterior);

            return respAvaluo.Code < 0
                ? CrearErrorGuardar(respAvaluo.Description)
                : CrearOkGuardar();
        }

        private static ErrorDto<FrmVivGarantiaGuardarResponse> CrearOkGuardar()
        {
            return new ErrorDto<FrmVivGarantiaGuardarResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaGuardarResponse()
            };
        }

        #endregion
    }
}
