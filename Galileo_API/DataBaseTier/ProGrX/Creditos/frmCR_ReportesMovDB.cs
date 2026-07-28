using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrReportesMovDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrReportesMovDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catalogo de tipos de documento para reportes de movimientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Documentos_Obtener(int codEmpresa)
        {
            const string sqlDocumentos = @"
                select
                    rtrim(Tipo_Documento) as item,
                    rtrim(Descripcion) + space(5) + '[' + rtrim(Tipo_Documento) + ']' as descripcion
                from sif_documentos
                where Tipo_Documento in
                (
                    'FRM','ND','NC','RE','LIQ','RLIQ','PLA','AFR',
                    'CBR','TRA','REA','CAJA','CAJARE','CA'
                )
                order by Descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDocumentos
            );
        }

        /// <summary>
        /// Obtiene el catalogo de conceptos para reportes de movimientos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Conceptos_Obtener(int codEmpresa)
        {
            const string sqlConceptos = @"
                select
                    rtrim(cod_concepto) as item,
                    rtrim(descripcion) as descripcion
                from SIF_conceptos
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlConceptos
            );
        }

        /// <summary>
        /// Obtiene el catalogo de instituciones.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Instituciones_Obtener(int codEmpresa)
        {
            const string sqlInstituciones = @"
                select
                    rtrim(cod_institucion) as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlInstituciones
            );
        }

        /// <summary>
        /// Obtiene el catalogo de grupos, filtrando por linea cuando corresponde.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineaActiva"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Grupos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
        {
            if (lineaActiva && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineaActiva es true.",
                    Result = []
                };
            }

            const string sqlGruposBase = @"
                select
                    rtrim(cod_grupo) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_grupos
                order by descripcion;";

            const string sqlGruposLinea = @"
                select
                    rtrim(R.cod_grupo) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_grupos R
                inner join catalogo_AsignaGrp A
                    on R.cod_grupo = A.cod_grupo
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineaActiva)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlGruposLinea,
                    new
                    {
                        Codigo = codigo.Trim()
                    }
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlGruposBase
            );
        }

        /// <summary>
        /// Obtiene el catalogo de destinos, filtrando por linea cuando corresponde.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="lineaActiva"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Destinos_Obtener(
            int codEmpresa,
            bool lineaActiva,
            string? codigo)
        {
            if (lineaActiva && string.IsNullOrWhiteSpace(codigo))
            {
                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = -1,
                    Description = "Debe enviar el codigo cuando lineaActiva es true.",
                    Result = []
                };
            }

            const string sqlDestinosBase = @"
                select
                    rtrim(cod_destino) as item,
                    rtrim(descripcion) as descripcion
                from catalogo_destinos
                order by descripcion;";

            const string sqlDestinosLinea = @"
                select
                    rtrim(R.cod_destino) as item,
                    rtrim(R.descripcion) as descripcion
                from catalogo_destinos R
                inner join catalogo_destinosAsg A
                    on R.cod_destino = A.cod_destino
                where A.codigo = @Codigo
                order by R.descripcion;";

            if (lineaActiva)
            {
                return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlDestinosLinea,
                    new
                    {
                        Codigo = codigo.Trim()
                    }
                );
            }

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDestinosBase
            );
        }

        /// <summary>
        /// Obtiene el catalogo de lineas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Lineas_Obtener(int codEmpresa)
        {
            const string sqlLineas = @"
                select
                    rtrim(Codigo) as item,
                    rtrim(Descripcion) as descripcion
                from catalogo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlLineas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de oficinas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Oficinas_Obtener(int codEmpresa)
        {
            const string sqlOficinas = @"
                select
                    rtrim(cod_oficina) as item,
                    rtrim(descripcion) as descripcion
                from SIF_Oficinas
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlOficinas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de tipos de garantia.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Garantias_Obtener(int codEmpresa)
        {
            const string sqlGarantias = @"
                select
                    rtrim(Garantia) as item,
                    rtrim(descripcion) as descripcion
                from crd_garantia_tipos
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlGarantias
            );
        }

        /// <summary>
        /// Obtiene el catalogo de divisas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Divisas_Obtener(int codEmpresa)
        {
            const string sqlDivisas = @"
                select
                    rtrim(COD_DIVISA) as item,
                    rtrim(DESCRIPCION) as descripcion
                from vSys_Divisas;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlDivisas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de cargos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Cargos_Obtener(int codEmpresa)
        {
            const string sqlCargos = @"
                select
                    rtrim(COD_CARGO) as item,
                    rtrim(descripcion) as descripcion
                from vCrd_Cargos_Unificados_Lista
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlCargos
            );
        }

        /// <summary>
        /// Obtiene el catalogo de aseguradoras.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Aseguradoras_Obtener(int codEmpresa)
        {
            const string sqlAseguradoras = @"
                select
                    rtrim(COD_ASEGURADORA) as item,
                    rtrim(NOMBRE) as descripcion
                from CRD_POLIZAS_ASEGURADORAS
                order by NOMBRE;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlAseguradoras
            );
        }

        /// <summary>
        /// Obtiene el catalogo de polizas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Polizas_Obtener(int codEmpresa)
        {
            const string sqlPolizas = @"
                select
                    rtrim(COD_POLIZA) as item,
                    rtrim(descripcion) as descripcion
                from CRD_CATALOGO_POLIZAS
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlPolizas
            );
        }

        /// <summary>
        /// Obtiene el catalogo de gestores externos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrReportesMov_Gestores_Obtener(int codEmpresa)
        {
            const string sqlGestores = @"
                select
                    rtrim(Usuario) as item,
                    rtrim(Usuario) as descripcion
                from CBR_USUARIOS
                where OPERADOR_EXTERNO = 1;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sqlGestores
            );
        }

        /// <summary>
        /// Ejecuta el proceso de analisis cubo de movimientos de creditos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrReportesMov_AnalisisCubo_Ejecutar(int codEmpresa, CrReportesMovAnalisisCuboRequest request)
        {            
            const string sqlAnalisisCubo = @"
                exec spCrdMovAnalisisCubo
                    @FechaInicio,
                    @FechaCorte;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sqlAnalisisCubo,
                new
                {
                    FechaInicio = request.Fecha_Inicio.Date,
                    FechaCorte = request.Fecha_Corte.Date.AddDays(1).AddSeconds(-1)
                }
            );

            if (resp.Code < 0)
                return resp;

            return new ErrorDto
            {
                Code = 0,
                Description = "Proceso ejecutado satisfactoriamente..."
            };
        }
    }
}
