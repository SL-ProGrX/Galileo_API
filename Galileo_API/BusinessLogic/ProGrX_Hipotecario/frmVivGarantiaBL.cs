using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivGarantiaBL
    {
        private readonly FrmVivGarantiaDB _db;

        public FrmVivGarantiaBL(IConfiguration config)
            => _db = new FrmVivGarantiaDB(config);

        #region Principal

        public ErrorDto<FrmVivGarantiaPrincipalResponse> FrmVivGarantiaPrincipal_Cargar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
         {
 
            var respOperacion = _db.FrmVivGarantiaOperacion_Obtener(codEmpresa, request);
            if (respOperacion.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respOperacion.Code,
                    Description = respOperacion.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            var respProvincias = _db.FrmVivGarantiaProvincias_Obtener(codEmpresa);
            if (respProvincias.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respProvincias.Code,
                    Description = respProvincias.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            var respZonas = _db.FrmVivGarantiaZonas_Obtener(codEmpresa);
            if (respZonas.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaPrincipalResponse>
                {
                    Code = respZonas.Code,
                    Description = respZonas.Description,
                    Result = new FrmVivGarantiaPrincipalResponse()
                };
            }

            return new ErrorDto<FrmVivGarantiaPrincipalResponse>
            {
                Code = 0,
                Description = string.Empty,
                Result = new FrmVivGarantiaPrincipalResponse
                {
                    operacion = respOperacion.Result ?? new FrmVivGarantiaOperacionResponse(),
                    grados_hipoteca = ObtenerGradosHipoteca(),
                    tipos_poliza = ObtenerTiposPoliza(),
                    provincias = respProvincias.Result ?? [],
                    zonas = respZonas.Result ?? []
                }
            };
        }

        #endregion

        #region General

        public ErrorDto<List<FrmVivGarantiaGeneralItem>> FrmVivGarantiaGeneral_Listar(
            int codEmpresa,
            FrmVivGarantiaCargaRequest request)
        {
            return _db.FrmVivGarantiaGeneral_Listar(codEmpresa, request);
        }

        #endregion

        #region Garantia

        public ErrorDto<FrmVivGarantiaDetalleResponse> FrmVivGarantiaDetalle_Obtener(
    int codEmpresa,
    FrmVivGarantiaDetalleRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaDetalleResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
                    Result = new FrmVivGarantiaDetalleResponse()
                };
            }

            return _db.FrmVivGarantiaDetalle_Obtener(codEmpresa, request);
        }


        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaCantones_Obtener(
    int codEmpresa,
    FrmVivGarantiaProvinciaRequest request)
        {
            if (request.provincia <= 0)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe indicar una provincia válida.",
                    Result = []
                };
            }

            return _db.FrmVivGarantiaCantones_Obtener(codEmpresa, request);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivGarantiaDistritos_Obtener(
    int codEmpresa,
    FrmVivGarantiaCantonRequest request)
        {
            if (request.provincia <= 0 || request.canton <= 0)
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe indicar una provincia y cantón válidos.",
                    Result = []
                };
            }

            return _db.FrmVivGarantiaDistritos_Obtener(codEmpresa, request);
        }

        #endregion

        #region Derechos

        public ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>> FrmVivGarantiaDerechos_Listar(
            int codEmpresa,
            FrmVivGarantiaIdGarantiaRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<List<FrmVivGarantiaDerechoDuenoItem>>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
                    Result = []
                };
            }

            return _db.FrmVivGarantiaDerechos_Listar(codEmpresa, request);
        }

        public ErrorDto<FrmVivGarantiaSocioItem> FrmVivGarantiaSocio_Obtener(
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

            return _db.FrmVivGarantiaSocio_Obtener(codEmpresa, request);
        }

        public ErrorDto<FrmVivGarantiaSociosBuscarResponse> FrmVivGarantiaSocios_Buscar(
    int codEmpresa,
    FrmVivGarantiaSociosBuscarRequest request)
        {
            var resp = _db.FrmVivGarantiaSocios_Buscar(codEmpresa, request);
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
        #endregion

        #region Historial del Tramite

        public ErrorDto<FrmVivGarantiaHistorialResponse> FrmVivGarantiaHistorial_Obtener(
    int codEmpresa,
    FrmVivGarantiaIdGarantiaRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<FrmVivGarantiaHistorialResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
                    Result = new FrmVivGarantiaHistorialResponse()
                };
            }

            var respIngeniero = _db.FrmVivGarantiaHistorial_ObtenerPorTipo(codEmpresa, request.id_garantia, "I");
            if (respIngeniero.Code < 0)
            {
                return new ErrorDto<FrmVivGarantiaHistorialResponse>
                {
                    Code = respIngeniero.Code,
                    Description = respIngeniero.Description,
                    Result = new FrmVivGarantiaHistorialResponse()
                };
            }

            var respAbogado = _db.FrmVivGarantiaHistorial_ObtenerPorTipo(codEmpresa, request.id_garantia, "A");
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

        public ErrorDto<List<FrmVivGarantiaFincaAsociadaItem>> FrmVivGarantiaFincasAsociadas_Listar(
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

            return _db.FrmVivGarantiaFincasAsociadas_Listar(codEmpresa, request);
        }

        #endregion

        #region Notas

        public ErrorDto<List<FrmVivGarantiaNotaTramiteItem>> FrmVivGarantiaNotas_Listar(
    int codEmpresa,
    FrmVivGarantiaNotasRequest request)
        {
            if (request.id_garantia <= 0)
            {
                return new ErrorDto<List<FrmVivGarantiaNotaTramiteItem>>
                {
                    Code = -1,
                    Description = "Debe indicar una garantía válida.",
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

            var resp = _db.FrmVivGarantiaNotas_Listar(
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

        #endregion
    }
}
