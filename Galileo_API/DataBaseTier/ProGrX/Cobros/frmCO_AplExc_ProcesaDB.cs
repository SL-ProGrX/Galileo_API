using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaDb : CoProcesoDbBase
    {
        private readonly GenProcesoDb.CoAplicacion<CoAplExcProcInformacionData, CoAplExcProcesoEstadoResult> _aplicacion;

        public sealed class ResultadoException : Exception
        {
            public ResultadoException(string mensaje) : base(mensaje) { }
        }

        public FrmCoAplExcProcesaDb(IConfiguration config) : this(new PortalDB(config)) { }

        public FrmCoAplExcProcesaDb(PortalDB portalDB) : base(portalDB)
        {
            _aplicacion = new(_portalDB, _procesoDb, new()
            {
                GuiaSql = "dbo.spCBR_Excedente_Apl_Proceso_Guia_Aplicacion",
                CasoSql = "dbo.spCBR_Excedente_Apl_Proceso_Aplicacion",
                ValidacionSql = "SELECT dbo.fxCBR_Excedente_Apl_Proceso_Valida(@Cedula, @Aplicacion);",
                MensajeInicio = "Preparando la aplicación de excedentes.",
                CedulaMaxima = 20,
                VerificarEtapaGuia = false,
                Cedula = item => item?.cedula,
                CrearExcepcion = mensaje => new ResultadoException(mensaje),
                EsErrorResultado = error => error is ResultadoException,
                CrearError = datos => new CoAplExcProcesoErrorDto
                {
                    numero_caso = datos.Numero,
                    cedula = datos.Cedula,
                    nombre = datos.Item?.nombre ?? string.Empty,
                    mensaje = datos.Mensaje
                },
                AsignarDetalle = (estado, errores) =>
                {
                    estado.DetalleDisponible = true;
                    estado.Errores = errores.Cast<CoAplExcProcesoErrorDto>().ToList();
                },
                CrearResultado = (correctos, pendientes) => new CoAplExcProcesadosResult
                {
                    aplicados = correctos,
                    pendientes = pendientes
                }
            });
        }

        /// <summary>Obtiene la información disponible para esta aplicación.</summary>
        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
            => DbHelper.ExecuteListQuery<CoAplExcProcInformacionData>(
                _portalDB, codEmpresa,
                "exec dbo.spCBR_Excedente_Apl_Proceso_Carga_Informacion",
                commandTimeout: 0);

        /// <summary>Ejecuta la selección y conserva los errores individuales y el avance.</summary>
        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Ejecutar(
            int CodEmpresa, Guid procesoId, ExcedenteAplicarRequest request)
            => ConvertirResultado(_aplicacion.CO_Aplicacion_Ejecutar(CodEmpresa, procesoId,
                new() { Seleccionados = request.Seleccionados, Usuario = request.Usuario }));

        private static ErrorDto<CoAplExcProcesadosResult> ConvertirResultado(ErrorDto<object> origen)
            => new() { Code = origen.Code, Description = origen.Description, Result = (CoAplExcProcesadosResult?)origen.Result };

        /// <summary>Devuelve el detalle temporal o el avance registrado en GEN_PROCESO.</summary>
        public ErrorDto<CoAplExcProcesoEstadoResult> CO_AplExc_Procesa_Estado(
            int codEmpresa, Guid procesoId)
            => _aplicacion.CO_Aplicacion_Estado(codEmpresa, procesoId);
    }
}
