using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioPagoDB
    {
        /// <summary>
        /// Obtiene la lista de beneficios habilitados para pago.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de beneficios.</returns>
        public ErrorDto<List<AfiBenePagoData>> AfiBeneficioPagoLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_Beneficio AS item,
                                            RTRIM(cod_Beneficio) + ' - ' + descripcion AS descripcion
                                     FROM afi_beneficios
                                     WHERE estado = 'A'
                                       AND cod_beneficio IN (SELECT cod_beneficio FROM afi_bene_pago WHERE Estado = 'S')";
                return connection.Query<AfiBenePagoData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene la tabla de pagos pendientes de un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Lista de pagos.</returns>
        public ErrorDto<List<AfiBenePago>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string cod_beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT B.*
                                     FROM afi_bene_pago B
                                     INNER JOIN SOCIOS S ON B.CEDULA = S.CEDULA
                                     WHERE B.cod_beneficio = @cod_beneficio AND B.ESTADO = 'S'";
                return connection.Query<AfiBenePago>(sql, new { cod_beneficio }).ToList();
            });
        }

        /// <summary>
        /// Obtiene el nombre del beneficiario según cédula bancaria.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="consec">Consecutivo del pago.</param>
        /// <param name="cedulabn">Cédula del beneficiario bancario.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Nombre del beneficiario en Description.</returns>
        public ErrorDto Beneficiarios_Obtener(int CodCliente, int consec, string cedulabn, string cod_beneficio)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT nombre FROM beneficiarios
                                     WHERE cedula IN (SELECT cedula FROM afi_bene_pago
                                                      WHERE cod_beneficio = @cod_beneficio AND consec = @consec)
                                       AND cedulabn = @cedulabn";
                return connection.QueryFirstOrDefault<string>(sql, new { cod_beneficio, consec, cedulabn });
            });

            return new ErrorDto { Code = result.Code, Description = result.Result ?? string.Empty };
        }
    }
}
