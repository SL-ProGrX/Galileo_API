using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    public class CcProcesoMensualReportesEnvioDb
    {

        private readonly MProGrxMain _mProGrx;
        public CcProcesoMensualReportesEnvioDb(IConfiguration config)
        {
            _mProGrx = new MProGrxMain(config);
        }
        public ErrorDto<CcProcesoMensualReporteModel> CcProcesoMensual_ReporteGeneracionF02_Obtener(int codEmpresa, string usuario, decimal vFecha)
        {
            try
            {
                var contexto = CrearContextoReporte( codEmpresa,  usuario,  vFecha);

                var reporte = CrearReporteGeneracionF02( contexto, vFecha);

                return DbHelper.CreateOkResponse(reporte);
            }
            catch (Exception)
            {
                return CrearErrorReporte(
            "Error al obtener los parámetros del reporte de planillas generadas.");
            }
        }
        private CcProcesoMensualReporteContexto CrearContextoReporte(int codEmpresa, string usuario, decimal fechaProceso)
        {
            var globalesResp = _mProGrx.sbSifParametrosInicializa(codEmpresa, usuario);

            return new CcProcesoMensualReporteContexto
            {
                Usuario = usuario,
                FechaTexto = MCobroDb.fxFechaProcesoFormat(fechaProceso),
                NombreEmpresa = globalesResp?.Result?.GstrNombreEmpresa ?? string.Empty,
                NombreInstitucion = globalesResp?.Result?.GNombreInstitucion ?? string.Empty,
                CodInstitucion = globalesResp?.Result?.GInstitucion ?? 0
            };
        }
        private static CcProcesoMensualReporteModel CrearReporteGeneracionF02(CcProcesoMensualReporteContexto contexto, decimal fechaProceso)
        {
            return new CcProcesoMensualReporteModel
            {
                NombreReporte = "Sys_Planilla_Generada",
                Titulo = "Planillas - Información Generada",
                Fecha = contexto.FechaTexto,
                Empresa = contexto.NombreEmpresa,
                Usuario = contexto.Usuario,
                Institucion = contexto.NombreInstitucion,
                Filtros = $"PRM_PLANILLA.PROCESO = {fechaProceso} AND PRM_PLANILLA.COD_INSTITUCION = {contexto.CodInstitucion}"

            };
        }
        private static ErrorDto<CcProcesoMensualReporteModel> CrearErrorReporte(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CcProcesoMensualReporteModel>(
                mensaje,
                -1,
                new CcProcesoMensualReporteModel());
        }
        private sealed class CcProcesoMensualReporteContexto
        {
            public string Usuario { get; set; } = string.Empty;
            public string FechaTexto { get; set; } = string.Empty;
            public string NombreEmpresa { get; set; } = string.Empty;
            public string NombreInstitucion { get; set; } = string.Empty;
            public int CodInstitucion { get; set; } = 0;
        }
    }
}
