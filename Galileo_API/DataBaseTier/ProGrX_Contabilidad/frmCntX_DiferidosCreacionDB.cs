using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDiferidosCreacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private readonly int vModulo = 20;

        public FrmCntXDiferidosCreacionDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config)
            )
        { }

        public FrmCntXDiferidosCreacionDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMain = mProGrxMain;
        }

        /// <summary>
        /// Obtiene la informacion de un diferido especifico basado en su codigo de plantilla y codigo de diferido.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDifPlantilla"></param>
        /// <param name="codDiferido"></param>
        /// <returns></returns>
        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Obtener(
            int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            const string sql = @"
                select 
                    D.*, P.descripcion as DescPlantilla 
                from CntX_diferido_plantilla D
                inner join CntX_Diferidos P
                    on D.cod_diferido = P.cod_diferido
                   and D.cod_contabilidad = P.cod_contabilidad
                   and D.tipo_asiento = P.tipo_asiento
                where D.cod_contabilidad = @CodConta
                  and D.cod_DifPlantilla = @CodDifPlantilla
                  and D.cod_diferido = @CodDiferido;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                new CntXDiferidoCreacionData(),
                new { CodConta = codConta, CodDifPlantilla = codDifPlantilla, CodDiferido = codDiferido }
            );
        }

        /// <summary>
        /// Navega a traves de los diferidos utilizando scroll, 
        /// obteniendo el siguiente o anterior registro basado en el codigo de plantilla y codigo de diferido actual.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="scrollValue"></param>
        /// <param name="codDiferido"></param>
        /// <param name="codDifPlantillaActual"></param>
        /// <returns></returns>
        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Scroll_Obtener(
            int codEmpresa, int codConta, int scrollValue, int codDiferido, int codDifPlantillaActual)
        {
            codDiferido = codDiferido > 0 ? codDiferido : 1;
            codDifPlantillaActual = codDifPlantillaActual >= 0 ? codDifPlantillaActual : 0;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                string sqlNext;
                object param = new
                {
                    CodConta = codConta,
                    CodDiferido = codDiferido,
                    CodActual = codDifPlantillaActual
                };

                if (scrollValue == 1)
                {
                    sqlNext = @"
                    select top 1 cod_DifPlantilla, cod_diferido
                    from CntX_diferido_plantilla
                    where cod_contabilidad = @CodConta
                      and cod_diferido = @CodDiferido
                      and cod_DifPlantilla > @CodActual
                    order by cod_DifPlantilla asc;";
                }
                else
                {
                    sqlNext = @"
                    select top 1 cod_DifPlantilla, cod_diferido
                    from CntX_diferido_plantilla
                    where cod_contabilidad = @CodConta
                      and cod_diferido = @CodDiferido
                      and cod_DifPlantilla < @CodActual
                    order by cod_DifPlantilla desc;";
                }

                var next = conn.QueryFirstOrDefault<CntXDiferidoCreacionData>(sqlNext, param);

                if (next is null)
                    return CntXDiferidosCreacion_Obtener(codEmpresa, codConta, codDifPlantillaActual, codDiferido);

                return CntXDiferidosCreacion_Obtener(codEmpresa, codConta, next.cod_DifPlantilla, next.cod_diferido);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CntXDiferidoCreacionData?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene el historial de un diferido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDifPlantilla"></param>
        /// <param name="codDiferido"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXDiferidoHistoricoData>> CntXDiferidosCreacion_Historico_Obtener(
            int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            const string sql = @"
                select 
                    num_asiento,
                    tipo_asiento,
                    fecha,
                    anio,
                    mes
                from CntX_Diferido_Historico
                where cod_DifPlantilla = @CodDifPlantilla
                  and cod_diferido = @CodDiferido
                  and cod_contabilidad = @CodConta
                order by anio, mes;";

            return DbHelper.ExecuteListQuery<CntXDiferidoHistoricoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodConta = codConta, CodDifPlantilla = codDifPlantilla, CodDiferido = codDiferido }
            );
        }

        /// <summary>
        /// Obtiene la lista de plantillas de diferidos 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXDiferidosPlantillaData>> CntXDiferidosCreacion_PlantillaLista_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"
                select cod_DifPlantilla,cod_diferido,descripcion from CntX_diferido_plantilla 
                where cod_contabilidad = @CodConta";

            return DbHelper.ExecuteListQuery<CntXDiferidosPlantillaData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodConta = codConta }
            );
        }

        /// <summary>
        /// Obtiene la lista de diferidos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXDiferidoCreacionData>> CntXDiferidosCreacion_Lista_Obtener(int codEmpresa, int codConta)
        {
            const string sql = @"
                select cod_diferido,descripcion,case when tipo = 'I' Then 'INGRESOS' 
                when tipo = 'G' Then 'GASTOS' end as Tipo from CntX_Diferidos 
                where cod_contabilidad = @CodConta";

            return DbHelper.ExecuteListQuery<CntXDiferidoCreacionData>(
                _portalDb,
                codEmpresa,
                sql,
                new { CodConta = codConta }
            );
        }

        /// <summary>
        /// Guarda un diferido
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXDiferidosCreacion_Guardar(int codEmpresa, CntXDiferidosCreacionRequest request)
        {
            int codConta = request.cod_contabilidad;
            string usuario = (request.usuario ?? string.Empty).Trim();
            var d = request.data;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                if (request.edita)
                {
                    if (d == null || d.cod_DifPlantilla <= 0 || d.cod_diferido <= 0)
                        return new ErrorDto { Code = -2, Description = "Código inválido para modificar." };

                    var sql = @"
                        update CntX_diferido_plantilla 
                        set descripcion = @Descripcion,
                            detalle = @Detalle,
                            documento = @Documento,
                            estado = @Estado";

                    if (d.acumulado == 0)
                    {
                        string tipoAsiento = ObtenerTipoAsiento(conn, codConta, d.cod_diferido);

                        sql += @",
                            anio = @Anio,
                            mes = @Mes,
                            monto_diferir = @MontoDiferir,
                            acumulado = 0,
                            plazo = @Plazo,
                            cod_diferido = @CodDiferido,
                            tipo_asiento = @TipoAsiento";
                        d.tipo_asiento = tipoAsiento;
                    }

                    sql += @"
                        where cod_contabilidad = @CodConta 
                          and cod_DifPlantilla = @CodDifPlantilla 
                          and cod_diferido = @CodDiferido;";

                    conn.Execute(sql, new
                    {
                        CodConta = codConta,
                        CodDifPlantilla = d.cod_DifPlantilla,
                        CodDiferido = d.cod_diferido,
                        Descripcion = (d.descripcion ?? string.Empty).Trim().ToUpperInvariant(),
                        Detalle = (d.detalle ?? string.Empty),
                        Documento = (d.documento ?? string.Empty),
                        Estado = (d.estado ?? "A").Trim().Substring(0, 1),
                        Anio = d.anio,
                        Mes = d.mes,
                        MontoDiferir = d.monto_diferir,
                        Plazo = d.plazo,
                        TipoAsiento = d.tipo_asiento
                    });

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        movimiento: "Modifica - WEB",
                        detalle: $"DIFERIDO NUM: {d.cod_DifPlantilla} Emp {codConta}"
                    );

                    return new ErrorDto
                    {
                        Code = d.cod_DifPlantilla,
                        Description = "Informacion actualizada satisfactoriamente."
                    };
                }
                else
                {
                    if (d == null || d.cod_diferido <= 0)
                        return new ErrorDto { Code = -2, Description = "Debe indicar la plantilla (cod_diferido)." };

                    int vCodigo = ObtenerConsecutivoPlantilla(conn, codConta, d.cod_diferido);

                    string tipoAsiento = ObtenerTipoAsiento(conn, codConta, d.cod_diferido);

                    const string sqlInsert = @"
                        insert into CntX_diferido_plantilla
                        (cod_diferido, cod_contabilidad, tipo_asiento, cod_DifPlantilla, Anio, Mes,
                         fecha_crea, user_crea, monto_diferir, plazo, acumulado, consecutivo, detalle, documento, estado, descripcion)
                        values
                        (@CodDiferido, @CodConta, @TipoAsiento, @CodDifPlantilla, @Anio, @Mes,
                         getdate(), @Usuario, @MontoDiferir, @Plazo, 0, 0, @Detalle, @Documento, @Estado, @Descripcion);";

                    conn.Execute(sqlInsert, new
                    {
                        CodDiferido = d.cod_diferido,
                        CodConta = codConta,
                        TipoAsiento = tipoAsiento,
                        CodDifPlantilla = vCodigo,
                        Anio = d.anio,
                        Mes = d.mes,
                        Usuario = usuario,
                        MontoDiferir = d.monto_diferir,
                        Plazo = d.plazo,
                        Detalle = (d.detalle ?? string.Empty),
                        Documento = (d.documento ?? string.Empty),
                        Estado = (d.estado ?? "A").Trim().Substring(0, 1),
                        Descripcion = (d.descripcion ?? string.Empty).Trim().ToUpperInvariant()
                    });

                    RegistrarBitacora(
                        codEmpresa,
                        usuario,
                        movimiento: "Registra - WEB",
                        detalle: $"DIFERIDO NUM : {vCodigo} Emp {codConta}"
                    );

                    return new ErrorDto
                    {
                        Code = vCodigo,
                        Description = $"Informacion guardada satisfactoriamente. Código: {vCodigo}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Elimina un diferido especifico 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="usuario"></param>
        /// <param name="codDifPlantilla"></param>
        /// <param name="codDiferido"></param>
        /// <returns></returns>
        public ErrorDto CntXDiferidosCreacion_Eliminar(
            int codEmpresa, int codConta, string usuario, int codDifPlantilla, int codDiferido)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string sqlDelHist = @"
                    delete from CntX_Diferido_Historico
                    where cod_contabilidad = @CodConta
                      and cod_DifPlantilla = @CodDifPlantilla
                      and cod_diferido = @CodDiferido;";

                conn.Execute(sqlDelHist, new
                {
                    CodConta = codConta,
                    CodDifPlantilla = codDifPlantilla,
                    CodDiferido = codDiferido
                });

                const string sqlDel = @"
                    delete from CntX_diferido_plantilla
                    where cod_contabilidad = @CodConta
                      and cod_DifPlantilla = @CodDifPlantilla
                      and cod_diferido = @CodDiferido;";

                conn.Execute(sqlDel, new
                {
                    CodConta = codConta,
                    CodDifPlantilla = codDifPlantilla,
                    CodDiferido = codDiferido
                });

                RegistrarBitacora(
                    codEmpresa,
                    (usuario ?? string.Empty).Trim(),
                    movimiento: "Elimina - WEB",
                    detalle: $"Diferido Plantilla : {codDifPlantilla} / Plantilla {codDiferido} Conta.{codConta}"
                );

                return new ErrorDto { Code = 0, Description = "OK" };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        #region helpers CntXDiferidosCreacion_Guardar 
        private static string ObtenerTipoAsiento(System.Data.IDbConnection conn, int codConta, int codDiferido)
        {
            const string sql = @"
                select tipo_asiento
                from CntX_Diferidos
                where cod_contabilidad = @CodConta
                  and cod_diferido = @CodDiferido;";

            var tipo = conn.QueryFirstOrDefault<string>(
                sql,
                new { CodConta = codConta, CodDiferido = codDiferido }
            );

            if (string.IsNullOrWhiteSpace(tipo))
                throw new InvalidOperationException("No se encontró tipo_asiento para la plantilla indicada (CntX_Diferidos).");

            return tipo.Trim().ToUpperInvariant();
        }

        private static int ObtenerConsecutivoPlantilla(System.Data.IDbConnection conn, int codConta, int codDiferido)
        {
            const string sql = @"
                update CntX_Diferidos
                set consecutivo = isnull(consecutivo,0) + 1
                output inserted.consecutivo
                where cod_contabilidad = @CodConta
                  and cod_diferido = @CodDiferido;";

            var next = conn.QueryFirstOrDefault<int?>(
                sql,
                new { CodConta = codConta, CodDiferido = codDiferido }
            );

            if (!next.HasValue || next.Value <= 0)
                throw new InvalidOperationException("No fue posible generar consecutivo para cod_DifPlantilla (CntX_Diferidos).");

            return next.Value;
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
        #endregion
    }
}
