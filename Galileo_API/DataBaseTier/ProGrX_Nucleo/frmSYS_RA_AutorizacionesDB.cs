using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using Galileo.DataBaseTier;
using Microsoft.Extensions.Configuration;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysRaAutorizacionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;


        public FrmSysRaAutorizacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private ErrorDto TryBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }
        }


        /// <summary>
        /// Consulta de usaurios autorizados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RA_AutorizacionesUsuariosAutorizados_Obtener(int CodEmpresa)
        {
            const string query = @"select USUARIO as item, NOMBRE as descripcion from vSYS_RA_Usuarios_Autorizados";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query);
        }


        /// <summary>
        /// Consulta listado de casos de autorizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_AutorizacionesCasos_Obtener(int CodEmpresa)
        {
            const string query = @"SELECT Persona_Id, Cedula, NOMBRE, Estado FROM vSYS_RA_Casos";
            return DbHelper.ExecuteListQuery<SysAutorizacionesData>(_portalDB, CodEmpresa, query);
        }


        /// <summary>
        /// Consulta de datos de un caso autorizado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="persona_id"></param>
        /// <returns></returns>
        public ErrorDto<SysAutorizacionesData> SYS_RA_AutorizacionesCasosDatos_Obtener(int CodEmpresa, int persona_id)
        {
            const string query = @"select *, isnull(Fecha_Vence, '2300/01/01') as Fecha_Vence_Id from vSYS_RA_Casos where Persona_Id = @id";

            var r = DbHelper.ExecuteSingleQuery<SysAutorizacionesData>(_portalDB, CodEmpresa, query, defaultValue: null, parameters: new { id = persona_id });

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<SysAutorizacionesData> { Code = r.Code, Description = r.Description, Result = null };

            return new ErrorDto<SysAutorizacionesData>
            {
                Code = 0,
                Description = "Ok",
                Result = r.Result ?? new SysAutorizacionesData { persona_id = 0, horas = 0 }
            };
        }


        /// <summary>
        /// Método para guardar autorizacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <param name="clave"></param>
        /// <returns></returns>
        public ErrorDto SYS_RA_Autorizaciones_Autorizar(int CodEmpresa, string usuario, SysAutorizacionesData datos, string clave)
        {
            if (datos == null)
                return DbHelper.ErrorResponse("Datos inválidos");

            var usuarioLocal = (usuario ?? string.Empty).ToUpperInvariant();
            var claveLocal = clave ?? string.Empty;

            var exec = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var id = connection.QuerySingle<int>(
                    "spSYS_RA_Autorizacion",
                    new
                    {
                        PersonaId = datos.persona_id,
                        Horas = datos.horas,
                        Usuario = datos.usuario_autorizado,
                        Notas = datos.notas,
                        Aut_Usuario = usuarioLocal,
                        Aut_Clave = claveLocal
                    },
                    commandType: CommandType.StoredProcedure);

                return id;
            });

            if ((exec.Code ?? -1) != 0)
                return new ErrorDto { Code = exec.Code ?? -1, Description = exec.Description ?? "Error" };

            var autorizacionId = exec.Result;

            var result = new ErrorDto { Code = autorizacionId, Description = "Ok" };

            if (autorizacionId > 0)
            {
                var detalle = $"Autorización: {autorizacionId} Expediente Restringido: {datos.persona_id} Cedula = {datos.cedula}";
                var bit = TryBitacora(CodEmpresa, usuarioLocal, "Registra - WEB", detalle);

                if ((bit.Code ?? -1) != 0)
                    result.Description = bit.Description ?? result.Description;
            }

            return result;
        }

    }
}