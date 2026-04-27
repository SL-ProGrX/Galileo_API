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
        #endregion

        #region Historial del Tramite
        #endregion

        #region Fincas
        #endregion

        #region Notas
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
