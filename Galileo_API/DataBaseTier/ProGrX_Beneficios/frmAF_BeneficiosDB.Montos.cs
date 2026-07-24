using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosDB
    {
        /// <summary>
        /// Guarda un monto de beneficio: inserta si es nuevo (id_bene = 0) o actualiza si ya existe.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Monto">Datos del monto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficioMontos_Guardar(int CodCliente, AfiBeneficioMontoData Monto)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return Monto.id_bene == 0
                    ? AfiBeneficioMontos_Insertar(connection, CodCliente, Monto)
                    : AfiBeneficioMonto_Actualizar(connection, CodCliente, Monto);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un monto de beneficio calculando su consecutivo y deja traza en bitácora.
        /// </summary>
        private ErrorDto AfiBeneficioMontos_Insertar(SqlConnection connection, int CodCliente, AfiBeneficioMontoData Monto)
        {
            const string sqlConsec = "SELECT ISNULL(MAX(id_bene), 0) + 1 FROM afi_beneficio_montos WHERE cod_beneficio = @cod_beneficio";
            var consecutivo = connection.QueryFirstOrDefault<int>(sqlConsec, new { Monto.cod_beneficio });

            const string sql = @"INSERT afi_beneficio_montos (id_bene, cod_beneficio, inicio, corte, monto)
                                 VALUES (@consecutivo, @cod_beneficio, @inicio, @corte, @monto)";
            connection.Execute(sql, new { consecutivo, Monto.cod_beneficio, Monto.inicio, Monto.corte, Monto.monto });

            RegistrarBitacora(CodCliente, "Inserta-Web",
                $"El monto es [{Monto.monto}] y el plazo es [{Monto.inicio}-{Monto.corte}]", Monto.cod_beneficio, Monto.registra_user);

            return new ErrorDto { Code = 0, Description = consecutivo.ToString() };
        }

        /// <summary>
        /// Actualiza un monto de beneficio y deja traza en bitácora.
        /// </summary>
        private ErrorDto AfiBeneficioMonto_Actualizar(SqlConnection connection, int CodCliente, AfiBeneficioMontoData Monto)
        {
            const string sql = @"UPDATE afi_beneficio_montos SET inicio = @inicio, corte = @corte, monto = @monto
                                 WHERE id_bene = @id_bene AND cod_beneficio = @cod_beneficio";
            connection.Execute(sql, new { Monto.inicio, Monto.corte, Monto.monto, Monto.id_bene, Monto.cod_beneficio });

            RegistrarBitacora(CodCliente, "Act-Web",
                $"El nuevo monto es [{Monto.monto}] y el plazo es Fecha Desde: {Monto.inicio} Dias - Fecha Hasta: {Monto.corte} Dias",
                Monto.cod_beneficio, Monto.registra_user);

            return new ErrorDto { Code = 0, Description = Monto.id_bene.ToString() };
        }

        /// <summary>
        /// Elimina un monto de beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_bene">Identificador del monto.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficioMontos_Eliminar(int CodCliente, int id_bene, string cod_beneficio)
        {
            const string sql = "DELETE afi_beneficio_montos WHERE id_bene = @id_bene AND cod_beneficio = @cod_beneficio";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { id_bene, cod_beneficio });
        }

        /// <summary>
        /// Obtiene las fechas de pago automático de un beneficio; si no existen, genera las 12 mensuales.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <param name="Periodo">Periodo consultado.</param>
        /// <returns>Lista de fechas de pago.</returns>
        public ErrorDto<List<AfiBeneFechaPagoData>> AfiBeneFechasPago_Obtener(int CodCliente, string Cod_Beneficio, int Periodo)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var codCategoria = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_CATEGORIA FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = @Cod_Beneficio", new { Cod_Beneficio });

                var existe = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM AFI_BENE_FECHA_PAGO_AUTOMATICO WHERE COD_BENEFICIO = @Cod_Beneficio AND PERIODO = @Periodo",
                    new { Cod_Beneficio, Periodo });

                if (existe > 0)
                {
                    const string sqlExistente = "SELECT * FROM AFI_BENE_FECHA_PAGO_AUTOMATICO WHERE COD_BENEFICIO = @Cod_Beneficio AND PERIODO = @Periodo";
                    return connection.Query<AfiBeneFechaPagoData>(sqlExistente, new { Cod_Beneficio, Periodo }).ToList();
                }

                const string sqlGenerar = @"SELECT n AS id_fecha_pago, @Cod_Beneficio AS cod_beneficio, @codCategoria AS cod_categoria,
                                                EOMONTH(DATEFROMPARTS(YEAR(GETDATE()), n, 1)) AS fecha_corte, n AS mes,
                                                @Periodo AS periodo, 0 AS monto, 1 AS activo
                                            FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12)) AS meses(n)";
                return connection.Query<AfiBeneFechaPagoData>(sqlGenerar, new { Cod_Beneficio, codCategoria, Periodo }).ToList();
            });
        }

        /// <summary>
        /// Guarda las fechas de pago automático validando que el total coincida con el monto del grupo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="DataFechas">Lista de fechas de pago.</param>
        /// <param name="Usuario">Usuario que guarda.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneFechasPago_Guardar(int CodCliente, List<AfiBeneFechaPagoData> DataFechas, string Usuario)
        {
            if (DataFechas == null || DataFechas.Count == 0)
            {
                return DbHelper.ErrorResponse("No hay fechas para guardar");
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var primero = DataFechas[0];

                const string sqlMonto = @"SELECT monto FROM AFI_BENE_GRUPOS
                                          WHERE COD_GRUPO IN (SELECT COD_GRUPO FROM Afi_beneficios WHERE COD_BENEFICIO = @cod_beneficio)";
                var monto = connection.QueryFirstOrDefault<float>(sqlMonto, new { primero.cod_beneficio });

                var sumaMonto = DataFechas.Sum(x => x.monto);
                const float toleranciaMonto = 0.01f;
                if (Math.Abs(monto - sumaMonto) > toleranciaMonto && primero.cod_categoria != "B_RECO" && primero.cod_categoria != "B_FENA")
                {
                    return DbHelper.ErrorResponse("El monto total debe ser igual al monto del grupo asignado: " + monto);
                }

                connection.Execute(
                    "DELETE FROM AFI_BENE_FECHA_PAGO_AUTOMATICO WHERE COD_BENEFICIO = @cod_beneficio AND PERIODO = @periodo",
                    new { primero.cod_beneficio, primero.periodo });

                const string sqlInsert = @"INSERT INTO AFI_BENE_FECHA_PAGO_AUTOMATICO
                                            (cod_beneficio, cod_categoria, fecha_corte, activo, periodo, mes, registro_fecha, registro_usuario, monto)
                                           VALUES
                                            (@cod_beneficio, @cod_categoria, @fecha_corte, @activo, @periodo, @mes, GETDATE(), @usuario, @monto)";

                foreach (var dato in DataFechas)
                {
                    var fechaCorte = dato.fecha_corte.ToString("yyyy-MM-dd") + " 05:00:00";
                    connection.Execute(sqlInsert, new
                    {
                        dato.cod_beneficio,
                        dato.cod_categoria,
                        fecha_corte = fechaCorte,
                        activo = dato.activo ? 1 : 0,
                        dato.periodo,
                        dato.mes,
                        usuario = Usuario,
                        dato.monto
                    });
                }

                return new ErrorDto { Code = 0, Description = "Registros guardados correctamente" };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
