using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        /// <summary>
        /// Obtiene los datos generales del beneficio otorgado (montos, estado, motivo, etc.).
        /// </summary>
        public ErrorDto<BeneficioGeneral> BeneficioIntegralGeneral_Obtener(int CodCliente, int? id_beneficio)
        {
            const string sql = @"
                SELECT A.ID_BENEFICIO, A.CONSEC, A.COD_BENEFICIO, A.TIPO, A.NOTAS, A.CRECE_GRUPO, A.CEDULA, A.SOLICITA, A.NOMBRE,
                       A.MONTO, A.MONTO_APLICADO, A.MODIFICA_MONTO, B.NOTAS AS observaciones_monto, A.ESTADO,
                       C.NOTAS AS estadoObservaciones, A.FENA_NOMBRE AS desa_nombre, A.FENA_DESCRIPCION AS desa_descripcion,
                       A.SEPELIO_IDENTIFICACION, A.SEPELIO_NOMBRE, A.SEPELIO_FECHA_FALLECIMIENTO, D.COD_MOTIVO,
                       A.REGISTRA_FECHA, A.REGISTRA_USER, A.MODIFICA_USUARIO, A.MODIFICA_FECHA, A.ID_PROFESIONAL, A.ID_APT_CATEGORIA,
                       A.REQUIERE_JUSTIFICACION,
                       (SELECT TOP 1 PAGOS_MULTIPLES FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = A.COD_BENEFICIO) AS PAGOS_MULTIPLES,
                       A.APLICA_MORA, A.APLICA_PAGO_MASIVO
                FROM AFI_BENE_OTORGA A
                LEFT JOIN AFI_BENE_REGISTRO_MONTOS B ON A.COD_BENEFICIO = B.COD_BENEFICIO AND A.CONSEC = B.CONSEC
                LEFT JOIN AFI_BENE_REGISTRO_ESTADOS C ON A.COD_BENEFICIO = C.COD_BENEFICIO AND A.CONSEC = C.CONSEC
                LEFT JOIN AFI_BENE_REGISTRO_MOTIVOS D ON A.COD_BENEFICIO = D.COD_BENEFICIO AND A.CONSEC = D.CONSEC
                WHERE ID_BENEFICIO = @idBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<BeneficioGeneral>(sql, new { idBeneficio = id_beneficio }));

            return new ErrorDto<BeneficioGeneral>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneficioIntegralGeneral_Obtener - " + result.Description,
                Result = result.Result
            };
        }

        /// <summary>
        /// Obtiene los productos asignados al beneficio.
        /// </summary>
        public ErrorDto<List<AfiBenProductoDto>> BeneIntegralGenProductos_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            const string sql = @"
                SELECT [CONSEC], [COD_BENEFICIO], A.[COD_PRODUCTO], B.DESCRIPCION AS prodDesc, [CANTIDAD], A.[COSTO_UNIDAD]
                FROM [AFI_BENE_PRODASG] A
                LEFT JOIN AFI_BENE_PRODUCTOS B ON A.COD_PRODUCTO = B.COD_PRODUCTO
                WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<AfiBenProductoDto>(sql, new { consec, codBeneficio = cod_beneficio }).ToList());

            return new ErrorDto<List<AfiBenProductoDto>>
            {
                Code = result.Code,
                Description = result.Code == 0 ? result.Description : "BeneIntegralGenProductos_Obtener - " + result.Description,
                Result = result.Result ?? new List<AfiBenProductoDto>()
            };
        }

        /// <summary>
        /// Obtiene el registro de mora del beneficio.
        /// </summary>
        public ErrorDto<BeneRegistroMoraDto> BeneRegistroMora_Obtener(int CodCliente, int consec, string beneficio)
        {
            const string sql = @"
                SELECT ID_MORA, ACUERDO, ACUERDO_FECHA, CANCELACION_MORA, MES_CANCELACION, ADELANTO_CUOTA, MES_ADELANTO,
                       CANCELACION_TOTAL_OPERACION, NUMERO_OPERACION
                FROM AFI_BENE_REGISTRO_MORA
                WHERE CONSEC = @consec AND COD_BENEFICIO = @beneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.QueryFirstOrDefault<BeneRegistroMoraDto>(sql, new { consec, beneficio }));

            return new ErrorDto<BeneRegistroMoraDto>
            {
                Code = result.Code,
                Description = result.Description,
                Result = result.Result
            };
        }
    }
}
