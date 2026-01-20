using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesConceptosDB
    {
        private readonly PortalDB _portalDB;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly MCntLinkDB _mCnt;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmTesConceptosDB(IConfiguration? config)
        {
            _portalDB = new PortalDB(config!);
            _mCnt = new MCntLinkDB(config!);
            _Security_MainDB = new MSecurityMainDb(config!);
        }

        /// <summary>
        /// Obtiene una lista de conceptos de tesorería con paginacion y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TesConceptosLista> Tes_ConceptosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            filtros ??= new FiltrosLazyLoadData();

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<TesConceptosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesConceptosLista
                {
                    total = 0,
                    lista = new List<TesConceptosData>()
                }
            };

            try
            {
                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                var offset = filtros.pagina!;
                var fetch = filtros.paginacion!;
                var usarPaginacion = fetch > 0;

                // Whitelist de sortField (evita inyección por ORDER BY)
                var sortField = (filtros.sortField ?? string.Empty).Trim();
                var orderByField = sortField switch
                {
                    "cod_concepto" => "cod_concepto",
                    "descripcion" => "descripcion",
                    "activo" => "activo",
                    "cod_cuenta_Mask" => "cod_cuenta_Mask",
                    "AUTO_REGISTRO" => "AUTO_REGISTRO",
                    "DP_TRAMITE_APL" => "DP_TRAMITE_APL",
                    _ => "cod_concepto"
                };

                var direction = filtros.sortOrder == 1 ? "DESC" : "ASC";

                const string sqlCount = @"
                        SELECT COUNT(1)
                        FROM vTes_conceptos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(cod_concepto AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (cod_cuenta_Mask LIKE @like);";

                result.Result.total = conn.QuerySingle<int>(sqlCount, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                });

                var sqlList = $@"
                        SELECT
                            cod_concepto,
                            descripcion,
                            activo,
                            cod_cuenta_Mask,
                            AUTO_REGISTRO,
                            DP_TRAMITE_APL
                        FROM vTes_conceptos
                        WHERE
                            (@filtro IS NULL)
                         OR (CAST(cod_concepto AS NVARCHAR(50)) LIKE @like)
                         OR (descripcion LIKE @like)
                         OR (cod_cuenta_Mask LIKE @like)
                        ORDER BY {orderByField} {direction}";

                                        if (usarPaginacion)
                                        {
                                            sqlList += @"
                        OFFSET @offset ROWS
                        FETCH NEXT @fetch ROWS ONLY;";
                }

                result.Result.lista = conn.Query<TesConceptosData>(sqlList, new
                {
                    filtro = hasFiltro ? texto : null,
                    like,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<TesConceptosData>();
            }

            return result;
        }

        /// <summary>
        /// Guarda un concepto de tesorería, ya sea insertando o actualizando según corresponda.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto Tes_Conceptos_Guardar(int CodEmpresa, string usuario ,TesConceptosData concepto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                var query = $@"select isnull(count(*),0) as Existe from tes_conceptos 
                                 where cod_concepto = @concepto";
                var existe = conn.QueryFirstOrDefault<int>(query, new { concepto = concepto.cod_concepto });

                string vCuenta = _mCnt.fxgCntCuentaFormato(CodEmpresa, false, concepto.cod_cuenta_mask, 0);
                bool cuentaValida = _mCnt.fxgCntCuentaValida(CodEmpresa, vCuenta);
                if (!cuentaValida)
                {
                    return DbHelper.ErrorResponse("La cuenta contable no es válida.");
                }

                if (existe > 0)
                {
                    return Tes_Conceptos_Actualizar(CodEmpresa, usuario, vCuenta, concepto);
                }
                else
                {
                    return Tes_Conceptos_Insertar(CodEmpresa, usuario, vCuenta, concepto);
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo concepto de tesorería en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cuenta"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto Tes_Conceptos_Insertar(int CodEmpresa, string usuario, string cuenta ,TesConceptosData concepto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            INSERT INTO tes_conceptos (
                                cod_concepto,descripcion,estado,cod_cuenta,
                                AUTO_REGISTRO, DP_TRAMITE_APL, REGISTRO_FECHA, REGISTRO_USUARIO
                            )
                            VALUES (
                                @cod_concepto, @descripcion, @estado, @cod_cuenta,
                                @auto_registro, @dp_tramite_apl,
                                dbo.myGetdate(), @usuario
                            )";

                conn.Execute(query, new
                {
                    cod_concepto = concepto.cod_concepto,
                    descripcion = concepto.descripcion,
                    estado = concepto.activo ? 'A' : 'I',
                    cod_cuenta = cuenta,
                    auto_registro = concepto.auto_registro ? 1 : 0,
                    dp_tramite_apl = concepto.dp_tramite_apl ? 1 : 0,
                    usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Concepto Desembolso: {concepto.cod_concepto} - {concepto.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Concepto de tesorería insertado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            
        }

        /// <summary>
        /// Actualiza un concepto de tesorería existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cuenta"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        private ErrorDto Tes_Conceptos_Actualizar(int CodEmpresa, string usuario,string cuenta, TesConceptosData concepto)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = @"
                            UPDATE tes_conceptos
                            SET 
                                descripcion = @descripcion,
                                estado = @estado,
                                cod_cuenta = @cod_cuenta,
                                AUTO_REGISTRO = @auto_registro,
                                DP_TRAMITE_APL = @dp_tramite_apl,
                                MODIFICA_FECHA = dbo.myGetdate(),
                                MODIFICA_USUARIO = @usuario
                            WHERE cod_concepto = @cod_concepto";

                conn.Execute(query, new
                {
                    cod_concepto = concepto.cod_concepto,
                    descripcion = concepto.descripcion,
                    estado = (concepto.activo) ? 'A' : 'I',
                    cod_cuenta = cuenta,
                    auto_registro = concepto.auto_registro ? 1 : 0,
                    dp_tramite_apl = concepto.dp_tramite_apl ? 1 : 0,
                    usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Concepto Desembolso: {concepto.cod_concepto} - {concepto.descripcion}",
                    Movimiento = "MODIFICA - WEB",
                    Modulo = vModulo
                });

                return DbHelper.OkResponse("Concepto de tesorería actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un concepto de tesorería por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Tes_Conceptos_Eliminar(int CodEmpresa, string tipo ,string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string query = $@"DELETE FROM tes_conceptos 
                                      WHERE cod_concepto = @cod_concepto";
                conn.Execute(query, new { cod_concepto = tipo });
                _Security_MainDB.Bitacora(
                    new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = "Concepto Desembolso: " + tipo,
                        Movimiento = "ELIMINAR - WEB",
                        Modulo = vModulo
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Método para buscar conceptos para exportar por excel y pdf.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesConceptosData>> Tes_Conceptos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var result = new ErrorDto<List<TesConceptosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesConceptosData>()
            };

            try
            {
                var texto = filtros?.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var like = hasFiltro ? $"%{texto}%" : null;

                const string sql = @"
                            SELECT
                                cod_concepto,
                                descripcion,
                                activo,
                                cod_cuenta_Mask,
                                AUTO_REGISTRO,
                                DP_TRAMITE_APL
                            FROM vTes_conceptos
                            WHERE
                                (@filtro IS NULL)
                             OR (CAST(cod_concepto AS NVARCHAR(50)) LIKE @like)
                             OR (descripcion LIKE @like)
                             OR (cod_cuenta_Mask LIKE @like)
                            ORDER BY cod_concepto;";

                result.Result = conn.Query<TesConceptosData>(sql, new
                {
                    filtro = hasFiltro ? texto : null,
                    like
                }).ToList();

                return DbHelper.CreateOkResponse(result.Result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<TesConceptosData>>(ex.Message);
            }
        }

        /// <summary>
        /// Valida si un concepto de tesorería existe en la base de datos.
        /// Valor -1 para error
        /// Valor  1 para existe
        /// Valor  0 para no existe
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto Tes_Concepto_Valida(int CodEmpresa, string codigo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = $@"SELECT COUNT('X') FROM tes_conceptos 
                                      WHERE  UPPER(COD_CONCEPTO) =  @codigo ";
                var existe = conn.QueryFirstOrDefault<int>(query, new { codigo = codigo.ToUpper() });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("El concepto de tesorería ya existe."); 
                }
                else
                {
                    return DbHelper.OkResponse("El concepto de tesorería no existe.");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
           
        }
    }
}
