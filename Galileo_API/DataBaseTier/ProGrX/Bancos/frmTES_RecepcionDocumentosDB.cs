using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesRecepcionDocumentosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb DBBitacora;

        public FrmTesRecepcionDocumentosDB(IConfiguration config)
        {
            DBBitacora = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener ubicaciones para dropdown
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_RecepcionDoc_Ubicaciones_Obtener(int CodEmpresa, string Usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select cod_ubicacion as item, rtrim(cod_ubicacion) + ' - ' + descripcion as descripcion from tes_ubicaciones
                        where usuario = @usuario order by cod_ubicacion";

                return conn.Query<DropDownListaGenericaModel>(query, new { usuario = Usuario }).ToList();
            });
        }

        /// <summary>
        /// Obtener remesa mediante navegacion por scroll,
        /// busca la remesa siguiente o anterior mediante el scrollCode
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollCode"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Scroll_Obtener(
    int CodEmpresa,
    int scrollCode,
    int Remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                // 1 = siguiente (>)
                // 2 (o cualquier otro) = anterior (<)
                const string NextSql = @"
            SELECT TOP (1) cod_remesa
            FROM tes_ubi_remesa
            WHERE cod_remesa > @codigo
            ORDER BY cod_remesa ASC;";

                const string PrevSql = @"
            SELECT TOP (1) cod_remesa
            FROM tes_ubi_remesa
            WHERE cod_remesa < @codigo
            ORDER BY cod_remesa DESC;";

                var sql = scrollCode == 1 ? NextSql : PrevSql;

                var codRemesa = conn.QueryFirstOrDefault<int?>(sql, new { codigo = Remesa });

                if (!codRemesa.HasValue)
                    return DbHelper.CreateOkResponse(new TesUbiRemesaDto());

                return TES_RecepcionDoc_Remesa_Obtener(CodEmpresa, codRemesa.Value);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesUbiRemesaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener información de la remesa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TesUbiRemesaDto> TES_RecepcionDoc_Remesa_Obtener(int CodEmpresa, int Remesa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = @"select R.*,rtrim(X.cod_ubicacion) + ' - ' + X.descripcion as OUbicacion
                        ,rtrim(Y.cod_ubicacion) + ' - ' + Y.descripcion as DUbicacion
                        from tes_ubi_remesa R inner join tes_ubicaciones X on R.cod_ubicacion = X.cod_ubicacion
                        inner join tes_ubicaciones Y on R.cod_ubicacion_Destino = Y.cod_ubicacion
                        where R.cod_remesa = @codigo";
                var result = conn.QueryFirstOrDefault<TesUbiRemesaDto>(query,
                    new { codigo = Remesa });

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<TesUbiRemesaDto>("No se encontró registro verifique...");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TesUbiRemesaDto>(ex.Message);
            }
        }

        /// <summary>
        /// Obtener información de las solicitudes para recepción de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Remesa"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_RecepcionDocumentos_Obtener(
    int CodEmpresa,
    int Remesa,
    FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var response = new TablasListaGenericaModel();

                // Normaliza entrada (evita negativos, etc.)
                var pagina = Math.Max(0, filtros?.pagina ?? 0);
                var paginacion = Math.Max(1, filtros?.paginacion ?? 10);

                // Filtro de texto seguro
                var rawFilter = filtros?.filtro?.Trim();
                var hasFilter = !string.IsNullOrWhiteSpace(rawFilter);

                // Ojo: si tu BD es SQL Server, este patrón es correcto.
                var countSql = @"
            SELECT COUNT(C.nsolicitud)
            FROM Tes_Transacciones C
            INNER JOIN tes_ubi_remDet D ON C.nsolicitud = D.nsolicitud
            INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_Banco
            INNER JOIN TES_Tipos_Doc T ON C.Tipo = T.tipo
            WHERE D.cod_remesa = @codigo
        ";

                var sql = new System.Text.StringBuilder(@"
            SELECT C.nsolicitud, C.id_banco, C.tipo, C.ndocumento,
                   D.estado, D.observacion, D.observa_rec,
                   B.descripcion AS BancoX, T.descripcion AS TipoX,
                   D.fecha_rec, D.usuario_rec
            FROM Tes_Transacciones C
            INNER JOIN tes_ubi_remDet D ON C.nsolicitud = D.nsolicitud
            INNER JOIN Tes_Bancos B ON C.id_Banco = B.id_Banco
            INNER JOIN TES_Tipos_Doc T ON C.Tipo = T.tipo
            WHERE D.cod_remesa = @codigo
        ");

                var parameters = new Dapper.DynamicParameters();
                parameters.Add("@codigo", Remesa);

                if (hasFilter)
                {
                    // Usa un único parámetro para todas las columnas
                    // (Dapper lo reutiliza sin problema)
                    parameters.Add("@filter", $"%{rawFilter}%");

                    // Si esas columnas son numéricas en BD, quita el LIKE o castea explícitamente.
                    sql.Append(@"
                AND (
                    CAST(C.nsolicitud AS VARCHAR(50)) LIKE @filter
                    OR C.ndocumento LIKE @filter
                    OR CAST(C.tipo AS VARCHAR(50)) LIKE @filter
                    OR D.observa_rec LIKE @filter
                    OR D.usuario_rec LIKE @filter
                    OR CAST(C.id_banco AS VARCHAR(50)) LIKE @filter
                )
            ");

                    // Para que el total coincida con el filtro, aplicamos lo mismo al COUNT
                    countSql += @"
                AND (
                    CAST(C.nsolicitud AS VARCHAR(50)) LIKE @filter
                    OR C.ndocumento LIKE @filter
                    OR CAST(C.tipo AS VARCHAR(50)) LIKE @filter
                    OR D.observa_rec LIKE @filter
                    OR D.usuario_rec LIKE @filter
                    OR CAST(C.id_banco AS VARCHAR(50)) LIKE @filter
                )
            ";
                }

                sql.Append(@"
            ORDER BY C.nsolicitud DESC
            OFFSET @offset ROWS
            FETCH NEXT @take ROWS ONLY;
        ");

                parameters.Add("@offset", pagina);
                parameters.Add("@take", paginacion);

                response.total = conn.QueryFirstOrDefault<int>(countSql, parameters);
                response.lista = conn.Query<TesRecepcionDocumentoDto>(sql.ToString(), parameters).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>(ex.Message);
            }
        }

        /// <summary>
        /// Aplicar la recepción de documentos 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto TES_RecepcionDocumentos_Aplicar(int CodEmpresa, TesRecepcionDocumentoFiltros parametros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                foreach (var item in parametros.solicitudes)
                {
                    var query = @"update tes_ubi_remDet set observa_rec = @notas,fecha_rec = dbo.MyGetdate(), 
                        usuario_rec = @usuario, estado = @estado  where cod_remesa = @remesa and Nsolicitud = @solicitud";
                    conn.Execute(query,
                    new
                    {
                        notas = item.observa_rec,
                        parametros.usuario,
                        estado = item.estado ? 1 : 0,
                        remesa = parametros.cod_remesa,
                        solicitud = item.nsolicitud
                    });
                }

                var queryUpdate = @"update tes_ubi_remesa set estado = 'R' where cod_remesa = @remesa";
                conn.Query<TesRecepcionDocumentoDto>(queryUpdate,
                    new { remesa = parametros.cod_remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = parametros.usuario.ToUpper(),
                    DetalleMovimiento = "Recepcion de la Remesa Documentos: " + parametros.cod_remesa,
                    Movimiento = "APLICA - WEB",
                    Modulo = 9
                });

                return DbHelper.OkResponse("Recepción de documentos aplicada correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse("Error al aplicar la recepción de documentos: " + ex.Message);
            }
        }
    }
}
