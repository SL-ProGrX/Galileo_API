
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_RRHH;
using Galileo_API.Models.ProGrX_RRHH;

namespace Galileo_API.BusinessLogic.ProGrX_RRHH
{
    /// <summary>
    /// Coordina las operaciones de negocio del catálogo
    /// de conceptos de Recursos Humanos.
    /// </summary>
    public class FrmRHCatConceptosBL
    {
        private readonly FrmRHCatConceptosDB _db;

        /// <summary>
        /// Inicializa la capa de negocio del catálogo de conceptos.
        /// </summary>
        /// <param name="config"></param>
        public FrmRHCatConceptosBL(IConfiguration config)
        {
            _db = new FrmRHCatConceptosDB(config);
        }

        #region Conceptos

        /// <summary>
        /// Obtiene la información inicial del catálogo de conceptos,
        /// incluyendo las opciones de los distintos selectores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptosInicialData>
            RH_Cat_Conceptos_Inicial_Obtener(
                int CodEmpresa)
        {
            return _db.RH_Cat_Conceptos_Inicial_Obtener(
                CodEmpresa);
        }
        public ErrorDto<List<RhCatConceptoEntidadBusquedaData>> RH_Cat_Conceptos_Entidades_F4_Obtener(
        int CodEmpresa,
        RhCatConceptoEntidadBusquedaRequest? request)
        {
            return _db.RH_Cat_Conceptos_Entidades_F4_Obtener(
                CodEmpresa,
                request);
        }
        /// <summary>
        /// Obtiene la información general de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                RhCatConceptoObtenerRequest? request)
        {
            return _db.RH_Cat_Conceptos_Obtener(
                CodEmpresa,
                CodContabilidad,
                request);
        }

        /// <summary>
        /// Obtiene los conceptos disponibles para el buscador F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<RhCatConceptoBusquedaData>>
            RH_Cat_Conceptos_F4_Obtener(
                int CodEmpresa,
                RhCatConceptoBusquedaRequest? request)
        {
            return _db.RH_Cat_Conceptos_F4_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Navega entre los conceptos registrados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Scroll_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                RhCatConceptoScrollRequest? request)
        {
            return _db.RH_Cat_Conceptos_Scroll_Obtener(
                CodEmpresa,
                CodContabilidad,
                request);
        }

        /// <summary>
        /// Genera el siguiente código disponible para un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoCodigoNuevoData>
            RH_Cat_Conceptos_Codigo_Nuevo(
                int CodEmpresa)
        {
            return _db.RH_Cat_Conceptos_Codigo_Nuevo(
                CodEmpresa);
        }

        /// <summary>
        /// Genera la siguiente prioridad para el tipo
        /// de concepto indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoPrioridadNuevaData>
            RH_Cat_Conceptos_Prioridad_Nueva(
                int CodEmpresa,
                RhCatConceptoPrioridadNuevaRequest? request)
        {
            return _db.RH_Cat_Conceptos_Prioridad_Nueva(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Guardar(
            int CodEmpresa,
            int CodContabilidad,
            string? usuario,
            RhCatConceptoGuardarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Guardar(
                CodEmpresa,
                CodContabilidad,
                usuario,
                request);
        }

        /// <summary>
        /// Verifica si un código de concepto está disponible
        /// para registrar un concepto nuevo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Codigo_Validar(
            int CodEmpresa,
            RhCatConceptoCodigoValidarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Codigo_Validar(
                CodEmpresa,
                request);
        }

        #endregion

        #region Tablas

        /// <summary>
        /// Obtiene los rangos de aplicación de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoTablasLista>
            RH_Cat_Conceptos_Tabla_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoTablaListaRequest? request)
        {
            return _db.RH_Cat_Conceptos_Tabla_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un rango de la tabla de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Tabla_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoTablaGuardarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Tabla_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina un rango de la tabla de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Tabla_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoTablaEliminarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Tabla_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Detalle

        /// <summary>
        /// Obtiene los detalles asociados a un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoDetallesLista>
            RH_Cat_Conceptos_Detalle_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoDetalleListaRequest? request)
        {
            return _db.RH_Cat_Conceptos_Detalle_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un detalle de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Detalle_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoDetalleGuardarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Detalle_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina un detalle de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Detalle_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoDetalleEliminarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Detalle_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Exclusiones

        /// <summary>
        /// Obtiene los conceptos excluidos de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatConceptoExclusionesLista>
            RH_Cat_Conceptos_Exclusion_Lista_Obtener(
                int CodEmpresa,
                RhCatConceptoExclusionListaRequest? request)
        {
            return _db.RH_Cat_Conceptos_Exclusion_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra la exclusión de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Exclusion_Guardar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoExclusionGuardarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Exclusion_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina la exclusión de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Exclusion_Eliminar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoExclusionEliminarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Exclusion_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Actualizar Personas

        /// <summary>
        /// Actualiza el concepto de nómina en todas
        /// las personas vinculadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_Cat_Conceptos_Personas_Actualizar(
            int CodEmpresa,
            string? usuario,
            RhCatConceptoPersonasActualizarRequest? request)
        {
            return _db.RH_Cat_Conceptos_Personas_Actualizar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion
    }
}
