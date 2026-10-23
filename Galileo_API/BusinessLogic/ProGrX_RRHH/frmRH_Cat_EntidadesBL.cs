using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_RRHH;
using Galileo_API.DataBaseTier.ProGrX_RRHH;

namespace Galileo_API.BusinessLogic.ProGrX_RRHH
{
    /// <summary>
    /// Coordina las operaciones de negocio del catálogo
    /// de entidades relacionadas de Recursos Humanos.
    /// </summary>
    public class FrmRHCatEntidadesBL
    {
        private readonly FrmRHCatEntidadesDB _db;

        /// <summary>
        /// Inicializa la capa de negocio del catálogo
        /// de entidades relacionadas.
        /// </summary>
        /// <param name="config"></param>
        public FrmRHCatEntidadesBL(IConfiguration config)
        {
            _db = new FrmRHCatEntidadesDB(config);
        }

        #region Entidades

        /// <summary>
        /// Obtiene la información general de una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadData> RH_CatEntidades_Obtener(
            int CodEmpresa,
            RhCatEntidadObtenerRequest? request)
        {
            return _db.RH_CatEntidades_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Navega entre las entidades relacionadas registradas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadData> RH_CatEntidades_Scroll_Obtener(
            int CodEmpresa,
            RhCatEntidadScrollRequest? request)
        {
            return _db.RH_CatEntidades_Scroll_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Obtiene las entidades disponibles para el buscador F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<RhCatEntidadBusquedaData>>
            RH_CatEntidades_Busqueda_Obtener(
                int CodEmpresa,
                RhCatEntidadBusquedaRequest? request)
        {
            return _db.RH_CatEntidades_Busqueda_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_CatEntidades_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatEntidadGuardarRequest? request)
        {
            return _db.RH_CatEntidades_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Bancos y cuentas

        /// <summary>
        /// Obtiene los bancos disponibles para el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<RhCatEntidadBancoData>>
            RH_CatEntidades_Bancos_Obtener(
                int CodEmpresa,
                string? usuario)
        {
            return _db.RH_CatEntidades_Bancos_Obtener(
                CodEmpresa,
                usuario);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias asociadas
        /// a la identificación de una entidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadCuentasLista>
            RH_CatEntidades_Cuentas_Obtener(
                int CodEmpresa,
                string? identificacion)
        {
            return _db.RH_CatEntidades_Cuentas_Obtener(
                CodEmpresa,
                identificacion);
        }

        #endregion

        #region Conceptos

        /// <summary>
        /// Obtiene los conceptos activos disponibles
        /// y asignados a una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codEr"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadConceptosLista>
            RH_CatEntidades_Conceptos_Obtener(
                int CodEmpresa,
                string? codEr)
        {
            return _db.RH_CatEntidades_Conceptos_Obtener(
                CodEmpresa,
                codEr);
        }

        /// <summary>
        /// Asigna o desasigna un concepto de nómina
        /// a una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_CatEntidades_Concepto_Actualizar(
            int CodEmpresa,
            string? usuario,
            RhCatEntidadConceptoActualizarRequest? request)
        {
            return _db.RH_CatEntidades_Concepto_Actualizar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion
    }
}