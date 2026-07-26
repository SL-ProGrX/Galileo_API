using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoTramitesBl
    {
        private readonly FrmCrSeguimientoTramitesDb _db;

        public FrmCrSeguimientoTramitesBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoTramitesDb(config);
        }

        public ErrorDto<CrSeguimientoTramitesInicializarData> Cr_SeguimientoTramites_Inicializar(
            int codEmpresa,
            string usuario)
            => _db.Cr_SeguimientoTramites_Inicializar(codEmpresa, usuario);

        public ErrorDto<List<CrSeguimientoTramitesBusquedaItem>> Cr_SeguimientoTramites_Buscar(
            int codEmpresa,
            string? cedula,
            string? nombre)
            => _db.Cr_SeguimientoTramites_Buscar(codEmpresa, cedula, nombre);

        public ErrorDto<CrSeguimientoTramitesOperacionData> Cr_SeguimientoTramites_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _db.Cr_SeguimientoTramites_Operacion_Obtener(codEmpresa, operacion);

        public ErrorDto<CrSeguimientoTramitesRecepcionGuardarResult>
            Cr_SeguimientoTramites_Recepcion_Guardar(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGuardarRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Guardar(codEmpresa, request);

        /// <summary>
        /// Busca socios disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionSocioItem>>
            Cr_SeguimientoTramites_Recepcion_Socios_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Socios_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca líneas de crédito disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionLineaItem>>
            Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca promotores disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionPromotorItem>>
            Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca proveedores disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionProveedorItem>>
            Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Obtiene el contexto dependiente de persona y línea de crédito.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData>
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionLineaContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene cálculos y reglas dependientes de la garantía.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionGarantiaContextoData>
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene las cuentas bancarias de la persona para el banco seleccionado.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesOpcionItem>>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionBancoCuentasRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene contratos y cálculos del fondo de garantía.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionFondoContextoData>
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionFondoContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(codEmpresa, request);
    }
}
