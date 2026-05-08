using Dapper;
using Galileo.DataBaseTier; 
using Galileo.Models.ERROR;
using System.Data;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaParametrosModels;
 

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _bitacora;
        private readonly int vModulo = 3;
        private readonly string vModifica = "Modifica - WEB";

        public FrmPreaParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inicializa los parámetros de preanálisis y retorna los datos del grid.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Inicializar(int CodEmpresa)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string procesoQuery = @"EXEC spCRDPreaParametros";
                connection.Execute(procesoQuery);

                return PreaParametros_Grid_Obtener(CodEmpresa);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<List<PreaParametroModel>>(
                    "Error al inicializar los parámetros de preanálisis.",
                    -1,
                    new List<PreaParametroModel>()
                );
            }
        }

        /// <summary>
        /// Obtiene los parámetros de preanálisis para el grid principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Grid_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT
                0 AS Seleccion,
                RTRIM(cod_parametro) AS CodParametro,
                RTRIM(descripcion) AS Descripcion,
                RTRIM(valor) AS Valor,
                FechaActualiza,
                RTRIM(ISNULL(UsuarioActualiza, '')) AS UsuarioActualiza,
                RTRIM(ISNULL(Valor_Anterior, '')) AS ValorAnterior
            FROM Crd_Prea_parametros
            ORDER BY cod_parametro";

                return conn.Query<PreaParametroModel>(query).ToList();
            });
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
            _bitacora.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
         
        /// <summary>
        /// Obtiene los últimos 100 registros históricos de un parámetro de preanálisis.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codParametro"></param>
        /// <returns></returns>
        public ErrorDto<List<PreaParametroHistoricoModel>> PreaParametros_Historico_Obtener(int CodEmpresa, string codParametro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT TOP 100
                        IdHistorico,
                        RTRIM(CodParametro) AS CodParametro,
                        RTRIM(ISNULL(Valor, '')) AS Valor,
                        FechaActualiza,
                        RTRIM(ISNULL(UsuarioActualiza, '')) AS UsuarioActualiza
                    FROM CRD_PREA_PARAMETROS_HISTORICO
                    WHERE CodParametro = @CodParametro
                    ORDER BY IdHistorico DESC";

                return conn.Query<PreaParametroHistoricoModel>(
                    query,
                    new { CodParametro = codParametro }
                ).ToList();
            });
        }

        /// <summary>
        /// Actualiza el valor de un parámetro de preanálisis.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto PreaParametros_Parametro_Actualizar(int CodEmpresa, PreaParametroActualizarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateQuery = @"
                        UPDATE Crd_Prea_parametros
                        SET
                            valor_anterior = valor,
                            FechaActualiza = GETDATE(),
                            UsuarioActualiza = @Usuario,
                            valor = @Valor
                        WHERE cod_parametro = @CodParametro";

                var parametros = new
                {
                    Usuario = request.Usuario?.ToUpper() ?? string.Empty,
                    request.Valor,
                    request.CodParametro
                };

                var filasAfectadas = connection.Execute(updateQuery, parametros, transaction);

                if (filasAfectadas <= 0)
                {
                    transaction.Rollback();

                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "No se encontró el parámetro de preanálisis para actualizar."
                    };
                }
                RegistrarBitacora(CodEmpresa, request.Usuario?.ToUpper() ?? string.Empty, movimiento: vModifica, detalle: $"Parametro de PreAnalisis Cod : {request.CodParametro}");

                const string insertHistoricoQuery = @"
                INSERT INTO CRD_PREA_PARAMETROS_HISTORICO
                (
                    CodParametro,
                    Valor,
                    FechaActualiza,
                    UsuarioActualiza
                )
                VALUES
                (
                    @CodParametro,
                    @Valor,
                    GETDATE(),
                    @Usuario
                )";

                connection.Execute(insertHistoricoQuery, parametros, transaction);
                transaction.Commit();

                return new ErrorDto
                {
                    Code = 0,
                    Description = $"Parámetro {request.CodParametro} actualizado satisfactoriamente."
                };
            }
            catch (Exception)
            {
                transaction.Rollback();
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Error al actualizar el parámetro de preanálisis."
                };
            }
        }

    }
}
