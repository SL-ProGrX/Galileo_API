namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    using Galileo.DataBaseTier;
    using Dapper;
    using Galileo.Models.ERROR;
    using Galileo_API.Models.ProGrX.Creditos;
    using System.Text;

    public class FrmCrBitacoraEspecialDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrBitacoraEspecialDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene listado de socios ordenado por nombre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrBitacoraEspecialSocioModel>> CrBitacoraEspecial_Socios_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                Select Cedula, Nombre
                From Socios
                Order By Nombre";

            return DbHelper.ExecuteListQuery<CrBitacoraEspecialSocioModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Obtiene listado de usuarios ordenado por nombre.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrBitacoraEspecialUsuarioModel>> CrBitacoraEspecial_Usuarios_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                Select Nombre, Descripcion
                From Usuarios
                Order By Nombre";

            return DbHelper.ExecuteListQuery<CrBitacoraEspecialUsuarioModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Obtiene listado de movimientos de bitácora especial para módulo créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrBitacoraEspecialMovimientoModel>> CrBitacoraEspecial_Movimientos_Obtener(int CodEmpresa)
        {
            const string sqlQuery = @"
                select MOVIMIENTO, DESCRIPCION
                from US_MOVIMIENTOS_BE
                where MODULO = 3
                order by MOVIMIENTO";

            return DbHelper.ExecuteListQuery<CrBitacoraEspecialMovimientoModel>(
                _portalDb,
                CodEmpresa,
                sqlQuery);
        }

        /// <summary>
        /// Obtiene registros de bitácora especial según filtros seleccionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrBitacoraEspecialRegistroModel>> CrBitacoraEspecial_Registros_Obtener(int CodEmpresa, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                var (sql, parameters) = BuildBitacoraEspecialQuery(request);
                return conn.Query<CrBitacoraEspecialRegistroModel>(sql, parameters).ToList();
            });
        }

        private (string, DynamicParameters) BuildBitacoraEspecialQuery(CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            var sql = new StringBuilder(@"
                select
                    C.id_Credito_SuBit as ID,
                    C.id_solicitud as Id_Solicitud,
                    C.Movimiento,
                    C.Codigo,
                    C.Tipo,
                    C.Detalle,
                    C.Notas,
                    C.Fecha,
                    C.Usuario,
                    C.Revisado_Fecha,
                    C.Revisado_Usuario,
                    S.cedula as Cedula,
                    S.nombre as Nombre,
                    M.Descripcion as MovimientoDesc,
                    case when C.revisado_fecha is null then 0 else 1 end as Revisado
                from credito_subit C
                inner join reg_Creditos R on C.id_solicitud = R.id_solicitud
                inner join Socios S on S.cedula = R.cedula
                inner join US_MOVIMIENTOS_BE M on C.Movimiento = M.Movimiento
                where M.Modulo = 3");

            var parameters = new DynamicParameters();

            AddCedulaFilter(sql, parameters, request);
            AddFechasFilter(sql, parameters, request);
            AddMovimientosFilter(sql, parameters, request);
            AddUsuariosFilter(sql, parameters, request);
            AddTipoFilter(sql, parameters, request);
            AddRevisionFilter(sql, request);
            sql.Append(request.ChkRevision ? " order by C.Revisado_fecha" : " order by C.Fecha");

            return (sql.ToString(), parameters);
        }

        private static void AddCedulaFilter(StringBuilder sql, DynamicParameters parameters, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Cedula))
            {
                sql.Append(" and S.cedula like @Cedula");
                parameters.Add("Cedula", $"%{request.Cedula.Trim()}%");
            }
        }

        private static void AddFechasFilter(StringBuilder sql, DynamicParameters parameters, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            if (!request.ChkFechas && request.Fecha_Inicio.HasValue && request.Fecha_Corte.HasValue)
            {
                var fechaInicio = request.Fecha_Inicio.Value.Date;
                var fechaCorte = request.Fecha_Corte.Value.Date.AddHours(23).AddMinutes(59);
                if (request.ChkRevision)
                    sql.Append(" and C.Revisado_fecha between @FechaInicio and @FechaCorte");
                else
                    sql.Append(" and C.Fecha between @FechaInicio and @FechaCorte");
                parameters.Add("FechaInicio", fechaInicio);
                parameters.Add("FechaCorte", fechaCorte);
            }
        }

        private static void AddMovimientosFilter(StringBuilder sql, DynamicParameters parameters, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            if (request.Movimientos != null && request.Movimientos.Count > 0)
            {
                var movimientos = request.Movimientos
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();
                if (movimientos.Count > 0)
                {
                    sql.Append(" and C.movimiento in @Movimientos");
                    parameters.Add("Movimientos", movimientos);
                }
            }
        }

        private static void AddUsuariosFilter(StringBuilder sql, DynamicParameters parameters, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            if (!request.ChkUsuarios && !string.IsNullOrWhiteSpace(request.Usuario))
            {
                if (request.ChkRevision)
                    sql.Append(" and C.Revisado_Usuario = @Usuario");
                else
                    sql.Append(" and C.Usuario = @Usuario");
                parameters.Add("Usuario", request.Usuario.Trim());
            }
        }

        private static void AddTipoFilter(StringBuilder sql, DynamicParameters parameters, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            var tipo = (request.Tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (tipo == "C" || tipo == "R")
            {
                sql.Append(" and C.Tipo = @Tipo");
                parameters.Add("Tipo", tipo);
            }
        }

        private static void AddRevisionFilter(StringBuilder sql, CrBitacoraEspecialRegistrosObtenerRequest request)
        {
            var revision = (request.Revision ?? string.Empty).Trim().ToUpperInvariant();
            if (revision == "PENDIENTES")
                sql.Append(" and C.Revisado_Fecha is null");
            else if (revision == "REVISADOS")
                sql.Append(" and C.Revisado_Fecha is not null");
        }


        /// <summary>
        /// Asigna usuario y fecha de revisión a un registro de bitácora especial.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrBitacoraEspecial_Asignar(int CodEmpresa, CrBitacoraEspecialAsignarRequest request)
        {
            const string sqlQuery = @"
                update CREDITO_SUBIT
                set revisado_usuario = @Revisado_Usuario,
                    revisado_fecha = dbo.MyGetdate()
                where id_Credito_SuBit = @Id_Credito_SuBit";

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                CodEmpresa,
                sqlQuery,
                new
                {
                    request.Id_Credito_SuBit,
                    Revisado_Usuario = (request.Revisado_Usuario ?? string.Empty).Trim()
                });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = result.Code, Description = result.Description };
            }
            if (result.Result == 0)
            {
                return new ErrorDto { Code = -2, Description = "No se actualizó ningún registro. Verifique el Id_Credito_SuBit." };
            }
            return new ErrorDto { Code = 0, Description = "OK" };
        }
    }
}
