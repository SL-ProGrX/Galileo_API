using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndProcesaDb : CoProcesoDbBase
    {
        private readonly GenProcesoDb.CoAplicacion<CoAplFndProcInformacionData, CoAplFndProcesoErrorDto, CoAplFndProcesoEstadoResult, CoAplFndProcesadosResult> _aplicacion;

        public sealed class ResultadoException : Exception
        {
            public ResultadoException(string mensaje) : base(mensaje) { }
        }

        public FrmCoAplFndProcesaDb(IConfiguration config) : this(new PortalDB(config)) { }

        public FrmCoAplFndProcesaDb(PortalDB portalDB) : base(portalDB)
        {
            _aplicacion = new(_portalDB, _procesoDb, new()
            {
                GuiaSql = "dbo.spCBR_Fondos_Apl_Proceso_Guia_Aplicacion",
                CasoSql = "dbo.spCBR_Fondos_Apl_Proceso_Aplicacion",
                ValidacionSql = "SELECT dbo.fxCBR_Fondos_Apl_Proceso_Valida(@Cedula, @Aplicacion);",
                MensajeInicio = "Preparando la aplicación de fondos.",
                CedulaMaxima = 15,
                VerificarEtapaGuia = true,
                Cedula = item => item?.cedula,
                CrearExcepcion = mensaje => new ResultadoException(mensaje),
                EsErrorResultado = error => error is ResultadoException,
                CrearError = datos => new CoAplFndProcesoErrorDto
                {
                    numero_caso = datos.Numero,
                    cedula = datos.Cedula,
                    nombre = datos.Item?.nombre ?? string.Empty,
                    mensaje = datos.Mensaje
                },
                AsignarDetalle = (estado, errores) =>
                {
                    estado.DetalleDisponible = true;
                    estado.Errores = errores;
                },
                CrearResultado = (correctos, pendientes) => new CoAplFndProcesadosResult
                {
                    aplicados = correctos,
                    pendientes = pendientes
                }
            });
        }

        /// <summary>Obtiene la información disponible para esta aplicación.</summary>
        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
            => DbHelper.ExecuteListQuery<CoAplFndProcInformacionData>(
                _portalDB, codEmpresa,
                "exec dbo.spCBR_Fondos_Apl_Proceso_Carga_Informacion",
                commandTimeout: 0);

        /// <summary>Ejecuta la selección y conserva los errores individuales y el avance.</summary>
        public ErrorDto<CoAplFndProcesadosResult> CO_AplFnd_Procesa_Ejecutar(
            int codEmpresa, Guid procesoId, FondosAplicarRequest request)
            => _aplicacion.CO_Aplicacion_Ejecutar(codEmpresa, procesoId,
                new() { Seleccionados = request.Seleccionados, Usuario = request.Usuario });

        /// <summary>Devuelve el detalle temporal o el avance registrado en GEN_PROCESO.</summary>
        public ErrorDto<CoAplFndProcesoEstadoResult> CO_AplFnd_Procesa_Estado(
            int codEmpresa, Guid procesoId)
            => _aplicacion.CO_Aplicacion_Estado(codEmpresa, procesoId);
    }
}