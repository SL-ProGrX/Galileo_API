using Dapper;
using Newtonsoft.Json;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralOrPDb
    {
        /// <summary>Resultado interno de la validación de existencia de orden de pago.</summary>
        private sealed record ValidaExisteResultado(int Codigo, string Mensaje);

        /// <summary>
        /// Obtiene el beneficio otorgado del socio según los filtros recibidos (JSON AfiBeneOtorgaFiltros).
        /// </summary>
        public ErrorDto<AfiBeneOtorgaData> AfiBeneOtorga_CedulaSocio_Obtener(int CodCliente, string Filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiBeneOtorgaFiltros>(Filtros) ?? new AfiBeneOtorgaFiltros();

            var where = " WHERE O.CEDULA = @cedula";
            if (filtro.categoria != null) where += " AND B.COD_CATEGORIA = @categoria";
            if (filtro.consec != null) where += " AND O.CONSEC = @consec";
            if (filtro.cod_beneficio != null) where += " AND O.COD_BENEFICIO = @codBeneficio";

            var sql = $@"
                SELECT O.*, B.Descripcion, B.PAGOS_MULTIPLES
                FROM afi_bene_otorga O
                INNER JOIN afi_beneficios B ON O.cod_beneficio = B.cod_beneficio
                {where}";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<AfiBeneOtorgaData>(sql, new
                {
                    cedula = filtro.cedula,
                    categoria = filtro.categoria,
                    consec = filtro.consec,
                    codBeneficio = filtro.cod_beneficio
                }));

            return new ErrorDto<AfiBeneOtorgaData>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Obtiene la tabla de pagos (órdenes) del beneficio del socio.
        /// </summary>
        public ErrorDto<List<AfiBeneIntegralOrP>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            const string sql = @"SELECT * FROM afi_bene_pago
                                  WHERE cedula = @cedula AND cod_beneficio = @codBeneficio AND consec = @consec";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBeneIntegralOrP>(sql, new { cedula = Cedula, codBeneficio = Cod_Beneficio, consec = Consec }).ToList());

            return new ErrorDto<List<AfiBeneIntegralOrP>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBeneIntegralOrP>()
            };
        }

        /// <summary>
        /// Valida si ya existe una orden de pago para el expediente y su estado de procesamiento.
        /// Code 0 = no existe; 1 = existe pendiente; 2 = ya procesada.
        /// </summary>
        public ErrorDto AfiBeneficioPagos_ValidaExiste(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            const string sqlExiste = @"
                IF EXISTS (SELECT 1 FROM afi_bene_pago WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND CONSEC = @consec)
                    SELECT 1 AS existe;
                ELSE
                    SELECT 0 AS existe;";

            const string sqlEstado = @"SELECT COUNT(*) FROM afi_bene_pago
                                        WHERE cod_beneficio = @codBeneficio AND cedula = @cedula AND CONSEC = @consec AND estado != 'S'";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var parametros = new { cedula = Cedula, codBeneficio = Cod_Beneficio, consec = Consec };

                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, parametros);
                if (existe != 1)
                {
                    return new ValidaExisteResultado(0, "Ok");
                }

                var estadoValido = connection.QueryFirstOrDefault<int>(sqlEstado, parametros);
                return estadoValido == 0
                    ? new ValidaExisteResultado(1, "Ya existe una orden de pago para este expediente")
                    : new ValidaExisteResultado(2, "La orden de pago para este expediente ya fue procesada");
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return new ErrorDto { Code = result.Result!.Codigo, Description = result.Result.Mensaje };
        }

        /// <summary>
        /// Obtiene la lista de proyecciones de pago del beneficio del socio.
        /// </summary>
        public ErrorDto<List<AfiBenePagoProyecta>> AfiBeneficioIntegralProyeccionPago_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            const string sql = @"SELECT * FROM AFI_BENE_PAGO_PROYECTA
                                  WHERE cedula = @cedula AND cod_beneficio = @codBeneficio AND consec = @consec
                                  ORDER BY FECHA_VENCE ASC";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBenePagoProyecta>(sql, new { cedula = Cedula, codBeneficio = Cod_Beneficio, consec = Consec }).ToList());

            return new ErrorDto<List<AfiBenePagoProyecta>>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result ?? new List<AfiBenePagoProyecta>()
            };
        }
    }
}
