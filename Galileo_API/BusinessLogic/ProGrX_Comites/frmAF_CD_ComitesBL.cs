using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;

namespace Galileo_API.BusinessLogic.ProGrX_Comites
{
    public class FrmAfCdComitesBL
    {
        private readonly FrmAfCdComitesDB _db;

        public FrmAfCdComitesBL(IConfiguration config)
        {
            _db = new FrmAfCdComitesDB(config);
        }

        /// <summary>
        /// Obtiene el detalle completo de un comite.
        /// </summary>
        public ErrorDto<AfCdComiteDetalleDto> AfCdComites_Detalle(int codEmpresa, string codComite)
            => _db.AfCdComites_Detalle(codEmpresa, codComite);

        /// <summary>
        /// Lista directores disponibles.
        /// </summary>
        public ErrorDto<List<AfCdDirectorDto>> AfCdComites_DirectoresLista(int codEmpresa)
            => _db.AfCdComites_DirectoresLista(codEmpresa);

        /// <summary>
        /// Busca unidades programaticas por filtro.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarUnidades(int codEmpresa, string filtro)
            => _db.AfCdComites_BuscarUnidades(codEmpresa, filtro);

        /// <summary>
        /// Busca actividades por filtro.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarActividades(int codEmpresa, string filtro)
            => _db.AfCdComites_BuscarActividades(codEmpresa, filtro);

        /// <summary>
        /// Busca ejecutivos por filtro.
        /// </summary>
        public ErrorDto<List<AfCdComiteListaDto>> AfCdComites_BuscarEjecutivos(int codEmpresa, string filtro)
            => _db.AfCdComites_BuscarEjecutivos(codEmpresa, filtro);

        /// <summary>
        /// Busca el comite asociado a una unidad.
        /// </summary>
        public ErrorDto<string?> AfCdComites_BuscarPorUnidad(int codEmpresa, string codigoUp)
            => _db.AfCdComites_BuscarPorUnidad(codEmpresa, codigoUp);

        /// <summary>
        /// Obtiene el comite anterior o siguiente.
        /// </summary>
        public ErrorDto<AfCdComiteDetalleDto?> AfCdComites_Scroll(int codEmpresa, string? codComite, int direccion)
            => _db.AfCdComites_Scroll(codEmpresa, codComite, direccion);

        /// <summary>
        /// Guarda la cabecera del comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_Guardar(int codEmpresa, AfCdComiteGuardarRequest request)
            => _db.AfCdComites_Guardar(codEmpresa, request);

        /// <summary>
        /// Asocia una unidad, actividad o ejecutivo.
        /// </summary>
        public ErrorDto<bool> AfCdComites_Asociar(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
            => _db.AfCdComites_Asociar(codEmpresa, tipo, request);

        /// <summary>
        /// Elimina una asociacion del comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_EliminarAsociacion(int codEmpresa, string tipo, AfCdComiteAsociacionRequest request)
            => _db.AfCdComites_EliminarAsociacion(codEmpresa, tipo, request);

        /// <summary>
        /// Obtiene miembros del comite.
        /// </summary>
        public ErrorDto<List<AfCdComiteMiembroDto>> AfCdComites_Miembros(int codEmpresa, string codComite, bool activos)
            => _db.AfCdComites_Miembros(codEmpresa, codComite, activos);

        /// <summary>
        /// Obtiene datos de un miembro.
        /// </summary>
        public ErrorDto<AfCdComiteMiembroDto?> AfCdComites_DatosMiembro(int codEmpresa, string cedula)
            => _db.AfCdComites_DatosMiembro(codEmpresa, cedula);

        /// <summary>
        /// Guarda datos de un miembro.
        /// </summary>
        public ErrorDto<bool> AfCdComites_GuardarMiembro(int codEmpresa, AfCdComiteMiembroGuardarRequest request)
            => _db.AfCdComites_GuardarMiembro(codEmpresa, request);

        /// <summary>
        /// Elimina un miembro del comite.
        /// </summary>
        public ErrorDto<bool> AfCdComites_EliminarMiembro(int codEmpresa, string codComite, string cedula, string usuario)
            => _db.AfCdComites_EliminarMiembro(codEmpresa, codComite, cedula, usuario);

        /// <summary>
        /// Obtiene historial de miembros.
        /// </summary>
        public ErrorDto<List<AfCdComiteMiembroHistorialDto>> AfCdComites_HistorialMiembros(int codEmpresa, string codComite)
            => _db.AfCdComites_HistorialMiembros(codEmpresa, codComite);

        /// <summary>
        /// Obtiene mensajes vigentes.
        /// </summary>
        public ErrorDto<List<AfCdComiteMensajeDto>> AfCdComites_Mensajes(int codEmpresa, string codComite)
            => _db.AfCdComites_Mensajes(codEmpresa, codComite);
    }
}
