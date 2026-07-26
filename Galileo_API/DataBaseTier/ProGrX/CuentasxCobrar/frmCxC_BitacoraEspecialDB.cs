using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo.Models.Security; 
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCBitacoraEspecialModels;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCBitacoraEspecialDb
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCxC = 31;
        private const string MovModifica = "MODIFICA - WEB";


        public FrmCxCBitacoraEspecialDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);

        }
        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        /// <summary>
        /// Consulta de listado de movimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialMovimientos_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(MOVIMIENTO) as item,rtrim(Descripcion) as descripcion 
                        from US_MOVIMIENTOS_BE where MODULO = @ModuloCxC";

                return conn.Query<DropDownListaGenericaModel>(query, new { ModuloCxC }).ToList();
            });
        }

        /// <summary>
        /// Consulta de listado de personas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialPersonas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(Cedula) as item,rtrim(Nombre) as descripcion 
                        from CXC_PERSONAS ";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Consulta de lista de usuarios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxCBitacoraEspecialUsuarios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select rtrim(Nombre) as item,rtrim(Nombre) as descripcion 
                        from Usuarios ";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        public ErrorDto CxCBitacoraEspecial_Actualizar(int codEmpresa, string usuario, int idBitacora)
        {

            const string sqlUpdate = @"     
                UPDATE CXC_BITACORA_ESPECIAL
                SET revisado_usuario = @usuario,
                    revisado_fecha =dbo.MyGetdate()              
                WHERE BITACORA_ID = @idBitacora;

            ";

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpdate, defaultValue: "",
                parameters: new
                {
                    usuario,
                    idBitacora
                });

            if (upsert.Code != 0)
            {
                return DbHelper.ErrorResponse("No fue posible actualizar.");

            }
            var detalle = $"Bitacora especial id: {idBitacora}";

            LogBitacora(codEmpresa, usuario, detalle, MovModifica);


            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Consulta de bitacora segun filtros
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<BitacoraEspeciaLista> CxCBitacoraEspecialBuscar(
              int codEmpresa,
              BitacoraEspeciaFiltros filtros,
              bool esExportar)
        {

            if (filtros is null)
                return DbHelper.CreateErrorResponse<BitacoraEspeciaLista>("El parámetro 'filtros' es obligatorio.");
            if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
                return DbHelper.CreateErrorResponse<BitacoraEspeciaLista>("El rango de fechas es obligatorio (fecha_inicio/fecha_corte).");

            // 2) Normalizaciones de datos
            NormalizeDates(filtros, out var fi, out var fc);
            var movimientos = ParseLists(filtros);

            var (offset, pageSize) = GetPaging(filtros, esExportar);

            // 3) WHERE seguro (parametrizado)
            var whereSql = BuildWhereSql(filtros, movimientos);

            // 4) SQL (base + count + list)
            const string selectBase = @"
     	        select   C.*,R.cod_concepto, S.cedula,S.nombre,M.Descripcion as MovimientoDesc,case when C.revisado_fecha is null then 0 else 1 end as 'Revisado'
	                from CXC_BITACORA_ESPECIAL C 
		                inner join CXC_CUENTAS R on C.OPERACION = R.OPERACION
	                 inner join CXC_PERSONAS S on S.cedula = R.cedula
	                 inner join US_MOVIMIENTOS_BE M on C.Movimiento = M.Movimiento";

            var orderBySql = BuildOrderBySql(filtros);
            var pagingSql = esExportar ? string.Empty : " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var listSql = $"{selectBase}{whereSql}{orderBySql}{pagingSql}";

            var countSql = $@"SELECT COUNT(1)
                      FROM CXC_BITACORA_ESPECIAL C
                     inner join CXC_CUENTAS R on C.OPERACION = R.OPERACION
	                 inner join CXC_PERSONAS S on S.cedula = R.cedula
	                 inner join US_MOVIMIENTOS_BE M on C.Movimiento = M.Movimiento
                      {whereSql}";


            var ctx = new QueryContext(
                Filtros: filtros,
                FechaInicio: fi,
                FechaCorte: fc,
                movimientos: movimientos,
                EsExportar: esExportar,
                Offset: offset,
                PageSize: pageSize
            );

            var p = BuildParameters(ctx);

            // 6) Ejecutar con DbHelper (usa tu PortalDB que depende de _config)


            var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, countSql, defaultValue: 0, parameters: p);


            var listResp = DbHelper.ExecuteListQuery<BitacoraEspeciaData>(_portalDB, codEmpresa, listSql, parameters: p);


            // 7) Mapear respuesta final
            var result = new BitacoraEspeciaLista
            {
                total = totalResp.Result,

                lista = listResp.Result ?? new List<BitacoraEspeciaData>()
            };

            return DbHelper.CreateOkResponse(result);
        }



        private static void NormalizeDates(
          BitacoraEspeciaFiltros filtros,
          out DateTime fi,
          out DateTime fc)
        {

            fi = DateTime.SpecifyKind(filtros.fecha_inicio.Value.Date, DateTimeKind.Unspecified);
            fc = DateTime.SpecifyKind(filtros.fecha_corte.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
        }

        private static string[] ParseLists(BitacoraEspeciaFiltros filtros)
        {
            var movimientos = (filtros.lista_movimientos ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return movimientos;
        }
        private static string BuildWhereSql(BitacoraEspeciaFiltros filtros, string[] movimientos)
        {
            var where = new List<string>();


            where.Add("M.Modulo = @modulo");

            if (!string.IsNullOrWhiteSpace(filtros.cedula))
                where.Add("S.cedula LIKE '%' + @cedula + '%'");


            if (!filtros.fechasChk)
            {
                if (filtros.revisionChk)
                {
                    where.Add("C.Revisado_fecha  BETWEEN @fecha_inicio AND @fecha_corte");
                }
                else
                {
                    where.Add("C.fecha  BETWEEN @fecha_inicio AND @fecha_corte");
                }
            }

            if (movimientos.Length > 0)
                where.Add("C.movimiento IN @movimientos");

            if (!filtros.usuariosChk && filtros.usuario?.Trim() != "" && filtros.revisionChk)
            {
                where.Add("C.Revisado_Usuario =  @usuario ");
            }
            else if (!filtros.usuariosChk && filtros.usuario?.Trim() != "")
            {
                where.Add("C.Usuario =  @usuario ");
            }


            switch (filtros.revision)
            {
                case "P":
                    where.Add(" C.Revisado_Fecha is null");
                    break;
                case "R":
                    where.Add("C.Revisado_Fecha is not null");
                    break;
            }


            if (!string.IsNullOrWhiteSpace(filtros.filtro))
                where.Add("(C.bitacora_id LIKE '%' + @term + '%' OR C.Operacion LIKE '%' + @term + '%' OR C.Usuario LIKE '%' + @term + '%' OR S.cedula LIKE '%' + @term + '%'" +
                    "OR M.Descripcion LIKE '%' + @term + '%'" +
                    "OR C.notas LIKE '%' + @term + '%'" +
                    "OR S.nombre LIKE '%' + @term + '%'" +
                    "OR C.Revisado_Usuario LIKE '%' + @term + '%')");

            return " WHERE " + string.Join(" AND ", where);
        }
        private static string BuildOrderBySql(BitacoraEspeciaFiltros filtros)
        {
            var filtro = "";
            if (filtros.revisionChk)
            {
                filtro = "C.Revisado_fecha";
            }
            else
            {
                filtro = "  C.fecha";
            }
            var sortField = (filtros.sortField ?? string.Empty).Trim();

            var field = sortField switch
            {
                "bitacora_id" => "bitacora_id",
                "Operacion" => "Operacion",
                "Usuario" => "Usuario",
                "MovimientoDesc" => "MovimientoDesc",
                "Cod_Concepto" => "Cod_Concepto",
                "Detalle" => "Detalle",
                "Cedula" => "Cedula",
                "Nombre" => "Nombre",
                "codigo" => "codigo",
                "Notas" => "Notas",
                "Revisado_Usuario" => "Revisado_Usuario",
                _ => filtro
            };

            var dir = filtros.sortOrder == 0 ? "DESC" : "ASC";
            return $" ORDER BY {field} {dir}";

        }
        private static (int offset, int pageSize) GetPaging(BitacoraEspeciaFiltros filtros, bool esExportar)
        {
            if (esExportar) return (0, 0);
            var pageSize = Math.Max(1, filtros.paginacion.GetValueOrDefault(30));
            var offsetRaw = filtros.pagina.GetValueOrDefault(0);
            var offset = Math.Max(0, offsetRaw);

            return (offset, pageSize);
        }
        private sealed record QueryContext(
        BitacoraEspeciaFiltros Filtros,
        DateTime FechaInicio,
        DateTime FechaCorte,
        string[] movimientos,
        bool EsExportar,
        int Offset,
        int PageSize
        );
        private static DynamicParameters BuildParameters(QueryContext ctx)
        {
            var p = new DynamicParameters();

            p.Add("@cedula", ctx.Filtros.cedula ?? string.Empty);
            p.Add("@usuario", ctx.Filtros.usuario ?? string.Empty);
            p.Add("@term", ctx.Filtros.filtro ?? string.Empty);
            p.Add("@modulo", ModuloCxC);
            if (ctx.movimientos.Length > 0)
                p.Add("@movimientos", ctx.movimientos);

            p.Add("@fecha_inicio", ctx.FechaInicio);
            p.Add("@fecha_corte", ctx.FechaCorte);

            if (!ctx.EsExportar)
            {
                p.Add("@offset", ctx.Offset);
                p.Add("@pageSize", ctx.PageSize);
            }

            return p;
        }


    }
}
