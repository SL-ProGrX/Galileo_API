using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivEstadoProfesionalesDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _Bitacora;
        private readonly int vModulo = 3;

        public FrmVivEstadoProfesionalesDb(IConfiguration config)
           : this(
                 new PortalDB(config),
                 new MSecurityMainDb(config))
        {
        }

        public FrmVivEstadoProfesionalesDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDb = portalDB;
            _Bitacora = dbBitacora;
        }

        /// <summary>
        /// Obtiene la lista de profesionales 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ViviendaContactosData>> ViviendaContactos_Lista_Obtener(int codEmpresa)
        {
            const string query = @"select Identificacion,idContacto,nombre from ViviendaContactos";
            return DbHelper.ExecuteListQuery<ViviendaContactosData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene informacion del estado de un profesional 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idContacto"></param>
        /// <returns></returns>
        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_Obtener(int codEmpresa, int idContacto)
        {
            const string query = @"SELECT IdContacto, IdEmpresa, TipoContacto, Identificacion, Nombre 
            ,Case TipoProfesional  WHEN 'A' THEN 'Abogado' WHEN 'I' THEN 'Ingeniero' ELSE 'Contacto' END AS 'Profesional' 
            ,TipoProfesional, ESTADO,SuspensionInicio, SuspensionCorte, Observacion 
            ,dbo.fxCrd_Viv_Profesional_Suspendido(P.IdContacto) as 'SuspendeActual' 
            from ViviendaContactos P where P.idContacto = @idContacto";
            return DbHelper.ExecuteSingleQuery<ViviendaContactosData>(_portalDb, codEmpresa, query, null, new { idContacto });
        }

        /// <summary>
        /// Obtiene informacion del estado de un profesional por cedula
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_ConsultaExterna_Obtener(int codEmpresa, string cedula)
        {
            const string query = @"select idContacto from ViviendaContactos where Identificacion = @cedula";
            var idContacto = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, query, 0, new { cedula }).Result;

            return VivEstadoProfesionales_Obtener(codEmpresa, idContacto);
        }

        /// <summary>
        /// Suspende a un profesional
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto VivEstadoProfesionales_Suspender(int codEmpresa, string usuario, ViviendaContactosData request)
        {

            const string sql = @"EXEC spCRDVivEstadoContacto_M  @IdContacto, @Estado, 
            @SuspensionInicio, @SuspensionCorte, @Observacion, @SuspendeUsuario;";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    IdContacto = request.idcontacto,
                    Estado = request.estado,
                    SuspensionInicio = request.suspensioninicio,
                    SuspensionCorte = request.suspensioncorte,
                    Observacion = request.observacion,
                    SuspendeUsuario = usuario
                }
            );

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                usuario,
                movimiento: "Aplica - WEB",
                detalle: $"Hipotecario> Suspensión de Persona: {request.idcontacto} estado:{request.estado}"
            );

            return resp;
        }

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _Bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
