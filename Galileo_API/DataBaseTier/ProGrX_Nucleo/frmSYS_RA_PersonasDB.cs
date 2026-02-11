using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysRaPersonasDB
    {
        private readonly int vModulo = 10;
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmSysRaPersonasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Busca los expedientes restringidos de personas según los filtros proporcionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysRaExpedientesData>> SYS_RA_Personas_Buscar(int CodEmpresa, SysExpedienteFiltroData filtros)
        {
            // Normalización defensiva
            var ced = $"%{filtros.cedula?.Trim()}%";
            var nombre = $"%{filtros.nombre?.Trim()}%";

            if (!filtros.vence)
            {
                if (filtros.inicioVenc == null)
                    return DbHelper.CreateErrorResponse("filtros.inicioVenc cannot be null", -1, (List<SysRaExpedientesData>)null!);

                if (filtros.finVenc == null)
                    return DbHelper.CreateErrorResponse("filtros.finVenc cannot be null", -1, (List<SysRaExpedientesData>)null!);

                DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.inicioVenc, System.Globalization.CultureInfo.InvariantCulture);
                DateTimeOffset fecha_fin = DateTimeOffset.Parse(filtros.finVenc, System.Globalization.CultureInfo.InvariantCulture);

                // Rango inclusivo día completo
                var ini = fecha_inicio.Date;
                var fin = fecha_fin.Date.AddDays(1).AddSeconds(-1);

                const string query = @"select *, isnull(Fecha_Vence, '2300/01/01') as 'Vence_Fix'
                                          from vSYS_RA_Casos
                                          where cedula like @ced
                                            and nombre like @nombre
                                            and Estado = @estado
                                            and isnull(Fecha_Vence, '2300/01/01') between @ini and @fin";

                return DbHelper.ExecuteListQuery<SysRaExpedientesData>(_portalDb, CodEmpresa, query, new
                {
                    ced,
                    nombre,
                    estado = filtros.estado,
                    ini,
                    fin
                });
            }

            const string queryAll = @"select *, isnull(Fecha_Vence, '2300/01/01') as 'Vence_Fix'
                                          from vSYS_RA_Casos
                                          where cedula like @ced
                                            and nombre like @nombre
                                            and Estado = @estado";

            return DbHelper.ExecuteListQuery<SysRaExpedientesData>(_portalDb, CodEmpresa, queryAll, new
            {
                ced,
                nombre,
                estado = filtros.estado
            });
        }


        /// <summary>
        /// Guarda o actualiza los datos en el sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="personaId"></param>
        /// <param name="datos"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto SYS_RA_Personas_Guardar(int CodEmpresa, int personaId, SysRaExpedientesData datos, string usuario)
        {
            // Validaciones mínimas para evitar NRE
            var cedula = (datos.cedula ?? string.Empty).Trim();
            if (cedula == "")
                return DbHelper.ErrorResponse("La cédula es requerida", -1);

            var db = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                return connection.QuerySingle<int>(
                    "spSYS_RA_Persona_Add",
                    new
                    {
                        PersonaId = datos.persona_id,
                        Cedula = cedula,
                        Estado = datos.estado,
                        TipoId = datos.tipo_id,
                        Vence = datos.vence && datos.vencimiento != null ? datos.vencimiento.Value.Date : (DateTime?)null,
                        Notas = datos.notas,
                        Usuario = usuario
                    },
                    commandType: CommandType.StoredProcedure);
            });

            if (db.Code != 0)
                return DbHelper.ErrorResponse(db.Description ?? "Error al guardar", -1);

            var personaCreadaId = db.Result;

            // Bitácora solo si el SP fue OK
            var movimiento = (datos.persona_id == 0) ? "Registra - WEB" : "Modifica - WEB";
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Expediente Restringido: {datos.persona_id} Cedula = {cedula}",
                Movimiento = movimiento,
                Modulo = vModulo
            });

            return new ErrorDto
            {
                Code = personaCreadaId,
                Description = "Ok"
            };
        }


        /// <summary>
        /// Obtiene la lista de usuarios según el código de empresa 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_Usuarios_Obtener(int CodEmpresa)
        {
            const string query = @"select CEDULA as 'item',NOMBRE as 'descripcion' from SOCIOS";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query);
        }


        /// <summary>
        /// Obtiene la lista de tipos  
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> SYS_RaTipos_Obtener(int CodEmpresa)
        {
            const string query = @"select rtrim(TIPO_ID) as 'item',rtrim(descripcion) as 'descripcion' from SYS_EXP_TIPOS where Activo = 1 order by TIPO_ID";
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, CodEmpresa, query);
        }


        /// <summary>
        /// Obtiene los casos restringidos por cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SysAutorizacionesData>> SYS_RA_CasosPorCedula_Obtener(int CodEmpresa, string filtro)
        {
            filtro = (filtro == "undefined" ? "" : (filtro ?? ""));

            const string query = @"SELECT Persona_Id, Cedula, NOMBRE, Estado FROM vSYS_RA_Casos where cedula like @ced";

            return DbHelper.ExecuteListQuery<SysAutorizacionesData>(_portalDb, CodEmpresa, query, new
            {
                ced = $"%{filtro.Trim()}%"
            });
        }
    }
}